using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Models.Brands;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.Web;
using Concentrator.Objects.Models.Attributes;

namespace Concentrator.Objects.Models.Contents
{
  public class ContentPrice : AuditObjectBase<ContentPrice>
  {
    public Int32 ContentPriceRuleID { get; set; }

    public Int32 VendorID { get; set; }

    public Int32 ConnectorID { get; set; }

    public Int32? ProductGroupID { get; set; }

    public Int32? BrandID { get; set; }

    public Int32? ProductID { get; set; }

    public Int32? CompareSourceID { get; set; }

    public String Margin { get; set; }

    public Decimal? UnitPriceIncrease { get; set; }

    public Decimal? CostPriceIncrease { get; set; }

    public Int32? MinimumQuantity { get; set; }

    public Int32 ContentPriceRuleIndex { get; set; }

    public Int32 PriceRuleType { get; set; }

    public Decimal? FixedPrice { get; set; }

    public Int32? ContentPriceCalculationID { get; set; }

    public DateTime? FromDate { get; set; }

    public DateTime? ToDate { get; set; }

    public Decimal? BottomMargin { get; set; }

    public Decimal? SellMargin { get; set; }

    public Int32? ComparePricePosition { get; set; }

    public Int32? MinComparePricePosition { get; set; }

    public Int32? MaxComparePricePosition { get; set; }

    public virtual Brand Brand { get; set; }

    public virtual Connector Connector { get; set; }

    public virtual Products.Product Product { get; set; }

    public virtual ProductGroup ProductGroup { get; set; }

    public virtual Vendor Vendor { get; set; }

    public int? AttributeID { get; set; }

    public string AttributeValue { get; set; }

    public virtual ProductAttributeMetaData ProductAttributeMetaData { get; set; }

    public virtual ContentPriceCalculation ContentPriceCalculation { get; set; }

    public virtual ProductCompareSource ProductCompareSource { get; set; }

    public string ContentPriceLabel { get; set; }

    public override System.Linq.Expressions.Expression<Func<ContentPrice, bool>> GetFilter()
    {
      return (cp => Client.User.VendorIDs.Contains(cp.VendorID));
    }
  }
}