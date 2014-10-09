using System;
using System.Linq;
using Concentrator.Objects.ConcentratorService;
using Concentrator.Objects.Models.Orders;
using Concentrator.Vendors.PFA.FileFormats;
using Concentrator.Objects.Models.Vendors;
using System.IO;
using FileHelpers;
using Concentrator.Objects;
using Concentrator.Objects.Enumerations;

//TODO: REMOVE

namespace Concentrator.Plugins.PFA
{
  public class ShopSalesOrderExportPFA : ConcentratorPlugin
  {
    public override string Name
    {
      get { return "Shop Sales order export PFA"; }
    }

    protected override void Process()
    {
      try
      {
        var config = GetConfiguration();

        NetworkExportUtility util = new NetworkExportUtility();
        using (var unit = GetUnitOfWork())
        {
          var fileLocation = config.AppSettings.Settings["DatColLocation"].Value;

          if (string.IsNullOrEmpty(fileLocation))
            throw new Exception("No DatColLocation vendorsetting");

          var userName = config.AppSettings.Settings["DatColLocationUserName"].Value;
          if (string.IsNullOrEmpty(userName))
            throw new Exception("No DatColLocation UserName");

          var password = config.AppSettings.Settings["DatColLocationPassword"].Value;
          if (string.IsNullOrEmpty(password))
            throw new Exception("No DatColLocation Password");

          fileLocation = util.ConnectorNetworkPath(fileLocation, "Z:", userName, password);

          var vendorID = int.Parse(config.AppSettings.Settings["ccVendorID"].Value);


#if DEBUG
          fileLocation = @"E:\Concentrator_TESTING";
#endif

          var ledgerRepo = unit.Scope.Repository<OrderLedger>();

          using (var engine = new MultiRecordEngine(typeof(DatColTransfer), typeof(DatColNormalSales)))
          {
            var file = Path.Combine(fileLocation, "datcol");


            if (!File.Exists(file))
            {
              File.Create(file).Dispose();
            }

            engine.BeginAppendToFile(file);

            #region Orders
            var orderlinesReady = unit.Scope.Repository<OrderLine>().GetAll(x => x.isDispatched && x.OrderLedgers.Any(y => y.Status == (int)OrderLineStatus.ReadyToOrder) && x.Order.PaymentTermsCode == "Shop").ToList();
            var orderlinesExport = unit.Scope.Repository<OrderLine>().GetAll(x => x.isDispatched && x.OrderLedgers.Any(y => y.Status == (int)OrderLineStatus.ProcessedExportNotification && x.Order.PaymentTermsCode == "Shop")).ToList();

            var lines = orderlinesReady.Except(orderlinesExport).ToList();

            lines.GroupBy(c => c.OrderID).ToList().ForEach(orderLinesCollection =>
            {
              var order = unit.Scope.Repository<Order>().GetSingle(c => c.OrderID == orderLinesCollection.Key);

              var salesSlipNr = order.WebSiteOrderNumber.Split(new char[] { '_' }, StringSplitOptions.RemoveEmptyEntries)[1];
              int counter = 1;
              foreach (var line in orderLinesCollection)
              {
                var barcode = line.Product.ProductBarcodes.FirstOrDefault(x => x.BarcodeType.HasValue && x.BarcodeType.Value == 0);
                var pfaCode = line.Product.ProductBarcodes.Where(c => c.BarcodeType == (int)BarcodeTypes.PFA).FirstOrDefault();
                var articleSizeColorArray = line.Product.VendorItemNumber.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                var va = line.Product.VendorAssortments.FirstOrDefault(x => x.VendorID == line.DispatchedToVendorID);
                var productPrice = (va != null && va.VendorPrices.FirstOrDefault() != null ? va.VendorPrices.FirstOrDefault().Price.Value : (line.Price.HasValue ? Convert.ToDecimal(line.Price.Value) : 0));
                var specialPrice = (va != null && va.VendorPrices.FirstOrDefault() != null && va.VendorPrices.FirstOrDefault().SpecialPrice.HasValue ? va.VendorPrices.FirstOrDefault().SpecialPrice.Value : 0);

                var articleCode = string.Empty;
                var colorCode = string.Empty;
                if (articleSizeColorArray.Count() > 0)
                {
                  articleCode = articleSizeColorArray[0].PadLeft(13, '0');
                }
                if (articleSizeColorArray.Count() > 1)
                {
                  colorCode = articleSizeColorArray[1].PadLeft(3, '0');
                }
                var sizeCode = (pfaCode != null ? pfaCode.Barcode.PadLeft(4, '0') : string.Empty);

                var shopNumber = line.Order.ShippedToCustomer.CompanyName.Replace("X", string.Empty);

                DatColTransfer sale = new DatColTransfer()
                {
                  ShopAndPosNr = string.Format("{0} 01", shopNumber),
                  SalesslipNumber = int.Parse(salesSlipNr),
                  DateStamp = DateTime.Now,
                  Quantity = line.Quantity,
                  ReceivedFrom = 890,
                  TransferNumber = string.Format("00{0}{1}", shopNumber, salesSlipNr),
                  ArticleColorSize = String.Format("{0}{1}{2}", articleCode, colorCode, sizeCode),
                  Barcode = barcode != null ? barcode.Barcode : string.Empty,
                  MarkdownValue = (int)Math.Round((decimal)(line.BasePrice - line.UnitPrice) * 100),
                  OriginalRetailValue = (int)(Math.Round((decimal)(line.Price * 100))),
                  RecordSequence = counter * 100
                };

                line.SetStatus(OrderLineStatus.ProcessedExportNotification, ledgerRepo);
                engine.WriteNext(sale);
                counter++;
              }
            });
            #endregion

            engine.Flush();
#if !DEBUG
            util.DisconnectNetworkPath(fileLocation);
#endif
            unit.Save();
          }
        }
      }
      catch (Exception ex)
      {
        log.AuditError("Sales order export failed", ex);
      }
    }
  }
}
