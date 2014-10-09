using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Concentrator.Objects.Web;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Models.Media;
using Concentrator.Objects.Services.ServiceInterfaces;
using Concentrator.Web.Shared;
using Concentrator.Web.Shared.Controllers;
using System.Configuration;
using System.IO;
using Concentrator.Objects.Utility;


namespace Concentrator.ui.Management.Controllers
{
  public class ProductMediaController : BaseController
  {
    [RequiresAuthentication(Functionalities.GetProductMedia)]
    public ActionResult GetMedia(int productID)
    {
      using (var unit = GetUnitOfWork())
      {
        var product = unit.Service<Product>().Get(c => c.ProductID == productID);
        var productMedia = unit.Service<ProductMedia>().GetAll(c => c.ProductID == productID).ToList();

        if (product.ChildProducts.Count() > 0 && product.ChildProducts.FirstOrDefault().IsConfigurable)
        {
          productMedia = unit.Service<Product>().GetAll(c => c.ParentProductID == productID).SelectMany(c => c.ProductMedias).ToList();
        }

        var media = (from p in productMedia
                     group p by p.Vendor
                       into vc
                       select new
                       {
                         vc.Key.Name,
                         vc.Key.VendorID,
                         Media = (from mj in vc

                                  select new
                                  {
                                    mj.MediaType.Type,
                                    mj.MediaType.TypeID,
                                    mj.MediaUrl,
                                    Visible = mj.Product.Visible,
                                    mj.MediaPath,
                                    mj.MediaID,
                                    mj.Sequence,
                                    mj.Size,
                                    mj.Product.VendorItemNumber,
                                    mj.Resolution,
                                    mj.Description
                                  }).OrderBy(c => c.VendorItemNumber).ThenBy(c => c.Sequence)
                       }).ToList();
        return Json(new { results = media });
      }
    }

    [RequiresAuthentication(Functionalities.GetProductMedia)]
    public ActionResult GetList()
    {
      return List(unit => from m in unit.Service<ProductMedia>().GetAll()
                          select new
                          {
                            m.MediaID,
                            m.ProductID,
                            Description = m.Product.ProductDescriptions.Select(x => x.ShortContentDescription),
                            m.Sequence,
                            m.VendorID,
                            Vendor = m.Vendor.Name,
                            m.TypeID,
                            MediaType = m.MediaType.Type,
                            m.MediaUrl,
                            m.MediaPath
                          });
    }

    [RequiresAuthentication(Functionalities.CreateProductMedia)]
    public ActionResult CreateMedia()
    {
      return Create<ProductMedia>();
    }

    [RequiresAuthentication(Functionalities.DeleteProductMedia)]
    public ActionResult DeleteMedia(int id)
    {
      return Delete<ProductMedia>(m => m.MediaID == id);
    }

    [RequiresAuthentication(Functionalities.UpdateProductMedia)]
    public ActionResult UpdateMedia(int id)
    {
      return Update<ProductMedia>(m => m.MediaID == id);
    }

    [RequiresAuthentication(Functionalities.GetProductMedia)]
    public ActionResult GetMediaTypes(int productID)
    {
      return SimpleList(unit => from x in unit.Service<MediaType>().GetAll()
                                select new
                                {
                                  x.Type,
                                  x.TypeID
                                });
    }

    [RequiresAuthentication(Functionalities.CreateProductMedia)]
    public ActionResult Create(HttpPostedFileBase files, string mediaurl, int typeID, int productID, int vendorID, string description, bool IsSearched = false)
    {
      try
      {
        using (var unit = GetUnitOfWork())
        {
          List<int> allProductIDsThatChange = null;
          if (IsSearched)//change it for all underlying products
          {
            //see on wich level you want to udate the products
            allProductIDsThatChange = unit.Service<Product>().GetAll(c => c.ParentProductID == productID).Select(c => c.ProductID).ToList();


            //if (level == 1)//change it for all the underlying products
            //{
            //  allProductIDsThatChange = unit.Service<RelatedProduct>().GetAll(c => c.ProductID == productID).Select(c => c.RelatedProductID).ToList();
            //  allProductIDsThatChange.Remove(productID);
            //}
            //else
            //{
            //  //get the storeID 
            //  var storeId = unit.Service<ContentProductMatch>().Get(c => c.ProductID == productID && c.Index == 0).StoreID;
            //  allProductIDsThatChange = unit.Service<ContentProductMatch>().GetAll(c => c.StoreID == storeId).Select(c => c.ProductID).ToList();
            //  allProductIDsThatChange.Remove(productID);
            //}

          }

          var file = Request.Files.Get("MediaPath");

          if (file != null)
          {
            ((IProductService)unit.Service<Product>()).CreateProductMedia(file.InputStream, file.FileName, mediaurl, productID, typeID, vendorID, description, allProductIDsThatChange);
          }
          else
          {
            ((IProductService)unit.Service<Product>()).CreateProductMediaByUrl(mediaurl, productID, typeID, vendorID, description, allProductIDsThatChange);
          }

          unit.Save();

          return Success("Media uploaded and saved successfully", isMultipartRequest: true);
        }
      }
      catch (Exception e)
      {
        return Failure("Media upload failed", e, true);
      }
    }

