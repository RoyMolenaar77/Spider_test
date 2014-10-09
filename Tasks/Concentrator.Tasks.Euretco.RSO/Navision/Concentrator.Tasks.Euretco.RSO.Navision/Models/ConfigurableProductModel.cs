using System;

namespace Concentrator.Tasks.Euretco.RSO.Navision.Models
{
  public class ConfigurableProductModel
  {
    public Int32 ProductID { get; set; }
    public String VendorItemNumber { get; set; }
    public String ProductName { get; set; }
    public String ShortDescription { get; set; }
    public String BrandName { get; set; }
    public decimal CostPrice { get; set; }
    public decimal Price { get; set; }
  }
}
