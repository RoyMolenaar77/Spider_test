using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using FileHelpers;
using Concentrator.Plugins.Axapta.Helpers;
using Concentrator.Plugins.Axapta.Models;
using Concentrator.Plugins.Axapta.Repositories;
using Concentrator.Objects.Models.Orders;
using Concentrator.Plugins.Axapta.Enum;
using Concentrator.Objects.Ftp;
using System.Configuration;

namespace Concentrator.Plugins.Axapta.Services
{
  public class OrderService : IOrderService
  {
    private Int32 _shipmentCostProductID;
    private Int32 _returnCostProductID;
    private string _magentoOrderNumber;
    private string WebOrderFileName
    {
      get
      {
        return String.Format("WEBORD_{0:yyyy-MM-dd_Hmmss}_{1}.txt", DateTime.Now, _magentoOrderNumber);
      }
    }
    private string ReturnWebOrderFileName
    {
      get
      {
        return String.Format("WEBRET_{0:yyyy-MM-dd_Hmmss}_{1}.txt", DateTime.Now, _magentoOrderNumber);
      }
    }

    private readonly FtpSetting _ftpSetting = new FtpSetting();
    private const int CancelledStatus = (int)OrderLineStatus.Cancelled;
    private const int ReturnStatus = (int)OrderLineStatus.ProcessedReturnNotification;

    const int DefaultVendorID = 50;
    const string ILNSapphNumber = "8717891000003";

    const string FtpAddressSettingKey = "FtpAddress";
    const string FtpUsernameSettingKey = "FtpUsername";
    const string FtpPasswordSettingKey = "FtpPassword";
    const string PathWebOrder = "Ax Ftp Dir WebOrder";
    const string SettingKeySapphConnectorID = "RelatedConnectorID";
    private const string SettingKeyILNClientNumber = "AxaptaILNNumber";
    private const string SettingKeyReturnCostsProduct = "ReturnCostsProduct";
    private const string SettingKeyShipmentCostsProduct = "ShipmentCostsProduct";

    private readonly ILogger _log;
    private readonly IProductRepository _productRepo;
    private readonly IOrderRepository _orderRepo;
    private readonly IVendorSettingRepository _vendorSettingRepo;
    private readonly IVendorRepository _vendorRepo;

    public OrderService(ILogger log, IOrderRepository orderRepo, IVendorSettingRepository vendorSettingRepo, IProductRepository productRepo, IVendorRepository vendorRepo)
    {
      _log = log;
      _orderRepo = orderRepo;
      _vendorSettingRepo = vendorSettingRepo;
      _productRepo = productRepo;
      _vendorRepo = vendorRepo;
    }

    public void Process()
    {
      var listOfSapphVendors = _vendorRepo.GetVendorAndChildVendorsByID(DefaultVendorID);

      foreach (var vendor in listOfSapphVendors) //todo: REMOVE THIS 
      {
        int connectorID;
        string ILNClientNumber = _vendorSettingRepo.GetVendorSetting(vendor.VendorID, SettingKeyILNClientNumber);
        if (!string.IsNullOrEmpty(ILNClientNumber))
        {
          if (int.TryParse(_vendorSettingRepo.GetVendorSetting(vendor.VendorID, SettingKeySapphConnectorID), out connectorID))
          {
            try
            {
              FillSettings(vendor.VendorID);
            }
            catch (Exception error)
            {
              _log.Info(string.Format("Faild to get settings from database, please check your setting in the vendorsetting table. Error Message: '{2}'", error.Message));
              continue;
            }
          
            ProcessWebOrders(connectorID, ILNClientNumber);
            ProcessCancellationWebOrders(connectorID, ILNClientNumber);
            ProcessReturnedWebOrders(connectorID, ILNClientNumber);

          }
          else
          {
            _log.Info("RelatedConnectorID setting key does not exists!");
          }
        }
        else
        {
          _log.Info(string.Format("ILNClientNumber is missing for vendor '{0}'. Please insert vendor setting '{1}'", vendor, SettingKeyILNClientNumber));
          continue;
        }

      }
    }

