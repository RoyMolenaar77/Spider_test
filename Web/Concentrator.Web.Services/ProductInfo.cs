using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Complex;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Models.Media;

namespace Concentrator.Web.Services
{
  public class ProductInfo
  {
    public int ProductID { get; set; }
    public int BrandID { get; set; }
    public string Brand { get; set; }
    public string ManufacturerID { get; set; }

    public string BrandVendorCode { get; set; }
    public string ShortDescription { get; set; }
    public string LongDescription { get; set; }
    public bool IsConfigurable { get; set; }
    public List<VendorPriceResult> Prices { get; set; }
    public List<VendorStockResult> RetailStock { get; set; }
    public int? QuantityAvailable { get; set; }

    public string ConnectorProductID { get; set; }
    public string StockStatus { get; set; }
    public DateTime? PromisedDeliveryDate { get; set; }

    public int? QuantityToReceive { get; set; }

    public IEnumerable<ProductBarcodeView> Barcodes { get; set; }
    public IEnumerable<ImageView> Images { get; set; }


    public string LineType { get; set; }
    public string LedgerClass { get; set; }
    public string ProductDesk { get; set; }
    public bool? ExtendedCatalog { get; set; }
    public int VendorID { get; set; }
    public int? DeliveryHours { get; set; }
    public DateTime?  CutOffTime { get; set; }
    public string ProductName { get; set; }



    public override int GetHashCode()
    {
      return ProductID.GetHashCode();
    }
    public override bool Equals(object obj)
    {
      var res = obj as ProductInfo;
      if (res == null || !res.ProductID.Equals(this.ProductID))
        return false;

      return true;
    }


    public bool? IsNonAssortmentItem { get; set; }
  }
}
