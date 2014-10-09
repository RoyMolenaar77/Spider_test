using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Service;
using System.Drawing;
using System.Configuration;
using System.Net;
using System.IO;
using Concentrator.Objects;
using System.Drawing.Imaging;
using Concentrator.Objects.Media;

namespace Concentrator.Plugins.ImageDownloader
{
  public class FtpDownload : ConcentratorPlugin
  {

    public override string Name
    {
      get { return "Concentrator Image (FTP) Download Plugin"; }
    }

    protected override void Process()
    {
      using (ConcentratorDataContext ctx = new ConcentratorDataContext())
      {
        var imagelist = (from i in ctx.ProductImages
                         where i.ImagePath == null
                         orderby i.ProductID descending //newest products first
                         select i).ToList();

        string imageDir = "";

        try
        {
          int couterProduct = 0;
          int logCount = 0;
          int totalProducts = imagelist.Count;
          log.DebugFormat("Found {0} images to process", totalProducts);
          string imageDirectory = GetConfiguration().AppSettings.Settings["FTPImageDirectory"].Value;
          imageDir = imageDirectory;
          foreach (var image in imagelist)
          {
            couterProduct++;
            logCount++;
            if (logCount == 50)
            {
              log.DebugFormat("Images Processed : {0}/{1}", couterProduct, totalProducts);
              logCount = 0;
            }
            string fileName = string.Format("{0}_{1}_{2}_{3}", image.ImageID, image.ProductID, image.Sequence, image.VendorID);

            string imagePath = GetImage(image.ImageUrl, fileName, "Products", imageDirectory);
            if (!string.IsNullOrEmpty(imagePath))
            {
              image.ImagePath = imagePath;
              ctx.SubmitChanges();
            }
          }
          log.Debug("Finish download image process");
        }
        catch (Exception ex)
        {
          log.Error("Error processing images", ex);
        }

//        log.InfoFormat("Start Cleanup image directory");
//#region Cleanup
//        imagelist = (from i in ctx.ProductImages
//                         where i.ImagePath != null
//                         orderby i.ProductID descending //newest products first
//                         select i).ToList();

//        try
//        {
//          int couterProduct = 0;
//          int logCount = 0;

//         string imageDirectory = GetConfiguration().AppSettings.Settings["FTPImageDirectory"].Value;

//         var files = Directory.GetFiles(Path.Combine(imageDirectory, "Products"));
//                      int totalProducts = files.Count();
//          log.DebugFormat("Found {0} images to check", totalProducts);


//          foreach(var file in files)
//          {
//            couterProduct++;
//            logCount++;
//            if (logCount == 50)
//            {
//              log.DebugFormat("Images Processed : {0}/{1}", couterProduct, totalProducts);
//              logCount = 0;
//            }
//            if (!imagelist.Any(x => Path.Combine(imageDirectory, x.ImagePath) == file))
//            {
//              try
//              {
//                log.DebugFormat("Try to delete {0}", file);
//                File.Delete(file);
//              }
//              catch (Exception ex)
//              {
//                log.ErrorFormat("Delete file {0} failed: {1}", file, ex.InnerException);
//              }
//            }
//          }
//          log.Debug("Finish download image process");
//        }
//        catch (Exception ex)
//        {
//          log.Error("Error processing images", ex);
//        }
//#endregion
//        log.Info("Finish cleanup images");

        log.Info("Start import brand images");
        #region Brand image
        var brandImages = (from i in ctx.BrandVendors
                           where i.Logo != null
                           && i.BrandID > 0
                           select i).ToList();

        try
        {
          int couterProduct = 0;
          int logCount = 0;
          int totalProducts = brandImages.Count;
          log.DebugFormat("Found {0} brand images to process", totalProducts);
          string imageDirectory = GetConfiguration().AppSettings.Settings["FTPImageDirectory"].Value;
          foreach (var image in brandImages)
          {
            couterProduct++;
            logCount++;
            if (logCount == 50)
            {
              log.DebugFormat("Brand images Processed : {0}/{1}", couterProduct, totalProducts);
              logCount = 0;
            }
            string fileName = string.Format("{0}_{1}_{2}", image.BrandID, image.Name, image.VendorID);

            string imagePath = GetImage(image.Logo, fileName, "Brand", imageDirectory);
            if (!string.IsNullOrEmpty(imagePath))
            {
              image.Brand.ImagePath = imagePath;
              ctx.SubmitChanges();
            }
          }
          log.Debug("Finish download image process");
        }
        catch (Exception ex)
        {
          log.Error("Error processing images", ex);
        }
        #endregion
        log.Info("Finish import brand images");

        //log.InfoFormat("Start Cleanup brand image directory");
        //#region Cleanup brands

        //var exBrandImages = (from i in ctx.Brands
        //               where i.ImagePath != null
        //               && i.BrandID > 0
        //               select i).ToList();
        //try
        //{
        //  int couterProduct = 0;
        //  int logCount = 0;

        //  string imageDirectory = GetConfiguration().AppSettings.Settings["FTPImageDirectory"].Value;

        //  var files = Directory.GetFiles(Path.Combine(imageDirectory, "Brand"));
        //  int totalProducts = files.Count();
        //  log.DebugFormat("Found {0} images to check", totalProducts);


        //  foreach (var file in files)
        //  {
        //    couterProduct++;
        //    logCount++;
        //    if (logCount == 50)
        //    {
        //      log.DebugFormat("Images Processed : {0}/{1}", couterProduct, totalProducts);
        //      logCount = 0;
        //    }
        //    if (!exBrandImages.Any(x => Path.Combine(imageDirectory, x.ImagePath) == file))
        //    {
        //      try
        //      {
        //        log.DebugFormat("Try to delete {0}", file);
        //        File.Delete(file);
        //      }
        //      catch (Exception ex)
        //      {
        //        log.ErrorFormat("Delete file {0} failed: {1}", file, ex.InnerException);
        //      }
        //    }
        //  }
        //  log.Debug("Finish download image process");
        //}
        //catch (Exception ex)
        //{
        //  log.Error("Error processing images", ex);
        //}
        //#endregion
        //log.Info("Finish cleanup brand images");

        log.InfoFormat("Start Copy to Media");
        imagelist = (from i in ctx.ProductImages
                     where i.ImagePath != null
                     orderby i.ProductID descending //newest products first
                     select i).ToList();

        var mediaImages = (from m in ctx.ProductMedia
                           where m.TypeID == 1
                           select m).ToList();

        try
        {
          int couterProduct = 0;
          int logCount = 0;
          int totalProducts = imagelist.Count;
          log.DebugFormat("Found {0} images to copy", totalProducts);
          foreach (var image in imagelist)
          {
            couterProduct++;
            logCount++;
            if (logCount == 50)
            {
              log.DebugFormat("Images Processed : {0}/{1}", couterProduct, totalProducts);
              logCount = 0;
            }

            var m = mediaImages.Where(x => x.VendorID == image.VendorID
               && x.Sequence == image.Sequence
               && x.ProductID == image.ProductID
               && x.MediaUrl == image.ImageUrl
               && x.MediaPath == image.ImagePath).FirstOrDefault();

            if (m != null)
            {
              mediaImages.Remove(m);
            }
            else
            {
              m = new ProductMedia()
              {
                MediaPath = image.ImagePath,
                Resolution = getRes(Path.Combine(imageDir, image.ImagePath)),
                Size = getSize(Path.Combine(imageDir, image.ImagePath)).HasValue ? getSize(Path.Combine(imageDir, image.ImagePath)) : null,
                MediaUrl = image.ImageUrl,
                ProductID = image.ProductID,
                Sequence = image.Sequence,
                TypeID = 1,
                VendorID = image.VendorID
              };
              ctx.ProductMedia.InsertOnSubmit(m);
              ctx.SubmitChanges();
            }

          }

          ctx.ProductMedia.DeleteAllOnSubmit(mediaImages);
          ctx.SubmitChanges();
        }
        catch (Exception ex)
        {
          log.Error("Error processing images", ex);
        }


        log.Info("Finish copy to media");
      }
    }

