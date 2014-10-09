using System;
using System.Linq;
using Concentrator.Objects.ConcentratorService;
using System.Drawing;
using System.Net;
using System.IO;
using Concentrator.Objects;
using System.Drawing.Imaging;
using Concentrator.Objects.Models.Products;

namespace Concentrator.Plugins.ImageDownloader
{
  public class MediaDownload : ConcentratorPlugin
  {

    public override string Name
    {
      get { return "Concentrator Media Download Plugin"; }
    }

    protected override void Process()
    {
      using (var unit = GetUnitOfWork())
      {
        log.InfoFormat("Start Cleanup image directory");

        string drive = GetConfiguration().AppSettings.Settings["FTPImageDirectory"].Value;
        string mediaDirectory = drive;
        
        #region Cleanup
        var _mediaRepo = unit.Scope.Repository<ProductMedia>();

        //var imagelist = _mediaRepo.GetAll(i => i.MediaPath != null).OrderBy(c => c.ProductID).ToList();
        //try
        //{
        //  int couterProduct = 0;
        //  int logCount = 0;
        //  string imageDirectory = drive;//GetConfiguration().AppSettings.Settings["FTPImageDirectory"].Value;

        //  imageDirectory = Path.Combine(imageDirectory, "Concentrator");
        //  var files = Directory.GetFiles(Path.Combine(imageDirectory, "Products"));
        //  int totalProducts = files.Count();
        //  log.DebugFormat("Found {0} images to check", totalProducts);

        //  if (totalProducts != imagelist.Count)
        //  {
        //    foreach (var file in files)
        //    {
        //      couterProduct++;
        //      logCount++;
        //      if (logCount == 50)
        //      {
        //        log.DebugFormat("Media files Processed : {0}/{1}", couterProduct, totalProducts);
        //        logCount = 0;
        //      }
        //      if (!imagelist.Any(x => Path.Combine(imageDirectory, x.MediaPath) == file))
        //      {
        //        try
        //        {
        //          log.DebugFormat("Try to delete {0}", file);
        //          File.Delete(file);
        //        }
        //        catch (Exception ex)
        //        {
        //          log.ErrorFormat("Delete file {0} failed: {1}", file, ex.InnerException);
        //        }
        //      }
        //    }
        //  }
        //  log.Debug("Finish download image process");
        //}
        //catch (Exception ex)
        //{
        //  log.Error("Error processing images", ex);
        //}
        #endregion
        log.Info("Finish cleanup images");

        var mediaList = _mediaRepo.GetAll(i => i.MediaPath == null && i.MediaUrl != "").OrderBy(c => c.ProductID).ToList();
       
          int couterProduct = 0;
          int logCount = 0;
          int totalProducts = mediaList.Count;
          log.DebugFormat("Found {0} media items to process", totalProducts);
        try
        {  
          foreach (var mediaItem in mediaList)
          {
            couterProduct++;
            logCount++;
            if (logCount == 50)
            {
              log.DebugFormat("Media Processed : {0}/{1}", couterProduct, totalProducts);
              logCount = 0;
            }
            string fileName = string.Format("{0}_{1}_{2}_{3}", mediaItem.MediaID, mediaItem.ProductID, mediaItem.Sequence, mediaItem.VendorID);

            string imagePath = string.Empty;
            try
            {
              imagePath = GetImage(mediaItem.MediaUrl, fileName, "Products", mediaDirectory);
            }
            catch
            {
              imagePath = GetFile(mediaItem.MediaUrl, fileName, "Products", mediaDirectory);
            }

            if (!string.IsNullOrEmpty(imagePath))
            {
              mediaItem.MediaPath = imagePath;
              mediaItem.Resolution = MediaUtility.getRes(Path.Combine(mediaDirectory, imagePath));
              mediaItem.Size = MediaUtility.getSize(Path.Combine(mediaDirectory, imagePath)).HasValue ? MediaUtility.getSize(Path.Combine(mediaDirectory, imagePath)) : null;
              unit.Save();
            }
          }
          log.Debug("Finish download image process");
        }
        catch (Exception ex)
        {
          log.Error("Error processing images", ex);
        }

        log.Debug("Start set media information");
        mediaList = _mediaRepo.GetAll(i => i.MediaPath != null && (i.Resolution == null || i.Size == null)).OrderBy(c => c.ProductID).ToList();
        couterProduct = 0;
        logCount = 0;
        totalProducts = mediaList.Count;
        log.DebugFormat("Found {0} media items to set information", totalProducts);

        mediaList.ForEach(media =>
        {
          couterProduct++;
          logCount++;
          if (logCount == 50)
          {
            log.DebugFormat("Media information Processed : {0}/{1}", couterProduct, totalProducts);
            logCount = 0;
          }

          media.Resolution = MediaUtility.getRes(Path.Combine(mediaDirectory, media.MediaPath));
          media.Size = MediaUtility.getSize(Path.Combine(mediaDirectory, media.MediaPath)).HasValue ? MediaUtility.getSize(Path.Combine(mediaDirectory, media.MediaPath)) : null;
          unit.Save();
        });
        log.Debug("Finish set media information");
      }
    }

    private string GetImage(string url, string name, string directory, string imageDirectory)
    {
      Image _productImage;
      string file = string.Empty;
      
      string path = Path.Combine(imageDirectory, directory);
      
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
              if (!Directory.Exists(path))
              {
                Directory.CreateDirectory(path);
              }

              try
              {
                using (Image originalImage = Image.FromStream(ms, true, true))
                {
                  Image i = Image.FromStream(ms);
                  Bitmap bit = new Bitmap(new Bitmap(i));

                  name += ".jpg";
                  file = Path.Combine(path, name);

                  //_productImage = new Bitmap(originalImage.Width, originalImage.Height);
                  // bit.Save(url);

                  i.Save(file, ImageFormat.Jpeg);
                }
              }
              catch (ArgumentException ex)
              {

                try
                {
                  name += url.Substring(url.LastIndexOf('.'));
                  file = Path.Combine(path, name);

                  using (var wc = new System.Net.WebClient())
                  {
                    wc.DownloadFile(url, file);
                  }
                }
                catch (Exception excep)
                {

                  _productImage = new Bitmap(1, 1);
                  log.Error("Error downloading image", excep);
                }
              }
            }
          }
        }
        catch (Exception ex)
        {
          log.Error("Image not availible " + name + " url " + url, ex);
          return string.Empty;
        }
      }
      return Path.Combine(directory, name);
    }


    private string GetFile(string url, string name, string directory, string mediaDirectory)
    {
      string file = string.Empty;

      name += Path.GetExtension(url);

      string path = Path.Combine(mediaDirectory, directory);
      file = Path.Combine(path, name);

      if (!Directory.Exists(path))
      {
        Directory.CreateDirectory(path);
      }

      if (!File.Exists(file))
      {
        try
        {
          HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(url);
          WebResponse myResp = myReq.GetResponse();

          using (Stream stream = myResp.GetResponseStream())
          {
            using (StreamReader receiveStreamReader = new StreamReader(stream))
            {
              using (StreamWriter ms = new StreamWriter(file))
              {
                try
                {
                  ms.Write(receiveStreamReader.ReadToEnd());
                }
                catch (ArgumentException)
                {
                  log.AuditError(string.Format("Faile to download file {0}", url));
                }
              }
            }
          }
        }
        catch
        {
          //log.Error("Image not availible", ex);
          return string.Empty;
        }
      }
      return Path.Combine(directory, name);
    }
  }
}
