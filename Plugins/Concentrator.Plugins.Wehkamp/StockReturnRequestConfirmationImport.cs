using System;
using System.Globalization;
using System.IO;
using System.Linq;
using Concentrator.Objects.ConcentratorService;
using Concentrator.Objects.Models.Orders;
using Concentrator.Plugins.Wehkamp.Enums;
using Concentrator.Plugins.Wehkamp.Helpers;
using System.Collections.Generic;
using Concentrator.Plugins.PFA.Objects.Helper;

namespace Concentrator.Plugins.Wehkamp
{
  public class StockReturnRequestConfirmationImport : ConcentratorPlugin
  {
    private readonly Monitoring.Monitoring _monitoring = new Monitoring.Monitoring();

    public override string Name
    {
      get { return "Wehkamp Stock Return Request Confirmation Import"; }
    }

    protected override void Process()
    {
      _monitoring.Notify(Name, 0);
      log.InfoFormat("Start processing Stock Return Request Confirmation Import");

      log.InfoFormat("Get messages to process");
      var files = MessageHelper.GetMessagesByStatusAndType(WehkampMessageStatus.Created, MessageHelper.WehkampMessageType.StockReturnRequestConfirmation);

      log.InfoFormat("Found {0} messages to process", files.Count);
      foreach (var file in files)
      {
        var wehkampOrderID = -1;

        try
        {
          log.Info(string.Format("{0} - Loading file: {1}", Name, file.Filename));

          retourUitslag stockReturnRequestConfirmation;
          var result = retourUitslag.LoadFromFile(Path.Combine(file.Path, file.Filename), out stockReturnRequestConfirmation);

          //Set message status
          MessageHelper.UpdateMessageStatus(file.MessageID, result ? WehkampMessageStatus.InProgress : WehkampMessageStatus.Error);

          if (result)
          {
            var vendorID = file.VendorID;
            var defaultWarehouseCode = VendorHelper.GetReturnDifferenceShopNumber(vendorID);
            var confirmationItems = new List<StockReturnRequestConfirmationLineObject>();

            #region foreach (var confirmation in stockReturnRequestConfirmation.bevestiging)

            log.InfoFormat("Found {0} confirmations to process", stockReturnRequestConfirmation.uitslag.Count);
            var counter = 0;

            var sendingTime = DateTime.ParseExact(string.Format("{0} {1}", stockReturnRequestConfirmation.header.berichtDatumTijd.ToString("yyyy-MM-dd"), DateTime.Now.ToString("HH:mm:ss")), "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

            var groupedUitslag =
              from u in stockReturnRequestConfirmation.uitslag
              group u by new
              {
                u.artikelNummer,
                u.kleurNummer,
                u.maat
              }
                into gu
                select new retourUitslagUitslag
                {
                  artikelNummer = gu.Key.artikelNummer,
                  kleurNummer = gu.Key.kleurNummer,
                  maat = gu.Key.maat,
                  locusStatus = "not used",
                  verzondenAantal = gu.Sum(u => int.Parse(u.verzondenAantal)).ToString(CultureInfo.InvariantCulture)
                };

            //foreach (var confirmation in stockReturnRequestConfirmation.uitslag)
            foreach (var confirmation in groupedUitslag)
            {
              int orderID;
              int orderLineID;

              //create object
              var productID = Helpers.ProductHelper.GetProductIDByWehkampData(confirmation.artikelNummer, confirmation.kleurNummer, confirmation.maat, vendorID);

              if (StockReturnHelper.OrderLineExistForProductAndOrderType((int)OrderTypes.ReturnOrder, productID))
              {
                orderLineID = StockReturnHelper.GetLastOrderLineIDByOrderTypeAndProductID((int)OrderTypes.ReturnOrder, productID);
                orderID = StockReturnHelper.GetOrderIDByOrderLineID(orderLineID);
              }
              else
              {
                //Order doesn't exists in the database.
                //Create order and orderline for this product.

                //Check if we already created an order for this file
                if (wehkampOrderID == -1)
                {
                  var receivedDate = DateTime.ParseExact(string.Format("{0} {1}", stockReturnRequestConfirmation.header.berichtDatumTijd.ToString("yyyy-MM-dd"), DateTime.Now.ToString("HH:mm:ss")), "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                  wehkampOrderID = StockReturnHelper.CreateOrderRowForWehkampReturns(VendorSettingsHelper.GetConnectorIDByVendorID(vendorID), Path.GetFileNameWithoutExtension(file.Filename), file.Filename, receivedDate.ToUniversalTime());
                }

                orderLineID = StockReturnHelper.CreateOrderLineRow(wehkampOrderID, productID, vendorID, defaultWarehouseCode.ToString(CultureInfo.InvariantCulture));
                orderID = wehkampOrderID;
              }

              var c = new StockReturnRequestConfirmationLineObject
              {
                OrderID = orderID,
                OrderLineID = orderLineID,
                ProductID = productID,
                Quantity = int.Parse(confirmation.verzondenAantal)
              };

              confirmationItems.Add(c);

              if (counter % 25 == 0)
              {
                log.Debug(string.Format("Loaded {0} confirmations from file", counter));
              }
              counter++;
            }

            counter = 0;
            foreach (var item in confirmationItems)
            {
              StockReturnHelper.CreateOrderLedgerRow(item.OrderLineID, item.Quantity, sendingTime.ToUniversalTime());

              if (counter % 25 == 0)
              {
                log.Debug(string.Format("Inserted {0} confirmations into database", counter));
              }
              counter++;
            }

            #endregion

            MessageHelper.Archive(file.MessageID);
          }
        }
        catch (Exception exFile)
        {
          log.Fatal(string.Format("Error processing file {0} ", file.Filename), exFile);
          MessageHelper.Error(file.MessageID);
        }
      }

      log.InfoFormat("Finished processing Stock Return Request Confirmation Import");
      _monitoring.Notify(Name, 1);
    }
  }

  internal class StockReturnRequestConfirmationLineObject
  {
    public int OrderID { get; set; }
    public int OrderLineID { get; set; }
    public int ProductID { get; set; }
    public int Quantity { get; set; }

  }
}
