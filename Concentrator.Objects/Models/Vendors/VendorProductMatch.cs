using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Web;

namespace Concentrator.Objects.Models.Vendors
{
  public class VendorProductMatch : BaseModel<VendorProductMatch>
  {

    public Int32 VendorID { get; set; }

    public Int32 ProductID { get; set; }

    public Int32 VendorProductID { get; set; }

    public int VendorProductMatchID { get; set; }

    public String VendorItemNumber { get; set; }

    public Int32 MatchPercentage { get; set; }

    public virtual Products.Product Product { get; set; }

    public virtual Products.Product VendorProduct { get; set; }

    public virtual Vendor Vendor { get; set; }


    public override System.Linq.Expressions.Expression<Func<VendorProductMatch, bool>> GetFilter()
    {
      return (v => Client.User.VendorIDs.Contains(v.VendorID));

    }
  }
}