using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Concentrator.Objects.Web;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Services.ServiceInterfaces;
using Concentrator.Objects.Models.Contents;
using Concentrator.Web.Shared;
using Concentrator.Web.Shared.Controllers;
using System.Data.SqlClient;
using Concentrator.Objects;
using Concentrator.Objects.Models.Connectors;

namespace Concentrator.ui.Management.Controllers
{
  public class ContentProductGroupMappingController : BaseController
  {
    [RequiresAuthentication(Functionalities.GetContentProductGroupMapping)]
    public ActionResult GetList()
    {
      return List(unit => (from c in ((IProductGroupMappingService)unit.Service<ProductGroupMapping>()).
                                      GetContentProductGroupMappings()
                           select new
                           {
                             c.ConnectorID,
                             c.ContentProductGroupID,
                             c.FilterByParentGroup,
                             c.FlattenHierarchy,
                             c.ParentProductGroupMappingID,
                             c.ProductGroupID,
                             c.ProductGroupMappingID,
                             c.ProductGroupName,
                             c.ProductID,
                             c.ProductName,
                             c.Score
                           }));
    }

    [RequiresAuthentication(Functionalities.UpdateContentProductGroupMapping)]
    public ActionResult Copy(int sourceConnectorID, int destinationConnectorID)
    {
      try
      {
        using (var unit = GetUnitOfWork())
        {
          ((IProductGroupMappingService)unit.Service<ProductGroupMapping>()).CopyProductGroupMapping(sourceConnectorID, destinationConnectorID);
        }
        return Success("Product group mappings have been successfully copied");
      }
      catch (SqlException e)
      {
        return HandleSqlException(e);
      }
      catch (Exception e)
      {
        return Failure("Something went wrong: " + e.Message);
      }
    }

    [RequiresAuthentication(Functionalities.GetContentProductGroupMapping)]
    public ActionResult GetByProductGroupMapping(int ProductGroupMappingID, bool? IsConfigurable)
    {
      return List(unit => (from c in unit.Service<ContentProductGroup>().GetAll(c => c.ProductGroupMappingID == ProductGroupMappingID)
                           let desc = c.Product.ProductDescriptions.FirstOrDefault(x => x.LanguageID == Client.User.LanguageID)
                           where (IsConfigurable.HasValue ? c.Product.IsConfigurable == IsConfigurable.Value : true) && c.ConnectorID == Client.User.ConnectorID
                           select new
                           {
                             c.ProductID,
                             ProductName = desc.ProductName,
                             c.ConnectorID,
                             Connector = c.Connector.Name,
                             ShortDescription = desc.ShortContentDescription,
                             LongDescription = desc.LongContentDescription,
                             VendorItemNumber = c.Product.VendorItemNumber,
                             IsConfigurable = c.Product.IsConfigurable,
                             c.IsExported
                           }).AsQueryable());
    }

    [RequiresAuthentication(Functionalities.GetContentProductGroupMapping)]
    public ActionResult Update(int _ProductID, int _ConnectorID, int ProductGroupMappingID)
    {
      try
      {

        using (var unit = GetUnitOfWork())
        {
          var connectorID = Client.User.ConnectorID;
          var connectors = unit.Service<Connector>().GetAll(c => c.ConnectorID == connectorID || c.ParentConnectorID == connectorID).Select(c => c.ConnectorID).ToList();

          foreach (var id in connectors)
          {
            var conteProductGroup = unit.Service<ContentProductGroup>().Get(c => c.ProductGroupMappingID == ProductGroupMappingID && c.ConnectorID == id && c.ProductID == _ProductID);

            if (conteProductGroup != null)
            {
              TryUpdateModel<ContentProductGroup>(conteProductGroup, new string[] { "IsExported" });
            }
          }
          unit.Save();
        }
        return Success("Successfully updated item");
      }
      catch
      {
        return Failure("Something went wrong while updating");
      }
    }

    [RequiresAuthentication(Functionalities.UpdateContentProductGroupMapping)]
    public ActionResult AddByProductGroupMapping(int ProductGroupMappingID, int ProductID, int ConnectorID)
    {
      using (var unit = GetUnitOfWork())
      {
        List<ContentProductGroup> groups = new List<ContentProductGroup>();

        try
        {
          var product = unit.Service<Product>().Get(c => c.ProductID == ProductID);
          if (product == null) return Failure("No product found");

          ConstructContentProductGroupHierarchy(groups, product, ProductGroupMappingID);

          foreach (var connector in unit.Service<Connector>().GetAll(c => c.ConnectorID == ConnectorID || (c.ParentConnectorID.HasValue && c.ParentConnectorID.Value == ConnectorID)).ToList())
          {
            foreach (var cpg in groups)
            {
              var existing = unit.Service<ContentProductGroup>().Get(c => c.ProductID == cpg.ProductID && c.ProductGroupMappingID == ProductGroupMappingID && c.ConnectorID == connector.ConnectorID);

              //Check if also in content table because of foreign key constraint for this connector
              var productContent = unit.Service<Content>().Get(c => c.ProductID == cpg.ProductID && c.ConnectorID == connector.ConnectorID);

              if (existing == null && productContent != null)
              {
                unit.Service<ContentProductGroup>().Create(new ContentProductGroup()
                {
                  ProductID = cpg.ProductID,
                  ConnectorID = connector.ConnectorID,
                  IsCustom = true,
                  ProductGroupMappingID = ProductGroupMappingID
                });
              }
            }
          }

          var existingProduct = unit.Service<ContentProductGroup>().Get(c => c.ProductID == product.ProductID && c.ProductGroupMappingID == ProductGroupMappingID && c.ConnectorID == ConnectorID);

          if (existingProduct != null)
            return Failure(string.Format("Product {0} was already added to the category.", product.VendorItemNumber));

          unit.Save();
          return Success("Added product(s) to category");
        }
        catch (Exception e)
        {
          return Failure("Something went wrong", e);
        }
      }
    }

    private void ConstructContentProductGroupHierarchy(List<ContentProductGroup> contentProductGroupList, Product p, int productGroupMappingID)
    {
      //add selected product to the set
      contentProductGroupList.Add(new ContentProductGroup()
            {
              ProductID = p.ProductID,
              IsCustom = true,
              ProductGroupMappingID = productGroupMappingID
            });

      //if product is toplevel product, add all the colorlevels including childs to the set
      if (p.ParentProductID == null)
      {
        var colorLevel = p.ChildProducts;

        contentProductGroupList.AddRange(from colorLevelProduct in colorLevel
                                         select new ContentProductGroup()
                                           {
                                             ProductID = colorLevelProduct.ProductID,
                                             IsCustom = true,
                                             ProductGroupMappingID = productGroupMappingID
                                           });

        contentProductGroupList.AddRange(from colorLevelProduct in colorLevel
                                         from simpleProduct in colorLevelProduct.ChildProducts
                                         select new ContentProductGroup()
                                           {
                                             ProductID = simpleProduct.ProductID,
                                             IsCustom = true,
                                             ProductGroupMappingID = productGroupMappingID
                                           });
      }
      else
      {
        //get all child related products and push them to the set
        var children = p.RelatedProductsSource.Where(c => c.IsConfigured).Select(c => c.RelatedProductID).Distinct().ToList();
        
        contentProductGroupList.AddRange(from rp in children
                                         select new ContentProductGroup()
                                           {
                                             ProductID = rp,
                                             IsCustom = true,
                                             ProductGroupMappingID = productGroupMappingID
                                           });
      }
    }
  }
}
