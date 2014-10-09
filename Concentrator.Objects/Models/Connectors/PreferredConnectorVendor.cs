using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.Web;

namespace Concentrator.Objects.Models.Connectors
{
  public class PreferredConnectorVendor : BaseModel<PreferredConnectorVendor>
  {
    public Int32 VendorID { get; set; }

    public Int32 ConnectorID { get; set; }

    public Boolean isPreferred { get; set; }

    public Boolean isContentVisible { get; set; }

    public String VendorIdentifier { get; set; }

    public Boolean CentralDelivery { get; set; }

    public virtual Connector Connector { get; set; }

    public virtual Vendor Vendor { get; set; }


    public override System.Linq.Expressions.Expression<Func<PreferredConnectorVendor, bool>> GetFilter()
    {
      return (pcv => Client.User.VendorIDs.Contains(pcv.VendorID));
    }
  }
}