using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Objects.Logic
{
  public class VendorAssortmentResult
  {
    public int VendorAssortmentID { get; set; }
    public int ProductID { get; set; }
    public int VendorID { get; set; }
  }

  public class VendorStockResult
  {
    public int ProductID { get; set; }
    public int VendorID { get; set; }
    public int QuantityOnHand { get; set; }
    public DateTime? PromisedDeliveryDate { get; set; }
    public int? QuantityToReceive { get; set; }
    public string VendorName { get; set; }
    public string BackendVendorCode { get; set; }
    public string StockStatus { get; set; }
    public int? ConcentratorStatusID { get; set; }
    public int VendorStockTypeID { get; set; }
    public decimal? UnitCost { get; set; }
  }

  public class AttributeResult
  {
    public int AttributeID { get; set; }
    public string AttributeName { get; set; }
    public string AttributeValue { get; set; }
    public string Sign { get; set; }
    public int OrderIndex { get; set; }
    public bool IsVisible { get; set; }
    public bool IsSearchable { get; set; }
    public int GroupID { get; set; }
    public string GroupName { get; set; }
    public int GroupIndex { get; set; }
    public bool NeedsUpdate { get; set; }
    public string AttributeCode { get; set; }
    public DateTime LastUpdate { get; set; }
    public int? ProductID { get; set; }
    public int VendorID { get; set; }
  }

  public class VendorPriceResult
  {
    public int VendorAssortmentID { get; set; }
    public decimal? Price { get; set; }
    public decimal? CostPrice { get; set; }
    public decimal? TaxRate { get; set; }
    public int MinimumQuantity { get; set; }
    public string CommercialStatus { get; set; }
    public int? ConcentratorStatusID { get; set; }
  }

  public class CustomItemNumberResult
  {
    public int ProductID { get; set; }
    public string CustomItemNumber { get; set; }
    public int VendorID { get; set; }
    public int ConnectorID { get; set; }
    public bool isPreferred { get; set; }
    public bool isContentVisible { get; set; }
  }

  public class AdvancedPricing
  {
    public decimal Price { get; set; }
    public int MinimumQuantity { get; set; }
    public decimal TaxRate { get; set; }
    public int ProductID { get; set; }
  }
}
