using Concentrator.Core.Services.Models.Products;

namespace Concentrator.Core.Services.Models.Helper
{
  internal class RelatedProductData
  {
    public int Index { get; set; }
    public int ProductID { get; set; }
    public int RelatedProductID { get; set; }
    public int RelatedProductTypeID { get; set; }
    public int ConnectorID { get; set; }
    public RelatedProductTypes RelationType { get; set; }
  }
}
