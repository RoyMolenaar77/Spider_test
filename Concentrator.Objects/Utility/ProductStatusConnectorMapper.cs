using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.DataAccess.Repository;

namespace Concentrator.Objects.Utility
{
  public class ProductStatusConnectorMapper
  {
    private List<ConnectorProductStatus> Statuses;
    private int ConnectorID;
    //private Table<ConnectorProductStatus> allStatuses;


    /// <summary>
    /// Init a new instance of mapper
    /// </summary>
    /// <param name="connectorID">The connector id for export</param>
    /// <param name="connectorStatuses">All connectorproductstatus records (context.ConnectorProductStatus)</param>
    public ProductStatusConnectorMapper(int connectorID, IRepository<ConnectorProductStatus> connectorStatuses)
    {
      ConnectorID = connectorID;
      Statuses = connectorStatuses.GetAll(c => c.ConnectorID == connectorID).ToList();
    }

    /// <summary>
    /// Init a new instance of mapper
    /// </summary>
    /// <param name="connectorID">The connector id for export</param>
    /// <param name="connectorStatuses">All connectorproductstatus records (context.ConnectorProductStatus)</param>
    public ProductStatusConnectorMapper(int connectorID, List<ConnectorProductStatus> statuses)
    {
      ConnectorID = connectorID;
      Statuses = statuses;
    }

    /// <summary>
    /// Get the status expected on the connector. Matched on id defined by connector
    /// </summary>
    /// <param name="connectorStatusID"></param>
    /// <returns></returns>
    public string GetForConnector(int connectorStatusID)
    {
      return Statuses.FirstOrDefault(c => c.ConcentratorStatusID == connectorStatusID).Try(x => x.ConnectorStatus, string.Empty);
    }

    public string GetForConnector(int? connectorStatusID)
    {
      return connectorStatusID.HasValue ? GetForConnector(connectorStatusID.Value) : string.Empty;
    }
  }
  
}
