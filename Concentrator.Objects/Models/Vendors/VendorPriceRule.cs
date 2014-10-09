using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Models.Brands;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Enumerations;
using Concentrator.Objects.Web;

namespace Concentrator.Objects.Models.Vendors
{
  public class VendorPriceRule : AuditObjectBase<VendorPriceRule>
  {
    public int VendorPriceRuleID { get; set; }

    public int VendorID { get; set; }

    public int? ProductID { get; set; }

    public int? BrandID { get; set; }

    public int? ProductGroupID { get; set; }

    public int? VendorPriceCalculationID { get; set; }

    public string Margin { get; set; }

    public decimal? UnitPriceIncrease { get; set; }

    public decimal? CostPriceIncrease { get; set; }

    public int? MinimumQuantity { get; set; }

    public int VendorPriceRuleIndex { get; set; }

    public int PriceRuleType { get; set; }

    public virtual Brand Brand { get; set; }

    public virtual Product Product { get; set; }

    public virtual ProductGroup ProductGroup { get; set; }

    public virtual Vendor Vendor { get; set; }

    public virtual VendorPriceCalculation VendorPriceCalculation { get; set; }

    public virtual ICollection<VendorPrice> VendorPrices { get; set; }

    public override System.Linq.Expressions.Expression<Func<VendorPriceRule, bool>> GetFilter()
    {
      return (v => Client.User.VendorIDs.Contains(v.VendorID));
      
    }
  }

}
