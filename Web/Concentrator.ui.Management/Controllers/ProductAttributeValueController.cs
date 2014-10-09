using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Concentrator.Objects.Web;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Models.Attributes;
using Concentrator.Web.Shared;
using Concentrator.Web.Shared.Controllers;
using Concentrator.Objects.Models.Contents;
using System.Data.SqlClient;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.Services.ServiceInterfaces;

namespace Concentrator.ui.Management.Controllers
{
  public class ProductAttributeValueController : BaseController
  {
    [RequiresAuthentication(Functionalities.GetProductAttributeValue)]
    public ActionResult GetList(int attributeID, int productGroupMappingID, bool? showUnmappedValues)
    {
      if (showUnmappedValues.HasValue && showUnmappedValues.Value)
      {
        return List(unit => (from pv in unit.Service<ProductAttributeValue>().GetAll()
                             join a in unit.Service<AttributeMatchStore>().GetAll() on pv.AttributeID equals a.AttributeID
                             where pv.AttributeID == attributeID
                             select new
                             {
                               pv.ProductID,
                               pv.Value,
                               pv.Product.BrandID,
                               BrandName = pv.Product.Brand.Name,
                               pv.Product.VendorItemNumber,
                               pv.AttributeValueID
                             }));
      }
      else
      {
        return List(unit => (from pv in unit.Service<ProductAttributeValue>().GetAll()
                             where pv.AttributeID == attributeID
                             select new
                             {
                               pv.ProductID,
                               pv.Value,
                               pv.Product.BrandID,
                               BrandName = pv.Product.Brand.Name,
                               pv.Product.VendorItemNumber,
                               pv.AttributeValueID
                             }));
      }
    }

    [RequiresAuthentication(Functionalities.CreateProductAttributeValue)]
    public ActionResult Create(int AttributeID, int ProductID, bool isSearched = false)
    {
      var languageIDParam = Request.Params["ID"];

      int? languageID = null;
      if (!string.IsNullOrEmpty(languageIDParam))
        languageID = int.Parse(languageIDParam);

      return Create<ProductAttributeValue>((unit, value) =>
      {
        value.LanguageID = languageID;
        value.ProductID = ProductID;
        unit.Service<ProductAttributeValue>().Create(value);
      });

    }

    [RequiresAuthentication(Functionalities.DeleteProductAttributeValue)]
    public ActionResult Delete(int id, bool isSearched = false)
    {
      if (isSearched)
      {
        using (var unit = GetUnitOfWork())
        {
          ProductAttributeValue value = unit.Service<ProductAttributeValue>().Get(c => c.AttributeValueID == id);
          List<int> underlyingProducts = GetAllUnderlyingProds(value.ProductID);
          if (underlyingProducts.Contains(value.ProductID))
          {
            underlyingProducts.Remove(value.ProductID);
          }
          List<ProductAttributeValue> underlyingValues = unit.Service<ProductAttributeValue>().GetAll(
            c => underlyingProducts.Contains(c.ProductID)
              && c.AttributeID == value.AttributeID).ToList();

          unit.Service<ProductAttributeValue>().Delete(underlyingValues);
        }
      }
      return Delete<ProductAttributeValue>(c => c.AttributeValueID == id);
    }

    [RequiresAuthentication(Functionalities.UpdateProductAttributeValue)]
    public ActionResult Update(int productID, int id, String Value)
    {
      return Update<ProductAttributeValue>(x => x.AttributeValueID == id, (unit, attributeValue) =>
      {
        attributeValue.Value = Value;
      }, true);

      //using (var unit = GetUnitOfWork())
      //{
      //  var attributeValue = unit.Scope
      //    .Repository<ProductAttributeValue>()
      //    .Include(x => x.ProductAttributeMetaData)
      //    .GetSingle(x => x.AttributeValueID == id);

      //  if (attributeValue != null)
      //  {

      //    unit.Save();
      //  }
      //}
    }

    [RequiresAuthentication(Functionalities.GetValueGrouping)]
    public ActionResult GetGroupedValues(int valueGroupID, int attributeID)
    {
      int connector = Client.User.ConnectorID.Value;

      return List(unit => from v in unit.Service<ProductAttributeValue>().GetAll()
                          //join c in unit.Service<ProductAttributeValueValueGroup>().GetAll() on new { v.AttributeID, v.Value } equals new { AttributeID = c.AttributeID.Value, c.Value }
                          where v.AttributeID == attributeID
                          //&&
                          // c.ConnectorID == connector && c.AttributeValueGroupID == valueGroupID
                          select new
                          {
                            v.Value,
                            v.AttributeValueID,
                            v.ProductID,
                            Product = v.Product.VendorItemNumber
                          });
    }

