using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Models.Management;

namespace Concentrator.Objects.Models.Dashboards
{
  public class Portal : BaseModel<Portal>
  {
    public Int32 PortalID { get; set; }

    public String Name { get; set; }

    public virtual ICollection<ManagementGroup> ManagementGroups { get; set; }

    public virtual ICollection<UserPortal> UserPortals { get; set; }

    public override System.Linq.Expressions.Expression<Func<Portal, bool>> GetFilter()
    {
      return null;
    }
  }
}