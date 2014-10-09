using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.Models.Contents;
using Concentrator.Objects.Models.MastergroupMapping;
using Concentrator.Plugins.ConnectorProductSync.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Concentrator.Plugins.ConnectorProductSync.Services
{
  public class SyncContentProductGroupService : ISyncContentProductGroupService
  {

    private IContentRepository contentRepo;
    private IMasterGroupMappingRepository masterGroupMappingRepo;
    private IContentProductGroupRepository contentProductGroupRepo;
    public SyncContentProductGroupService(
      IContentRepository contentRepo,
      IMasterGroupMappingRepository masterGroupMappingRepo,
      IContentProductGroupRepository contentProductGroupRepo
      )
    {
      this.contentRepo = contentRepo;
      this.masterGroupMappingRepo = masterGroupMappingRepo;
      this.contentProductGroupRepo = contentProductGroupRepo;
    }

    public Dictionary<int, List<Content>> GetListOfContentsPerProductGroupByConnector(Connector connector)
    {
      Dictionary<int, List<Content>> listOfContentsPerProductGroup = new Dictionary<int, List<Content>>();
      List<Content> listOfContents = contentRepo.GetListOfContentsByConnector(connector);
      var listOfProductGroups = masterGroupMappingRepo.GetListOfProductGroupsByConnector(connector.ConnectorID);

      listOfProductGroups.ForEach(productGroup => { 
        List<MasterGroupMappingProduct> listOfMappedProducts = masterGroupMappingRepo.GetListOfMappedProductsByMasterGroupMapping(productGroup.MasterGroupMappingID);
        List<Content> listOfMappedContents = 
          (
            from c in listOfContents
            join p in listOfMappedProducts on c.ProductID equals p.ProductID
            select c
          ).ToList();
        listOfContentsPerProductGroup.Add(productGroup.MasterGroupMappingID, listOfMappedContents);
      });
      return listOfContentsPerProductGroup;
    }

    public List<ContentProductGroup> GetListOfContentProductGroups(Connector connector)
    {
      return contentProductGroupRepo.GetListOfContentProductGroupsByConnector(connector);
    }

    public List<ContentProductGroup> GetListOfContentProductGroupsToInsert(Dictionary<int, List<Content>> listOfContents, List<ContentProductGroup> listOfCurrentContentProductGroups)
    {
      List<ContentProductGroup> listOfContentProductGroupsToInsert = new List<ContentProductGroup>();
      listOfContents.ForEach(productGroup => {
        int productGroupID = productGroup.Key;
        List<Content> listOfContentPerProductGroup = productGroup.Value;

        List<Content> listOfContentPerProductGroupToInsert =
          (
            from c in listOfContentPerProductGroup
            join cpr in listOfCurrentContentProductGroups.Where(x => x.MasterGroupMappingID == productGroupID) on c.ProductID equals cpr.ProductID into notExistContents
            from nec in notExistContents.DefaultIfEmpty()
            where nec == null
            select c
          ).ToList();

        listOfContentPerProductGroupToInsert.ForEach(content =>
        {
          ContentProductGroup contentProductGroup = new ContentProductGroup()
          {
            ConnectorID = content.ConnectorID,
            MasterGroupMappingID = productGroupID,
            ProductID = content.ProductID,
            ProductGroupMappingID = 1546 // todo: remove this
          };
          listOfContentProductGroupsToInsert.Add(contentProductGroup);
        });
      });
      return listOfContentProductGroupsToInsert;
    }

    public List<ContentProductGroup> GetListOfContentProductGroupsToDelete(Dictionary<int, List<Content>> listOfContents, List<ContentProductGroup> listOfCurrentContentProductGroups)
    {
      List<ContentProductGroup> listOfContentProductGroupsToDelete = new List<ContentProductGroup>();
      listOfContents.ForEach(productGroup =>
      {
        int productGroupID = productGroup.Key;
        List<Content> listOfContentPerProductGroup = productGroup.Value;

        List<ContentProductGroup> listOfContentProductGroupsPerProductGroupToDelete =
          (
            from cpr in listOfCurrentContentProductGroups.Where(x => x.MasterGroupMappingID == productGroupID)
            join c in listOfContentPerProductGroup on cpr.ProductID equals c.ProductID into notExistContentproductGroups
            from nec in notExistContentproductGroups.DefaultIfEmpty()
            where nec == null
            select cpr
          ).ToList();
        listOfContentProductGroupsToDelete.AddRange(listOfContentProductGroupsPerProductGroupToDelete);
      });

      return listOfContentProductGroupsToDelete;
    }

    public void InsertContentProductGroup(ContentProductGroup contentProductGroup)
    {
      contentProductGroupRepo.InsertContentProductGroup(contentProductGroup);
    }

    public void DeleteContentProductGroup(ContentProductGroup contentProductGroup)
    {
      contentProductGroupRepo.DeleteContentProductGroup(contentProductGroup);
    }
  }
}