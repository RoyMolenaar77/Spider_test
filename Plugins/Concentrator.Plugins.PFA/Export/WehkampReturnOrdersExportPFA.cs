using Concentrator.Objects.ConcentratorService;
using Concentrator.Objects.Models.Orders;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Plugins.PFA.Models;
using Concentrator.Plugins.PFA.Objects.Helper;
using Concentrator.Plugins.PFA.Transfer.Model.ReceivedTransfer;
using Concentrator.Plugins.PfaCommunicator.Objects.Services;
using FileHelpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Concentrator.Plugins.PFA.Export
{
  public class WehkampReturnOrdersExportPFA : ConcentratorPlugin
  {
    public override string Name
    {
      get { return "Return orders export to PFA"; }
    }

    protected override void Process()
    {
      using (var unit = GetUnitOfWork())
      {
        var vendors = unit.Scope.Repository<Vendor>().GetAll().ToList().Where(c => ((VendorType)c.VendorType).Has(VendorType.SupportsPFATransferOrders)).ToList();

        var orderLedgerRepo = unit.Scope.Repository<OrderLedger>();

        foreach (var vendor in vendors)
        {
          var rule = vendor.ContentProducts.FirstOrDefault(c => c.IsAssortment);

          rule.ThrowIfNull("Publication rule with IsAssortment is missing for vendor " + vendor.Name);

          IQueryable<OrderLine> lines = unit.Scope.Repository<OrderLine>().GetAll(x => x.Order.OrderType == (int)OrderTypes.ReturnOrder && x.Order.ConnectorID == rule.ConnectorID && x.OrderLedgers.Any(y => y.Status == (int)OrderLineStatus.StockReturnRequestConfirmation));

          IQueryable<OrderLine> processed = unit.Scope.Repository<OrderLine>().GetAll(x => x.Order.OrderType == (int)OrderTypes.ReturnOrder && x.Order.ConnectorID == rule.ConnectorID && x.OrderLedgers.Any(y => y.Status == (int)OrderLineStatus.ProcessedStockReturnRequestConfirmation));

          var ledgerRepo = unit.Scope.Repository<OrderLedger>();

          var storeID = ConnectorHelper.GetStoreNumber(rule.ConnectorID);
          var employeeNumber = VendorHelper.GetEmployeeNumber(vendor.VendorID);
          int differenceShopNumber = VendorHelper.GetReturnDifferenceShopNumber(vendor.VendorID);

          lines = lines.Except(processed);

          var toProcess = lines.GroupBy(line => line.Order).ToList().Select(c => new
          {
            Order = c.Key,
            OrderLines = c.ToList()
          }).ToList();

          foreach (var order in toProcess)
          {
            int recordSequenceDetail = 0;
            int recordSequeceCompleteShipment = 200;

            //get all three different shop numbers
            var salesSlipNumber = GenericSlipNumberHelper.GetSlipNumberForTransfer(vendor.VendorID, ReceiptHelper.STOCK_SALESSLIP_RECEIPT_NUMBER_SETTING_KEY);
            int salesSlipNumber_OtherFilials = 0; //used for any line with warehouse code != 982


            var salesSlipNumberComplete = salesSlipNumber;
            var salesSlipNumberShop = salesSlipNumberComplete;

            var fileEngine = new FileHelperEngine(typeof(ReturnOrderDatcolModel));
            var returns = new List<ReturnOrderDatcolModel>();

            var fileName = String.Format("{0}{1}", "returnOrders", DateTime.Now.ToString("yyyyMMddHHmmss"));

            //process contents
            DateTime messageTime = order.OrderLines.FirstOrDefault().OrderLedgers.FirstOrDefault(c => c.Status == (int)OrderLineStatus.StockReturnRequestConfirmation).LedgerDate.ToLocalTime();
            int totalSkus = 0;

            if (order.OrderLines.Any(c => c.WareHouseCode != differenceShopNumber.ToString()))
            {
              salesSlipNumber_OtherFilials = GenericSlipNumberHelper.GetSlipNumberForTransfer(vendor.VendorID, ReceiptHelper.STOCK_SALESSLIP_RECEIPT_NUMBER_SETTING_KEY);
            }

            foreach (var orderLine in order.OrderLines)
            {
              var detailSalesSlipNumber = salesSlipNumber;

              var ledger = orderLine.OrderLedgers.FirstOrDefault(c => c.Status == (int)OrderLineStatus.StockReturnRequestConfirmation);
              totalSkus += ledger.Quantity.Value;

              recordSequenceDetail += 200;

              if (orderLine.WareHouseCode != differenceShopNumber.ToString())
                returns.Add(GetDetailLevel(storeID, employeeNumber, salesSlipNumber, salesSlipNumber_OtherFilials, ledger.LedgerDate.ToLocalTime(), ledger.Quantity.Value, differenceShopNumber, recordSequenceDetail, orderLine.ProductID.Value, vendor.VendorID));

              else
                returns.Add(GetDetailLevel(storeID, employeeNumber, salesSlipNumber, salesSlipNumber, ledger.LedgerDate.ToLocalTime(), ledger.Quantity.Value, differenceShopNumber, recordSequenceDetail, orderLine.ProductID.Value, vendor.VendorID));
            }

            var codes = order.OrderLines.Select(c => c.WareHouseCode).Where(c => !string.IsNullOrEmpty(c));

            if (codes.Any(c => c != differenceShopNumber.ToString()))
              returns.Add(GetCompleteLevel(storeID, employeeNumber, salesSlipNumberComplete, salesSlipNumber_OtherFilials, messageTime, totalSkus, differenceShopNumber));

            foreach (var orderLineFilial in order.OrderLines.Where(c => !string.IsNullOrEmpty(c.WareHouseCode) && c.WareHouseCode != differenceShopNumber.ToString()).GroupBy(c => c.WareHouseCode).ToList())
            {
              var salesSlipNumber_Difference = GenericSlipNumberHelper.GetSlipNumberForTransfer(vendor.VendorID, ReceiptHelper.STOCK_SALESSLIP_RECEIPT_NUMBER_SETTING_KEY_SURPLUS);
              foreach (var orderLine in orderLineFilial)
              {
                recordSequenceDetail += 200;
                var ledger = orderLine.OrderLedgers.FirstOrDefault(c => c.Status == (int)OrderLineStatus.StockReturnRequestConfirmation);

                returns.Add(GetDetailLevelForStore(int.Parse(orderLine.WareHouseCode), employeeNumber, salesSlipNumberShop, salesSlipNumber_Difference, messageTime, ledger.Quantity.Value, differenceShopNumber, recordSequenceDetail, orderLine.ProductID.Value, vendor.VendorID));
              }
            }

            if (returns.Count > 0)
            {
              var file = fileEngine.WriteString(returns);
              var path = CommunicatorService.GetMessagePath(vendor.VendorID, PfaCommunicator.Objects.Models.MessageTypes.WehkampReturn);
              File.WriteAllText(Path.Combine(path.MessagePath, fileName), file);
            }

            foreach (var line in order.OrderLines) line.SetStatus(OrderLineStatus.ProcessedStockReturnRequestConfirmation, ledgerRepo);
          }
          unit.Save();
        }
      }
    }

    private ReturnOrderDatcolModel GetDetailLevelForStore(int storeNumber, string employeeNumber, int salesSlipNumber, int transferSalesSlipNumber, DateTime messageTime, int ledgerQuantity, int differenceNumber, int recordSequence, int productID, int vendorId)
    {
      var line = new ReturnOrderDatcolModel()
      {
        StoreNumber = string.Format("{0} {1}", differenceNumber.ToString("D3"), "01"),
        EmployeeNumber = employeeNumber,
        ReceiptNumber = salesSlipNumber,
        TransactionType = "20",  //
        DateNotified = messageTime.ToString("yyyyMMddhhmm"),
        RecordType = "01",
        SubType = "00",
        NumberOfDifferences = ledgerQuantity,
        Price = (int)Math.Round(PriceHelper.GetPrice(productID, vendorId) * 100),
        ReceivingStore = storeNumber,
        RecordSequence = recordSequence,
        TransferNumber = string.Format("{0}{1}", differenceNumber.ToString("D3"), transferSalesSlipNumber.ToString("D4")),
        SkuVendorItemNumber = ProductHelper.GetPFAItemNumber(null, null, productID),
        EmployeeNumber2 = employeeNumber,
        Barcode = BarcodeHelper.GetBarcode(productID).PadLeft(20, '0'),
      };
      return line;
    }

    private ReturnOrderDatcolModel GetCompleteLevel(int storeNumber, string employeeNumber, int salesSlipNumber, int transferSalesSlipNumber, DateTime messageTime, int ledgerQuantity, int differenceNumber)
    {
      var line = new ReturnOrderDatcolModel()
      {
        StoreNumber = string.Format("{0} {1}", differenceNumber.ToString("D3"), "01"),
        EmployeeNumber = employeeNumber,
        ReceiptNumber = salesSlipNumber,
        TransactionType = "21",
        DateNotified = messageTime.ToString("yyyyMMddhhmm"),
        RecordType = "99",
        SubType = "02",
        NumberOfDifferences = 0,
        ReceivingStore = storeNumber,
        RecordSequence = ledgerQuantity,
        TransferNumber = string.Format("{0}{1}", storeNumber.ToString("D3"), transferSalesSlipNumber.ToString("D4")),
        EmployeeNumber2 = employeeNumber,
        Barcode = "00000000000000000000",
        SkuVendorItemNumber = "0"
      };
      return line;
    }

    private ReturnOrderDatcolModel GetDetailLevel(int storeNumber, string employeeNumber, int salesSlipNumber, int transferSalesSlipNumber, DateTime messageTime, int ledgerQuantity, int differenceStoreNumber, int recordSequence, int productID, int vendorId)
    {
      var line = new ReturnOrderDatcolModel()
      {
        StoreNumber = string.Format("{0} {1}", storeNumber.ToString("D3"), "01"),
        EmployeeNumber = employeeNumber,
        ReceiptNumber = salesSlipNumber,
        TransactionType = "20",
        DateNotified = messageTime.ToString("yyyyMMddhhmm"),
        RecordType = "01",
        SubType = "00",
        NumberOfDifferences = ledgerQuantity,
        ReceivingStore = differenceStoreNumber,
        FixedField2 = 0,
        RecordSequence = recordSequence,
        Price = (int)Math.Round(PriceHelper.GetPrice(productID, vendorId) * 100),
        SkuVendorItemNumber = ProductHelper.GetPFAItemNumber(null, null, productID),
        Barcode = BarcodeHelper.GetBarcode(productID).PadLeft(20, '0'),
        TransferNumber = string.Format("{0}{1}", storeNumber.ToString("D3"), transferSalesSlipNumber.ToString("D4")),
        EmployeeNumber2 = employeeNumber
      };
      return line;
    }

  }
}
