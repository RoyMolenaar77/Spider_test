using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.Models.Contents;
using Concentrator.Objects.Models.Products;

namespace Concentrator.Plugins.ConnectorProductSync.Services
{
  public interface ISyncContentProductGroupService
  {
    Dictionary<int, List<Content>> GetListOfContentsPerProductGroupByConnector(Connector connector);
    List<ContentProductGroup> GetListOfContentProductGroups(Connector connector);

    List<ContentProductGroup> GetListOfContentProductGroupsToInsert(Dictionary<int, List<Content>> listOfContents, List<ContentProductGroup> listOfCurrentContentProductGroups);
    List<ContentProductGroup> GetListOfContentProductGroupsToDelete(Dictionary<int, List<Content>> listOfContents, List<ContentProductGroup> listOfCurrentContentProductGroups);

    void InsertContentProductGroup(ContentProductGroup contentProductGroup);
    void DeleteContentProductGroup(ContentProductGroup contentProductGroup);
  }
}