using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Web;

namespace Concentrator.Objects.Models.Vendors
{
  public class VendorAccruel : BaseModel<VendorAccruel>
  {
    public Int32 VendorAssortmentID { get; set; }

    public String AccruelCode { get; set; }

    public String Description { get; set; }

    public Decimal UnitPrice { get; set; }

    public Int32 MinimumQuantity { get; set; }

    public virtual VendorAssortment VendorAssortment { get; set; }
    
    public override System.Linq.Expressions.Expression<Func<VendorAccruel, bool>> GetFilter()
    {
      return (v => Client.User.VendorIDs.Contains(v.VendorAssortment.VendorID));
    }
  }
}