    private void FillSettings(int vendorID)
    {
      _ftpSetting.FtpAddress = !string.IsNullOrEmpty(_vendorSettingRepo.GetVendorSetting(vendorID, FtpAddressSettingKey)) ? _vendorSettingRepo.GetVendorSetting(vendorID, FtpAddressSettingKey) : _vendorSettingRepo.GetVendorSetting(DefaultVendorID, FtpAddressSettingKey);
      _ftpSetting.FtpUsername = !string.IsNullOrEmpty(_vendorSettingRepo.GetVendorSetting(vendorID, FtpUsernameSettingKey)) ? _vendorSettingRepo.GetVendorSetting(vendorID, FtpUsernameSettingKey) : _vendorSettingRepo.GetVendorSetting(DefaultVendorID, FtpUsernameSettingKey);
      _ftpSetting.FtpPassword = !string.IsNullOrEmpty(_vendorSettingRepo.GetVendorSetting(vendorID, FtpPasswordSettingKey)) ? _vendorSettingRepo.GetVendorSetting(vendorID, FtpPasswordSettingKey) : _vendorSettingRepo.GetVendorSetting(DefaultVendorID, FtpPasswordSettingKey);
      _ftpSetting.Path = !string.IsNullOrEmpty(_vendorSettingRepo.GetVendorSetting(vendorID, PathWebOrder)) ? _vendorSettingRepo.GetVendorSetting(vendorID, PathWebOrder) : _vendorSettingRepo.GetVendorSetting(DefaultVendorID, PathWebOrder);

      var shipmentVendorItemNumber = _vendorSettingRepo.GetVendorSetting(vendorID, SettingKeyShipmentCostsProduct);
      if (shipmentVendorItemNumber == null) throw new NullReferenceException("ShipmentCosts Product cannot be null");

      try
      {
        _shipmentCostProductID = _productRepo.GetProductByVendorItemNumber(shipmentVendorItemNumber).ProductID;
      }
      catch (Exception)
      {
        throw new NullReferenceException("ShipmentCosts Product does not exist.");
      }

      var returnVendorItemNumber = _vendorSettingRepo.GetVendorSetting(vendorID, SettingKeyReturnCostsProduct);
      if (returnVendorItemNumber == null) throw new NullReferenceException("ReturnCosts Product cannot be null");

      try
      {
        _returnCostProductID = _productRepo.GetProductByVendorItemNumber(returnVendorItemNumber).ProductID;
      }
      catch (Exception)
      {
        throw new NullReferenceException("ReturnCosts Product does not exist.");
      }
    }

