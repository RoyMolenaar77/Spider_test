using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.IO;
using System.Drawing;
using Concentrator.Web.Shared;
using Concentrator.Web.Shared.Controllers;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Models.Brands;
using Concentrator.Objects.Models.Media;


namespace Concentrator.ui.Management.Controllers
{
  public class BrandMediaController : BaseController
  {
    [RequiresAuthentication(Functionalities.GetBrandMedia)]
    public ActionResult GetMedia(int brandID)
    {
      using (var unit = GetUnitOfWork())
      {
        var media = (from p in unit.Service<BrandMedia>().GetAll(c => c.BrandID == brandID)
                     orderby p.Sequence ascending
                     select new
                     {
                       p.MediaType.Type,
                       p.MediaType.TypeID,
                       p.MediaUrl,
                       p.MediaPath,
                       p.MediaID,
                       p.Sequence,
                       p.Size,
                       p.Resolution

                     }).ToList();

        return Json(new { results = media });

      }
    }

    [RequiresAuthentication(Functionalities.GetBrandMedia)]
    public ActionResult GetList()
    {
      return List(unit => from m in unit.Service<BrandMedia>().GetAll()
                          select new
                          {
                            m.MediaID,
                            m.Sequence,
                            m.TypeID,
                            m.MediaUrl,
                            m.MediaPath
                          });
    }

    [RequiresAuthentication(Functionalities.CreateBrandMedia)]
    public ActionResult CreateMedia()
    {
      return Create<BrandMedia>();
    }

    [RequiresAuthentication(Functionalities.DeleteBrandMedia)]
    public ActionResult DeleteMedia(int id)
    {
      return Delete<BrandMedia>(m => m.MediaID == id);
    }

    [RequiresAuthentication(Functionalities.UpdateBrandMedia)]
    public ActionResult UpdateMedia(int id)
    {
      return Update<BrandMedia>(m => m.MediaID == id);
    }

    [RequiresAuthentication(Functionalities.GetBrandMedia)]
    public ActionResult GetMediaTypes(int productID)
    {
      using (var unit = GetUnitOfWork())
      {
        var types = (from p in unit.Service<MediaType>().GetAll()
                     select new
                              {
                                p.Type,
                                p.TypeID
                              }).ToList();


        return Json(new { results = types });
      }
    }




    [RequiresAuthentication(Functionalities.CreateProductMedia)]
    public ActionResult Create(HttpPostedFileBase file, string mediaurl, int TypeID, int brandID)
    {

      return Create<BrandMedia>((unit, media) =>
      {

        //media.BrandID = brandID;
        //media.TypeID = TypeID;

        var mediaTypes = (from type in unit.Service<MediaType>().GetAll()
                          select new
                          {
                            Name = type.Type,
                            ID = type.TypeID
                          }).ToList();

        foreach (string f in Request.Files)
        {
          var upload = Request.Files.Get(f);
          var path = ConfigurationManager.AppSettings["FTPMediaDirectory"];
          var prodPath = ConfigurationManager.AppSettings["FTPMediaBrandDirectory"];
          var innerPath = Path.Combine(path, prodPath);
          var filePath = Path.Combine(prodPath, upload.FileName);
          var fullPath = Path.Combine(path, filePath);
          //var typeID = (Request["TypeID"]).ToString();
          var extension = upload.FileName.Substring(upload.FileName.LastIndexOf('.') + 1);
          //no file
          if (string.IsNullOrEmpty(mediaurl))
          {
            if (!Directory.Exists(innerPath))
              Directory.CreateDirectory(innerPath);

            using (var st = upload.InputStream)
            {
              //copy it locally
              byte[] buff = new byte[st.Length];
              st.Read(buff, 0, (int)st.Length);
              System.IO.File.WriteAllBytes(fullPath, buff);

              //set the path
              media.MediaPath = filePath;
              st.Close();
            }
            if (extension == "jpg" | extension == "png" | extension == "gif")
            {

              Image img = Image.FromFile(fullPath);
              media.Resolution = string.Format("{0}x{1}", img.Width, img.Height);
              media.Size = (int)Math.Round(new FileInfo(fullPath).Length / 1024d, 0);
            }
          }
          else
          {
            media.MediaUrl = mediaurl;

          }

          var MediaType = unit.Service<MediaType>().Get(x => x.TypeID == TypeID);

          media.MediaType = MediaType;

          var se = unit.Service<BrandMedia>().Get(x => x.BrandID == brandID);
          media.Sequence = se != null ? se.Sequence : 0;

          unit.Service<BrandMedia>().Create(media);
        }
      }, true);
    }

