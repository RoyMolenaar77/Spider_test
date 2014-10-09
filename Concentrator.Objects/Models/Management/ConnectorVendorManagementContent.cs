using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Web;

namespace Concentrator.Objects.Models.Management
{
  public class ConnectorVendorManagementContent : BaseModel<ConnectorVendorManagementContent>
  {
    public int ConnectorID { get; set; }

    public int VendorID { get; set; }

    public bool IsDisplayed { get; set; }

    public virtual Connector Connector { get; set; }

    public virtual Vendor Vendor { get; set; }

    public override System.Linq.Expressions.Expression<Func<ConnectorVendorManagementContent, bool>> GetFilter()
    {
      return (c => Client.User.VendorIDs.Contains(c.VendorID));
    }
  }
}