    [RequiresAuthentication(Functionalities.DeleteValueGrouping)]
    public ActionResult RemoveValueFromGroup(int id)
    {
      try
      {
        using (var unit = GetUnitOfWork())
        {
          var attrVal = unit.Service<ProductAttributeValue>().Get(c => c.AttributeValueID == id);


          unit.Save();
        }
        return Success("Successfully removed attribute value from group");
      }
      catch (Exception e)
      {
        return Failure("Something went wrong: ", e);
      }
    }

    [RequiresAuthentication(Functionalities.GetValueGrouping)]
    public ActionResult GetUngrouped(bool? isMatched)
    {

      int? connectorID = null;

      if (Client.User.ConnectorID.HasValue)
        connectorID = Client.User.ConnectorID.Value;

      return List(unit =>
                  (from f in ((IFilterService)unit.Service<ProductAttributeMetaData>()).GetProductAttributeValueGrouping(connectorID, Client.User.LanguageID)
                   select new
                   {
                     f.AttributeID,
                     f.Value,
                     Attribute = f.Name,
                     f.AttributeValueGroupID,
                     isMatched = (f.AttributeValueGroupID != -1)
                   }).AsQueryable());


    }

    [RequiresAuthentication(Functionalities.GetValueGrouping)]
    public ActionResult GetValueGroups(string value, int attributeID)
    {
      int? connectorID = Client.User.ConnectorID;
      int languageID = Client.User.LanguageID;

      return List(unit =>
              from u in unit.Service<ProductAttributeValueConnectorValueGroup>().GetAll(c => c.Value == value && c.AttributeID == attributeID && (!c.ConnectorID.HasValue || (connectorID.HasValue && c.ConnectorID == connectorID)))
              let nameDefault = u.ProductAttributeValueGroup.ProductAttributeValueGroupNames.FirstOrDefault(c => c.LanguageID == languageID)
              let nameActual = nameDefault == null ? u.ProductAttributeValueGroup.ProductAttributeValueGroupNames.OrderBy(c => c.LanguageID).FirstOrDefault() : nameDefault
              select new
              {
                AttributeValue = u.Value,
                u.AttributeValueGroupID,
                AttributeValueGroup = nameActual.Name,
                u.Value,
                u.AttributeID
              }
              );
    }

    [RequiresAuthentication(Functionalities.DeleteValueGrouping)]
    public ActionResult DeleteValueGroup(string _Value, int _AttributeID, int _AttributeValueGroupID)
    {
      try
      {
        var connectorID = Client.User.ConnectorID;

        using (var unit = GetUnitOfWork())
        {
          unit.Service<ProductAttributeValueConnectorValueGroup>().Delete(c =>
            c.AttributeID == _AttributeID
            && c.AttributeValueGroupID == _AttributeValueGroupID
            && c.ConnectorID.Equals(connectorID)
            && c.Value == _Value);

          unit.Save();

          return Success("Successfully removed attribute from group");
        }

      }
      catch (Exception e)
      {
        return Failure("Something went wrong: ", e);
      }
    }

    [RequiresAuthentication(Functionalities.UpdateValueGrouping)]
    public ActionResult AddValueToGroup(int attributeID, string value, int AttributeValueGroupID)
    {
      try
      {
        var connectorID = Client.User.ConnectorID;


        using (var unit = GetUnitOfWork())
        {
          //unit.Service<ProductAttributeValueValueGroup>().Delete(c => c.ConnectorID == connectorID && c.AttributeValueID == id);
          unit.Service<ProductAttributeValueConnectorValueGroup>().Create(new ProductAttributeValueConnectorValueGroup()
                            {
                              Value = value,
                              AttributeID = attributeID,
                              ConnectorID = connectorID,
                              AttributeValueGroupID = AttributeValueGroupID
                            });

          unit.Save();

          return Success("Successfully grouped item");
        }
      }
      catch (Exception e)
      {
        return Failure("Something went wrong: ", e);
      }
    }
  }

  public class AttributeValueComparer : IEqualityComparer<ProductAttributeValue>
  {
    #region IEqualityComparer<ProductAttributeValue> Members

    public bool Equals(ProductAttributeValue x, ProductAttributeValue y)
    {
      return x.Value == y.Value;
    }

    public int GetHashCode(ProductAttributeValue obj)
    {
      return obj.AttributeValueID;
    }

    #endregion
  }

}
