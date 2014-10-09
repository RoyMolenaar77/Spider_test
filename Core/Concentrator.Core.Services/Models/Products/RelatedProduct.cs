namespace Concentrator.Core.Services.Models.Products
{
  public class RelatedProduct
  {
    public RelatedProductTypes RelationType { get; set; }

    public int Index { get; set; }

    public int RelatedProductID { get; set; }
  }

  public enum RelatedProductTypes
  {
    CrossSell = 1,
    UpSell = 2,
    Accessory = 3
  }
}