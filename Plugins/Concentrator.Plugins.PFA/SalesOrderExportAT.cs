using System;
using System.Collections.Generic;
using System.Linq;
using Concentrator.Objects.ConcentratorService;
using Concentrator.Objects;
using Concentrator.Objects.Models.Orders;
using Concentrator.Plugins.PFA.Helpers;
using Concentrator.Vendors.PFA.FileFormats.AT;
using Concentrator.Objects.Enumerations;
using Concentrator.Objects.Models.Vendors;
using FileHelpers;
using System.IO;
using Concentrator.Plugins.PFA.Configuration;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.DataAccess.Repository;

namespace Concentrator.Plugins.PFA
{
  public class SalesOrderExportAT : ConcentratorPlugin
  {
    private string _atShipmentCostsProduct;
    private string _atReturnCostsProduct;

    private string _atKialaShipmentCostsProduct;
    private string _atKialaReturnCostsProduct;
    private Dictionary<int, int> _returnCodes = new Dictionary<int, int>();
    private List<OrderLine> linestoProcess = new List<OrderLine>();
    private List<OrderLine> cancelledLinesToProcess = new List<OrderLine>();
    private List<OrderLine> returnedLinesToProcess = new List<OrderLine>();
    private List<int> _connectorIDs;
    private readonly Monitoring.Monitoring _monitoring = new Monitoring.Monitoring();


    public SalesOrderExportAT()
    {

    }

    public override string Name
    {
      get { return "Sales order export at"; }
    }

