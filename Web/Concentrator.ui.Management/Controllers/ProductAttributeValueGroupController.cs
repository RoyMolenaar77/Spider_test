using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Concentrator.Web.Shared.Controllers;
using Concentrator.Objects.Models.Users;
using Concentrator.Web.Shared;
using Concentrator.Objects.Models.Attributes;
using Concentrator.Objects.Web;
using Concentrator.Objects.Services.ServiceInterfaces;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Models.Localization;
using System.Configuration;
using System.IO;
using System.Drawing.Imaging;
using System.Drawing;
using Concentrator.Objects.Images;

namespace Concentrator.ui.Management.Controllers
{
  public class ProductAttributeValueGroupController : BaseController
  {
    [RequiresAuthentication(Functionalities.Default)]
    public ActionResult Search(string query)
    {
      query = query.IfNullOrEmpty("");
      return Search(unit => from a in unit.Service<ProductAttributeValueGroup>().GetAll()
                            where (a.ProductAttributeValueGroupNames.Any(c => c.Name.Contains(query)) && (!a.ConnectorID.HasValue || a.ConnectorID.Value == Client.User.ConnectorID.Value))
                            select new
                            {
                              AttributeValueGroup = a.ProductAttributeValueGroupNames.FirstOrDefault(c => c.LanguageID == Client.User.LanguageID).Name,
                              AttributeValueGroupID = a.AttributeValueGroupID
                            });
    }

    [RequiresAuthentication(Functionalities.Default)]
    public ActionResult Create()
    {
      return Create<ProductAttributeValueGroup>((unit, group) =>
        {
          var langs = GetPostedLanguages();

          ((IProductService)unit.Service<Product>()).CreateProductAttributeValueGroup(group, GetPostedLanguages());

        });
    }

    [RequiresAuthentication(Functionalities.Default)]
    public ActionResult GetList()
    {
      return List(unit =>
                  from u in unit.Service<ProductAttributeValueGroup>().GetAll()
                  let valueGroupNameDefault = u.ProductAttributeValueGroupNames.FirstOrDefault(c => c.LanguageID == Client.User.LanguageID)
                  let valueGroupName = valueGroupNameDefault == null ? u.ProductAttributeValueGroupNames.OrderBy(c => c.LanguageID).FirstOrDefault() : valueGroupNameDefault
                  where (!u.ConnectorID.HasValue || u.ConnectorID.Value == Client.User.ConnectorID.Value)
                  select new
                  {
                    u.AttributeValueGroupID,
                    AttributeValueGroup = valueGroupName.Name,
                    u.Score,
                    u.ImagePath,
                    u.ConnectorID
                  });



    }

    [RequiresAuthentication(Functionalities.Default)]
    public ActionResult GetTranslations(int? attributeValueGroupID)
    {
      return List(unit => (from l in unit.Service<Language>().GetAll()
                           join p in unit.Service<ProductAttributeValueGroupName>().GetAll().Where(c => attributeValueGroupID.HasValue ? attributeValueGroupID == c.AttributeValueGroupID : true) on l.LanguageID equals p.LanguageID into temp
                           from tr in temp.DefaultIfEmpty()
                           select new
                           {
                             l.LanguageID,
                             Language = l.Name,
                             tr.Name,
                             AttributeValueGroupID = (tr == null ? 0 : tr.AttributeValueGroupID)
                           }));
    }

    [RequiresAuthentication(Functionalities.Default)]
    public ActionResult SetTranslation(int _LanguageID, int AttributeValueGroupID, string name)
    {
      if (string.IsNullOrEmpty(name))
      {
        try
        {
          using (var unit = GetUnitOfWork())
          {
            unit.Service<ProductAttributeValueGroupName>().Delete(c => c.AttributeValueGroupID == AttributeValueGroupID && c.LanguageID == _LanguageID);

            unit.Save();
            return Success("Update translation successfully");
          }
        }
        catch (Exception e)
        {
          return Failure("Something went wrong: ", e);
        }
      }
      else
      {
        try
        {
          using (var unit = GetUnitOfWork())
          {
            var nameG = unit.Service<ProductAttributeValueGroupName>().Get(c => c.AttributeValueGroupID == AttributeValueGroupID && c.LanguageID == _LanguageID);
            if (nameG == null)
            {
              nameG = new ProductAttributeValueGroupName();
              nameG.AttributeValueGroupID = AttributeValueGroupID;
              nameG.LanguageID = _LanguageID;
              nameG.Name = name;
              unit.Service<ProductAttributeValueGroupName>().Create(nameG);
            }
            else
            {
              nameG.Name = name;
            }
            unit.Save();
            return Success("Update translation successfully");
          }
        }
        catch (Exception e)
        {
          return Failure("Something went wrong: ", e);
        }
      }
    }

