using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Models.Dashboards;

namespace Concentrator.Objects.Models.Management
{
  public class ManagementGroup : BaseModel<ManagementGroup>
  {
    public int GroupID { get; set; }

    public String Group { get; set; }

    public int? PortalID { get; set; }

    public String DashboardName { get; set; }

    public virtual Portal Portal { get; set; }

    public virtual ICollection<ManagementPage> ManagementPages { get; set; }

    public override System.Linq.Expressions.Expression<Func<ManagementGroup, bool>> GetFilter()
    {
      return null;
    }
  }
}