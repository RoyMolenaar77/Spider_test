using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.Models.Contents;
using Concentrator.Objects.Models.Products;
using Concentrator.Plugins.ConnectorProductSync.Models;
using Concentrator.Plugins.ConnectorProductSync.Repositories;

namespace Concentrator.Plugins.ConnectorProductSync.Services
{
  public class SyncContentService : ISyncContentService
  {

    private IContentRepository contentRepo;
    private IProductRepository productRepo;
    private IConnectorPublicationRuleRepository connectorPublicationRuleRepo;

    public SyncContentService(
      IContentRepository contentRepo,
      IProductRepository productRepo,
      IConnectorPublicationRuleRepository connectorPublicationRuleRepo
      )
    {
      this.contentRepo = contentRepo;
      this.productRepo = productRepo;
      this.connectorPublicationRuleRepo = connectorPublicationRuleRepo;
    }

    public List<Content> GetListOfContentByConnector(Connector connector)
    {
      return contentRepo.GetListOfContentsByConnector(connector);
    }

    public List<ContentInfo> GetListOfMappedProductsByConnector(Connector connector)
    {
      return productRepo.GetListOfMappedProductByConnector(connector);
    }

    public List<Content> GetListOfContentToDelete(List<Content> listOfCurrentContents, List<ContentInfo> listOfMappedProducts)
    {
      List<Content> listOfContentsToDelete =
        (
          from c in listOfCurrentContents
          join p in listOfMappedProducts on c.ProductID equals p.ProductID into notExistProducts
          from nep in notExistProducts.DefaultIfEmpty()
          where nep == null
          select c
        ).ToList();

      return listOfContentsToDelete;
    }

    public List<Content> GetListOfContentToUpdate(List<Content> listOfCurrentContents, List<ContentInfo> listOfMappedProducts)
    {
      List<Content> listOfContentsToUpdate =
        (
          from c in listOfCurrentContents
          join p in listOfMappedProducts on c.ProductID equals p.ProductID
          where c.ShortDescription != p.ShortDescription ||
            c.LongDescription != p.LongDescription ||
            c.LineType != p.LineType ||
            //c.LedgerClass != p.LedgerClass ||
            ((c.LedgerClass != null && c.LedgerClass.Equals(p.LedgerClass)) || (p.LedgerClass != null && p.LedgerClass.Equals(c.LedgerClass))) ||
            c.ProductDesk != p.ProductDesk ||
            c.ExtendedCatalog != p.ExtendedCatalog ||
            c.ConnectorPublicationRuleID != p.ConnectorPublicationRuleID
          select new Content
          {
            ProductID = p.ProductID,
            ConnectorID = p.ConnectorID,
            ShortDescription = p.ShortDescription,
            LongDescription = p.LongDescription,
            LineType = p.LineType,
            LedgerClass = p.LedgerClass,
            ProductDesk = p.ProductDesk,
            ExtendedCatalog = p.ExtendedCatalog,
            CreatedBy = c.CreatedBy,
            CreationTime = c.CreationTime,
            LastModificationTime = DateTime.Now,
            ConnectorPublicationRuleID = p.ConnectorPublicationRuleID
          }
        ).ToList();

      return listOfContentsToUpdate;
    }

    public List<Content> GetListOfContentToInsert(List<Content> listOfCurrentContents, List<ContentInfo> listOfMappedProducts, Connector connector)
    {
      List<Content> listOfContentsToInsert = new List<Content>();

      List<ContentInfo> listOfProductsToInsert =
        (
          from p in listOfMappedProducts
          join c in listOfCurrentContents on p.ProductID equals c.ProductID into productsToInsert
          from nep in productsToInsert.DefaultIfEmpty()
          where nep == null
          select p
        ).ToList();

      listOfProductsToInsert.ForEach(contentInfo =>
      {
        Content content = new Content()
        {
          ProductID = contentInfo.ProductID,
          ConnectorID = contentInfo.ConnectorID,
          ShortDescription = contentInfo.ShortDescription,
          LongDescription = contentInfo.LongDescription,
          ExtendedCatalog = contentInfo.ExtendedCatalog,
          LedgerClass = contentInfo.LedgerClass,
          LineType = contentInfo.LineType,
          CreatedBy = Concentrator.Objects.Web.Client.User.UserID,
          ConnectorPublicationRuleID = contentInfo.ConnectorPublicationRuleID
        };
        listOfContentsToInsert.Add(content);
      });
      return listOfContentsToInsert;
    }

    public void InsertContent(Content content)
    {
      contentRepo.InsertContent(content);
    }

    public void UpdateContent(Content content)
    {
      contentRepo.UpdateContent(content);
    }

    public void DeleteContent(Content content)
    {
      contentRepo.DeleteContent(content);
    }
  }
}
