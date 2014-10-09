using System.Linq;
using System.IO;
using Concentrator.Objects.ConcentratorService;
using Concentrator.Plugins.Wehkamp.Helpers;
using PetaPoco;
using Concentrator.Objects.Environments;
using System.Globalization;
using Concentrator.Plugins.PFA.Objects.Helper;
using System.Collections.Generic;
using System;
using System.Data;

namespace Concentrator.Plugins.Wehkamp
{
  public class ShipmentConfirmationImport : ConcentratorPlugin
  {
    private readonly Monitoring.Monitoring _monitoring = new Monitoring.Monitoring();

    public override string Name
    {
      get { return "Wehkamp Shipment Confirmation Import"; }
    }

    protected override void Process()
    {
      _monitoring.Notify(Name, 0);

      var messages = MessageHelper.GetMessagesByStatusAndType(Enums.WehkampMessageStatus.Created, MessageHelper.WehkampMessageType.ShipmentConfirmation);
      foreach (var message in messages)
      {
        log.Info(string.Format("{0} - Loading file: {1}", Name, message.Filename));

        MessageHelper.UpdateMessageStatus(message.MessageID, Enums.WehkampMessageStatus.InProgress);
        aankomstBevestiging retourData;

        var loaded = aankomstBevestiging.LoadFromFile(Path.Combine(message.Path, message.Filename), out retourData);

        if (!loaded)
        {
          log.AuditError(string.Format("Error while loading file {0}", message.Filename));
          MessageHelper.Error(message);
          continue;
        }

        var connectorID = VendorHelper.GetConnectorIDByVendorID(message.VendorID);
        var defaultWarehouseCode = VendorHelper.GetReturnDifferenceShopNumber(message.VendorID);

        var customerOrderReferences = string.Join(",", retourData.aankomst.Select(c => c.ggb).Distinct().ToArray());

        customerOrderReferences = "'" + customerOrderReferences.Replace(",", "','") + "'";

        var productTranslations = retourData.aankomst.Select(c => new
        {
          value = Concentrator.Plugins.Wehkamp.Helpers.ProductHelper.GetProductIDByWehkampData(c.artikelNummer, c.kleurNummer, c.maat, message.VendorID),
          key = string.Format("{0} {1} {2}", c.artikelNummer, c.kleurNummer, c.maat)
        }).Distinct().ToDictionary(c => c.key, c => c.value);

        var shipmentConfirmationsImported = false;

        using (var db = new Database(Environments.Current.Connection, "System.Data.SqlClient"))
        {
          var orderMatches = db.Dictionary<string, int>(string.Format(@"SELECT DISTINCT CustomerOrderReference, OrderID 
                                                                        FROM [Order] 
                                                                        WHERE ConnectorID = {1} AND WebSiteOrderNumber IN ({0})", customerOrderReferences, connectorID));

          var groupedRetourData =
            from r in retourData.aankomst
            group r by new
            {
              r.artikelNummer,
              r.kleurNummer,
              r.maat,
              r.ggb
            }
              into gr
              select new aankomstBevestigingAankomst
              {
                artikelNummer = gr.Key.artikelNummer,
                kleurNummer = gr.Key.kleurNummer,
                maat = gr.Key.maat,
                ggb = gr.Key.ggb,
                aantalOntvangen = gr.Sum(r => int.Parse(r.aantalOntvangen)).ToString(CultureInfo.InvariantCulture),
                werkelijkeAankomstDatum = gr.First().werkelijkeAankomstDatum,
                locusStatus = gr.First().locusStatus,
                goederenSoort = gr.First().goederenSoort
              };

          shipmentConfirmationsImported = ProcessShipmentConfirmations(db, groupedRetourData, productTranslations, orderMatches, message);

          if (!shipmentConfirmationsImported)
          {
            int failedAttempts = db.ExecuteScalar<int>(string.Format("SELECT FailedAttempts from WehkampMessage WHERE MessageID = {0}", message.MessageID));
            failedAttempts++;

            db.Execute("UPDATE WehkampMessage SET FailedAttempts = @0 WHERE MessageID = @1;", failedAttempts, message.MessageID);

            if (failedAttempts <= 5)
            {
              MessageHelper.UpdateMessageStatus(message.MessageID, Enums.WehkampMessageStatus.Created);
            }
            else
            {
              MessageHelper.UpdateMessageStatus(message.MessageID, Enums.WehkampMessageStatus.Error);
              MessageHelper.Error(message);
            }
          }
          else
          {
            MessageHelper.Archive(message);
          }
        }
      }
      _monitoring.Notify(Name, 1);
    }

    internal bool ProcessShipmentConfirmations(Database db, IEnumerable<aankomstBevestigingAankomst> groupedRetourData, Dictionary<string, int> productTranslations, Dictionary<string, int> orderMatches, MessageHelper.WehkampMessage message)
    {
      db.BeginTransaction(IsolationLevel.ReadCommitted);

      try
      {
        foreach (var response in groupedRetourData)
        {
          var combinedVendorItemNumber = string.Format("{0} {1} {2}", response.artikelNummer, response.kleurNummer, response.maat);

          if (!productTranslations.ContainsKey(combinedVendorItemNumber))
          {
            log.AuditError(string.Format("Could not find {0} in CustomerOrderReference in [Order], response {1} in message {2} not processed", response.ggb, response.artikelNummer, message.Filename));
            continue;
          }

          if (!orderMatches.ContainsKey(response.ggb))
          {
            log.AuditError(string.Format("Could not find {0} in VendorItemNumber in Product, response {1} in message {2} not processed", combinedVendorItemNumber, response.artikelNummer, message.Filename));
            continue;
          }

          var productID = productTranslations[combinedVendorItemNumber];
          var orderID = orderMatches[response.ggb];

            var rowsAffected = db.Execute(string.Format("INSERT INTO OrderLedger ([OrderLineID],[Status],[LedgerDate],[Quantity]) (SELECT TOP 1 OrderLineID, '{2}', '{3}', '{4}' FROM OrderLine WHERE OrderID = {0} AND ProductID = {1})", orderID, productID, (int)OrderLineStatus.ReceivedTransfer, response.werkelijkeAankomstDatum.ToUniversalTime().ToString(MessageHelper.ISO8601, CultureInfo.InvariantCulture), response.aantalOntvangen));
          if (rowsAffected == 0)
          {
            //Create OrderLine
            var orderLineID = Helpers.StockReturnHelper.CreateOrderLineRow(orderID, productID, message.VendorID);

            //and the orderLedger
              db.Execute(string.Format("INSERT INTO OrderLedger ([OrderLineID],[Status],[LedgerDate],[Quantity]) (SELECT TOP 1 OrderLineID, '{2}', '{3}', '{4}' FROM OrderLine WHERE OrderID = {0} AND ProductID = {1})", orderID, productID, (int)OrderLineStatus.ReceivedTransfer, response.werkelijkeAankomstDatum.ToUniversalTime().ToString(MessageHelper.ISO8601, CultureInfo.InvariantCulture), response.aantalOntvangen));

            log.AuditInfo(string.Format("Order and OrderLedgers creted for {0} in message {1}, no existing Orderlines found!", combinedVendorItemNumber, message.MessageID));
          }

          if (rowsAffected > 1)
            log.AuditInfo(string.Format("Multiple rows added for {0} in message {1}, please check data consistency", combinedVendorItemNumber, message.MessageID));

        }
      }
      catch (Exception ex)
      {
        //If any response fails, abort transaction and set file for reprocessing.
        db.AbortTransaction();
        log.ErrorFormat("Error processing ShipmentConfirmations, enabled MessageID: {0} for reprocessing. Exception: {1}", message.MessageID, ex.StackTrace);
        return false;
      }

      //Success
      db.CompleteTransaction();
      return true;
    }
  }
}