    [RequiresAuthentication(Functionalities.UpdateProductMedia)]
    public ActionResult Update(int ProductID, int VendorID, int sequence_new, int sequence_old, int TypeID, string MediaPath, string MediaUrl)
    {
      //TODO: add logic for shifting sequences 
      using (var unit = GetUnitOfWork())
      {
        var media = (from m in unit.Service<BrandMedia>().GetAll() where m.BrandID == ProductID && m.BrandID == VendorID && m.TypeID == TypeID select m).ToList();

        var mediaItem = media.FirstOrDefault(c => c.Sequence == sequence_old);

        var mediaItemNext = media.FirstOrDefault(c => c.Sequence == sequence_new);

        mediaItem.Sequence = sequence_new;
        mediaItem.MediaPath = MediaPath;
        mediaItem.MediaUrl = MediaUrl;
        mediaItemNext.Sequence = sequence_old;

        unit.Save();
      }

      return Success("Media updated");
    }

    [RequiresAuthentication(Functionalities.DeleteProductMedia)]
    public ActionResult Delete(int mediaID)
    {
      try
      {
        using (var unit = GetUnitOfWork())
        {
          var media = unit.Service<BrandMedia>().Get(c => c.MediaID == mediaID);
          System.IO.File.Delete(Path.Combine(ConfigurationManager.AppSettings["FTPMediaDirectory"], media.MediaPath));
        }

        return Delete<BrandMedia>(c => c.MediaID == mediaID);

      }
      catch (Exception e)
      {
        return Failure("Something went wrong: " + e.Message);
      }
    }

    public ActionResult DownloadByID(int id)
    {
      using (var unit = GetUnitOfWork())
      {
        string fullPath = string.Empty;
        string pathInner = string.Empty;
        try
        {
          var media = unit.Service<BrandMedia>().Get(c => c.BrandID == id);

          pathInner = media.MediaPath;
          fullPath = Path.Combine(ConfigurationManager.AppSettings["FTPMediaDirectory"], pathInner);

          return new Concentrator.Web.Shared.Results.FileResult(fullPath, string.Empty, MimeType(fullPath));
        }
        catch
        {
          return new Concentrator.Web.Shared.Results.FileResult(fullPath, string.Empty, MimeType(fullPath));
        }

      }
    }

    public ActionResult Download(string path)
    {
      var directory = ConfigurationManager.AppSettings["FTPMediaDirectory"];
      var prodPath = ConfigurationManager.AppSettings["FTPMediaBrandDirectory"];
      var fullPath = Path.Combine(directory, path);

      try
      {
        return new Concentrator.Web.Shared.Results.FileResult(fullPath, string.Empty, MimeType(path));
      }
      catch
      {
        return new Concentrator.Web.Shared.Results.FileResult(path, string.Empty, MimeType(path));
      }
    }

    private string MimeType(string Filename)
    {
      string mime = "application/octetstream";
      string ext = System.IO.Path.GetExtension(Filename).ToLower();
      Microsoft.Win32.RegistryKey rk = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext);
      if (rk != null && rk.GetValue("Content Type") != null)
        mime = rk.GetValue("Content Type").ToString();
      return mime;
    }

  }
}
