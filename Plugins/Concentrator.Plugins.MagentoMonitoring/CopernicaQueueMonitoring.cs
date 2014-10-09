using Concentrator.Objects.ConcentratorService;
using Concentrator.Objects.Environments;
using Concentrator.Objects.Models.Connectors;
using MySql.Data.MySqlClient;
using PetaPoco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Concentrator.Plugins.MagentoMonitoring
{
  public class CopernicaQueueMonitoring : ConcentratorPlugin
  {
    public override string Name
    {
      get { return "Copernica Queue Monitor"; }
    }


    private readonly Monitoring.Monitoring _monitoring = new Monitoring.Monitoring();
    private List<int> ConnectorIDsToMonitor = new List<int> { 5, 6, 11 };

    protected override void Process()
    {
      _monitoring.Notify(Name, 0);
      using (var db = new Database(Environments.Current.Connection, "System.Data.SqlClient"))
      {
        foreach (var connectorID in ConnectorIDsToMonitor)
        {
          var connector = db.FirstOrDefault<Connector>("select * from Connector where connectorid = @0", connectorID);
          if (connector == null) continue;


          var connection = connector.Connection;
          using (MySqlConnection mgConnection = new MySqlConnection(connection))
          {
            mgConnection.Open();
            using (var command = new MySqlCommand())
            {
              command.Connection = mgConnection;
              command.CommandText = @"select count(*) from copernica_queue";
              command.CommandType = System.Data.CommandType.Text;
              var count = command.ExecuteScalar();
              if (count != null && count != DBNull.Value)
              {
                if (Convert.ToInt32(count) > 7000)
                {
                  _monitoring.Notify(Name, -connectorID);
                }
              }
            }
          }
        }
      }
      _monitoring.Notify(Name, 1);
    }
  }
}
