using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PetaPoco;
using Concentrator.Objects.Models.Connectors;

namespace Concentrator.Plugins.ConnectorProductSync.Repositories
{
  public class ConnectorPublicationRuleRepository : IConnectorPublicationRuleRepository
  {
    private IDatabase petaPoco;

    public ConnectorPublicationRuleRepository(IDatabase petaPoco)
    {
      this.petaPoco = petaPoco;
    }

    public List<ConnectorPublicationRule> GetListOfAllConnectorPublicationRules()
    {
      throw new NotImplementedException();
    }

    public List<ConnectorPublicationRule> GetListOfConnectorPublicationRuleByConnector(int connectorID)
    {
      List<ConnectorPublicationRule> connectorRules = petaPoco.Fetch<ConnectorPublicationRule>(string.Format(@"
        SELECT *
        FROM dbo.ConnectorPublicationRule
        WHERE ConnectorID = {0} AND IsActive = 1
      ", connectorID));

      return connectorRules;
    }
  }
}
