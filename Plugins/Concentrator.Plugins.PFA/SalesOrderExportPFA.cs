using System;
using System.Collections.Generic;
using System.Linq;
using Concentrator.Objects.ConcentratorService;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.Models.Orders;
using Concentrator.Plugins.PFA.Configuration;
using Concentrator.Vendors.PFA.FileFormats;
using Concentrator.Objects.Models.Vendors;
using System.IO;
using FileHelpers;
using Concentrator.Objects;
using Concentrator.Objects.Enumerations;
using Concentrator.Plugins.PFA.Helpers;
using Concentrator.Objects.DataAccess.Repository;
using System.Globalization;

namespace Concentrator.Plugins.PFA
{
  public class SalesOrderExportPFA : ConcentratorPlugin
  {
    private List<OrderLine> linestoProcess = new List<OrderLine>();
    private List<OrderLine> cancelledLinesToProcess = new List<OrderLine>();
    private List<OrderLine> returnedLinesToProcess = new List<OrderLine>();
    private List<int> _connectorIDs;
    private Dictionary<int, string> _warehouseCodes = new Dictionary<int, string>();
    private Dictionary<int, int> _returnCodes = new Dictionary<int, int>();
    private Dictionary<int, string> _vatCodes = new Dictionary<int, string>();
    private readonly Monitoring.Monitoring _monitoring = new Monitoring.Monitoring();

    public override string Name
    {
      get { return "Sales order export PFA"; }
    }