    protected override void Process()
    {
      _monitoring.Notify(Name, 0);
      _atShipmentCostsProduct = PfaAmericaTodayConfiguration.Current.ShipmentCostsProduct;
      _atReturnCostsProduct = PfaAmericaTodayConfiguration.Current.ReturnCostsProduct;

      _atKialaShipmentCostsProduct = PfaAmericaTodayConfiguration.Current.KialaShipmentCostsProduct;
      _atKialaReturnCostsProduct = PfaAmericaTodayConfiguration.Current.KialaReturnCostsProduct;

      var config = GetConfiguration();

      _connectorIDs = GetConfiguration().AppSettings.Settings["atConnectorID"].Value.Split(',').Select(int.Parse).ToList();

      var vendorID = int.Parse(config.AppSettings.Settings["ATVendorID"].Value);
      //var connectorID = int.Parse(config.AppSettings.Settings["atConnectorID"].Value);

      var cjpCode = GetConfiguration().AppSettings.Settings["CJPDiscountCode"].Try(c => c.Value, "CJP");

      NetworkExportUtility util = new NetworkExportUtility();
      using (var unit = GetUnitOfWork())
      {
        try
        {

          _returnCodes = (from c in unit.Scope.Repository<Connector>().GetAll().ToList()
                          where _connectorIDs.Contains(c.ConnectorID)
                          select new
                          {
                            c.ConnectorID,
                            Code = c.ConnectorSettings.GetValueByKey<int?>("ReturnCodeOverride", null)
                          }).Where(c => c.Code != null).ToDictionary(p => p.ConnectorID, p => p.Code.Value);

          var fileLocation = config.AppSettings.Settings["DatColLocationAT"].Value;
          if (string.IsNullOrEmpty(fileLocation))
            throw new Exception("No DatColLocation vendorsetting");

#if DEBUG
          fileLocation = @"D:\Concentrator_TESTING";
#endif

          var userName = config.AppSettings.Settings["DatColLocationUserNameAT"].Value;
          if (string.IsNullOrEmpty(userName))
            throw new Exception("No DatColLocation UserName");

          var password = config.AppSettings.Settings["DatColLocationPasswordAT"].Value;
          if (string.IsNullOrEmpty(password))
            throw new Exception("No DatColLocation Password");

#if !DEBUG
          fileLocation = util.ConnectorNetworkPath(fileLocation, "M:", userName, password);
#endif
          var salesSlip = unit.Scope.Repository<VendorSetting>().GetAll(x => x.SettingKey == "SalesslipNumber" && x.VendorID == vendorID).FirstOrDefault();
          var salesSlipNr = 0;

          if (salesSlip == null)
          {
            salesSlip = new VendorSetting()
            {
              SettingKey = "SalesslipNumber",
              Value = salesSlipNr.ToString(),
              VendorID = vendorID
            };
            unit.Scope.Repository<VendorSetting>().Add(salesSlip);
          }
          else
          {
            salesSlipNr = int.Parse(salesSlip.Value);

            if (salesSlipNr == 9999)
              salesSlipNr = 0;
          }

          var ledgerRepo = unit.Scope.Repository<OrderLedger>();

          var timeStamp = DateTime.Now.ToString("yyMMddhhmm");

          var repositoryOL = unit.Scope.Repository<OrderLine>();

          foreach (var connector in _connectorIDs)
          {
            LoadOrderLines(repositoryOL, connector);
          }

          if (!HasOrdersToProcess())
          {
            util.DisconnectNetworkPath(fileLocation); //disconnect the path
            return; //shortcircuit here if there are no orders to process
          }


          using (var engine = new MultiRecordEngine(typeof(DatColReceiveRegular), typeof(DatColNormalSales), typeof(DatColReturn)))
          {

            var file = Path.Combine(fileLocation, string.Format("datcol.{0}", timeStamp));
            var okFile = Path.Combine(fileLocation, string.Format("datcol.{0}.ok", timeStamp));

            if (!File.Exists(file))
            {
              File.Create(file).Dispose();
            }

            engine.BeginAppendToFile(file);

            #region Orders
            ///contains all order lines
            linestoProcess.GroupBy(c => c.OrderID).ToList().ForEach(orderLinesCollection =>
            {
              string shopOrderNumber = string.Empty;
              decimal totalAmount = 0;
              DateTime timeStampOrder = DateTime.Now;

              foreach (var line in orderLinesCollection)
              {
                var barcode = line.Product.ProductBarcodes.FirstOrDefault(x => x.BarcodeType.HasValue && x.BarcodeType.Value == 0);
                var pfaCode = line.Product.ProductBarcodes.Where(c => c.BarcodeType == (int)BarcodeTypes.PFA).FirstOrDefault();
                var articleSizeColorArray = line.Product.VendorItemNumber.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                var va = line.Product.VendorAssortments.FirstOrDefault(x => x.VendorID == line.DispatchedToVendorID);

                var productPriceEnt = va.Try(c => c.VendorPrices.FirstOrDefault(), null);

                var productPrice = productPriceEnt != null
                  ? va.VendorPrices.FirstOrDefault().Price.Value : (line.Price.HasValue ? Convert.ToDecimal(line.Price.Value) : 0);

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

                if (line.OrderLedgers.Any(c => c.Status == (int)OrderLineStatus.ProcessedKasmut && c.Quantity.Value == line.Quantity))
                  continue;

                int quantityToProcess = 0;
                int quantityProcessed = 0;

                if (line.OrderLedgers.Any(c => c.Status == (int)OrderLineStatus.ProcessedKasmut))
                {
                  var ledg = line.OrderLedgers.FirstOrDefault(c => c.Status == (int)OrderLineStatus.ProcessedKasmut);
                  quantityToProcess = line.Quantity - ledg.Quantity.Value;
                  quantityProcessed = ledg.Quantity.Value;
                }

                var discount = DatcolHelper.GetDiscount(line, quantityToProcess);
                var lineRevenue = DatcolHelper.GetUnformattedRevenue(line, quantityProcessed, quantityToProcess);
                timeStampOrder = line.Order.ReceivedDate.ToLocalTime();
                totalAmount += (decimal)lineRevenue;
                shopOrderNumber = getShopNumberByConnector(line.Order.Connector);

                DatColNormalSales sale = new DatColNormalSales()
                {
                  ShopAndPosNr = string.Format("{0} 01", shopOrderNumber),
                  SalesslipNumber = salesSlipNr,
                  DateStamp = timeStampOrder,
                  Quantity = quantityToProcess == 0 ? line.Quantity : quantityToProcess,
                  ReceivedFrom = (int)Math.Round(line.BasePrice.Value * 100),
                  MarkdownValue = (int)Math.Round((decimal)(line.BasePrice - line.UnitPrice) * 100),
                  Discount = discount,
                  Revenue = DatcolHelper.FormatRevenueForDatcol(lineRevenue),
                  ArticleColorSize = String.Format("{0}{1}{2}", articleCode, colorCode, sizeCode),
                  Barcode = barcode != null ? barcode.Barcode : string.Empty,
                  VatCode = DatcolHelper.GetBTWCode(articleCode).ToString(),
                  RecordType = GetRecordType(line),
                  FixedField1 = GetDiscountCode(line, cjpCode, discount),
                  FixedField2 = (int)Math.Round(GetDiscountFromSet(line) * 100),
                  FixedField3 = GetRecordType(line) == "02" ? "101" : "000"
                };

                line.SetStatus(OrderLineStatus.ProcessedExportNotification, ledgerRepo, useStatusOnNonAssortmentItems: true);
                engine.WriteNext(sale);

              }
              DatcolHelper.SaveDatcolLink(orderLinesCollection.First().Order, shopOrderNumber, timeStampOrder, totalAmount, salesSlipNr, "Sales order");
              DatcolHelper.IncrementSalesSlipNumber(ref salesSlipNr);
            });

            _monitoring.Notify(Name, 10);
            #endregion

            engine.Flush();

            salesSlip.Value = salesSlipNr.ToString();

            unit.Save();

            #region Cancellations
            var cancelledStatus = (int)OrderLineStatus.Cancelled;


            //preliminary : check for cancellation of shipment
            //if at least one line was cancelled from an order
            foreach (var order in cancelledLinesToProcess.Select(c => c.Order).Distinct().ToList())   //unit.Scope.Repository<Order>().GetAll(c => cancelLines.Any(l => l.OrderID == c.OrderID)).ToList())
            {
              //get its order lines without the ones that are non-assortment
              var originalOrderLines = order.OrderLines.Where(c => !c.Product.IsNonAssortmentItem.HasValue || (c.Product.IsNonAssortmentItem.HasValue && !c.Product.IsNonAssortmentItem.Value)).ToList();
              bool cancelShipmentLine = true; //always include by default the shipment cost

              var cancelledLines = order.OrderLines.Where(c => c.OrderLedgers.Any(l => l.Status == cancelledStatus)).ToList();

              if (originalOrderLines.Count() != cancelledLines.Count) //if not all lines have been cancelled -> move on
              {
                continue;
              }

              foreach (var line in originalOrderLines)
              {
                if (!line.OrderLedgers.Any(c => c.Status == cancelledStatus)) continue;
                if (line.OrderLedgers.FirstOrDefault(c => c.Status == cancelledStatus).Quantity != line.Quantity)
                  cancelShipmentLine = false;
              }

              if (cancelShipmentLine)
              {
                var l = order.OrderLines.Where(c => c.Product.VendorItemNumber == _atShipmentCostsProduct || c.Product.VendorItemNumber == _atKialaShipmentCostsProduct).FirstOrDefault();

                if (l != null)
                {
                  l.SetStatus(OrderLineStatus.Cancelled, unit.Scope.Repository<OrderLedger>(), 1, true);
                  cancelledLinesToProcess.Add(l);
                }
              }
            }
            unit.Save();

            cancelledLinesToProcess.GroupBy(c => c.OrderID).ToList().ForEach(orderLinesCollection =>
            {

              decimal totalAmount = 0;
              DateTime timeStampOrder = DateTime.Now;
              string orderShopNumber = string.Empty;

              foreach (var line in orderLinesCollection)
              {
                var barcode = line.Product.ProductBarcodes.FirstOrDefault(x => x.BarcodeType.HasValue && x.BarcodeType.Value == 0);
                var pfaCode = line.Product.ProductBarcodes.Where(c => c.BarcodeType == (int)BarcodeTypes.PFA).FirstOrDefault();
                var articleSizeColorArray = line.Product.VendorItemNumber.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                var va = line.Product.VendorAssortments.FirstOrDefault(x => x.VendorID == line.DispatchedToVendorID);
                var productPrice = (va != null && va.VendorPrices.FirstOrDefault() != null ? va.VendorPrices.FirstOrDefault().Price.Value : (line.Price.HasValue ? Convert.ToDecimal(line.Price.Value) : 0));
                var specialPrice = (va != null && va.VendorPrices.FirstOrDefault() != null && va.VendorPrices.FirstOrDefault().SpecialPrice.HasValue ? va.VendorPrices.FirstOrDefault().SpecialPrice.Value : 0);
                var productPriceEnt = va.Try(c => c.VendorPrices.FirstOrDefault(), null);
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

                var returnLedger = line.OrderLedgers.FirstOrDefault(x => x.Status == cancelledStatus);
                var returnQuantity = (returnLedger != null && returnLedger.Quantity.HasValue) ? returnLedger.Quantity.Value : line.Quantity;
                DatColReturn sale = null;
                var discount = DatcolHelper.GetDiscount(line, returnQuantity);

                timeStampOrder = returnLedger.Try(c => c.LedgerDate, DateTime.Now.ToLocalTime());
                orderShopNumber = getShopNumberByConnector(line.Order.Connector);
                var lineRevenue = DatcolHelper.GetUnformatedNegativeRevenue(line, returnQuantity);


                totalAmount += (decimal)lineRevenue;

                sale = new DatColReturn()
                {
                  ShopAndPosNr = string.Format("{0} 01", orderShopNumber),
                  SalesslipNumber = salesSlipNr,
                  DateStamp = timeStampOrder,
                  Quantity = -returnQuantity,
                  ReceivedFrom = -(int)Math.Round(line.BasePrice.Value * 100),
                  MarkdownValue = -(int)Math.Round((decimal)(line.BasePrice - line.UnitPrice) * 100),
                  Discount = discount,
                  Revenue = DatcolHelper.FormatNegativeRevenueForDatcol(lineRevenue),
                  ArticleColorSize = String.Format("{0}{1}{2}", articleCode, colorCode, sizeCode),
                  Barcode = barcode != null ? barcode.Barcode : string.Empty,
                  VatCode = DatcolHelper.GetBTWCode(articleCode).ToString(),
                  RecordType = GetRecordType(line),
                  FixedField1 = GetDiscountCode(line, cjpCode, discount),
                  FixedField2 = (int)Math.Round(GetDiscountFromSet(line) * 100),
                  FixedField3 = GetRecordType(line) == "02" ? "101" : "000"
                };

                line.SetStatus(OrderLineStatus.ProcessedCancelExportNotification, ledgerRepo, useStatusOnNonAssortmentItems: true);
                engine.WriteNext(sale);
              }
              DatcolHelper.SaveDatcolLink(orderLinesCollection.First().Order, orderShopNumber, timeStampOrder, totalAmount, salesSlipNr, "Cancellation");
              DatcolHelper.IncrementSalesSlipNumber(ref salesSlipNr);
              DatcolHelper.SaveDatcolLink(orderLinesCollection.First().Order, orderShopNumber, timeStampOrder, totalAmount, salesSlipNr, "Cancellation");
            });

            engine.Flush();
            _monitoring.Notify(Name, 20);
            #endregion

            #region Returns
            var returnStatus = (int)OrderLineStatus.ProcessedReturnNotification;

            returnedLinesToProcess.GroupBy(c => c.OrderID).ToList().ForEach(orderLinesCollection =>
            {
              decimal totalAmount = 0;
              DateTime timeStampOrder = DateTime.Now;
              string orderShopNumber = string.Empty;

              foreach (var line in orderLinesCollection)
              {
                var barcode = line.Product.ProductBarcodes.FirstOrDefault(x => x.BarcodeType.HasValue && x.BarcodeType.Value == 0);
                var pfaCode = line.Product.ProductBarcodes.Where(c => c.BarcodeType == (int)BarcodeTypes.PFA).FirstOrDefault();
                var articleSizeColorArray = line.Product.VendorItemNumber.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                var va = line.Product.VendorAssortments.FirstOrDefault(x => x.VendorID == line.DispatchedToVendorID);
                var productPrice = (va != null && va.VendorPrices.FirstOrDefault() != null ? va.VendorPrices.FirstOrDefault().Price.Value : (line.Price.HasValue ? Convert.ToDecimal(line.Price.Value) : 0));
                var specialPrice = (va != null && va.VendorPrices.FirstOrDefault() != null && va.VendorPrices.FirstOrDefault().SpecialPrice.HasValue ? va.VendorPrices.FirstOrDefault().SpecialPrice.Value : 0);
                var productPriceEnt = va.Try(c => c.VendorPrices.FirstOrDefault(), null);
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

                var returnLedger = line.OrderLedgers.FirstOrDefault(x => x.Status == returnStatus);
                var returnQuantity = (returnLedger != null && returnLedger.Quantity.HasValue) ? returnLedger.Quantity.Value : line.Quantity;
                DatColReturn sale = null;

                int returnOrComplaintCode = 4;
                var returnCode = getFixedReturnCode(line.Order.ConnectorID);
                if (returnCode.HasValue)
                {
                  returnOrComplaintCode = returnCode.Value;
                }

                timeStampOrder = returnLedger.Try(c => c.LedgerDate, DateTime.Now.ToLocalTime());
                orderShopNumber = getShopNumberByConnector(line.Order.Connector);

                if (line.Product.VendorItemNumber == _atReturnCostsProduct || line.Product.VendorItemNumber == _atKialaReturnCostsProduct)
                {
                  var lineRevenue = line.Price == 0 ? 0 : (decimal)(line.Price - line.LineDiscount.Try(c => c.Value, 0));
                  var formattedRevenue = (int)Math.Round(lineRevenue * 100);
                  totalAmount += (decimal)lineRevenue;
                  sale = new DatColReturn()
                  {
                    ShopAndPosNr = string.Format("{0} 01", orderShopNumber),
                    SalesslipNumber = salesSlipNr,
                    DateStamp = timeStampOrder,
                    Quantity = returnQuantity,
                    ReceivedFrom = (int)Math.Round(line.BasePrice.Try(c => c.Value, 0) * 100),
                    MarkdownValue = 0,
                    Discount = line.LineDiscount.HasValue ? (int)Math.Round((line.LineDiscount.Value * 100)) : 0,
                    Revenue = formattedRevenue,
                    ArticleColorSize = String.Format("{0}{1}{2}", articleCode, colorCode, sizeCode),
                    Barcode = barcode != null ? barcode.Barcode : string.Empty
                  };
                }
                else
                {
                  var lineRevenue = DatcolHelper.GetUnformatedNegativeRevenue(line, returnQuantity);

                  totalAmount += (decimal)lineRevenue;
                  var discount = DatcolHelper.GetDiscount(line, returnQuantity);

                  sale = new DatColReturn()
                  {
                    ShopAndPosNr = string.Format("{0} 01", orderShopNumber),
                    SalesslipNumber = salesSlipNr,
                    DateStamp = timeStampOrder,
                    Quantity = -returnQuantity,
                    ReceivedFrom = -(int)Math.Round(line.BasePrice.Value * 100),
                    MarkdownValue = -(int)Math.Round((decimal)(line.BasePrice - line.UnitPrice) * 100),
                    Discount = discount,
                    Revenue = DatcolHelper.FormatNegativeRevenueForDatcol(lineRevenue),
                    ArticleColorSize = String.Format("{0}{1}{2}", articleCode, colorCode, sizeCode),
                    Barcode = barcode != null ? barcode.Barcode : string.Empty,
                    VatCode = DatcolHelper.GetBTWCode(articleCode).ToString(),
                    RecordType = GetRecordType(line),
                    FixedField1 = GetDiscountCode(line, cjpCode, discount),
                    FixedField2 = (int)Math.Round(GetDiscountFromSet(line) * 100),
                    FixedField3 = GetRecordType(line) == "02" ? "101" : "000",
                    FixedField6 = returnCode.HasValue ? returnCode.Value.ToString("D2") : "04"
                  };
                }
                line.SetStatus(OrderLineStatus.ProcessedReturnExportNotification, ledgerRepo, useStatusOnNonAssortmentItems: true);
                engine.WriteNext(sale);
              }
              DatcolHelper.SaveDatcolLink(orderLinesCollection.First().Order, orderShopNumber, timeStampOrder, totalAmount, salesSlipNr, "Refund");
              DatcolHelper.IncrementSalesSlipNumber(ref salesSlipNr);
            });
            _monitoring.Notify(Name, 30);
            #endregion

            if (!File.Exists(okFile))
            {
              File.Create(okFile).Dispose();
            }

            engine.Flush();
#if !DEBUG
            util.DisconnectNetworkPath(fileLocation);
#endif


          }
          salesSlip.Value = salesSlipNr.ToString();

          //add time stamp
          var vendor = unit.Scope.Repository<Vendor>().GetSingle(c => c.VendorID == vendorID);
          var datcolTimeStampSetting = vendor.VendorSettings.FirstOrDefault(c => c.SettingKey == "DatcolTimeStamp");

          if (datcolTimeStampSetting == null)
          {
            datcolTimeStampSetting = new VendorSetting()
            {
              VendorID = vendorID,
              SettingKey = "DatcolTimeStamp"
            };

            unit.Scope.Repository<VendorSetting>().Add(datcolTimeStampSetting);
          }
          datcolTimeStampSetting.Value = DateTime.Now.ToString();

          unit.Save();
          _monitoring.Notify(Name, 1);
        }
        catch (Exception e)
        {
          log.Debug("Something went wrong with the generation of the DATCOLS ", e);
          _monitoring.Notify(Name, -1);
        }
      }
    }

