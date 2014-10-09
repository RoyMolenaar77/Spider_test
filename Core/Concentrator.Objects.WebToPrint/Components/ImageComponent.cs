using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Net;
using PdfSharp.Drawing;
using System.Diagnostics;

namespace Concentrator.Objects.WebToPrint.Components
{

  public class ImageComponent : PrintableComponent
  {

    Image SourceImage;
    private static Dictionary<string, Image> imageCache;

    public ImageComponent(double left, double top, double width, double height, double angle)
      : base(left,top,width,height, angle)
    {

    }

    public override void Render(ref PdfSharp.Drawing.XGraphics gfx, double offsetLeft, double offsetTop, double scaleX, double scaleY)
    {
      PreRender(ref gfx, offsetLeft, offsetTop, scaleX, scaleY);
      if (SourceImage != null)
      {
        double widthScale = (Width * scaleX) / SourceImage.Width;
        double heightScale = (Height * scaleY) / SourceImage.Height;

        double scale = Math.Min(widthScale, heightScale);

        double width = SourceImage.Width * scale;
        double height = SourceImage.Height * scale;

        double offsetLeftImage = ((Width * scaleX) - width)/2.0;
        double offsetTopImage = ((Height * scaleY) - height) / 2.0;

        gfx.DrawImage(SourceImage,
          Util.MillimeterToPoint(offsetLeft + Left + offsetLeftImage),
          Util.MillimeterToPoint(offsetTop + Top + offsetTopImage),
          Util.MillimeterToPoint(width),
          Util.MillimeterToPoint(height));
      }
      else
      {
        gfx.DrawRectangle(XBrushes.White, new Rectangle(
          (int)Util.MillimeterToPoint(offsetLeft + Left),
          (int)Util.MillimeterToPoint(offsetTop + Top),
          (int)Util.MillimeterToPoint(Width * scaleX),
          (int)Util.MillimeterToPoint(Height * scaleY)));
      }
     
      base.Render(ref gfx, offsetLeft, offsetTop, scaleX, scaleY);
    }

    public override void SetData(string data)
    {
      if (data != "")
      {
        try
        {
          if (data.StartsWith("http://"))
            SourceImage = Util.LoadImageFromUrl(data);
          else
            SourceImage = Util.LoadImageFromPath(data);
        }
        catch (Exception e)
        {
          Debug.WriteLine(string.Format("Found error with document, can't find image: {0}",data));
        }
      }
      
      base.SetData(data);
    }
  }
}
