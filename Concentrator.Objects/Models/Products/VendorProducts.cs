using System;

namespace Concentrator.Objects.Models.Products
{
  public class VendorProducts
  {
    public Int32 ProductID { get; set; }
    public String VendorItemNumber { get; set; }
    public String ShortDescription { get; set; }
    public Int32 CountVendorItemNumbers { get; set; }
    public Boolean IsBlocked { get; set; }
    public String BrandName { get; set; }
    public Boolean HasProductAnImage { get; set; }
    public Int32 MatchPercentage { get; set; }
    public Int32 CountMatches { get; set; }
    public Boolean IsApproved { get; set; }
    public Boolean IsProductMapped { get; set; }
    public Boolean IsConfigurable { get; set; }
  }
}
