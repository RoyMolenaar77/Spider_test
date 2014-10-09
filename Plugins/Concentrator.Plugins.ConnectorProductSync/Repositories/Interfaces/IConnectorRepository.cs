using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Connectors;

namespace Concentrator.Plugins.ConnectorProductSync.Repositories
{
  public interface IConnectorRepository
  {
    Connector GetConnectorByID(int connectorID);
    List<Connector> GetListOfConnectors();
    List<Connector> GetListOfActiveConnectors();
  }
}
