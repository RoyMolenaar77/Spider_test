using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects.Web;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Models.Brands;
using Concentrator.Objects.Models.Products;
using Concentrator.Web.Shared;
using Concentrator.Web.Shared.Controllers;
using System.Configuration;

namespace Concentrator.ui.Management.Controllers
{
  public class RelatedProductController : BaseController
  {
    [RequiresAuthentication(Functionalities.GetRelatedProduct)]
    public ActionResult GetRelatedProductsByProductID(int productID)
    {
      var result = List(unit => (from c in unit.Service<RelatedProduct>().GetAll(x => x.ProductID == productID)
                           let relatedProd = c.RProduct
                           let productName = c.RProduct != null ?
                                            c.RProduct.ProductDescriptions.FirstOrDefault(x => x.LanguageID == Client.User.LanguageID) ??
                                            c.RProduct.ProductDescriptions.FirstOrDefault() : null
                           select new
                           {
                             c.ProductID,
                             RelatedProductID = c.RelatedProductID,
                             RelatedProductDescription = productName != null && !string.IsNullOrEmpty(productName.ProductName) ? productName.ProductName : c.RProduct != null ? c.RProduct.VendorItemNumber : null,
                             c.VendorID,
                             c.RelatedProductTypeID,
                             c.IsConfigured,
                             c.IsActive,
                             c.Index,
                             RelatedProdctTypeName = c.RelatedProductType.Type,
                             relatedProd.VendorItemNumber
                           }));

      return result;
    }

    [RequiresAuthentication(Functionalities.CreateRelatedProduct)]
    public ActionResult Create()
    {
      return Create<RelatedProduct>((unit, relatedProduct) =>
        {
          relatedProduct.Index = GetNextRelatedProductIndex(unit, relatedProduct);
          unit.Service<RelatedProduct>().Create(relatedProduct);
        });
    }

    private static int GetNextRelatedProductIndex(IServiceUnitOfWork unit, RelatedProduct relatedProduct)
    {
      if (relatedProduct.RelatedProductTypeID == 8 || relatedProduct.CreatedBy == 1)
      {
        return relatedProduct.Index;
      }

      var relatedProducts = unit.Service<RelatedProduct>().GetAll(x => x.ProductID == relatedProduct.ProductID);
      foreach (var product in relatedProducts.OrderByDescending(r=>r.Index))
      {
        return product.Index > 1000 ? product.Index++ : 1001;
      }


      return relatedProduct.Index;
    }

   

    [RequiresAuthentication(Functionalities.UpdateRole)]
    public ActionResult Update(int _RelatedProductID, int _ProductID)
    {
      return Update<RelatedProduct>(r => r.ProductID == _ProductID && r.RelatedProductID == _RelatedProductID, (unit, relatedProduct) => { });
    }

    [RequiresAuthentication(Functionalities.DeleteRelatedProduct)]
    public ActionResult Delete(int _RelatedProductID, int _ProductID)
    {
      return Delete<RelatedProduct>(c => c.RelatedProductID == _RelatedProductID && c.ProductID == _ProductID);
    }

    [RequiresAuthentication(Functionalities.GetRelatedProduct)]
    public ActionResult Search(string query)
    {
      return SimpleList(unit => from p in unit.Service<ProductDescription>().GetAll(c => c.LanguageID == Client.User.LanguageID)
                                .Search(query, (c => c.Name == "ProductName"))
                                select new
                                {
                                  RelatedProductID = p.ProductID,
                                  RelatedProductDescription = p.ProductName
                                });
    }

    [RequiresAuthentication(Functionalities.CreateRelatedProduct)]
    public ActionResult CreateRelatedProductType()
    {
      return Create<RelatedProductType>();
    }

    [RequiresAuthentication(Functionalities.GetRelatedProduct)]
    public ActionResult SearchRelatedProductType(string query)
    {
      if (query != null && query != String.Empty && query != "")
      {
        return SimpleList(unit => from o in unit.Service<RelatedProductType>().Search(query)
                                  select new
                                  {
                                    o.RelatedProductTypeID,
                                    o.Type
                                  });
      }
      else
      {
        return SimpleList(unit => from o in unit.Service<RelatedProductType>().GetAll()
                                  select new
                                  {
                                    o.RelatedProductTypeID,
                                    o.Type
                                  });
      }
    }
  }
}
