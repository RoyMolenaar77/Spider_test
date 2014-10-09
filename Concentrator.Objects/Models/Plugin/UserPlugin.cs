using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Models.Management;
using Concentrator.Objects.Models.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Objects.Models.Plugin
{
  public class UserPlugin : BaseModel<UserPlugin>
  {
    public Int32 UserID { get; set; }

    public Int32 PluginID { get; set; }

    public Int32 TypeID { get; set; }

    public virtual Plugin Plugin { get; set; }

    public virtual User User { get; set; }

    public virtual EventType EventType { get; set; }

    public DateTime SubscriptionTime { get; set; }

    public override System.Linq.Expressions.Expression<Func<UserPlugin, bool>> GetFilter()
    {
      return null;
    }
  }
}
