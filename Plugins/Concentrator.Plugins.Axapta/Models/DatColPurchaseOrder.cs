using FileHelpers;
using System;

namespace Concentrator.Plugins.Axapta.Models
{
  [DelimitedRecord(";")]
  [IgnoreEmptyLines] 
  public class DatColPurchaseOrder
  {
    public string VendorCode;
    public string ReceivedDate;
    public string VendorName;
    public string OrderNumber;
    public string SeasonCode;
    public string ModelCode;
    public string ModelDescription;
    public string ModelColor;
    public string ModelColorDescription;
    public string Size;
    public string Subsize;
    public string StockWarehouse;
    public string Quantity;
    public string CustomItemNumber;
    public string Barcode;
    public string PurchaseOrderID;
  }

  [DelimitedRecord(";")]
  [IgnoreEmptyLines]
  public class DatColReceivedPurchaseOrderConfirmation
  {
    public string VendorCode;
    public string ReceivedDate;
    public string VendorName;
    public string OrderNumber;
    public string SeasonCode;
    public string ModelCode;
    public string ModelDescription;
    public string ModelColor;
    public string ModelColorDescription;
    public string Size;
    public string Subsize;
    public string StockWarehouse;
    public string Quantity;
    public string ReceivedQuantity;
    public string CustomItemNumber;
    public string Barcode;
    public string PurchaseOrderID;
  }
}
