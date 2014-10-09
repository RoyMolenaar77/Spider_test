using Concentrator.Objects.Models.MastergroupMapping;
using System.Collections.Generic;

namespace Concentrator.Plugins.ConnectorProductSync.Services
{
  public interface IFlattenHierachyProductGroupService
  {
    List<MasterGroupMapping> GetListOfProductGroupsWithFlattenHierachy(int connectorID);
    List<MasterGroupMapping> GetListOfHighestProductGroupWithFlattenHierachy(List<MasterGroupMapping> listOfFlattenHierachyProductGroups);
    void MoveProductsToHighestFlattenHierachyProductGroup(List<MasterGroupMapping> listOfHighestFlattenHierachyProductGroups);
  }
}
