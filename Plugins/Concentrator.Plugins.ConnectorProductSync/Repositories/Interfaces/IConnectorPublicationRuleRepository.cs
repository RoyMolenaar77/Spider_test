using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Connectors;

namespace Concentrator.Plugins.ConnectorProductSync.Repositories
{
  public interface IConnectorPublicationRuleRepository
  {
    List<ConnectorPublicationRule> GetListOfAllConnectorPublicationRules();
    List<ConnectorPublicationRule> GetListOfConnectorPublicationRuleByConnector(int connectorID);
  }
}
