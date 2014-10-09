using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Models.Connectors;

namespace Concentrator.Objects.Models.Orders
{
  public class Outbound : BaseModel<Outbound>
  {
    public Int32 OutboundID { get; set; }

    public String OutboundMessage { get; set; }

    public Int32 ConnectorID { get; set; }

    public Boolean Processed { get; set; }

    public DateTime CreationTime { get; set; }

    public String Type { get; set; }

    public String OutboundUrl { get; set; }

    public String ResponseRemark { get; set; }

    public Int32? ResponseTime { get; set; }

    public Int32? ProcessedCount { get; set; }

    public String ErrorMessage { get; set; }

    public DateTime? ProcessDate { get; set; }

    public Int32 OrderID { get; set; }

    public virtual Connector Connector { get; set; }

    public virtual Order Order { get; set; }

    public override System.Linq.Expressions.Expression<Func<Outbound, bool>> GetFilter()
    {
      return null;
    }
  }
}