using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.Models.Contents;

namespace Concentrator.Plugins.ConnectorProductSync.Repositories
{
  public interface IContentProductGroupRepository
  {
    ContentProductGroup GetContentProductGroupByID(int connectorID, int masterGroupMappingID, int productID);

    void InsertContentProductGroup(ContentProductGroup contentProductGroup);
    void DeleteContentProductGroup(ContentProductGroup contentProductGroup);

    List<ContentProductGroup> GetListOfContentProductGroups();
    List<ContentProductGroup> GetListOfContentProductGroupsByConnector(Connector connector);
  }
}