    protected override void Process()
    {
      _monitoring.Notify(Name, 0);

      var config = GetConfiguration();

      var ccShipmentCostsProduct = PfaCoolcatConfiguration.Current.ShipmentCostsProduct;
      var ccReturnCostsProduct = PfaCoolcatConfiguration.Current.ReturnCostsProduct;
      var ccKialaShipmentCostsProduct = PfaCoolcatConfiguration.Current.KialaShipmentCostsProduct;
      var ccKialaReturnCostsProduct = PfaCoolcatConfiguration.Current.KialaReturnCostsProduct;

      _connectorIDs = GetConfiguration().AppSettings.Settings["ccConnectorID"].Value.Split(',').Select(int.Parse).ToList();
#if DEBUG
      _connectorIDs = new List<int>() { 13 };
#endif
      NetworkExportUtility util = new NetworkExportUtility();
      using (var unit = GetUnitOfWork())
      {
        var fileLocation = config.AppSettings.Settings["DatColLocation"].Value;
        _warehouseCodes = (from c in unit.Scope.Repository<Connector>().GetAll().ToList()
                           where _connectorIDs.Contains(c.ConnectorID)
                           select new
                           {
                             c.ConnectorID,
                             Code = c.ConnectorSettings.GetValueByKey<string>("DatcolShopNumber", "890")
                           }).ToDictionary(c => c.ConnectorID, c => c.Code);

        _returnCodes = (from c in unit.Scope.Repository<Connector>().GetAll().ToList()
                        where _connectorIDs.Contains(c.ConnectorID)
                        select new
                        {
                          c.ConnectorID,
                          Code = c.ConnectorSettings.GetValueByKey<int?>("ReturnCodeOverride", null)
                        }).Where(c => c.Code != null).ToDictionary(p => p.ConnectorID, p => p.Code.Value);


        _vatCodes = (from c in unit.Scope.Repository<Connector>().GetAll().ToList()
                     where _connectorIDs.Contains(c.ConnectorID)
                     select new
                     {
                       c.ConnectorID,
                       Code = c.ConnectorSettings.GetValueByKey<string>("DatcolVatCode", "1")
                     }).ToDictionary(c => c.ConnectorID, c => c.Code);
#if DEBUG
        fileLocation = @"D:\Concentrator_TESTING";
#endif
        try
        {
          if (string.IsNullOrEmpty(fileLocation))
            throw new Exception("No DatColLocation vendorsetting");
          var shopID = config.AppSettings.Settings["shop"].Value;


          var userName = config.AppSettings.Settings["DatColLocationUserName"].Value;
          if (string.IsNullOrEmpty(userName))
            throw new Exception("No DatColLocation UserName");

          var password = config.AppSettings.Settings["DatColLocationPassword"].Value;
          if (string.IsNullOrEmpty(password))
            throw new Exception("No DatColLocation Password");

#if !DEBUG
          fileLocation = util.ConnectorNetworkPath(fileLocation, "Z:", userName, password);
#endif

          var vendorID = int.Parse(config.AppSettings.Settings["ccVendorID"].Value);

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

          using (var engine = new MultiRecordEngine(typeof(DatColReceiveRegular), typeof(DatColNormalSales), typeof(DatColReturn)))
          {
            var file = Path.Combine(fileLocation, "datcol");

            if (!File.Exists(file))
            {
              File.Create(file).Dispose();
            }

            engine.BeginAppendToFile(file);

            #region Orders

            var repositoryOL = unit.Scope.Repository<OrderLine>();

            foreach (var connector in _connectorIDs)
            {
              LoadOrderLines(repositoryOL, connector);
            }

            foreach (var orderLinesCollection in linestoProcess.GroupBy(c => c.OrderID).ToList())
            {
              var firstLine = orderLinesCollection.FirstOrDefault();
              if (orderLinesCollection.Count() == 1 && (firstLine.Product.VendorItemNumber == ccShipmentCostsProduct || firstLine.Product.VendorItemNumber == ccKialaShipmentCostsProduct))
              {
                firstLine.SetStatus(OrderLineStatus.ProcessedExportNotification, ledgerRepo, useStatusOnNonAssortmentItems: true);
                continue;
              }

              decimal totalAmount = 0;
              DateTime timeStamp = DateTime.Now;
              string shopOrderNumber = string.Empty;

              var orderHasKasmut = orderLinesCollection.SelectMany(c => c.OrderLedgers).Any(c => c.Status == (int)OrderLineStatus.ProcessedKasmut);

              foreach (var line in orderLinesCollection)
              {
                var barcode = line.Product.ProductBarcodes.FirstOrDefault(x => x.BarcodeType.HasValue && x.BarcodeType.Value == 0);
                var pfaCode = line.Product.ProductBarcodes.FirstOrDefault(c => c.BarcodeType == (int)BarcodeTypes.PFA);
                var articleSizeColorArray = line.Product.VendorItemNumber.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                var va = line.Product.VendorAssortments.FirstOrDefault(x => x.VendorID == line.DispatchedToVendorID);

                var productPrice = (va != null && va.VendorPrices.FirstOrDefault() != null
                  ? va.VendorPrices.FirstOrDefault().Price.Value : (line.Price.HasValue ? Convert.ToDecimal(line.Price.Value) : 0));
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
                else
                {
                  colorCode = "990";
                }
                timeStamp = line.Order.ReceivedDate.ToLocalTime();
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

                var lineRevenue = DatcolHelper.GetUnformattedRevenue(line, quantityProcessed, quantityToProcess);

                totalAmount += (decimal)lineRevenue;
                shopOrderNumber = getShopNumberByConnector(line.Order.ConnectorID);

                DatColNormalSales sale = new DatColNormalSales()
                {
                  ShopAndPosNr = string.Format("{0} 01", shopOrderNumber),
                  SalesslipNumber = salesSlipNr,
                  DateStamp = line.Order.ReceivedDate.ToLocalTime(),
                  Quantity = quantityToProcess == 0 ? line.Quantity : quantityToProcess,
                  ReceivedFrom = (int)Math.Round(line.BasePrice.Value * 100),
                  MarkdownValue = (int)Math.Round((decimal)(line.BasePrice - line.UnitPrice) * 100),
                  Discount = DatcolHelper.GetDiscount(line, quantityToProcess),
                  Revenue = DatcolHelper.FormatRevenueForDatcol(lineRevenue),
                  ArticleColorSize = String.Format("{0}{1}{2}", articleCode, colorCode, sizeCode),
                  Barcode = barcode != null ? barcode.Barcode : string.Empty,
                  VatCode = getVatCodeByConnector(line.Order.ConnectorID)
                };

                line.SetStatus(OrderLineStatus.ProcessedExportNotification, ledgerRepo, useStatusOnNonAssortmentItems: true);
                engine.WriteNext(sale);
              }

              DatcolHelper.SaveDatcolLink(orderLinesCollection.First().Order, shopOrderNumber, timeStamp, totalAmount, salesSlipNr, orderHasKasmut ? "Sales Order Met Voorpick" : "Sales order");
              DatcolHelper.IncrementSalesSlipNumber(ref salesSlipNr);
            }

            _monitoring.Notify(Name, 10);
            #endregion

            engine.Flush();

            salesSlip.Value = salesSlipNr.ToString();

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
                if (line.OrderLedgers.FirstOrDefault(c => c.Status == cancelledStatus).Quantity != line.Quantity)
                  cancelShipmentLine = false;
              }

              if (cancelShipmentLine)
              {
                var l = order.OrderLines.FirstOrDefault(c => c.Product.VendorItemNumber == ccShipmentCostsProduct || c.Product.VendorItemNumber == ccKialaShipmentCostsProduct);

                if (l != null)
                {
                  l.SetStatus(OrderLineStatus.Cancelled, unit.Scope.Repository<OrderLedger>(), 1, true);
                  cancelledLinesToProcess.Add(l);
                }
              }
            }


            foreach (var orderLinesCollection in cancelledLinesToProcess.GroupBy(c => c.OrderID).ToList())
            {
              decimal totalAmount = 0;
              DateTime timeStamp = DateTime.Now.ToLocalTime();
              string orderShopNumber = string.Empty;


              //If there is only one cancelled line and the line is shipment costs -> don't put it in the datcol; just mark it as processed.
              var firstLine = orderLinesCollection.FirstOrDefault();
              if (orderLinesCollection.Count() == 1 && (firstLine.Product.VendorItemNumber == ccShipmentCostsProduct || firstLine.Product.VendorItemNumber == ccKialaShipmentCostsProduct))
              {
                if (!firstLine.OrderLedgers.Any(c => c.Status == (int)OrderLineStatus.ProcessedExportNotification))
                {
                  firstLine.SetStatus(OrderLineStatus.ProcessedCancelExportNotification, ledgerRepo, useStatusOnNonAssortmentItems: true);
                  continue;
                }
              }

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
                else
                {
                  colorCode = "990";
                }

                var sizeCode = (pfaCode != null ? pfaCode.Barcode.PadLeft(4, '0') : string.Empty);

                var returnLedger = line.OrderLedgers.FirstOrDefault(x => x.Status == cancelledStatus);
                var returnQuantity = (returnLedger != null && returnLedger.Quantity.HasValue) ? returnLedger.Quantity.Value : line.Quantity;
                DatColReturn sale = null;
                timeStamp = returnLedger.Try(c => c.LedgerDate, DateTime.Now.ToLocalTime());
                orderShopNumber = getShopNumberByConnector(line.Order.ConnectorID);

                var lineRevenue = DatcolHelper.GetUnformatedNegativeRevenue(line, returnQuantity);


                totalAmount += (decimal)lineRevenue;
                sale = new DatColReturn()
                  {
                    ShopAndPosNr = string.Format("{0} 02", orderShopNumber),
                    SalesslipNumber = salesSlipNr,
                    DateStamp = timeStamp,
                    Quantity = -returnQuantity,
                    ReceivedFrom = -(int)Math.Round(line.BasePrice.Value * 100),
                    MarkdownValue = -(int)Math.Round((decimal)(line.BasePrice - line.UnitPrice) * 100),
                    Discount = DatcolHelper.GetDiscount(line, returnQuantity),
                    Revenue = DatcolHelper.FormatNegativeRevenueForDatcol(lineRevenue),
                    ArticleColorSize = String.Format("{0}{1}{2}", articleCode, colorCode, sizeCode),
                    FixedField6 = "26",
                    Barcode = barcode != null ? barcode.Barcode : string.Empty,
                    VatCode = getVatCodeByConnector(line.Order.ConnectorID)
                  };

                line.SetStatus(OrderLineStatus.ProcessedCancelExportNotification, ledgerRepo, useStatusOnNonAssortmentItems: true);
                engine.WriteNext(sale);
              }
              DatcolHelper.SaveDatcolLink(orderLinesCollection.First().Order, orderShopNumber, timeStamp, totalAmount, salesSlipNr, "Cancellation");
              DatcolHelper.IncrementSalesSlipNumber(ref salesSlipNr);
            }

            engine.Flush();
            _monitoring.Notify(Name, 20);
            #endregion

            #region Returns
            var returnStatus = (int)OrderLineStatus.ProcessedReturnNotification;

            returnedLinesToProcess.GroupBy(c => c.OrderID).ToList().ForEach(orderLinesCollection =>
              {
                decimal totalAmount = 0;
                DateTime timeStamp = DateTime.Now.ToLocalTime();
                var isComplaint = true;
                string orderShopNumber = string.Empty;

                foreach (var orderLine in orderLinesCollection)
                {
                  if (orderLine.Product.VendorItemNumber == ccReturnCostsProduct ||
                      orderLine.Product.VendorItemNumber == ccKialaReturnCostsProduct)
                  {
                    isComplaint = false;
                    break;
                  }
                }

                //23 = return, 25 = complaint
                var returnOrComplaintCode = isComplaint ? 25 : 23;

                foreach (var line in orderLinesCollection)
                {
                  var barcode = line.Product.ProductBarcodes.FirstOrDefault(x => x.BarcodeType.HasValue && x.BarcodeType.Value == 0);
                  var pfaCode = line.Product.ProductBarcodes.FirstOrDefault(c => c.BarcodeType == (int)BarcodeTypes.PFA);
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
                  else
                  {
                    colorCode = "990";
                  }
                  var sizeCode = (pfaCode != null ? pfaCode.Barcode.PadLeft(4, '0') : string.Empty);

                  var returnLedger = line.OrderLedgers.FirstOrDefault(x => x.Status == returnStatus);
                  var returnQuantity = (returnLedger != null && returnLedger.Quantity.HasValue) ? returnLedger.Quantity.Value : line.Quantity;
                  DatColReturn sale = null;
                  orderShopNumber = getShopNumberByConnector(line.Order.ConnectorID);

                  //override per connector
                  var returnCode = getFixedReturnCode(line.Order.ConnectorID);
                  if (returnCode.HasValue)
                  {
                    returnOrComplaintCode = returnCode.Value;
                  }

                  timeStamp = returnLedger.Try(c => c.LedgerDate, DateTime.Now.ToLocalTime());

                  if (line.Product.VendorItemNumber == ccReturnCostsProduct || line.Product.VendorItemNumber == ccKialaReturnCostsProduct)
                  {
                    var lineRevenue = line.Price == 0 ? 0 : (decimal)(line.Price - line.LineDiscount.Try(c => c.Value, 0));
                    var formattedRevenue = (int)Math.Round(lineRevenue * 100);
                    totalAmount += (decimal)lineRevenue;

                    sale = new DatColReturn()
                    {
                      ShopAndPosNr = string.Format("{0} 02", orderShopNumber),
                      SalesslipNumber = salesSlipNr,
                      DateStamp = timeStamp,
                      Quantity = returnQuantity,
                      ReceivedFrom = (int)Math.Round(line.BasePrice.Value * 100),
                      MarkdownValue = (int)Math.Round((decimal)(line.BasePrice - line.UnitPrice) * 100),
                      Discount = DatcolHelper.GetDiscount(line, returnQuantity),
                      Revenue = formattedRevenue,
                      FixedField6 = returnOrComplaintCode.ToString(),
                      ArticleColorSize = String.Format("{0}{1}{2}", articleCode, colorCode, sizeCode),
                      Barcode = barcode != null ? barcode.Barcode : string.Empty,
                      VatCode = getVatCodeByConnector(line.Order.ConnectorID)
                    };
                  }
                  else
                  {
                    var lineRevenue = DatcolHelper.GetUnformatedNegativeRevenue(line, returnQuantity);

                    totalAmount += (decimal)lineRevenue;

                    sale = new DatColReturn()
                      {
                        ShopAndPosNr = string.Format("{0} 02", orderShopNumber),
                        SalesslipNumber = salesSlipNr,
                        DateStamp = timeStamp,
                        Quantity = -returnQuantity,
                        ReceivedFrom = -(int)Math.Round(line.BasePrice.Value * 100),
                        MarkdownValue = -(int)Math.Round((decimal)(line.BasePrice - line.UnitPrice) * 100),
                        Discount = DatcolHelper.GetDiscount(line, returnQuantity),
                        Revenue = DatcolHelper.FormatNegativeRevenueForDatcol(lineRevenue),
                        FixedField6 = returnOrComplaintCode.ToString(),
                        ArticleColorSize = String.Format("{0}{1}{2}", articleCode, colorCode, sizeCode),
                        Barcode = barcode != null ? barcode.Barcode : string.Empty,
                        VatCode = getVatCodeByConnector(line.Order.ConnectorID)
                      };
                  }
                  line.SetStatus(OrderLineStatus.ProcessedReturnExportNotification, ledgerRepo, useStatusOnNonAssortmentItems: true);
                  engine.WriteNext(sale);
                }
                DatcolHelper.SaveDatcolLink(orderLinesCollection.First().Order, orderShopNumber, timeStamp, totalAmount, salesSlipNr, "Refund");
                DatcolHelper.IncrementSalesSlipNumber(ref salesSlipNr);
              });
            _monitoring.Notify(Name, 30);
            #endregion

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
          log.AuditError("Something went wrong with the generation of the DATCOLS ", e);
          _monitoring.Notify(Name, -1);
        }
      }
    }

    private string getShopNumberByConnector(int connectorID)
    {
      return _warehouseCodes[connectorID];
    }

    private int? getFixedReturnCode(int connectorID)
    {
      if (_returnCodes.ContainsKey(connectorID))
        return _returnCodes[connectorID];

      return null;
    }

    private string getVatCodeByConnector(int connectorID)
    {
      return _vatCodes[connectorID];
    }

    private void LoadOrderLines(IRepository<OrderLine> iRepository, int connectorID)
    {
      linestoProcess.AddRange(GetReadyToExportOrderLines(iRepository, connectorID));
      cancelledLinesToProcess.AddRange(GetCancelledOrderLines(iRepository, connectorID));
      returnedLinesToProcess.AddRange(GetReturnedOrderLines(iRepository, connectorID));
    }

    /// <summary>
    /// Retrieves all order lines that are ready to be processed
    /// </summary>
    /// <param name="iRepository"></param>
    /// <param name="connectorID"></param>
    /// <returns></returns>
    private List<OrderLine> GetReadyToExportOrderLines(IRepository<OrderLine> iRepository, int connectorID)
    {
      var orderlinesReady = iRepository.GetAll(x =>
          x.Order.ConnectorID == connectorID &&
          x.isDispatched &&
          x.OrderLedgers.Any(y => y.Status == (int)OrderLineStatus.ReadyToOrder)
          && x.Order.PaymentTermsCode != "Shop"
          && x.Order.OrderType == (int)OrderTypes.SalesOrder
          ).ToList();

      var orderlinesExport = iRepository.GetAll(x =>
        x.Order.ConnectorID == connectorID &&
        x.isDispatched &&
        x.OrderLedgers.Any(y => y.Status == (int)OrderLineStatus.ProcessedExportNotification) &&
        x.Order.PaymentTermsCode != "Shop"
        && x.Order.OrderType == (int)OrderTypes.SalesOrder

        ).ToList();

      return orderlinesReady.Except(orderlinesExport).ToList();
    }

    /// <summary>175247
    /// Retrieves all cancelled orders that are ready to be processed
    /// </summary>
    /// <param name="iRepository"></param>
    /// <param name="connectorID"></param>
    /// <returns></returns>
    private List<OrderLine> GetCancelledOrderLines(IRepository<OrderLine> iRepository, int connectorID)
    {

      var cancelLinesReady = iRepository.GetAll(x =>
        x.Order.ConnectorID == connectorID &&
        x.isDispatched &&
        x.OrderLedgers.Any(y => y.Status == (int)OrderLineStatus.Cancelled)
        && x.Order.OrderType == (int)OrderTypes.SalesOrder
        ).ToList();

      var cancelLinesExported = iRepository.GetAll(x => x.Order.ConnectorID == connectorID &&
        x.isDispatched &&
        x.OrderLedgers.Any(y => y.Status == (int)OrderLineStatus.ProcessedCancelExportNotification)
        && x.Order.OrderType == (int)OrderTypes.SalesOrder 
        ).ToList();

      return cancelLinesReady.Except(cancelLinesExported).ToList();
    }

    private List<OrderLine> GetReturnedOrderLines(IRepository<OrderLine> iRepository, int connectorID)
    {
      var returnStatus = (int)OrderLineStatus.ProcessedReturnNotification;


      var returnOrderlinesReady = iRepository.GetAll(x =>
        x.Order.ConnectorID == connectorID &&
        x.isDispatched &&
        x.OrderLedgers.Any(y => y.Status == returnStatus)
        && x.Order.OrderType == (int)OrderTypes.SalesOrder

        ).ToList();

      var returnOrderlinesExport = iRepository.GetAll(x =>
        x.Order.ConnectorID == connectorID &&
        x.isDispatched &&
        x.OrderLedgers.Any(y => y.Status == (int)OrderLineStatus.ProcessedReturnExportNotification)
        && x.Order.OrderType == (int)OrderTypes.SalesOrder

        ).ToList();

      return returnOrderlinesReady.Except(returnOrderlinesExport).ToList();
    }
  }
}
