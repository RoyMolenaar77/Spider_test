using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Models.Statuses;
using Concentrator.Objects.Web;

namespace Concentrator.Objects.Models.Vendors
{
  public class VendorProductStatus : BaseModel<VendorProductStatus>
  {
    public Int32 VendorProductStatusID { get; set; }

    public Int32 VendorID { get; set; }

    public String VendorStatus { get; set; }

    public Int32 ConcentratorStatusID { get; set; }

    public Int32? VendorStatusID { get; set; }

    public virtual AssortmentStatus AssortmentStatus { get; set; }

    public virtual Vendor Vendor { get; set; }

    public override System.Linq.Expressions.Expression<Func<VendorProductStatus, bool>> GetFilter()
    {
      return (v => Client.User.VendorIDs.Contains(v.VendorID));

    }
  }
}