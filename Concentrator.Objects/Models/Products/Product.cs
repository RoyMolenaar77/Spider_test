using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Models.Brands;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.Models.Contents;
using Concentrator.Objects.Models.Orders;
using Concentrator.Objects.Models.Attributes;
using Concentrator.Objects.Models.Slurp;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.Models.Faq;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Web;
using Concentrator.Objects.Models.MastergroupMapping;

namespace Concentrator.Objects.Models.Products
{
  public class Product : AuditObjectBase<Product>
  {
    public Int32 ProductID { get; set; }
    public String VendorItemNumber { get; set; }
    public Int32 BrandID { get; set; }
    public Int32 SourceVendorID { get; set; }
    public bool IsConfigurable { get; set; }
    public bool? IsNonAssortmentItem { get; set; }
    public int? ParentProductID { get; set; }
    public bool Visible { get; set; }
    public Boolean IsBlocked { get; set; }

    public virtual Product ParentProduct { get; set; }
    public virtual List<Product> ChildProducts { get; set; }

    public virtual Brand Brand { get; set; }

    public virtual User User { get; set; }
    public virtual User User1 { get; set; }

    public virtual ICollection<ConnectorPublication> ConnectorPublications { get; set; }
    public virtual ICollection<ConnectorPublicationRule> ConnectorPublicationRules { get; set; }
    public virtual ICollection<Contents.Content> Contents { get; set; }
    public virtual ICollection<ContentLedger> ContentLedgers { get; set; }
    public virtual ICollection<ContentPrice> ContentPrices { get; set; }
    public virtual ICollection<ContentProduct> ContentProducts { get; set; }
    public virtual ICollection<ContentProductGroup> ContentProductGroups { get; set; }
    public virtual ICollection<ContentVendorSetting> ContentVendorSettings { get; set; }
    public virtual ICollection<OrderLine> OrderLines { get; set; }
    public virtual ICollection<ProductBarcode> ProductBarcodes { get; set; }
    public virtual ICollection<ProductDescription> ProductDescriptions { get; set; }
    public virtual ICollection<ProductAttributeValue> ProductAttributeValues { get; set; }
    public virtual ICollection<ProductCompetitorPrice> ProductCompetitorPrices { get; set; }
    public virtual ICollection<ProductImage> ProductImages { get; set; }
    public virtual ProductMatch ProductMatch { get; set; }
    public virtual ICollection<ProductMedia> ProductMedias { get; set; }
    public virtual ICollection<ProductReview> ProductReviews { get; set; }
    public virtual ICollection<RelatedProduct> RelatedProductsSource { get; set; }
    public virtual ICollection<RelatedProduct> RelatedProductsRelated { get; set; }
    public virtual ICollection<SlurpQueue> SlurpQueues { get; set; }
    public virtual ICollection<SlurpSchedule> SlurpSchedules { get; set; }
    public virtual ICollection<VendorAssortment> VendorAssortments { get; set; }
    public virtual ICollection<VendorFreeGood> VendorFreeGoods { get; set; }
    public virtual ICollection<VendorProductMatch> VendorProductMatches { get; set; }
    public virtual ICollection<VendorProductMatch> VendorProductMatches1 { get; set; }
    public virtual ICollection<VendorStock> VendorStocks { get; set; }
    public virtual ICollection<FaqProduct> FaqProducts { get; set; }
    public virtual ICollection<MissingContent> MissingContents { get; set; }
    public virtual ICollection<VendorPriceRule> VendorPriceRules { get; set; }
    public virtual ICollection<MasterGroupMappingProduct> MasterGroupMappingProducts { get; set; }
    public virtual ICollection<ProductAttributeMetaData> ProductAttributeMetaDatas { get; set; }
    public virtual ICollection<ContentProductMatch> ContentProductMatches { get; set; }
    public virtual ICollection<ProductPriceSet> ProductPriceSets { get; set; }
    public virtual ICollection<OrderResponseLine> OrderResponseLines { get; set; }

    public override System.Linq.Expressions.Expression<Func<Product, bool>> GetFilter()
    {
      var vendorIDs = Client.User.VendorIDs;

      return product => product.VendorAssortments
        .Select(vendorAssortment => vendorAssortment.VendorID)
        .Intersect(Client.User.VendorIDs)
        .Any();
    }

    public IEnumerable<Product> GetSimpleProducts()
    {
      if (!IsConfigurable)
      {
        throw new InvalidOperationException("This product is not a configurable product.");
      }

      return RelatedProductsSource
        .Where(rp => rp.RelatedProductType.IsConfigured)
        .Select(rp => rp.RProduct);
    }

    public override String ToString()
    {
      return String.Format("Product {0}: '{1}'", ProductID, VendorItemNumber);
    }
  }
}