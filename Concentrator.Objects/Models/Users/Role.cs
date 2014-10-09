using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Models.Management;
using Concentrator.Objects.Models;
using Concentrator.Objects.Models.Dashboards;

namespace Concentrator.Objects.Model.Users
{
  public class Role : BaseModel<Role>
  {
    public Int32 RoleID { get; set; }

    public String RoleName { get; set; }

    public Boolean isHidden { get; set; }

    public virtual ICollection<FunctionalityRole> FunctionalityRoles { get; set; }

    public virtual ICollection<ManagementPage> ManagementPages { get; set; }

    public virtual ICollection<UserRole> UserRoles { get; set; }

    public virtual ICollection<Portlet> Portlets { get; set; }


    public override System.Linq.Expressions.Expression<Func<Role, bool>> GetFilter()
    {
      return null;
    }
  }
}