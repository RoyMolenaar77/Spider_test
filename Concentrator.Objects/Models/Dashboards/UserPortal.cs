using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Models.Users;

namespace Concentrator.Objects.Models.Dashboards
{
  public class UserPortal : BaseModel<UserPortal>
  {
    public Int32 PortalID { get; set; }

    public Int32 UserID { get; set; }

    public Decimal West { get; set; }

    public Decimal East { get; set; }

    public virtual Portal Portal { get; set; }

    public virtual User User { get; set; }

    public virtual ICollection<UserPortalPortlet> UserPortalPortlets { get; set; }

    public override System.Linq.Expressions.Expression<Func<UserPortal, bool>> GetFilter()
    {
      return null;
    }
  }
}