    private void ProcessWebOrders(int connectorID, string ilnClientNumber)
    {
      var lines = _orderRepo.GetOrderLinesToExport(connectorID);
      WriteOrders(lines, WebOrderProcessType.WebOrder, OrderLineStatus.ProcessedExportNotification, ilnClientNumber);
    }
    private void ProcessCancellationWebOrders(int connectorID, string ilnClientNumber)
    {
      var cancelLines = _orderRepo.GetOrderLinesToCancel(connectorID);
      foreach (var order in cancelLines.Select(c => c.Order).Distinct().ToList())   //unit.Scope.Repository<Order>().GetAll(c => cancelLines.Any(l => l.OrderID == c.OrderID)).ToList())
      {
        //get its order lines without the ones that are non-assortment
        var originalOrderLines = order.OrderLines.Where(c => !c.Product.IsNonAssortmentItem.HasValue || (!c.Product.IsNonAssortmentItem.Value)).ToList();
        var cancelShipmentLine = true;

        var cancelledLines = order.OrderLines.Where(c => c.OrderLedgers.Any(l => l.Status == CancelledStatus)).ToList();

        if (originalOrderLines.Count() != cancelledLines.Count)
        {
          continue;
        }

        foreach (var line in originalOrderLines)
        {
          var firstOrDefault = line.OrderLedgers.FirstOrDefault(c => c.Status == CancelledStatus);
          if (firstOrDefault != null && firstOrDefault.Quantity != line.Quantity)
            cancelShipmentLine = false;
        }

        if (cancelShipmentLine)
        {
          var l = order.OrderLines.FirstOrDefault(c => c.Product.ProductID == _shipmentCostProductID);

          if (l != null)
          {
            _orderRepo.UpgradeOrderLineStatus(l.OrderLineID, OrderLineStatus.Cancelled, 1, true);
            cancelLines.Add(l);
          }
        }
      }
      WriteOrders(cancelLines, WebOrderProcessType.CancellationWebOrder, OrderLineStatus.ProcessedCancelExportNotification, ilnClientNumber);
    }
    private void ProcessReturnedWebOrders(int connectorID, string ilnClientNumber)
    {
      var lines = _orderRepo.GetOrderLinesToReturn(connectorID);
      WriteOrders(lines, WebOrderProcessType.ReturnedWebOrder, OrderLineStatus.ProcessedReturnExportNotification, ilnClientNumber);
    }

