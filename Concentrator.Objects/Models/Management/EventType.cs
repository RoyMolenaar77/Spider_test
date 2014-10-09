using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;

namespace Concentrator.Objects.Models.Management
{
  public class EventType : BaseModel<EventType>
  {
    public Int32 TypeID { get; set; }

    public String Type { get; set; }

    public virtual ICollection<Event> Events { get; set; }

    public override System.Linq.Expressions.Expression<Func<EventType, bool>> GetFilter()
    {
      return null;
    }
  }
}