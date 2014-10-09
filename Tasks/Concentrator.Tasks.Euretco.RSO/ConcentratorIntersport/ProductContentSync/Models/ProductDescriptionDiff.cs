namespace Concentrator.Tasks.Euretco.Rso.ProductContentSync.Models
{
  public class ProductDescriptionDiff
  {
    public int ProductID { get; set; }
    public int RsoProductID { get; set; }

    public string VendorItemNumber { get; set; }

    public string ShortContentDescription { get; set; }
    public string LongContentDescription { get; set; }
    public string ShortSummaryDescription { get; set; }
    public string LongSummaryDescription { get; set; }
    public string ProductName { get; set; }

    public string RsoShortContentDescription { get; set; }
    public string RsoLongContentDescription { get; set; }
    public string RsoShortSummaryDescription { get; set; }
    public string RsoLongSummaryDescription { get; set; }
    public string RsoProductName { get; set; }
  }
}