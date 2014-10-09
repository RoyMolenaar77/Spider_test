namespace Concentrator.Tasks.Euretco.Rso.ProductContentSync.Models
{
  public class ProductMediaDifferential
  {
    public int ProductID { get; set; }
    public string VendorItemNumber { get; set; }

    public int RsoProductID { get; set; }
    public string RsoVendorItemNumber { get; set; }
  }
}
