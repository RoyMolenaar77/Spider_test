using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Drawing;
using Concentrator.Objects.Images;
using System.Drawing.Imaging;
using System.IO;

namespace Concentrator.Web.Shared.Results
{
  public class ImageResult : ActionResult
  {
    private Image _image;
    private string _extension;

    //TODO
    //public ImageResult(string path, string defaultPath = @"Content\images\Icons\FileTypeIcons\unknown.png")
    //{
    //  path = path.IfNullOrEmpty(defaultPath);

    //  _extension = Path.GetExtension(path);

    //  _image = Image.FromFile(path);
    //}

    public ImageResult(Image image, string extension = "png")
    {
      _image = image;
      _extension = extension;
    }

    public override void ExecuteResult(ControllerContext context)
    {
      ImageFormat format = (ImageFormat)(typeof(ImageFormat).GetProperties().FirstOrDefault(c => c.Name.ToLower() == _extension.ToLower()).GetValue(null, null));

      context.RequestContext.HttpContext.Response.ContentType = "image/" + _extension;
      _image.Save(context.RequestContext.HttpContext.Response.OutputStream, format);
    }
  }
}