    private void LoadOrderLines(IRepository<OrderLine> iRepository, int connectorID)
    {
      linestoProcess.AddRange(GetReadyToExportOrderLines(iRepository, connectorID));
      cancelledLinesToProcess.AddRange(GetCancelledOrderLines(iRepository, connectorID));
      returnedLinesToProcess.AddRange(GetReturnedOrderLines(iRepository, connectorID));
    }

    /// <summary>
    /// Returns whether there are any orders to process
    /// </summary>
    /// <returns></returns>
    private bool HasOrdersToProcess()
    {
      return (linestoProcess.Count + cancelledLinesToProcess.Count + returnedLinesToProcess.Count) > 0;
    }

    /// <summary>
    /// Retrieves all order lines that are ready to be processed
    /// </summary>
    /// <param name="iRepository"></param>
    /// <param name="connectorID"></param>
    /// <returns></returns>
    private List<OrderLine> GetReadyToExportOrderLines(IRepository<OrderLine> iRepository, int connectorID)
    {
      var orderlinesReady = iRepository.GetAll(x => x.Order.ConnectorID == connectorID && x.isDispatched && x.OrderLedgers.Any(y => y.Status == (int)OrderLineStatus.ReadyToOrder) && x.Order.PaymentTermsCode != "Shop").ToList();
      var orderlinesExport = iRepository.GetAll(x => x.Order.ConnectorID == connectorID && x.isDispatched && x.OrderLedgers.Any(y => y.Status == (int)OrderLineStatus.ProcessedExportNotification) && x.Order.PaymentTermsCode != "Shop").ToList();
      return orderlinesReady.Except(orderlinesExport).ToList();
    }

