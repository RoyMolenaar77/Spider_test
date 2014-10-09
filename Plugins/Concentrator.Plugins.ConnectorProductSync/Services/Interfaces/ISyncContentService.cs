using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.Models.Contents;
using Concentrator.Objects.Models.Products;
using Concentrator.Plugins.ConnectorProductSync.Models;

namespace Concentrator.Plugins.ConnectorProductSync.Services
{
  public interface ISyncContentService
  {
    List<Content> GetListOfContentByConnector(Connector connector);
    List<ContentInfo> GetListOfMappedProductsByConnector(Connector connector);

    List<Content> GetListOfContentToDelete(List<Content> listOfCurrentContents, List<ContentInfo> listOfMappedProducts);
    List<Content> GetListOfContentToUpdate(List<Content> listOfCurrentContents, List<ContentInfo> listOfMappedProducts);
    List<Content> GetListOfContentToInsert(List<Content> listOfCurrentContents, List<ContentInfo> listOfMappedProducts, Connector connector);

    void InsertContent(Content content);
    void UpdateContent(Content content);
    void DeleteContent(Content content);
  }
}
