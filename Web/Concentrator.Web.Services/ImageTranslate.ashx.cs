using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Drawing;
using Concentrator.Web.Services.Base;
using Concentrator.Objects.Models.Attributes;
using System.Configuration;
using System.IO;
using Concentrator.Objects.Images;
using System.Net;
using Concentrator.Objects.Converters;
using Concentrator.Objects;

namespace Concentrator.Web.Services
{
  /// <summary>
  /// Summary description for ImageHandler
  /// </summary>
  public class ImageTranslate : BaseConcentratorService, IHttpHandler
  {

    public void ProcessRequest(HttpContext context)
    {
      context.Response.ContentType = "image/png";

      bool networkDrive = false;
      string drive = ConfigurationManager.AppSettings["FTPMediaDirectory"].ToString();
      //string drive = @"\\SOL\Company_Shares\Database Backup";
      bool.TryParse(ConfigurationManager.AppSettings["IsNetworkDrive"], out networkDrive);

      string destinationPath = drive;

      if (networkDrive)
      {
        NetworkDrive oNetDrive = new NetworkDrive();
        try
        {
          destinationPath = @"H:\";
          destinationPath = Path.Combine(destinationPath, "Concentrator");
          oNetDrive.LocalDrive = "H:";
          oNetDrive.ShareName = drive;
          oNetDrive.MapDrive("Diract", "Concentrator01");
          //oNetDrive.MapDrive();
        }
        catch (Exception err)
        {
          //log.Error("Invalid network drive", err);
        }
        oNetDrive = null;
      }

      var mediaPath = context.Request.Params["MediaPath"];
      //var dir = ConfigurationManager.AppSettings["FTPMediaDirectory"].ToString();

      int width = -1;
      if (context.Request.Params["width"] != null)
        int.TryParse(context.Request.Params["width"], out width);

      int heigth = -1;
      if (context.Request.Params["height"] != null)
        int.TryParse(context.Request.Params["height"], out heigth);

      var pathToLook = Path.Combine(destinationPath, mediaPath);

      var ext = Path.GetExtension(pathToLook).ToLower();
      var isTiff = (ext == ".tiff" || ext == ".tif");

      using (Image img = Image.FromFile(pathToLook))
      {        
        if (isTiff)
        {
          TiffConverter converter = new TiffConverter(Path.Combine(destinationPath, mediaPath));
          converter.WriteTo(context.Response.OutputStream, width , heigth );                    
        }
        else
        {
          var image = ImageUtility.GetFixedSizeImage(img, width > 0 ? width : img.Width, heigth > 0 ? heigth : img.Height, true);
          image.Save(context.Response.OutputStream, System.Drawing.Imaging.ImageFormat.Png);
        }
      }
    }

    public bool IsReusable
    {
      get
      {
        return false;
      }
    }
  }
}