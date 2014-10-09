using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.Web;

namespace Concentrator.Objects.Models.Brands
{
  public class BrandVendor : BaseModel<BrandVendor>
  {
    public Int32 BrandID { get; set; }

    public Int32 VendorID { get; set; }

    public String VendorBrandCode { get; set; }

    public String Name { get; set; }    
    
    public virtual Brand Brand { get; set; }

    public virtual Vendor Vendor { get; set; }

    public override System.Linq.Expressions.Expression<Func<BrandVendor, bool>> GetFilter()
    {
      return (b => Client.User.VendorIDs.Contains(b.VendorID));
    }
  }
}