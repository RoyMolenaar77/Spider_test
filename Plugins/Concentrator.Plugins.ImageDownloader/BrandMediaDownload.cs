using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.ConcentratorService;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects;
using System.IO;
using System.Drawing;
using System.Net;
using System.Drawing.Imaging;
using Concentrator.Objects.Models.Brands;

namespace Concentrator.Plugins.ImageDownloader
{
  public class BrandMediaDownload : ConcentratorPlugin
  {
    public override string Name
    {
      get { return "Concentrator Brand Media Download Plugin"; }
    }

    protected override void Process()
    {
      using (var unit = GetUnitOfWork())
      {
        log.InfoFormat("Start Cleanup image directory");

        bool networkDrive = false;
        string drive = GetConfiguration().AppSettings.Settings["FTPImageDirectory"].Value;
        bool.TryParse(GetConfiguration().AppSettings.Settings["IsNetworkDrive"].Value, out networkDrive);

        if (networkDrive)
        {
          NetworkDrive oNetDrive = new NetworkDrive();
          try
          {
            drive = "Z:";
            drive = Path.Combine(drive, "Concentrator");
            oNetDrive.LocalDrive = "Z:";
            oNetDrive.ShareName = drive;
            oNetDrive.MapDrive();
          }
          catch (Exception err)
          {
            log.Error("Invalid network drive", err);
          }
          oNetDrive = null;
        }

        var _mediaRepo = unit.Scope.Repository<BrandMedia>();
        var mediaList = _mediaRepo.GetAll(i => i.MediaPath == null && i.TypeID == 1 && i.MediaUrl != String.Empty).OrderBy(c => c.MediaID).ToList();

        try
        {
          int couterProduct = 0;
          int logCount = 0;
          int totalProducts = mediaList.Count;
          string mediaDirectory = drive;

          log.DebugFormat("Found {0} media items to process", totalProducts);

          foreach (var mediaItem in mediaList)
          {
            couterProduct++;
            logCount++;
            if (logCount == 50)
            {
              log.DebugFormat("Media Processed : {0}/{1}", couterProduct, totalProducts);
              logCount = 0;
            }
            
            //var productID = mediaItem.Brand.Products.Select(x => x.ProductID).FirstOrDefault();
            //var vendorID = mediaItem.Brand.BrandVendors.Select(x => x.VendorID).FirstOrDefault();            
            //string fileName = string.Format("{0}_{1}_{2}_{3}", mediaItem.MediaID, mediaItem.ProductID, mediaItem.Sequence, mediaItem.VendorID);
            string fileName = string.Format("{0}_{1}_{2}_{3}", mediaItem.MediaID, mediaItem.BrandID, mediaItem.Sequence);

            string imagePath = GetImage(mediaItem.MediaUrl, fileName, "Brands", mediaDirectory);

            if (!string.IsNullOrEmpty(imagePath))
            {
              mediaItem.MediaPath = imagePath;
              mediaItem.Resolution = getRes(Path.Combine(mediaDirectory, imagePath));
              mediaItem.Size = getSize(Path.Combine(mediaDirectory, imagePath)).HasValue ? getSize(Path.Combine(mediaDirectory, imagePath)) : null;
              unit.Save();
            }
          }
          log.Debug("Finish download image process");


        }
        catch (Exception ex)
        {
          log.Error("Error processing images", ex);
        }

        if (networkDrive)
        {
          NetworkDrive oNetDrive = new NetworkDrive();
          try
          {
            oNetDrive.LocalDrive = drive;
            oNetDrive.UnMapDrive();
          }
          catch (Exception err)
          {
            log.Error("Error unmap drive" + err.InnerException);
          }
          oNetDrive = null;
        }
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
        FileStream fs = null;
        try
        {         
          using (fs = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
          {
            using (var image = Image.FromStream(fs))
            {
              string res = string.Format("{0}x{1}", image.Width, image.Height);
              return res;
            }
          }
        }
        catch (Exception ex)
        {
          log.DebugFormat("Failed get image format for {0}", imagePath);
        }
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

                  if (!Directory.Exists(path))
                  {
                    Directory.CreateDirectory(path);
                  }

                  i.Save(file, ImageFormat.Jpeg);

                }
              }
              catch (ArgumentException ex)
              {
                _productImage = new Bitmap(1, 1);
                log.Error("Error downloading image", ex);
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
