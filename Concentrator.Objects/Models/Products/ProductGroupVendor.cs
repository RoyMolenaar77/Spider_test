using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.Web;
using Concentrator.Objects.Models.MastergroupMapping;

namespace Concentrator.Objects.Models.Products
{
  public class ProductGroupVendor : BaseModel<ProductGroupVendor>
  {
    public Int32 ProductGroupVendorID { get; set; }

    public Int32 ProductGroupID { get; set; }

    public Int32 VendorID { get; set; }

    public String VendorName { get; set; }

    public String BrandCode { get; set; }

    public String VendorProductGroupCode1 { get; set; }

    public String VendorProductGroupCode2 { get; set; }

    public String VendorProductGroupCode3 { get; set; }

    public String VendorProductGroupCode4 { get; set; }

    public String VendorProductGroupCode5 { get; set; }

    public String VendorProductGroupCode6 { get; set; }

    public String VendorProductGroupCode7 { get; set; }

    public String VendorProductGroupCode8 { get; set; }

    public String VendorProductGroupCode9 { get; set; }

    public String VendorProductGroupCode10 { get; set; }

    public virtual ProductGroup ProductGroup { get; set; }

    public virtual Vendor Vendor { get; set; }

    public Boolean IsBlocked { get; set; }

    public virtual ICollection<MasterGroupMapping> MasterGroupMappings { get; set; }

    public virtual ICollection<VendorAssortment> VendorAssortments { get; set; }

    public override System.Linq.Expressions.Expression<Func<ProductGroupVendor, bool>> GetFilter()
    {
      return (p => Client.User.VendorIDs.Contains(p.VendorID));
    }
  }
}