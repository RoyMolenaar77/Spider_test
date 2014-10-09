using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Services.Base;
using Concentrator.Objects.Services.ServiceInterfaces;
using Concentrator.Objects.Services.DTO;
using Concentrator.Objects.Models.Media;
using Concentrator.Objects.Web;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Configuration;
using System.Net;
using Concentrator.Objects.Utility;
using Concentrator.Objects.Images;
using Concentrator.Objects.Drawing;

namespace Concentrator.Objects.Services
{
  public class MediaService : Service<ProductMedia>, IMediaService
  {
    public ImageResultDto GetImageForProduct(int productID, decimal? height = null, decimal? width = null, int sequence = 0, int? connectorID = null, string defaultImagePath = "")
    {
      return GetImage(GetImageViewObject(productID, connectorID), height, width, defaultImagePath);
    }

    private ImageView GetImageViewObject(int productID, int? connectorID = null, int? sequence = null)
    {
      if (!connectorID.HasValue) connectorID = Client.User.ConnectorID;

      //TO DO: use the green sentence
      //var images = Repository<ImageView>().GetAll(c => (c.ProductID == productID) && c.ConnectorID == connectorID).ToList();
      var images = Repository<ImageView>().GetAll(c => c.ProductID == productID).ToList();

      if (images.Count() == 0) return null;

      int? minSequence = null;

      if (sequence.HasValue) minSequence = sequence.Value;
      else
      {
        minSequence = images.Min(c => c.Sequence);
      }
      return images.FirstOrDefault(c => c.Sequence == minSequence);
    }

    private ImageView GetImageViewObjectSequence(int productID, int sequence, int? connectorID = null)
    {
      if (!connectorID.HasValue) connectorID = Client.User.ConnectorID;

      var images = Repository<ImageView>().GetAll(c => (c.ProductID == productID) && c.ConnectorID == connectorID).ToList();

      if (images.Count() == 0) return null;

      return images.FirstOrDefault(c => c.Sequence == sequence);
    }



    private ImageResultDto UnknownImage(string defaultImagePath, decimal? width = null, decimal? height = null)
    {
      using (var s = File.OpenRead(defaultImagePath))
      {
        Image img = Image.FromStream(s, true);

        var im = ImageUtility.ResizeImage(img, width, height);

        return new ImageResultDto()
        {
          Extension = "png",
          Format = ImageFormat.Png,
          Image = im
        };
      }
    }

    private ImageResultDto GetImage(ImageView imageObj, decimal? height, decimal? width, string defaultImagePath)
    {
      string path = imageObj.ImagePath;
      string url = imageObj.ImageUrl;

      if (string.IsNullOrEmpty(path) && string.IsNullOrEmpty(url))
        return UnknownImage(defaultImagePath, width, height); //unknown image path

      Stream imageStream = null;

      //load image from path
      if (!string.IsNullOrEmpty(path))
      {
        try
        {
          path = Path.Combine(ConfigurationManager.AppSettings["FTPMediaDirectory"], path);
          imageStream = File.OpenRead(path);
        }
        catch (FileNotFoundException)
        {
          return UnknownImage(defaultImagePath, width, height); //file not found
        }
        catch (Exception e)
        {
          throw new Exception("Something went wrong while loading image : " + e.Message);
        }
      }
      else //web path
      {
        WebRequest request = WebRequest.Create(url);
        imageStream = request.GetResponse().GetResponseStream();
      }

      Image img = Image.FromStream(imageStream, true);
      imageStream.Close();
      imageStream.Dispose();

      var im = ImageUtility.ResizeImage(img, width, height);

      var ext = Path.GetExtension(path).Replace(".", "");
      return new ImageResultDto()
      {
        Image = im,
        Extension = ext,
        Format = (ImageFormat)typeof(ImageFormat).GetProperties().FirstOrDefault(c => c.Name.ToLower() == ext.ToLower()).GetValue(null, null)
      };
    }

    public string GetImagePath(int productID, int? connectorID = null, int sequence = 0)
    {
      ImageView imageObj;
      if (sequence == 0)
        imageObj = GetImageViewObject(productID, connectorID);
      else
        imageObj = GetImageViewObjectSequence(productID, sequence, connectorID);

      if (imageObj == null) return null;

      if (!string.IsNullOrEmpty(imageObj.ImagePath))
        return Path.Combine(ConfigurationManager.AppSettings["FTPMediaDirectory"], imageObj.ImagePath);

      return string.Empty;
    }

    public ImageView[] GetImagePaths(int productID, int connectorID)
    {
      return Repository<ImageView>().GetAll(c => c.ConnectorID == connectorID && c.ProductID == productID).OrderBy(c => c.Sequence).ToArray();
    }

    public Dictionary<int, ImageView[]> GetImagePathsCombined(int[] productID, int connectorID)
    {
      Dictionary<int, ImageView[]> results = new Dictionary<int, ImageView[]>();
      var imageviews = Repository<ImageView>().GetAll(c => c.ConnectorID == connectorID && productID.Contains(c.ProductID)).OrderBy(c => c.Sequence).ToList();
      foreach (int prodid in productID)
      {
        results.Add(prodid, imageviews.Where(c => c.ProductID == prodid).ToArray());
      }
      return results;
    }
  }
}
