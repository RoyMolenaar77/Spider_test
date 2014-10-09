using Concentrator.Objects.Environments;
using Concentrator.Plugins.PFA.Objects.Helper;
using Concentrator.Plugins.PFA.Objects.InterfaceMapping.Interface;
using Concentrator.Plugins.PFA.Objects.Model;
using System;
using System.Collections.Generic;

namespace Concentrator.Plugins.PFA.Objects.InterfaceMapping.Classes
{
  public class MapStockToDatcol : IMapStockToDatcol
  {
    private List<DatColStockModel> _list;
    private Int32 _index;

    public MapStockToDatcol()
    {
      _list = new List<DatColStockModel>();
    }

    public List<DatColStockModel> MapToDatCol(Int32 vendorId, List<Model.WehkampStockMutation> mutations)
    {
      using (var pDb = new PetaPoco.Database(Environments.Current.Connection, "System.Data.SqlClient"))
      {
        int connectorID = pDb.FirstOrDefault<int>(@"select connectorid from contentproduct where vendorid = @0 and isassortment = 1", vendorId);

        int shopNumber = ConnectorHelper.GetStoreNumber(connectorID);
        int differenteShopNumber = VendorHelper.GetDifferenceShopNumber(vendorId);
        string employeeNumber = VendorHelper.GetEmployeeNumber(vendorId);
        var salesSlipNumber = ReceiptHelper.GetSlipNumber(vendorId);

        int _receiptIndex = GenericSlipNumberHelper.GetSlipNumberForTransfer(vendorId, ReceiptHelper.STOCK_SALESSLIP_RECEIPT_NUMBER_SETTING_KEY);
        int _receiptIndexSurplu = GenericSlipNumberHelper.GetSlipNumberForTransfer(vendorId, ReceiptHelper.STOCK_SALESSLIP_RECEIPT_NUMBER_SETTING_KEY_SURPLUS);
        mutations.ForEach(mutation =>
        {
          _index += 200;

          var line = new DatColStockModel
          {
            StoreNumber = (mutation.MutationQuantity > 0 ? differenteShopNumber : shopNumber).ToString("D3") + " 01",
            EmployeeNumber = employeeNumber,
            ReceiptNumber = salesSlipNumber,
            TransactionType = "20",
            DateNotified = mutation.MutationDate.ToString("yyyyMMddHHmm"),
            RecordType = "01",
            SubType = "00",
            NumberOfSkus = Math.Abs(mutation.MutationQuantity),
            MancoOrSurplus = mutation.MutationQuantity > 0 ? shopNumber : differenteShopNumber,
            FixedField1 = "000000000+",
            RecordSequence = _index,
            FixedField2 = "000",
            FixedField3 = "000000000+",
            FixedField4 = "000",
            FixedField5 = "000000000+",
            FixedField6 = "000",
            OriginalSellingPrice = (int)Math.Round(PriceHelper.GetPrice(mutation.ProductID, vendorId) * 100),
            FixedField7 = "00",
            ArticleNumberColorCodeSizeCode = ProductHelper.GetPFAItemNumber(mutation.Articlenumber, mutation.Colorcode, mutation.ProductID),
            Barcode = BarcodeHelper.GetBarcode(mutation.ProductID),
            Receipt = string.Format("{0}{1}{2}", 0, mutation.MutationQuantity > 0 ? differenteShopNumber.ToString("D3") : shopNumber.ToString(), _receiptIndex.ToString().PadLeft(4, '0')),
            TaxCode = "1",
            EmployeeNumber2 = employeeNumber,
            ScannedWithBarcodeReader = 0
          };

          _list.Add(line);
        });
        ReceiptHelper.IncrementSalesSlipNumber(ref salesSlipNumber, vendorId, ReceiptHelper.STOCK_SALESSLIP_NUMBER_SETTINGKEY);        

        return _list;
      }
    }
  }
}
