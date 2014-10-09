using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Objects.DataClasses;

namespace Concentrator.Objects.Models.Complex
{

  public class VendorStockResult : ComplexObject
  {
    public int ProductID { get; set; }
    public int VendorID { get; set; }
    public int QuantityOnHand { get; set; }
    public DateTime? PromisedDeliveryDate { get; set; }
    public int? QuantityToReceive { get; set; }
    public string vendorName { get; set; }
    public string VendorStatus { get; set; }
    public string BackendVendorCode { get; set; }
    public string StockStatus { get; set; }
    public int? ConcentratorStatusID { get; set; }
    public int VendorStockTypeID { get; set; }
    public decimal? UnitCost { get; set; }
  }
}
