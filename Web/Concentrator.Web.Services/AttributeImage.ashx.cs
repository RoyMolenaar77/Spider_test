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
using Concentrator.Objects;

namespace Concentrator.Web.Services
{
  /// <summary>
  /// Summary description for ImageHandler
  /// </summary>
  public class AttributeImage : BaseConcentratorService, IHttpHandler
  {

    public void ProcessRequest(HttpContext context)
    {
      bool networkDrive = false;
      string drive = ConfigurationManager.AppSettings["AttributeImageDirectory"].ToString();
      //string drive = @"\\SOL\Company_Shares\Database Backup";
      bool.TryParse(ConfigurationManager.AppSettings["IsNetworkDrive"], out networkDrive);

      string destinationPath = drive;

      if (networkDrive)
      {
        NetworkDrive oNetDrive = new NetworkDrive();
        try
        {
          destinationPath = @"H:\";
          destinationPath = Path.Combine(destinationPath, "Concentrator", "Products");
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
      
      context.Response.ContentType = "image/png";

      int attributeValueID = int.Parse(context.Request.Params["ProductAttributeValueID"]);

      int width = 44;
      if (context.Request.Params["width"] != null)
        int.TryParse(context.Request.Params["width"], out width);

      int heigth = 44;
      if (context.Request.Params["height"] != null)
        int.TryParse(context.Request.Params["height"], out heigth);

      float fontSize = 7.5f;
      if (context.Request.Params["fontSize"] != null)
        float.TryParse(context.Request.Params["fontSize"], out fontSize);

      string font = "Univers LT 47 CondensedLt";

      if (context.Request.Params["font"] != null)
        font = context.Request.Params["font"];

      using (var unit = GetUnitOfWork())
      {
        var attributeImage = unit.Scope.Repository<ProductAttributeValue>().GetSingle(x => x.AttributeValueID == attributeValueID);

        if (attributeImage != null)
        {
          var dir = destinationPath;//ConfigurationManager.AppSettings["FTPMediaDirectory"].ToString();
          var imageFile = Path.Combine(dir, attributeImage.ProductAttributeMetaData.AttributePath);






          if (File.Exists(imageFile))
          {
            using (Image img = Image.FromFile(imageFile))
            {
              var image = ImageUtility.GetFixedSizeImage(img, width, heigth, true);






              using (Graphics g = Graphics.FromImage(image))
              {
                ////calculate font size
                //var desiredSizeF = new RectangleF(0, 0.11f, image.Width, image.Height - 0.11f).Size;
                //var drawnFont = new Font(font, fontSize, FontStyle.Bold);

                //while (desiredSizeF.Height < g.MeasureString(attributeImage.Value, drawnFont).Height)
                //{
                //  //smaller the font size by 1
                //  fontSize = fontSize - 1f;
                //  drawnFont = new Font(font, fontSize, FontStyle.Bold);
                //}

                StringFormat strFormat = new StringFormat();
                strFormat.Alignment = StringAlignment.Center;
                strFormat.LineAlignment = StringAlignment.Far;



                g.DrawString(attributeImage.Value, new Font(font, fontSize, FontStyle.Bold), Brushes.Black,
                    new RectangleF(0, -0.7f, image.Width, image.Height), strFormat);

              }

              image.Save(context.Response.OutputStream, System.Drawing.Imaging.ImageFormat.Png);
            }
          }
          else
          {
            context.Response.StatusCode = 404;
          }
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