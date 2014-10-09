using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.Models.Contents;

namespace Concentrator.Plugins.ConnectorProductSync.Repositories
{
  public interface IContentRepository
  {
    Content GetContentByID(int productID, int connectorID);

    void InsertContent(Content content);
    void UpdateContent(Content content);
    void DeleteContent(Content content);

    List<Content> GetListOfContents();
    List<Content> GetListOfContentsByConnector(Connector connector);
  }
}
