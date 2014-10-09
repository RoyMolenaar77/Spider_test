using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Models.Users;

namespace Concentrator.Objects.Models.Management
{
  public class Event : AuditObjectBase<Event>
  {
    public Int32 EventID { get; set; }

    public Int32 TypeID { get; set; }

    public String Message { get; set; }

    public String ProcessName { get; set; }

    public String ExceptionMessage { get; set; }

    public String StackTrace { get; set; }

    public bool Notified { get; set; }

    public virtual Concentrator.Objects.Models.Plugin.Plugin Plugin { get; set; }

    public String ExceptionLocation { get; set; }

    public int? PluginID { get; set; }

    public virtual EventType EventType { get; set; }

    public virtual User User { get; set; }

    public virtual User User1 { get; set; }

    public override System.Linq.Expressions.Expression<Func<Event, bool>> GetFilter()
    {
      return null;
    }
  }
}