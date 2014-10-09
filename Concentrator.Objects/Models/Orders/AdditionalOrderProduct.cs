using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.Web;

namespace Concentrator.Objects.Models.Orders
{
  public class AdditionalOrderProduct : AuditObjectBase<AdditionalOrderProduct>
  {
    public Int32 AdditionalOrderProductID { get; set; }

    public Int32 ConnectorID { get; set; }

    public String ConnectorProductID { get; set; }

    public Int32 VendorID { get; set; }

    public String VendorProductID { get; set; }

    public Decimal? UnitPrice { get; set; }

    public virtual Connector Connector { get; set; }

    public virtual Vendor Vendor { get; set; }


    public override System.Linq.Expressions.Expression<Func<AdditionalOrderProduct, bool>> GetFilter()
    {
      return (a => Client.User.VendorIDs.Contains(a.VendorID));
    }
  }
}