    /// <summary>
    /// Retrieves all cancelled orders that are ready to be processed
    /// </summary>
    /// <param name="iRepository"></param>
    /// <param name="connectorID"></param>
    /// <returns></returns>
    private List<OrderLine> GetCancelledOrderLines(IRepository<OrderLine> iRepository, int connectorID)
    {
      var cancelLinesReady = iRepository.GetAll(x => x.Order.ConnectorID == connectorID && x.isDispatched && x.OrderLedgers.Any(y => y.Status == (int)OrderLineStatus.Cancelled)).ToList();
      var cancelLinesExported = iRepository.GetAll(x => x.Order.ConnectorID == connectorID && x.isDispatched && x.OrderLedgers.Any(y => y.Status == (int)OrderLineStatus.ProcessedCancelExportNotification)).ToList();
      return cancelLinesReady.Except(cancelLinesExported).ToList();
    }

    private List<OrderLine> GetReturnedOrderLines(IRepository<OrderLine> iRepository, int connectorID)
    {
      var returnStatus = (int)OrderLineStatus.ProcessedReturnNotification;

      var returnOrderlinesReady = iRepository.GetAll(x => x.Order.ConnectorID == connectorID && x.isDispatched && x.OrderLedgers.Any(y => y.Status == returnStatus)).ToList();
      var returnOrderlinesExport = iRepository.GetAll(x => x.Order.ConnectorID == connectorID && x.isDispatched && x.OrderLedgers.Any(y => y.Status == (int)OrderLineStatus.ProcessedReturnExportNotification)).ToList();

      return returnOrderlinesReady.Except(returnOrderlinesExport).ToList();

    }