    private void WriteOrders(IEnumerable<OrderLine> orderLines, WebOrderProcessType processType, OrderLineStatus newStatus, string ilnClientNumber)
    {
      using (var engine = new MultiRecordEngine(typeof(DatColEnvelope), typeof(DatColHeader), typeof(DatColDate),
                                           typeof(DatColOrderLine), typeof(DatColCount)))
      {
        orderLines.GroupBy(c => c.OrderID).ToList().ForEach(orderLinesCollection =>
          {
            using (Stream memoryStream = new MemoryStream())
            using (var streamWriter = new StreamWriter(memoryStream))
            {
              var saveOrder = true;
              _magentoOrderNumber = orderLinesCollection.First().Order.WebSiteOrderNumber;

              engine.BeginWriteStream(streamWriter);

              var magentoOrderNumber = orderLinesCollection.First().Order.WebSiteOrderNumber;

              var envelope = new DatColEnvelope
                {
                  ILNClientNumber = ilnClientNumber,
                  ILNSapphNumber = ILNSapphNumber,
                  MagentoOrderNumber = magentoOrderNumber
                };
              engine.WriteNext(envelope);

              var header = new DatColHeader
                {
                  MagentoOrderNumber = magentoOrderNumber,
                  ILNSapphNumber = ILNSapphNumber,
                  ILNClientNumber = ilnClientNumber,
                  ILNClientNumber2 = ilnClientNumber
                };
              engine.WriteNext(header);

              var date = new DatColDate();
              engine.WriteNext(date);

              var orderLineCounter = 0;
              foreach (var line in orderLinesCollection)
              {
                if (line.Product == null)
                {
                  saveOrder = false;
                  continue;
                }

                var barcode = line.Product.ProductBarcodes.Where(x => x.BarcodeType.HasValue && x.BarcodeType.Value == 0).Select(x => x).FirstOrDefault();

                if (barcode == null)
                {
                  saveOrder = false;
                  continue;
                }

                decimal totalUnitPriceToProcess;
                var quantityToProcess = 0;

                switch (processType)
                {
                  case WebOrderProcessType.WebOrder:
                    if (!line.Price.HasValue) throw new ArgumentNullException();

                    var discount = line.LineDiscount.HasValue ? line.LineDiscount.Value : 0;

                    if (line.ProductID == _shipmentCostProductID)
                    {
                      totalUnitPriceToProcess = (decimal)(line.Price);
                    }
                    else
                    {
                      totalUnitPriceToProcess = (decimal)(line.Price.Value - discount);
                    }

                    if (line.OrderLedgers.Any(c => c.Status == (int)OrderLineStatus.ProcessedKasmut))
                    {
                      var ledg = line.OrderLedgers.FirstOrDefault(c => c.Status == (int)OrderLineStatus.ProcessedKasmut);
                      if (ledg != null)
                        if (ledg.Quantity != null)
                          quantityToProcess = line.Quantity - ledg.Quantity.Value;
                    }
                    break;
                  case WebOrderProcessType.CancellationWebOrder:
                    if (line.ProductID == _returnCostProductID)
                    {
                      quantityToProcess = line.Quantity;
                      totalUnitPriceToProcess = (decimal)(line.UnitPrice.HasValue ? line.UnitPrice.Value : 0);
                    }
                    else
                    {
                      var returnLedger = line.OrderLedgers.FirstOrDefault(x => x.Status == CancelledStatus);
                      quantityToProcess =
                        -((returnLedger != null && returnLedger.Quantity.HasValue)
                            ? returnLedger.Quantity.Value
                            : line.Quantity);
                      totalUnitPriceToProcess = (decimal)(Math.Abs(quantityToProcess) * line.UnitPrice);
                    }
                    break;
                  case WebOrderProcessType.ReturnedWebOrder:
                    if (line.ProductID == _returnCostProductID)
                    {
                      quantityToProcess = line.Quantity;
                      totalUnitPriceToProcess = (decimal)(line.BasePrice.HasValue ? line.BasePrice.Value : 0);
                    }
                    else
                    {
                      // if product not ReturnCost 
                      //    Quantity = Returned Quantity (if this not exists)
                      //               Shipped Quantity 
                      //    Price = Paid price per Product (with discount) * Quantity

                      var returnLedger = line.OrderLedgers.FirstOrDefault(x => x.Status == ReturnStatus);
                      quantityToProcess =
                        -((returnLedger != null && returnLedger.Quantity.HasValue)
                            ? returnLedger.Quantity.Value
                            : line.Quantity);
                      totalUnitPriceToProcess = (decimal)(Math.Abs(quantityToProcess) * (line.Price / line.Quantity));
                    }

                    break;
                  default:
                    throw new NotImplementedException();
                }

                var orderLine = new DatColOrderLine
                  {
                    OrderLineNumber = ++orderLineCounter,
                    Barcode = barcode.Barcode,
                    Quantity = quantityToProcess == 0 ? line.Quantity : quantityToProcess,
                    TotalUnitPrice = totalUnitPriceToProcess
                  };
                engine.WriteNext(orderLine);
              }
              var count = new DatColCount
                {
                  TotalOrderLine = orderLinesCollection.Count(),
                  TotalQuantity = 0
                };
              engine.WriteNext(count);

              engine.Flush();

              streamWriter.Flush();

              string webOrderFile;
              switch (processType)
              {
                case WebOrderProcessType.WebOrder:
                  webOrderFile = WebOrderFileName;
                  break;
                case WebOrderProcessType.CancellationWebOrder:
                case WebOrderProcessType.ReturnedWebOrder:
                  webOrderFile = ReturnWebOrderFileName;
                  break;
                default:
                  throw new NotImplementedException();
              }

              if (saveOrder)
              {
                var listOfOrderLines = orderLinesCollection.ToDictionary<OrderLine, int, int?>(line => line.OrderLineID, line => null);

                if (!_orderRepo.UpgradeOrderLinesStatus(listOfOrderLines, newStatus, true))
                {
                  _log.Info(string.Format("The system can not upgrade the status of order line {0} to {1}", listOfOrderLines.Keys, newStatus));
                }
                else
                {
                  var ftpManager = new FtpManager(_ftpSetting.FtpUri, null, false, true);
                  ftpManager.Upload(memoryStream.Reset(), webOrderFile);
                }
              }
              else
              {
                //todo: log corrupt lines
              }
            }
          });
      }
    }
  }
}
