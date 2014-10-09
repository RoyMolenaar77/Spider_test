using System.Diagnostics;
using FileHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Plugins.Axapta.Models
{
  [DebuggerDisplay("OrderNumber: {OrderNumber}")]
  public class PurchaseOrder
  {
    public string OrderNumber; //Inkooporder
    public string ReceivedDate; //Leveringsdatum
    public string VendorCode; //Leverancier
    public string VendorName; //Leverancier Naam

    public string CustomerOrderReference
    {
      get
      {
        return string.Format("{0} - {1}", VendorCode, VendorName);
      }
    }

    public List<PurchaseOrderLine> OrderLines;
  }

  public class PurchaseOrderLine
  {
    public string CustomItemNumber;
    public string ModelCode;
    public string ModelDescription;
    public string ModelColor;
    public string ModelColorDescription;
    public string Size;
    public string Subsize;
    public string Barcode;
    public string Quantity;
    public string StockWarehouse;

    public string CombineCustomItemNumber
    {
      get
      {
        return string.Format("{0} {1} {2}{3}", ModelCode, ModelColor, Size,
                      string.IsNullOrEmpty(Subsize) ? string.Empty : " " + Subsize);
      }
    }

    public string OriginalLine { get; set; }
  }
}
