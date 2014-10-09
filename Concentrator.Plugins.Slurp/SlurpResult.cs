using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Plugins.Slurp
{
  [Serializable]
  public class SlurpResult
  {
    public string SiteName { get; set; }
    public string ManufacturerId { get; set; }
    public ProductStatus ProductStatus { get; set; }
    public List<ShopResult> Shops { get; set; }

    public SlurpResult()
    {
      Shops = new List<ShopResult>();
      ProductStatus = ProductStatus.Valid;
    }
    public SlurpResult(string manufacturerId)
      : this()
    {
      ManufacturerId = manufacturerId;
    }
  }

  [Serializable]
  public class ShopResult
  {
    public string Name { get; set; }
    public decimal? Price { get; set; }
    public decimal? TotalPrice { get; set; }
    public DeliveryStatus Delivery { get; set; }
  }

  public enum ProductStatus
  {
    Valid,
    Obsolete
  }
}
