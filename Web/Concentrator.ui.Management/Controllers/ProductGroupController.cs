using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Concentrator.Objects.Web;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Models.Localization;
using Concentrator.Objects.Services.ServiceInterfaces;
using Concentrator.Web.Shared;
using Concentrator.Web.Shared.Controllers;
using System.Drawing;
using System.Configuration;
using System.IO;
using Concentrator.Objects.Images;
using Concentrator.Web.Shared.Results;

namespace Concentrator.ui.Management.Controllers
{
  public class ProductGroupController : BaseController
  {
    [RequiresAuthentication(Functionalities.GetProductGroup)]
    public ActionResult GetList()
    {
      return List(unit => from p in unit.Service<ProductGroup>().GetAll(x => x.ProductGroupID > 0)
                          let productGroupName = p.ProductGroupLanguages.FirstOrDefault(x => x.LanguageID == Client.User.LanguageID) ??
                  p.ProductGroupLanguages.FirstOrDefault() ?? null
                          select new
                          {
                            p.ProductGroupID,
                            ProductGroupVendorCount = p.ProductGroupVendors.Count(),
                            Languages = p.ProductGroupLanguages.Count(),
                            ContentProductGroup = p.ProductGroupMappings.Count(),
                            ActiveProducts = p.ProductGroupVendors.Where(x => x.VendorAssortments.Any(y => y.IsActive)).Count(),
                            Name = productGroupName != null ? productGroupName.Name : string.Empty,
                            p.Score,
                            p.Searchable
                          });
    }

    //TODO: this
    //[RequiresAuthentication(Functionalities.Default)]
    //public ActionResult SetTranslation(int _LanguageID, int productGroupID)
    //{
    //  return Merge<ProductGroupLanguage>(x => x.ProductGroupID == productGroupID && x.LanguageID == _LanguageID,
    //              populatePrimaryKeysAction: (group =>
    //              {
    //                group.LanguageID = _LanguageID;
    //                group.ProductGroupID = productGroupID;
    //              }
    //  ));
    //}

    [RequiresAuthentication(Functionalities.UpdateProductGroup)]
    public ActionResult SetTranslation(int _LanguageID, int productGroupID, string name)
    {
      using (var unit = GetUnitOfWork())
      {
        var service = ((IProductService)unit.Service<Product>());
        try
        {
          service.SetProductGroupTranslations(_LanguageID, productGroupID, name);
          unit.Save();
          return Success("Translations set");
        }
        catch (Exception e)
        {
          return Failure("Something went wrong", e);
        }
      }

    }

    [RequiresAuthentication(Functionalities.GetProductGroup)]
    public ActionResult GetTranslations(int? productGroupID)
    {
      return List(unit => (from l in unit.Service<Language>().GetAll()
                           join p in unit.Service<ProductGroupLanguage>().GetAll().Where(c => productGroupID.HasValue ? productGroupID == c.ProductGroupID : true) on l.LanguageID equals p.LanguageID into temp
                           from tr in temp.DefaultIfEmpty()
                           select new
                           {
                             l.LanguageID,
                             Language = l.Name,
                             tr.Name,
                             ProductGroupID = (tr == null ? productGroupID : tr.ProductGroupID)
                           }));
    }

    [RequiresAuthentication(Functionalities.GetProductGroup)]
    public ActionResult Search(string query)
    {
      return Search(unit => from o in unit.Service<ProductGroupLanguage>().Search(query)
                            select new
                            {
                              ProductGroupName = o.Name,
                              o.ProductGroupID
                            });
    }

    [RequiresAuthentication(Functionalities.UpdateProductGroup)]
    public ActionResult Update(int id)
    {
      return Update<ProductGroup>(c => c.ProductGroupID == id);
    }

    [RequiresAuthentication(Functionalities.GetProductGroup)]
    public ActionResult GetImage(int productGroupID, int width, int height)
    {
      using (var unit = GetUnitOfWork())
      {
        var imagePath = unit.Service<ProductGroup>().Get(c => c.ProductGroupID == productGroupID).ImagePath;
        string extension = string.Empty;

        Image img = null;

        if (string.IsNullOrEmpty(imagePath))
        {
          img = Image.FromFile(Request.PhysicalApplicationPath + @"Content\images\Icons\FileTypeIcons\unknown.png");
          extension = "png";
        }
        else
        {
          img = Image.FromFile(Path.Combine(ConfigurationManager.AppSettings["FTPMediaDirectory"], ConfigurationManager.AppSettings["FTPProductGroupMediaPath"], imagePath));
          extension = imagePath.Substring(imagePath.LastIndexOf('.') + 1);
        }

        var im = ImageUtility.ResizeImage(img, height, width);
        return new ImageResult(im, extension);
      }
    }

    [RequiresAuthentication(Functionalities.UpdateProductGroup)]
    public ActionResult AddImage(int ProductGroupID)
    {
      return Update<ProductGroup>(c => c.ProductGroupID == ProductGroupID,
      action: (unit, mapping) =>
           {
             foreach (string f in Request.Files)
             {
               string externalPath = ConfigurationManager.AppSettings["FTPMediaDirectory"];
               string internalPath = ConfigurationManager.AppSettings["FTPProductGroupMediaPath"];

               string dirPath = Path.Combine(externalPath, internalPath);

               if (!Directory.Exists(dirPath)) Directory.CreateDirectory(dirPath);

               var file = Request.Files.Get(f);
               string path = Path.Combine(dirPath, file.FileName);

               if (file.FileName != String.Empty)
               {
                 file.SaveAs(path);
               }

               mapping.ImagePath = file.FileName;
               file.InputStream.Close();
             }
           }, isMultipartRequest: true);
    }

    [RequiresAuthentication(Functionalities.DeleteProductGroup)]
    public ActionResult Delete(int id)
    {

      var success = true;
      var message = "Product Group successfully deleted";

      using (var unit = GetUnitOfWork())
      {

        var pGroup = (from pg in unit.Service<ProductGroup>().GetAll()
                      where pg.ProductGroupID == id
                      select pg).First();

        var languages = pGroup.ProductGroupLanguages.ToList();


        unit.Service<ProductGroupLanguage>().Delete(languages);
        unit.Service<ProductGroup>().Delete(pGroup);

        try
        {
          unit.Save();
        }

        catch (Exception ex)
        {
          success = false;
          message = "The product group is being used by another table" + ex.Message;
        }

        return Json(new
        {
          success,
          message
        });

      }
    }

    [RequiresAuthentication(Functionalities.CreateProductGroup)]
    public ActionResult Create(Dictionary<string, string> language)
    {
      return Create<ProductGroup>((unit, productGroup) =>
      {
        ((IProductService)unit.Service<Product>()).CreateProductGroup(productGroup, GetPostedLanguages());
      });
    }

    [RequiresAuthentication(Functionalities.GetProductGroup)]
    public ActionResult GetStore()
    {
      return Json(new
      {
        ProductGroups = SimpleList<ProductGroup>(v => new
        {
          ID = v.ProductGroupID,
          Name = v.ProductGroupLanguages.FirstOrDefault(pgl => pgl.LanguageID == Client.User.LanguageID).Name
        })
      });
    }
  }
}
