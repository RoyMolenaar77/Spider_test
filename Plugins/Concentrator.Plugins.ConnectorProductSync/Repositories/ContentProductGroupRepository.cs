using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.Models.Contents;
using PetaPoco;

namespace Concentrator.Plugins.ConnectorProductSync.Repositories
{
  public class ContentProductGroupRepository : IContentProductGroupRepository
  {
    private IDatabase petaPoco;

    public ContentProductGroupRepository(IDatabase petaPoco)
    {
      this.petaPoco = petaPoco;
    }

    public ContentProductGroup GetContentProductGroupByID(int connectorID, int masterGroupMappingID, int productID)
    {
      ContentProductGroup contentProductGroup = petaPoco.SingleOrDefault<ContentProductGroup>(string.Format(@"
        SELECT *
        FROM ContentProductGroup
        WHERE ConnectorID = {0}
          AND MasterGroupMappingID = {1}
          AND ProductID = {2}
      ", connectorID, masterGroupMappingID, productID));

      return contentProductGroup;
    }

    public void InsertContentProductGroup(ContentProductGroup contentProductGroup)
    {
      var newRecord =
        new ContentProductGroup()
        {
          ConnectorID = contentProductGroup.ConnectorID,
          ProductID = contentProductGroup.ProductID,
          ProductGroupMappingID = contentProductGroup.ProductGroupMappingID,
          MasterGroupMappingID = contentProductGroup.MasterGroupMappingID,
          Exists = true,
          CreationTime = DateTime.Now,
          CreatedBy = Concentrator.Objects.Web.Client.User.UserID
        };
      petaPoco.Insert(newRecord);
    }

    public void DeleteContentProductGroup(ContentProductGroup contentProductGroup)
    {
      petaPoco.Execute(string.Format(@"
        DELETE
        FROM ContentProductGroup
        WHERE ContentProductGroupID = {0}
      ", contentProductGroup.ContentProductGroupID));

    }

    public List<ContentProductGroup> GetListOfContentProductGroups()
    {
      List<ContentProductGroup> contentProductGroups = petaPoco.Fetch<ContentProductGroup>(@"
        SELECT *
        FROM ContentProductGroup
      ");
      return contentProductGroups;
    }

    public List<ContentProductGroup> GetListOfContentProductGroupsByConnector(Connector connector)
    {
      List<ContentProductGroup> contentProductGroups = petaPoco.Fetch<ContentProductGroup>(string.Format(@"
        SELECT *
        FROM ContentProductGroup
        WHERE ConnectorID = {0}
      ",connector.ConnectorID));
      return contentProductGroups;
    }
  }
}