    [RequiresAuthentication(Functionalities.UpdateProductMedia)]
    public ActionResult Update(int MediaID, int ProductID, int VendorID, int sequence_new, int sequence_old, int TypeID, string MediaPath, string MediaUrl, string Description, int TypeID_Old, bool isSearched = false)
    {
      using (var unit = GetUnitOfWork())
      {
        ((IProductService)unit.Service<Product>()).UpdateProductMedia(MediaID, ProductID, VendorID, sequence_new, sequence_old, TypeID, MediaPath, MediaUrl, Description, TypeID_Old);

        unit.Save();

        return Success("Media updated");
      }
    }

    private int getMediaIdForProduct(int underlyingProductID, int VendorID, int TypeID, string MediaPath, string MediaUrl)
    {
      using (var unit = GetUnitOfWork())
      {

        var media = unit.Service<ProductMedia>().Get(c =>
          c.ProductID == underlyingProductID
          && c.VendorID == VendorID
          && c.TypeID == c.TypeID
          && ((String.IsNullOrEmpty(MediaPath)) ? true : c.MediaPath == MediaPath
            || (String.IsNullOrEmpty(MediaUrl)) ? true : c.MediaUrl == MediaUrl)
          );

        return media == null ? 0 : media.MediaID;
      }

    }

    [RequiresAuthentication(Functionalities.DeleteProductMedia)]
    public ActionResult Delete(int mediaID, bool isSearched = false, bool deleteChildren = false)
    {
      try
      {
        using (var unit = GetUnitOfWork())
        {
          var media = unit.Service<ProductMedia>().Get(c => c.MediaID == mediaID);
          if (media == null)
            return Failure("Could not find product image");

          var productID = media.ProductID;
          if (deleteChildren)
          {

            var childrenProducts = unit.Service<Product>().GetAll(c => c.ParentProductID == productID).ToList();
            //assume we want to delete product images with same file name
            foreach (var childrenProduct in childrenProducts)
            {
              var mediaToDelete = childrenProduct.ProductMedias.Where(c => c.FileName == media.FileName).ToList();
              if (mediaToDelete == null || mediaToDelete.Count == 0) continue;

              mediaToDelete.ForEach(c =>
              {
                ((IProductService)unit.Service<Product>()).DeleteProductMedia(c.MediaID);
                unit.Service<ProductMedia>().Delete(l => l.MediaID == c.MediaID);
              }
              );
            }
          }

          ((IProductService)unit.Service<Product>()).DeleteProductMedia(mediaID);

          return Delete<ProductMedia>(x => x.MediaID == mediaID);
        }
      }
      catch (Exception e)
      {
        return Failure("Something went wrong: " + e.Message);
      }
    }

    [RequiresAuthentication(Functionalities.GetProductMedia)]
    public ActionResult Download(string path)
    {
      var directory = ConfigurationManager.AppSettings["FTPMediaDirectory"];
      var prodPath = ConfigurationManager.AppSettings["FTPMediaProductDirectory"];
      var fullPath = Path.Combine(directory, path);

      try
      {
        return new Concentrator.Web.Shared.Results.FileResult(fullPath, string.Empty, MimeUtility.MimeType(path));
      }
      catch
      {
        return new Concentrator.Web.Shared.Results.FileResult(path, string.Empty, MimeUtility.MimeType(path));
      }
    }
  }
}
