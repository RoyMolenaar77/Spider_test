using System;
using System.Linq;
using System.Web.Mvc;
using System.Drawing;
using System.Drawing.Imaging;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Models.Brands;
using System.IO;
using System.Configuration;
using Concentrator.Objects.Images;
using Concentrator.Web.Shared;
using Concentrator.Web.Shared.Controllers;
using Concentrator.Web.Shared.Models;
using Concentrator.Objects.Models.Media;

namespace Concentrator.ui.Management.Controllers
{
  public class BrandVendorController : BaseController
  {
    [RequiresAuthentication(Functionalities.GetBrandVendor)]
    public ActionResult GetList(int? BrandID, ContentFilter filter)
    {
      using (var unitOfWork = GetUnitOfWork())
      {
        var logoType = unitOfWork.Service<MediaType>().Get(x => x.Type == "Logo");

        if(logoType == null){
          logoType = new MediaType(){
            Type = "Logo"
          };
          unitOfWork.Service<MediaType>().Create(logoType);
          unitOfWork.Save();
        }

        return List((unit) =>
                    (from bv in unit.Service<BrandVendor>().GetAll()
                   where (BrandID.HasValue ? bv.BrandID == BrandID : true) &&
                   ((filter.UnmappedBrandVendor.HasValue ? bv.BrandID < 0 : true) ||
                   (filter.MappedBrandVendor.HasValue ? bv.BrandID > 0 : true))
                   let brandMedia = bv.Brand.BrandMedias.FirstOrDefault(x => x.TypeID == 1)
                     select new
                     {
                       bv.BrandID,
                       BrandName = bv.Brand.Name,
                       bv.VendorID,
                       bv.Name,
                       BrandVendorLogo = bv.Brand.BrandMedias.Where(x => x.TypeID == logoType.TypeID).FirstOrDefault() != null ? bv.Brand.BrandMedias.Where(x => x.TypeID == logoType.TypeID).FirstOrDefault().MediaPath : string.Empty,
                       bv.VendorBrandCode
                     }));
      }
    }

    [RequiresAuthentication(Functionalities.Default)]
    public void FetchLogoUrl(string imagePath, decimal? width, decimal? height)
    {
      ImageFormat _format = ImageFormat.Jpeg;
      Response.ContentType = "image/jpeg";
      Color _backgroundColor = Color.White;

      string path = string.Empty;
      //
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
          Image img = Image.FromFile(path);
          int _width = width.HasValue ? (int)width : img.Width;
          int _height = height.HasValue ? (int)height : img.Height;
          var im = ImageUtility.GetFixedSizeImage(img, _width, _height, true, _backgroundColor);

          EncoderParameters eps = new EncoderParameters(1);
          eps.Param[0] = new EncoderParameter(Encoder.Quality, (long)100);

          im.Save(Response.OutputStream, _format);
        }
      }
    }
   
 [RequiresAuthentication(Functionalities.Default)]
    public ActionResult UploadLogo(int BrandID)
    {
      using (var unit = GetUnitOfWork())
      {
        var Brand = unit.Service<Brand>().Get(b => b.BrandID == BrandID);
        string fileName = "";

        foreach (string f in Request.Files)
        {
          var upload = Request.Files.Get(f);
          //var extension = upload.FileName.Substring(upload.FileName.LastIndexOf(".") + 1);
          //var fileName = BrandID + "_logo." + extension;
          fileName = ExtractFileName(upload.FileName);

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
            //copy it locally
            byte[] buff = new byte[st.Length];
            st.Read(buff, 0, (int)st.Length);
            System.IO.File.WriteAllBytes(fullPath, buff);

            //set the path           
          }

          BrandMedia bm = new BrandMedia()
          {
            TypeID = 1,
            BrandID = Brand.BrandID,
            Sequence = 0,
            MediaPath = targetPath
          };
          unit.Service<BrandMedia>().Create(bm);
        }
        unit.Save();
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

    [RequiresAuthentication(Functionalities.CreateBrandVendor)]
    public ActionResult Create()
    {
      return Create<BrandVendor>();
    }

    [RequiresAuthentication(Functionalities.UpdateBrandVendor)]
    public ActionResult Update(string BrandName, int _BrandID, string _VendorBrandCode, int _VendorID, int? Identification)
    {
      return Update<BrandVendor>((p => p.BrandID == _BrandID && p.VendorID == _VendorID && p.VendorBrandCode == _VendorBrandCode),
        (unit, brandVendor) =>
        {
          var service = unit.Service<BrandVendor>();

          service.Delete(brandVendor);

          BrandVendor bv = new BrandVendor()
          {
            BrandID = Identification.HasValue ? Identification.Value : int.Parse(Request["BrandID"]),
            VendorBrandCode = _VendorBrandCode,
            VendorID = _VendorID,
            Name = BrandName
          };

          service.Create(bv);
        }, false);
    }

    [RequiresAuthentication(Functionalities.DeleteBrandVendor)]
    public ActionResult Delete(int _brandID, int _vendorID, string _vendorBrandCode)
    {
      return Delete<BrandVendor>(c => c.BrandID == _brandID && c.VendorID == _vendorID && c.VendorBrandCode == _vendorBrandCode);
    }
  }
}
