using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using Concentrator.Objects.ConcentratorService;
using Concentrator.Objects.Environments;
using Concentrator.Objects.Models.Orders;
using Concentrator.Plugins.Wehkamp.Enums;
using Concentrator.Plugins.Wehkamp.Helpers;
using PetaPoco;

namespace Concentrator.Plugins.Wehkamp
{
  public class SalesOrderImport : ConcentratorPlugin
  {
    private readonly Monitoring.Monitoring _monitoring = new Monitoring.Monitoring();

    public override string Name
    {
      get { return "Wehkamp Sales Order Import"; }
    }

    protected override void Process()
    {
      _monitoring.Notify(Name, 0);
      log.InfoFormat("Start processing Sales Order Import");

      log.InfoFormat("Get messages to process");
      var salesOrderFiles = MessageHelper.GetMessagesByStatusAndType(WehkampMessageStatus.Created, MessageHelper.WehkampMessageType.SalesOrder);

      log.InfoFormat("Found {0} messages to process", salesOrderFiles.Count);
      foreach (var file in salesOrderFiles)
      {
        log.Info(string.Format("{0} - Loading file: {1}", Name, file.Filename));

        kassaInformatie salesOrder;
        var result = kassaInformatie.LoadFromFile(Path.Combine(file.Path, file.Filename), out salesOrder);

        //Set message status
        MessageHelper.UpdateMessageStatus(file.MessageID, result ? WehkampMessageStatus.InProgress : WehkampMessageStatus.Error);

        var salesOrdersImported = false;

        if (result)
        {
          var vendorID = file.VendorID;
          var connectorID = VendorSettingsHelper.GetConnectorIDByVendorID(vendorID);
          var orderItems = new List<SalesOrderObject>();

          #region foreach (var so in salesOrder.kassabon)
          log.InfoFormat("Found {0} orders to process", salesOrder.kassabon.Count);
          var counter = 0;



          foreach (var so in salesOrder.kassabon)
          {
            var document = so.Serialize();
            var receivedDate = DateTime.ParseExact(string.Format("{0} {1}", so.zendingDatum.ToString("yyyy-MM-dd"), DateTime.Now.ToString("HH:mm:ss")), "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

            var salesOrderItem = new SalesOrderObject
            {
              SalesOrderID = -1,
              Document = document,
              ConnectorID = connectorID,
              IsDispatched = 1,
              ReceivedDate = receivedDate.ToUniversalTime(),
              IsDropShipment = 1,
              HoldOrder = 0,
              WebsiteOrderNumber = so.kassabonNummer,
              OrderType = (int)OrderTypes.SalesOrder,
              IsSalesOrder = so.klantMutatie.ToLower(CultureInfo.InvariantCulture) == "verkoop"
            };


            foreach (var r in so.kassabonregel)
            {
              var quantity = int.Parse(r.verkoopAantal, CultureInfo.InvariantCulture);
              var productID = ProductHelper.GetProductIDByWehkampData(r.artikelNummer, r.kleurNummer, r.maat, vendorID);

              if (productID != 0) //ProductHelper returns FirstOrDefault of the productid, which means it will have the value 0 when it can't be found in VendorAssortment. If we don't check for this here it will break when inserting an orderline.
              {
                var soLine = new SalesOrderLineObject
                {
                  ProductID = productID,
                  Quantity = quantity,
                  BasePrice = -1,
                  UnitPrice = r.factuurBedrag + r.kortingBedrag, // + korting bedrag per item
                  Price = (r.factuurBedrag + r.kortingBedrag) * quantity, // + korting bedrag per item
                  LineDiscount = r.kortingBedrag * quantity, //totaal korting bedrag (dus voor alle items totaal)
                  IsDispatched = 1,
                };

                salesOrderItem.SalesOrderLines.Add(soLine);
              }
              else
              {
                log.ErrorFormat("Unable to find ProductID in VendorAssortment for artikelNummer: {0} with kleurNummer: {1}, maat: {2} and vendorID: {3}. Aborting the processing of this Kassabon.", r.artikelNummer, r.kleurNummer, r.maat, vendorID);
                MessageHelper.UpdateMessageStatus(file.MessageID, WehkampMessageStatus.Error);
                return;
              }
            }

            orderItems.Add(salesOrderItem);

            counter++;
            if (counter % 25 == 0)
            {
              log.Debug(string.Format("Loaded {0} orders from file", counter));
            }

          }
          log.Debug(string.Format("Loaded {0} orders from file", counter));

          #endregion

        salesOrdersImported = ProcessSalesOrders(orderItems, vendorID);
        }
        else
        {
          log.Error(string.Format("Error loading file {0}", file.Filename));
          MessageHelper.Error(file.MessageID);
        }

        if (salesOrdersImported)
        {
          MessageHelper.Archive(file.MessageID);
        }
        else
        {
          MessageHelper.UpdateMessageStatus(file.MessageID, WehkampMessageStatus.Error);
        }
      }
      log.InfoFormat("Finished processing Sales Order Import");
      _monitoring.Notify(Name, 1);
    }


    internal bool ProcessSalesOrders(List<SalesOrderObject> salesOrders, int vendorID)
    {
      using (var database = new Database(Environments.Current.Connection, "System.Data.SqlClient"))
      {
        database.BeginTransaction(IsolationLevel.ReadCommitted);

        var counter = 0;

        try
        {
          foreach (var order in salesOrders)
          {
            //Create Order
            var orderID = SalesOrderHelper.CreateSalesOrderAndReturnOrderID(order, database);

            //Create all orderlines
            foreach (var orderLine in order.SalesOrderLines)
            {
              //Don't forget to set the correct OrderID
              orderLine.SalesOrderID = orderID;
              SalesOrderHelper.CreateSalesOrderLineAndReturnOrderLineID(orderLine, vendorID, order.IsSalesOrder, order.ReceivedDate, database);
            }

            counter++;
            if (counter % 25 == 0)
            {
              log.Debug(string.Format("Processed {0} orders to database", counter));
            }
          }
        }
        catch (Exception ex)
        {
          database.AbortTransaction();
          log.AuditError("Error in ProcessSalesOrders", ex);

          return false;
        }

        //Success
        database.CompleteTransaction();
        log.Debug(string.Format("Processed {0} orders to database", counter));

        return true;
      }
    }
  }

  internal class SalesOrderObject
  {
    internal SalesOrderObject()
    {
      SalesOrderLines = new List<SalesOrderLineObject>();
    }

    public int SalesOrderID { get; set; }
    public string Document { get; set; }
    public int ConnectorID { get; set; }
    public int IsDispatched { get; set; }
    public DateTime ReceivedDate { get; set; }
    public int IsDropShipment { get; set; }
    public string WebsiteOrderNumber { get; set; }
    public int HoldOrder { get; set; }
    public int OrderType { get; set; }
    public bool IsSalesOrder { get; set; }

    public List<SalesOrderLineObject> SalesOrderLines { get; set; }
  }

  internal class SalesOrderLineObject
  {
    public int SalesOrderID { get; set; }
    public int SalesOrderLineID { get; set; }
    public int ProductID { get; set; }
    public decimal Price { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal BasePrice { get; set; }
    public decimal LineDiscount { get; set; }
    public int Quantity { get; set; }
    public int IsDispatched { get; set; }
  }
}
