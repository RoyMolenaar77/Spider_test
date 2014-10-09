using Concentrator.Objects.ConcentratorService;
using Concentrator.Objects.Models.Connectors;
using System.Collections.Generic;
using System.Linq;

namespace Concentrator.Plugins.Axapta.Repositories
{
  public class ConnectorRepository : UnitOfWorkPlugin, IConnectorRepository
  {
    public Connector GetConnectorByID(int connectorID)
    {
      using (var db = new PetaPoco.Database(Connection, "System.Data.SqlClient"))
      {
        var connector = db.SingleOrDefault<Connector>(string.Format(@"
          SELECT *
          FROM Connector
          WHERE ConnectorID = {0}
        ", connectorID));

        return connector;  
      }
    }

    public IEnumerable<Connector> GetActiveConnectors()
    {
      using (var db = GetUnitOfWork())
      {
        IEnumerable<Connector> connectors = db
          .Scope
          .Repository<Connector>()
          .GetAll(x=>x.IsActive)
          .ToList();

        return connectors;
      }
    }
  }
}
