using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Models.Statuses;

namespace Concentrator.Objects.Models.Connectors
{
  public class ConnectorProductStatus : BaseModel<ConnectorProductStatus>
  {
    public Int32 ConnectorProductStatusID { get; set; }

    public Int32 ConnectorID { get; set; }

    public String ConnectorStatus { get; set; }

    public Int32 ConcentratorStatusID { get; set; }

    public Int32? ConnectorStatusID { get; set; }    

    public virtual AssortmentStatus AssortmentStatus { get; set; }

    public virtual Connector Connector { get; set; }

    public override System.Linq.Expressions.Expression<Func<ConnectorProductStatus, bool>> GetFilter()
    {
      return null;
    }
  }
}