    [RequiresAuthentication(Functionalities.Default)]
    public ActionResult Update(int id)
    {
      return Update<ProductAttributeValueGroup>(c => c.AttributeValueGroupID == id);
    }

    [RequiresAuthentication(Functionalities.Default)]
    public ActionResult Delete(int id)
    {
      return Delete<ProductAttributeValueGroup>(c => c.AttributeValueGroupID == id);
    }

    [RequiresAuthentication(Functionalities.Default)]
    public ActionResult UploadImage(int AttributeValueGroupID)
    {
      using (var unit = GetUnitOfWork())
      {
        var AttributeValueGroup = unit.Service<ProductAttributeValueGroup>().Get(a => a.AttributeValueGroupID == AttributeValueGroupID);
        string fileName = "";

        foreach (string f in Request.Files)
        {
          var upload = Request.Files.Get(f);
          //var extension = upload.FileName.Substring(upload.FileName.LastIndexOf(".") + 1);
          //var fileName = BrandID + "_logo." + extension;
          fileName = upload.FileName;

          var path = ConfigurationManager.AppSettings["FTPBrandLogoDirectory"];

          //var type = (Request["TypeID"]).ToString();
          var targetPath = Path.Combine(path, fileName);
          var physicalDirectory = Path.Combine(ConfigurationManager.AppSettings["FTPMediaDirectory"], path);
          var fullPath = Path.Combine(ConfigurationManager.AppSettings["FTPMediaDirectory"], targetPath);

          //no file
          if (!Directory.Exists(physicalDirectory))
            Directory.CreateDirectory(physicalDirectory);

          using (var st = upload.InputStream)
          {
            //set the path
            AttributeValueGroup.ImagePath = targetPath;
            unit.Save();

            //copy it locally
            byte[] buff = new byte[st.Length];
            st.Read(buff, 0, (int)st.Length);
            System.IO.File.WriteAllBytes(fullPath, buff);
          }


        }
        return new JsonResult
        {
          ContentType = "text/html",
          Data = new
          {
            success = true,
            message = fileName + " is succesfully uploaded."
          }
        };
      }
    }

    [RequiresAuthentication(Functionalities.Default)]
    private string ExtractFileName(string filepath)
    {
      // If path ends with a "\", it's a path only so return String.Empty.
      if (filepath.Trim().EndsWith(@"\"))
      {
        return String.Empty;
      }

      // Determine where last backslash is.
      int position = filepath.LastIndexOf('\\');

      // If there is no backslash, assume that this is a filename.
      if (position == -1)
      {
        // Determine whether file exists in the current directory.
        if (System.IO.File.Exists(Environment.CurrentDirectory + Path.DirectorySeparatorChar + filepath))
          return filepath;
        else
          return String.Empty;
      }
      else
      {
        // Determine whether file exists using filepath.
        if (System.IO.File.Exists(filepath))
          // Return filenname without file path
          return filepath.Substring(position + 1);
        else
          return String.Empty;
      }

    }

    [RequiresAuthentication(Functionalities.Default)]
    public Image FetchImageUrl(string imagePath, decimal? width, decimal? height)
    {
      Response.ContentType = "image/jpeg";
      ImageFormat _format = ImageFormat.Jpeg;

      Color _backgroundColor = Color.White;

      string path = string.Empty;
      //
      Image img = null;

      if (imagePath != null)
      {
        string extension = imagePath.Substring(imagePath.LastIndexOf('.') + 1);

        switch (extension.ToLower())
        {
          case "jpg":
            _format = ImageFormat.Jpeg;
            Response.ContentType = "image/jpeg";
            break;
          case "png":
            _format = ImageFormat.Png;
            Response.ContentType = "image/png";
            _backgroundColor = Color.Transparent;
            break;
          case "gif":
            _format = ImageFormat.Gif;
            Response.ContentType = "image/gif";
            _backgroundColor = Color.Transparent;
            break;
          // for unknown types
          default:
            break;

        }

        //if its an image
        if (extension == "jpg" | extension == "png" | extension == "gif")
        {
          path = Path.Combine(ConfigurationManager.AppSettings["FTPMediaDirectory"], imagePath);
        }


        if (System.IO.File.Exists(path))
        {
          img = Image.FromFile(path);
          int _width = width.HasValue ? (int)width : img.Width;
          int _height = height.HasValue ? (int)height : img.Height;
          var im = ImageUtility.GetFixedSizeImage(img, _width, _height, true, _backgroundColor);

          EncoderParameters eps = new EncoderParameters(1);
          eps.Param[0] = new EncoderParameter(Encoder.Quality, (long)100);

          im.Save(Response.OutputStream, _format);
        }
      }
      return img;
    }
  }
}
