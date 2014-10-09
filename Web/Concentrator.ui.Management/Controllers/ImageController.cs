using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Concentrator.Objects.Web;
using System.Drawing.Imaging;
using System.Drawing;
using System.Configuration;
using System.IO;
using System.Net;
using Concentrator.Objects.Images;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Web.Models;
using Concentrator.Objects.Services.ServiceInterfaces;
using Concentrator.Objects.Models.Contents;
using Concentrator.Objects.Models.Products;
using Concentrator.ui.Management.Models.Anychart;
using Concentrator.Web.Shared.Controllers;
using Concentrator.Web.Shared;

namespace Concentrator.ui.Management.Controllers
{
  public class ImageController : BaseController
  {
    [RequiresAuthentication(Functionalities.GetImage)]
    public void Fetch(string imagePath, decimal? width, decimal? height, bool? headerImage, string imageUrl)
    {
      Stream str = null;
      Color _backgroundColor = Color.White;
      ImageFormat _format = ImageFormat.Jpeg;
      Response.ContentType = "image/jpeg";

      bool isImage = true;
      var headImage = headerImage.HasValue ? headerImage.Value : false;

      try
      {
        string path = string.Empty;

        if (imagePath != null)
        {
          string extension = imagePath.Substring(imagePath.LastIndexOf('.') + 1).ToLower();
          switch (extension.ToLower())
          {
            case "jpg":
              _format = ImageFormat.Jpeg;
              Response.ContentType = "image/jpeg";
              break;
            case "png":
            case "tif":
            case "tiff":
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
              isImage = false;
              _format = ImageFormat.Png;
              Response.ContentType = "image/png";
              _backgroundColor = Color.Transparent;
              break;

          }

          //if its an image
          if (extension == "jpg" | extension == "png" | extension == "gif" | extension == "tif" | extension == "tiff" | headImage == true)
          {
            if (headImage && isImage == false)
            {
              //request header image, but theres no image, set default image
              path = Request.PhysicalApplicationPath + @"Content\images\Icons\FileTypeIcons\" + extension.ToLower() + ".png";
            }
            else
            {
              path = Path.Combine(ConfigurationManager.AppSettings["FTPMediaDirectory"], imagePath);
            }
          }
          else
          {
            path = Request.PhysicalApplicationPath + @"Content\images\Icons\FileTypeIcons\" + extension.ToLower() + ".png";
            if (!System.IO.File.Exists(path))
            {
              //if we dont have an image for this type, show default image
              path = Request.PhysicalApplicationPath + @"Content\images\Icons\FileTypeIcons\unknown.png";
            }
          }
        }
        else
        {
          if (!string.IsNullOrEmpty(imageUrl) && imageUrl.Contains("youtube.com"))
          {
            path = Request.PhysicalApplicationPath + @"Content\images\Icons\FileTypeIcons\youtube.png";
          }
          else
          {
            path = Request.PhysicalApplicationPath + @"Content\images\Icons\FileTypeIcons\www.png";
          }
        }

        if (!System.IO.File.Exists(path))
          path = Request.PhysicalApplicationPath + @"Content\images\Icons\FileTypeIcons\unknown.png";

        if (!string.IsNullOrEmpty(imageUrl) && imagePath == null)
        {
          WebRequest request = WebRequest.Create(imageUrl);
          try
          {

            request.GetResponse().GetResponseStream();
            str = request.GetResponse().GetResponseStream();

          }
          catch
          {

            imageUrl = path;

            WebRequest newRequest = WebRequest.Create(imageUrl);
            str = newRequest.GetResponse().GetResponseStream();

          }

        }
        else
        {
          str = System.IO.File.OpenRead(path);
        }

        Image img = Image.FromStream(str, true);
        str.Close();
        str.Dispose();
        int _width = width.HasValue ? (int)width : img.Width;
        int _height = height.HasValue ? (int)height : img.Height;
        var im = ImageUtility.GetFixedSizeImage(img, _width, _height, true, _backgroundColor);

        EncoderParameters eps = new EncoderParameters(1);
        eps.Param[0] = new EncoderParameter(Encoder.Quality, (long)100);

        im.Save(Response.OutputStream, _format);
      }
      catch
      {
        Response.ContentType = "image/png";
        _backgroundColor = Color.Transparent;
        var path = Request.PhysicalApplicationPath + @"Content\images\Icons\FileTypeIcons\unknown.png";
        Image img = Image.FromFile(path, true);
        int _width = width.HasValue ? (int)width : img.Width;
        int _height = height.HasValue ? (int)height : img.Height;
        var im = ImageUtility.GetFixedSizeImage(img, _width, _height, true, _backgroundColor);

        EncoderParameters eps = new EncoderParameters(1);
        eps.Param[0] = new EncoderParameter(Encoder.Quality, (long)100);

        im.Save(Response.OutputStream, _format);
      }
      finally
      {
        str.Close();
        str.Dispose();
      }
    }

