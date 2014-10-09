using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Concentrator.Objects.Web;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Models.Contents;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Services.ServiceInterfaces;
using Concentrator.Web.Shared; using Concentrator.Web.Shared.Controllers;
using Concentrator.Objects.Models.Attributes;

namespace Concentrator.ui.Management.Controllers
{
  public class ProductGroupAttributeMappingController : BaseController
  {
    [RequiresAuthentication(Functionalities.GetProductGroupAttributeMapping)]
    public ActionResult GetList(int productGroupMappingID)
    {
      return List(unit => (from cc in unit.Service<ContentProductGroup>().GetAll(c => c.ProductGroupMappingID == productGroupMappingID).SelectMany(c => c.Product.ProductAttributeValues.Select(l => l.ProductAttributeMetaData))
                           group cc by cc.AttributeID into g
                           let a = g.FirstOrDefault()
                           select new
                           {
                             a.AttributeID,
                             AttributeGroupID = a.ProductAttributeGroupID,
                             Attribute = a.ProductAttributeNames.FirstOrDefault(c => c.LanguageID == Client.User.LanguageID).Name,
                             AttributeGroup = a.ProductAttributeGroupMetaData.ProductAttributeGroupNames.FirstOrDefault(c => c.LanguageID == Client.User.LanguageID).Name,
                             a.Sign,
                             a.Index,
                             a.IsVisible,
                             a.IsSearchable,
                             productGroupMappingID = productGroupMappingID
                           }));
    }

    [RequiresAuthentication(Functionalities.CreateProductGroupAttributeMapping)]
    public ActionResult Create(int AttributeID, string value, int productGroupMappingID)
    {
      bool valueOverride = false;

      SetSpecialPropertyValues<bool>(valueOverride);

      try
      {
        using (var unit = GetUnitOfWork())
        {
          ((IProductGroupMappingService)unit.Service<ProductGroupMapping>()).CreateProductGroupAttributeMapping(AttributeID, value, productGroupMappingID);

          unit.Save();

          return Success("Successfully inserted attribute for all products");
        }
      }
      catch (Exception ex)
      {
        return Failure(String.Format("Something went wrong when adding an attribute: {0}", ex.Message));
      }
    }

    [RequiresAuthentication(Functionalities.DeleteProductGroupAttributeMapping)]
    public ActionResult Delete(int AttributeID, int productGroupMappingID)
    {
      using (var unit = GetUnitOfWork())
      {
        try
        {
          ((IProductGroupMappingService)unit.Service<ProductGroup>()).DeleteProductGroupMappingAttribute(AttributeID, productGroupMappingID);

          unit.Save();

          return Success("Successfully inserted attribute for all products");
        }
        catch (Exception e)
        {
          return Failure("Something went wrong when adding an attribute: ", e);
        }
      }
    }

    [RequiresAuthentication(Functionalities.UpdateProductGroupAttributeMapping)]
    public ActionResult Update(int _AttributeID, int productGroupMappingID, int AttributeGroupID)
    {
      return Update<ProductAttributeMetaData>(c => c.AttributeID == _AttributeID, (unit, attributeObject) => {
        attributeObject.ProductAttributeGroupID = AttributeGroupID;
      });
    }
  }
}

