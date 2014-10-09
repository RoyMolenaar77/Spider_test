using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Concentrator.Objects.ConcentratorService;
using Concentrator.Objects.Environments;
using Concentrator.Plugins.Wehkamp.Enums;
using Concentrator.Plugins.Wehkamp.Helpers;

namespace Concentrator.Plugins.Wehkamp
{
  public class StockReturnRequestExport : ConcentratorPlugin
  {
    private readonly Monitoring.Monitoring _monitoring = new Monitoring.Monitoring();

    public override string Name
    {
      get { return "Wehkamp Stock Return Request Export"; }
    }

    protected override void Process()
    {
      _monitoring.Notify(Name, 0);
      log.InfoFormat("Start processing Stock Return Request");

      var vendorIDsToProcess = VendorSettingsHelper.GetVendorIDsToExportToWehkamp(log);

      foreach (var vendorID in vendorIDsToProcess)
      {
        _monitoring.Notify(Name, vendorID);
        log.InfoFormat("Start processing Stock Return Request for VendorID {0}", vendorID);

        //Get all stock return request that we need to export to Wehkamp
        var stockReturns = GetStockReturnRequestData(vendorID);
        if (stockReturns == null || stockReturns.Count == 0)
          continue;

        //Add all return requests to the retourAanvraag file
        var returnRequest  = new retourAanvraag();

        log.InfoFormat("Start processing {0} Stock Return Requests for VendorID {1}", stockReturns.Count, vendorID);
        var start = DateTime.Now;
        var processedCount = 0;
        foreach (var stockReturn in stockReturns)
        {
          returnRequest.aanvraag.Add(CreateNewReturnRequestItem(stockReturn));

          processedCount++;
          if (DateTime.Now > start.AddSeconds(30))
          {
            log.InfoFormat("Processed {0} Stock Return Requests for VendorID {1}", processedCount, vendorID);
            start = DateTime.Now;
          }
        }

        //Save data to disk
        log.InfoFormat("Start saving Stock Return Requests");
        SaveStockReturn(vendorID, returnRequest);

        //Set all orders and orderlines as dispatched
        log.InfoFormat("Start setting Stock Return Requests as dispatched");
        SetReturnRequestsAsExportedToWehkamp(stockReturns);

        log.InfoFormat("Finished processing Stock Return Requests for VendorID {0}", vendorID);
      } //end foreach (var vendorID in vendorIDsToProcess)

      log.InfoFormat("Finished processing Stock Return Requests");
      _monitoring.Notify(Name, 1);
    }




    private static retourAanvraagAanvraag CreateNewReturnRequestItem(StockReturnRequest item)
    {
      return new retourAanvraagAanvraag
      {
        artikelNummer = item.Artikelnummer,
        kleurNummer = item.Kleurnummer,
        statusIndicatie = indicatieType.N
      };
    }


    private List<StockReturnRequest> GetStockReturnRequestData(int vendorID)
    {
      try
      {
        using (var db = new PetaPoco.Database(Environments.Current.Connection, "System.Data.SqlClient"))
        {
          db.CommandTimeout = 600; //10 minutes
          var products = db.Fetch<StockReturnRequest>(string.Format(QueryHelper.GetStockReturnRequestQuery(), vendorID));
          return products;
        }
      }
      catch (Exception ex)
      {
        log.Error("Error while retrieving stock return requests to export to Wehkamp.", ex);
        return null;
      }

    }
    
    private void SaveStockReturn(int vendorID, retourAanvraag stockReturnRequest)
    {
      var alliantieName = VendorSettingsHelper.GetAlliantieName(vendorID);
      var retailPartnerCode = VendorSettingsHelper.GetRetailPartnerCode(vendorID);
      var sequenceNumber = CommunicatorHelper.GetSequenceNumber(vendorID);

      stockReturnRequest.header.berichtDatumTijd = DateTime.Now;
      stockReturnRequest.header.berichtNaam = "retourAanvraag";
      stockReturnRequest.header.retailPartnerCode = retailPartnerCode;
      stockReturnRequest.header.bestandsNaam = string.Format("{0}{1}retourAanvraag.xml", sequenceNumber, alliantieName);

      var messageID = MessageHelper.InsertMessage(MessageHelper.WehkampMessageType.StockReturnRequest, stockReturnRequest.header.bestandsNaam, vendorID);

      try
      {
        stockReturnRequest.SaveToFile(string.Format(Path.Combine(ConfigurationHelper.StockReturnRequestRootFolder, vendorID.ToString(CultureInfo.InvariantCulture), stockReturnRequest.header.bestandsNaam)));
        MessageHelper.UpdateMessageStatus(messageID, WehkampMessageStatus.Success);
      }
      catch (Exception ex)
      {
        log.Fatal("Error while saving retourAanvraag file", ex);
        MessageHelper.UpdateMessageStatus(messageID, WehkampMessageStatus.Error);
      }
    }
    


    private static void SetReturnRequestsAsExportedToWehkamp(List<StockReturnRequest> returns)
    {
      using (var db = new PetaPoco.Database(Environments.Current.Connection, "System.Data.SqlClient"))
      {
        db.CommandTimeout = 30;

        //All OrderID's
        var sqlOrder = string.Format("UPDATE [Order] SET IsDispatched = 1 WHERE OrderID IN ({0})", string.Join(",", returns.Select(o => o.OrderID).Distinct().ToArray()));
        db.Execute(sqlOrder);

        //All OrderLineID's
        var sql = string.Format("UPDATE [OrderLine] SET IsDispatched = 1 WHERE OrderLineID IN (SELECT OrderLineID FROM OrderLine WHERE OrderID IN ({0}))", string.Join(",", returns.Select(o => o.OrderID).Distinct().ToArray()));
        db.Execute(sql);
      }
    }
  }

  internal class StockReturnRequest
  {
    public string Artikelnummer { get; set; }
    public string Kleurnummer { get; set; }
    public string FormattedKleurnummer { get; set; }
    public int ProductID { get; set; }
    public int VendorID { get; set; }
    public int OrderID { get; set; }
    public int OrderLineID { get; set; }
  }  
}