    private string getShopNumberByConnector(Connector c)
    {
      return c.ConnectorSettings.GetValueByKey<string>("DatcolShopNumber", "801");
    }

    private decimal GetDiscountFromSet(OrderLine line)
    {
      decimal amount = 0;

      if (DatcolHelper.IsSetSale(line))
      {
        var rule = line.OrderLineAppliedDiscountRules.FirstOrDefault(c => c.IsSet);
        amount = rule.DiscountAmount;

        if (rule.Percentage)
        {
          //if it is a percentage
          amount = Convert.ToDecimal(line.Price.Value) * (amount / 100);
        }

      }
      return amount;
    }

    private string GetDiscountCode(OrderLine line, string cjpCode, int discount)
    {

      string discountCode = "000";

      if (discount == 0)
        return discountCode;

      if ((line.LineDiscount ?? 0) > 0) discountCode = "005";

      if (ATDatcolHelper.IsCJPSale(line, cjpCode)) discountCode = "008";

      return discountCode;
    }

    private string GetRecordType(OrderLine line)
    {
      return DatcolHelper.IsSetSale(line) ? "02" : "01";
    }

    private int? getFixedReturnCode(int connectorID)
    {
      if (_returnCodes.ContainsKey(connectorID))
        return _returnCodes[connectorID];

      return null;
    }
  }
}