    private int? getSize(string imagePath)
    {
      if (File.Exists(imagePath))
      {
        return (int)Math.Round(new FileInfo(imagePath).Length / 1024d, 0);
      }
      return null;
    }

    private string getRes(string imagePath)
    {
      if (File.Exists(imagePath))
      {
        Image image = Image.FromFile(imagePath);
        string res = string.Format("{0}x{1}", image.Width, image.Height);
        return res;
      }
      return null;

    }

    private string GetImage(string url, string name, string directory, string imageDirectory)
    {
      Image _productImage;
      string file = string.Empty;

      name += ".jpg";

      string path = Path.Combine(imageDirectory, directory);
      file = Path.Combine(path, name);

      if (!File.Exists(file))
      {
        try
        {
          byte[] b;
          HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(url);
          WebResponse myResp = myReq.GetResponse();

          using (Stream stream = myResp.GetResponseStream())
          {

            using (BinaryReader br = new BinaryReader(stream))
            {
              b = br.ReadBytes((int)myResp.ContentLength);
              br.Close();
            }
            myResp.Close();

            using (MemoryStream ms = new MemoryStream(b))
            {
              try
              {
                using (Image originalImage = Image.FromStream(ms, true, true))
                {
                  Image i = Image.FromStream(ms);
                  Bitmap bit = new Bitmap(new Bitmap(i));

                  //_productImage = new Bitmap(originalImage.Width, originalImage.Height);

                  // bit.Save(url);
                  if (!Directory.Exists(path))
                  {
                    Directory.CreateDirectory(path);
                  }

                  i.Save(file, ImageFormat.Jpeg);

                }
              }
              catch (ArgumentException)
              {
                _productImage = new Bitmap(1, 1);
              }
            }
          }
        }
        catch (Exception ex)
        {
          //log.Error("Image not availible", ex);
          return string.Empty;
        }
      }
      return Path.Combine(directory, name);
    }
  }
}
