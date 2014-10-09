using Concentrator.Objects.Models.Connectors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Plugins.Axapta.Repositories
{
  public interface IConnectorRepository
  {
    Connector GetConnectorByID(int connectorID);
    IEnumerable<Connector> GetActiveConnectors();
  }
}
