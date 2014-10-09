using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Models.Brands;
using Concentrator.Objects.Models.Products;
using Concentrator.Web.Shared;
using Concentrator.Web.Shared.Controllers;

namespace Concentrator.ui.Management.Controllers
{
  public class BrandController : BaseController
  {

    [RequiresAuthentication(Functionalities.GetBrand)]
    public ActionResult GetList()
    {

      return List(unit => from b in unit.Service<Brand>().GetAll()
                          select new
                          {
                            b.BrandID,
                            b.Name
                          });
    }

    [RequiresAuthentication(Functionalities.GetBrand)]
    public ActionResult Search(string query)
    {
      return Search(unit => from o in unit.Service<Brand>().Search(query)
                            let brandMedia = o.BrandMedias.FirstOrDefault(x => x.TypeID == 1)
                            select new
                            {
                              BrandName = o.Name,
                              o.BrandID,                              
                              ImagePath = brandMedia != null ? brandMedia.MediaPath : string.Empty
                            });
    }

    [RequiresAuthentication(Functionalities.CreateBrand)]
    public ActionResult Create()
    {
      return Create<Brand>();
    }

    [RequiresAuthentication(Functionalities.UpdateBrand)]
    public ActionResult Update(int id)
    {
      return Update<Brand>(c => c.BrandID == id);
    }

    public ActionResult Delete(int id)
    {
      return Delete<Brand>(c => c.BrandID == id);
    }

    [RequiresAuthentication(Functionalities.GetBrand)]
    public ActionResult GetBrandPerProduct(int productID)
    {
      return List(unit => from b in unit.Service<Product>().GetAll()
                          where b.ProductID == productID
                          select new
                          {
                            b.ProductID,
                            b.BrandID,
                            BrandName = b.Brand.Name,
                            ProductName = b.ProductDescriptions.Select(x => x.ShortContentDescription).FirstOrDefault()
                          });
    }




    [RequiresAuthentication(Functionalities.UpdateBrand)]
    public ActionResult UpdateBrandperVendor(int productID, int brandID, bool isSearched = false)
    {

      //working version
      if (isSearched)
      {
        List<int> allProductIDs = GetAllUnderlyingProds(productID);

        using (var unit = GetUnitOfWork())
        {
          foreach (var item in allProductIDs)
          {
            var prod = unit.Service<Product>().Get(c => c.ProductID == item);
            prod.BrandID = brandID;

          }
          unit.Save();

        }
        return Success("Updated " + allProductIDs.Count + " objects ", isMultipartRequest: false, needsRefresh: false);

        //return UpdateForAllProducts<Product>(c => allProductIDs.Contains(c.ProductID),
        //  action: (unit, product) =>
        //  {
        //    product.BrandID = brandID;
        //    unit.Save();
        //  });
      }

      else
      {
        return Update<Product>(c => c.ProductID == productID,
          action: (unit, product) =>
        {
          product.BrandID = brandID;
        });
      }
      //end working version







      //if (isSearched)
      //{

      //code from stan that doenst work
      //List<int> allProductIDs2 = GetAllUnderlyingProds(productID);
      
      ////code from stan
      //return Update<Product>(c => c.ProductID == productID || (isSearched && allProductIDs2.Contains(c.ProductID)),
      //  action: (unit, product) =>
      //  {
      //    product.BrandID = brandID;
      //  });
      ////end code from stan that doesnt work


      //  List<int> allProductIDs = GetAllUnderlyingProds(productID);
      //  using (var unit = GetUnitOfWork())
      //  {
      //    foreach (var prod in allProductIDs)
      //    {
      //      var product = unit.Service<Product>().Get(c => c.ProductID == prod);
      //      product.BrandID = brandID;
      //      unit.Save();
      //    }
      //  }
      //  return Success("Updated object ", isMultipartRequest: false, needsRefresh: true);
      //}
      //else
      //{

      //  return Update<Product>(c => c.ProductID == productID,
      //    action: (unit, product) =>
      //  {
      //    product.BrandID = brandID;
      //  });

     // }
    }


  }


}