    [RequiresAuthentication(Functionalities.UpdateImage)]
    public void ResizeFromUrl(string url, decimal? width, decimal? height)
    {
      ImageFormat _format = ImageFormat.Jpeg;
      Response.ContentType = "image/jpeg";
      Color _backgroundColor = Color.White;
      bool isImage = true;

      string extension = url.Substring(url.LastIndexOf('.') + 1);
      switch (extension.ToLower())
      {
        case "jpg":
        case "jpeg":
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
          isImage = false;
          _format = ImageFormat.Png;
          Response.ContentType = "image/png";
          _backgroundColor = Color.Transparent;
          break;

      }

      Image img = null;
      if (isImage)
      {
        try
        {
          // Open a connection
          System.Net.HttpWebRequest _HttpWebRequest = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(url);

          //_HttpWebRequest.AllowWriteStreamBuffering = true;

          //_HttpWebRequest.Referer = "http://www.google.com/";

          // set timeout for 20 seconds (Optional)
          //_HttpWebRequest.Timeout = 20000;

          // Request response:
          using (System.Net.WebResponse _WebResponse = _HttpWebRequest.GetResponse())
          {
            // Open data stream:
            using (System.IO.Stream _WebStream = _WebResponse.GetResponseStream())
            {
              // convert webstream to image
              img = Image.FromStream(_WebStream);
            }
          }

        }
        catch (Exception _Exception)
        {
          // Error
          Console.WriteLine("Exception caught in process: {0}", _Exception.ToString());

        }
      }

      if (img != null)
      {
        int _width = width.HasValue ? (int)width : img.Width;
        int _height = height.HasValue ? (int)height : img.Height;
        var im = ImageUtility.GetFixedSizeImage(img, _width, _height, true, _backgroundColor);

        EncoderParameters eps = new EncoderParameters(1);
        eps.Param[0] = new EncoderParameter(Encoder.Quality, (long)100);

        im.Save(Response.OutputStream, _format);
      }
    }

