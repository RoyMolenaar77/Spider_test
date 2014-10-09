using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.Models.MastergroupMapping;
using System.Collections.Generic;

namespace Concentrator.Plugins.ConnectorProductSync.Services
{
  public interface IFilterByParentProductGroupService
  {
    List<MasterGroupMapping> GetListOfProductGroupsWithFilterByParent(Connector connector);
    bool IsProductGroupValide(MasterGroupMapping productGroup);
    void FilterProductGroupByParent(List<MasterGroupMapping> listOfProductGroups);
  }
}
