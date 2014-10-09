using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PdfSharp.Drawing;
using System.Drawing;
using PdfSharp.Drawing.Layout;
using System.Configuration;

namespace Concentrator.Objects.WebToPrint.Components
{
  public class ImageBagComponent : PrintableComponent
  {
    private Queue<Image> images;

    public double ImageWidth = 10.0, ImageHeight = 10.0, ImagePaddingX = 1.0, ImagePaddingY = 1.0;
    public bool RightToLeft = false;

    public ImageBagComponent(double left, double top, double width, double height, double angle)
      : base(left, top, width, height, angle)
    {
      images = new Queue<Image>();
    }

    public override void Render(ref PdfSharp.Drawing.XGraphics gfx, double offsetLeft, double offsetTop, double scaleX, double scaleY)
    {
        PreRender(ref gfx, offsetLeft, offsetTop, scaleX, scaleY);

        double  imgwidth = Util.MillimeterToPoint(ImageWidth)* scaleX,
                imgheight = Util.MillimeterToPoint(ImageHeight)* scaleY,
                imgpadx = Util.MillimeterToPoint(ImagePaddingX)* scaleX,
                imgpady = Util.MillimeterToPoint(ImagePaddingY)* scaleY,
                left = Util.MillimeterToPoint(offsetLeft + Left),
                top = Util.MillimeterToPoint(offsetTop + Top),
                width = Util.MillimeterToPoint(Width * scaleX),
                height = Util.MillimeterToPoint(Height * scaleY);


        for (double y = 0; y + imgpady + imgheight < height; y += imgheight + imgpady)
        {
          if (RightToLeft)
          {
            for (double x = width - imgwidth; x >0; x -= imgwidth + imgpadx)
            {
              if (images.Count > 0)
              {
                gfx.DrawImage((XImage)images.Dequeue(), left + x, top + y, imgwidth, imgheight);
              }
              else
                break;
            }
          }
          else
          {
            for (double x = 0; x + imgpadx + imgwidth < width; x += imgwidth + imgpadx)
            {
              if (images.Count > 0)
              {
                gfx.DrawImage((XImage)images.Dequeue(), left + x, top + y, imgwidth, imgheight);
              }
              else
                break;
            }
          }
          
          if (images.Count == 0)
            break;

        }


        base.Render(ref gfx, offsetLeft, offsetTop, scaleX, scaleY);
    }

    public override void SetData(string data)
    {
      string root = ConfigurationManager.AppSettings["WebServicesRoot"];
      foreach (string id in data.Split(','))
        images.Enqueue(Util.LoadImageFromUrl(root + "AttributeImage.ashx?ProductAttributeValueID=" + id));

      base.SetData(data);
    }

    public string GetDataValue()
    {
      return images.ToString() ?? "";
    }

    public override PrintableComponent Clone()
    {
      ImageBagComponent tc = new ImageBagComponent(Left, Top, Width, Height, Angle)
      {
        DataBindingSource = this.DataBindingSource,
        ImageWidth = this.ImageWidth,
        ImageHeight = this.ImageHeight,
        ImagePaddingX = this.ImagePaddingX,
        ImagePaddingY = this.ImagePaddingY,
        RightToLeft = this.RightToLeft
      };
      return tc;
      
    }
  }
}
