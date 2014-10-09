using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.Models.Contents;
using Concentrator.Plugins.ConnectorProductSync.Helpers;
using PetaPoco;

namespace Concentrator.Plugins.ConnectorProductSync.Repositories
{
  public class ContentRepository : IContentRepository
  {
    private IDatabase petaPoco;
    private IGenerateUpdateProperties generateUpdateProperties;

    public ContentRepository(IDatabase petaPoco, IGenerateUpdateProperties generateUpdateProperties)
    {
      this.petaPoco = petaPoco;
      this.generateUpdateProperties = generateUpdateProperties;
    }

    public Content GetContentByID(int productID, int connectorID)
    {
      Content content = petaPoco.SingleOrDefault<Content>(string.Format(@"
        SELECT *
        FROM content
        WHERE ConnectorID = {1}
	        AND productid = {0}
      ", productID, connectorID));

      return content;
    }
    
    public List<Content> GetListOfContents()
    {
      List<Content> contents = petaPoco.Fetch<Content>(@"
        SELECT *
        FROM Content
      ");
      return contents;
    }

    public List<Content> GetListOfContentsByConnector(Connector connector)
    {
      List<Content> contents = petaPoco.Fetch<Content>(string.Format(@"
        SELECT *
        FROM Content
        WHERE ConnectorID = {0}
      ", connector.ConnectorID));

      return contents;
    }

    public void InsertContent(Content content)
    {
      petaPoco.Insert("Content", "ProductID, ConnectorID", false, new {
        ProductID = content.ProductID,
        ConnectorID = content.ConnectorID,
        ShortDescription = content.ShortDescription,
        LongDescription = content.LongDescription,
        LineType = content.LineType,
        LedgerClass = content.LedgerClass,
        ProductDesk = content.ProductDesk,
        ExtendedCatalog = content.ExtendedCatalog,
        CreatedBy = content.CreatedBy,
        ConnectorPublicationRuleID = content.ConnectorPublicationRuleID      
      });
    }

    public void DeleteContent(Content content)
    {
      petaPoco.Delete("Content", "ProductID, ConnectorID", content);
    }
    
    public void UpdateContent(Content content)
    {
      Content currentContent = GetContentByID(content.ProductID, content.ConnectorID);

      List<string> listOfIngnoreProperties = generateUpdateProperties.GenerateIgnoreProperties(
        new Content(),
        x => x.CreatedBy,
        x => x.LastModifiedBy,
        x => x.LastModificationTime, 
        x => x.CreationTime);

      List<string> listOfChanges = generateUpdateProperties.GetPropertiesForUpdate(content, currentContent, listOfIngnoreProperties);

      if (listOfChanges.Count > 0)
      {
        var updateQuery = string.Join(",", listOfChanges);
        petaPoco.Update<Content>(string.Format(@"
          SET {2}
          WHERE ProductID = {0}
	          AND ConnectorID = {1}
        ", content.ProductID, content.ConnectorID, updateQuery));
      }
    }
  }
}
