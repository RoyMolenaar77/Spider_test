using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.Web;

namespace Concentrator.Objects.Models.Products
{
  public class ProductGroupContentVendor : BaseModel<ProductGroupContentVendor>
  {
    public Int32 ProductGroupID { get; set; }

    public Int32 VendorID { get; set; }

    public String ContentVendorProductGroupCode { get; set; }

    public Int32 ContentVendorProductGroupID { get; set; }

    public virtual ProductGroup ProductGroup { get; set; }

    public virtual Vendor Vendor { get; set; }

    public override System.Linq.Expressions.Expression<Func<ProductGroupContentVendor, bool>> GetFilter()
    {
      return (p => Client.User.VendorIDs.Contains(p.VendorID));
    }
  }
}