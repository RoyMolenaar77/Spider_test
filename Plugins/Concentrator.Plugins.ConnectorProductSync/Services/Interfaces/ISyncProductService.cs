using Concentrator.Objects.Models.MastergroupMapping;
using System.Collections.Generic;

namespace Concentrator.Plugins.ConnectorProductSync.Services
{
  public interface ISyncProductService
  {
    void SyncProductGroup(Dictionary<int, List<MasterGroupMappingProduct>> productsToSync);
  }
}
