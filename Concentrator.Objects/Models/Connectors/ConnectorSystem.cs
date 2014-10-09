using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;

namespace Concentrator.Objects.Models.Connectors
{
  public class ConnectorSystem : BaseModel<ConnectorSystem>
  {
    public Int32 ConnectorSystemID { get; set; }
          
    public String Name { get; set; }
          
    public virtual ICollection<Connector> Connectors { get;set;}


    public override System.Linq.Expressions.Expression<Func<ConnectorSystem, bool>> GetFilter()
    {
      return null;
    }
  }
}