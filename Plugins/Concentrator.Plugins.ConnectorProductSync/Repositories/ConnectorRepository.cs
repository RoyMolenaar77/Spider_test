using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.DataAccess.UnitOfWork;
using PetaPoco;

namespace Concentrator.Plugins.ConnectorProductSync.Repositories
{
  public class ConnectorRepository : IConnectorRepository
  {
    private IDatabase petaPoco;

    public ConnectorRepository(IDatabase petaPoco)
    {
      this.petaPoco = petaPoco;
    }

    public Connector GetConnectorByID(int connectorID)
    {
      Connector connector = petaPoco.SingleOrDefault<Connector>(string.Format(@"
        SELECT *
        FROM Connector
        WHERE ConnectorID = {0}
      ", connectorID));

      return connector;      
    }

    public List<Connector> GetListOfConnectors()
    {
      List<Connector> listOfConnectors = petaPoco.Fetch<Connector>(@"
        SELECT *
        FROM dbo.Connector
      ");

      return listOfConnectors;
    }

    public List<Connector> GetListOfActiveConnectors()
    {
      List<Connector> listOfConnectors = petaPoco.Fetch<Connector>(@"
        SELECT *
        FROM dbo.Connector
        WHERE IsActive = 1
      ");

      return listOfConnectors;
    }
  }
}
