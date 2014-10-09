using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Models.Statuses;
using Concentrator.Objects.Models.Contents;
using System.Data;
using System.Data.Objects;
using System.Data.Common;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Web;

namespace Concentrator.Objects.Models.Vendors
{
  public class VendorPrice : BaseModel<VendorPrice>
  {
    public int CalculatedPriceRuleID { get; set; }

    public Int32 VendorAssortmentID { get; set; }

    public Decimal? Price { get; set; }

    public Decimal? CostPrice { get; set; }
    
    public Decimal? SpecialPrice { get; set; }

    public Decimal? TaxRate { get; set; }

    public Int32 MinimumQuantity { get; set; }

    public String CommercialStatus { get; set; }

    public Int32? ConcentratorStatusID { get; set; }

    public virtual AssortmentStatus AssortmentStatus { get; set; }

    public virtual VendorAssortment VendorAssortment { get; set; }

    public decimal? BasePrice { get; set; }

    public decimal? BaseCostPrice { get; set; }

    public decimal? VPrice { get; set; }

    public virtual ProductGroup ProductGroup { get; set; }

    public int? VendorPriceRuleID { get; set; }

    public virtual VendorPriceRule VendorPriceRule { get; set; }

    public DateTime LastUpdated { get; set; }

    public override System.Linq.Expressions.Expression<Func<VendorPrice, bool>> GetFilter()
    {
      return (c => Client.User.VendorIDs.Contains(c.VendorAssortment.VendorID));
    }
  }
}