using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.Models.MastergroupMapping;
using System.Collections.Generic;

namespace Concentrator.Plugins.ConnectorProductSync.Services
{
  public interface ISyncProductGroupMappingService
  {
    List<MasterGroupMapping> GetListOfProductGroupMapping(Connector connector);
    void SyncChildConnectorMapping(List<MasterGroupMapping> parentProductGroupMappings, Connector childConnector);
  }
}