    [RequiresAuthentication(Functionalities.GetImage)]
    public ActionResult GetSizeGroups(ContentPortalFilter filter, bool? mb, bool? kb)
    {

      MergeSession(filter, ContentPortalFilter.SessionKey);

      var sizeFilter = GetNumericFilter();
      if (!sizeFilter.BiggerThan.HasValue && !sizeFilter.SmallerThan.HasValue && !sizeFilter.EqualTo.HasValue) sizeFilter.BiggerThan = 600;

      var isMb = mb.HasValue && mb.Value;
      var unitOfData = isMb ? "megabytes" : "kilobytes";

      using (var unit = GetUnitOfWork())
      {
        var missingContentResult = ((IContentService)unit.Service<Content>()).GetMissing(filter.Connectors, filter.Vendors, filter.BeforeDate, filter.AfterDate, filter.OnDate, filter.IsActive, filter.ProductGroups, filter.Brands, filter.LowerStockCount, filter.GreaterStockCount, filter.EqualStockCount, filter.Statuses);

        var content = missingContentResult.Value;
        var sizes = unit.Service<ProductMedia>().GetAll(c => content.Any(m => m.ConcentratorProductID == c.ProductID) && c.MediaType.Type == "Image").Select(c => c.Size);

        var validGroup = 0;
        var invalidGroup = 0;
        var noSize = 0;
        var validGroupLabel = string.Empty;
        var invalidGroupLabel = string.Empty;
        var validSize = new ImageSize();
        var invalidSize = new ImageSize();

        if (sizeFilter.BiggerThan.HasValue && sizeFilter.SmallerThan.HasValue)
        {
          validSize.minImageSize = sizeFilter.BiggerThan.Value;
          validSize.maxImageSize = sizeFilter.SmallerThan.Value;
          invalidSize.minImageSize = sizeFilter.SmallerThan.Value + 1;
          invalidSize.maxImageSize = int.MaxValue;
        }
        else if (sizeFilter.BiggerThan.HasValue)
        {
          validSize.minImageSize = sizeFilter.BiggerThan.Value;
          validSize.maxImageSize = int.MaxValue;
          invalidSize.minImageSize = 0;
          invalidSize.maxImageSize = sizeFilter.BiggerThan.Value - 1;
        }

        else if (sizeFilter.SmallerThan.HasValue)
        {
          validSize.minImageSize = 0;
          validSize.maxImageSize = sizeFilter.SmallerThan.Value;
          invalidSize.minImageSize = sizeFilter.SmallerThan.Value + 1;
          invalidSize.maxImageSize = int.MaxValue;
        }
        else if (sizeFilter.EqualTo.HasValue)
        {
          validSize.minImageSize = sizeFilter.EqualTo.Value;
          validSize.maxImageSize = sizeFilter.EqualTo.Value;
          invalidSize.minImageSize = sizeFilter.EqualTo.Value + 1;
          invalidSize.maxImageSize = int.MaxValue;
        }

        sizes.ForEach((size, id) =>
        {
          if (!size.HasValue) noSize++;
          else
          {
            if (isMb) size = size / 1024;

            //range
            if (sizeFilter.BiggerThan.HasValue && sizeFilter.SmallerThan.HasValue)
            {
              validGroupLabel = string.Format("Smaller than {0} {1} and bigger than {2} {3}", sizeFilter.SmallerThan, unitOfData, sizeFilter.BiggerThan, unitOfData);
              invalidGroupLabel = string.Format("Out of specified range");

              if (size > sizeFilter.BiggerThan.Value && size < sizeFilter.SmallerThan) validGroup++;
              else invalidGroup++;

            }
            else if (sizeFilter.BiggerThan.HasValue)
            {
              validGroupLabel = string.Format("Bigger than {0} {1}", sizeFilter.BiggerThan, unitOfData);
              invalidGroupLabel = string.Format("Smaller than {0} {1}", sizeFilter.BiggerThan, unitOfData);
              if (size > sizeFilter.BiggerThan.Value) validGroup++;
              else invalidGroup++;
            }
            else if (sizeFilter.SmallerThan.HasValue)
            {
              invalidGroupLabel = string.Format("Bigger than {0} {1}", sizeFilter.SmallerThan, unitOfData);
              validGroupLabel = string.Format("Smaller than {0} {1}", sizeFilter.SmallerThan, unitOfData);
              if (size < sizeFilter.SmallerThan.Value) validGroup++;
              else invalidGroup++;
            }
            else if (sizeFilter.EqualTo.HasValue)
            {
              validGroupLabel = string.Format("Equal to {0} {1}", sizeFilter.EqualTo, unitOfData);
              invalidGroupLabel = string.Format("Not Equal to {0} {1}", sizeFilter.EqualTo, unitOfData);
              if (size == sizeFilter.EqualTo.Value) validGroup++;
              else invalidGroup++;
            }
          }
        });

        var callbackUI = "content-item";

        var serie = new Serie(new List<Concentrator.ui.Management.Models.Anychart.Point>(){
          new PieChartPoint(validGroupLabel, validGroup, action: new AnychartAction(callbackUI, new { minImageSize = validSize.minImageSize, maxImageSize= validSize.maxImageSize})),
          
          new PieChartPoint(invalidGroupLabel, invalidGroup, action: new AnychartAction(callbackUI, new {minImageSize = invalidSize.minImageSize, maxImageSize = invalidSize.maxImageSize})),
          
          new PieChartPoint("No size set", noSize, action: new AnychartAction(callbackUI, new {hasImage = false}))
        }, "Size", "Default");
        var model = new AnychartComponentModel(new List<Serie>() { serie });

        return View("Anychart/DefaultPieChart", model);
      }
    }

    private struct ImageSize
    {
      public int minImageSize;
      public int maxImageSize;
    }
  }
}
