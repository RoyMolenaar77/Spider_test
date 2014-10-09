using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.Models.Contents;

namespace Concentrator.Objects.Services.ServiceInterfaces
{
  public interface IConnectorService
  {

    /// <summary>
    /// Updates the Connector Product Status   
    /// </summary>
    /// <param name="productStatusID"></param>
    /// <param name="concentratorStatusIDOld"></param>
    /// <param name="concentratorStatusIDNew"></param>
    /// <param name="connectorStatus"></param>
    void UpdateConnectorProductStatus(int connectorID, int connectorStatusIDOld, int connectorStatusIDNew, int concentratorStatusIDOld, int concentratorStatusIDNew, string connectorStatus);
  }
}
