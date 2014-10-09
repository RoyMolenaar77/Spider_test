using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.Services.Base;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects.Services.ServiceInterfaces;

namespace Concentrator.Objects.Services
{
  public class ConnectorService : Service<Connector>, IConnectorService
  {
    public override IQueryable<Connector> Search(string queryTerm)
    {
      if (queryTerm == null) queryTerm = string.Empty;

      return Repository().GetAll(c => c.Name.Contains(queryTerm)).AsQueryable();
    }

    public void UpdateConnectorProductStatus(int connectorID, int connectorStatusIDOld, int connectorStatusIDNew, int concentratorStatusIDOld, int concentratorStatusIDNew, string connectorStatus)
    {
      ((IFunctionScope)Scope).Repository().UpdateConnectorProductStatus(connectorID, connectorStatusIDOld, connectorStatusIDNew, concentratorStatusIDOld, concentratorStatusIDNew, connectorStatus);
    }
  }
}
