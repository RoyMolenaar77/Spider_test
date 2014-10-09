using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PdfSharp.Drawing;
using System.Drawing;

namespace Concentrator.Objects.WebToPrint.Components
{
  public class RectangleComponent : PrintableComponent
  {
    public double BorderRadius = 0.0;
    public double BorderWidth = 0.0;
    public XBrush FillColor = XBrushes.Transparent;
    public XColor BorderColor = XColors.Transparent;
    public bool DrawBorder = true, Fill = true;

    public RectangleComponent(double left, double top, double width, double height, double angle)
      : base(left, top, width, height, angle)
    {

    }

    public override void Render(ref PdfSharp.Drawing.XGraphics gfx, double offsetLeft, double offsetTop, double scaleX, double scaleY)
    {
      PreRender(ref gfx, offsetLeft, offsetTop, scaleX, scaleY);
      double borderWidth = Util.MillimeterToPoint(BorderWidth);
      double radiusx = Math.Min(Util.MillimeterToPoint(Width), Util.MillimeterToPoint(BorderRadius))*2.0;
      double radiusy = Math.Min(Util.MillimeterToPoint(Height), Util.MillimeterToPoint(BorderRadius))*2.0;

      XPen pen = new XPen(BorderColor, borderWidth);

      if (BorderRadius == 0.0)
      {
        gfx.DrawRectangle(FillColor,
            Util.MillimeterToPoint(offsetLeft + Left),
            Util.MillimeterToPoint(offsetTop + Top),
            Util.MillimeterToPoint(Width * scaleX),
            Util.MillimeterToPoint(Height * scaleY));

        gfx.DrawRectangle(pen,
            Util.MillimeterToPoint(offsetLeft + Left + (BorderWidth / 2.0)),
            Util.MillimeterToPoint(offsetTop + Top + (BorderWidth / 2.0)),
            Util.MillimeterToPoint((Width - BorderWidth) * scaleX),
            Util.MillimeterToPoint((Height - BorderWidth) * scaleY));
      }
      else
      {
        double width = (Width - BorderWidth) * scaleX;
        double height = (Height - BorderWidth) * scaleY;



        gfx.DrawRoundedRectangle(FillColor,
          Util.MillimeterToPoint(offsetLeft + Left),
           Util.MillimeterToPoint(offsetTop + Top),
           Util.MillimeterToPoint((Width) * scaleX),
           Util.MillimeterToPoint((Height) * scaleY),
          Math.Min(Util.MillimeterToPoint(Width),radiusx+Util.MillimeterToPoint(BorderWidth))-0.1,
            Math.Min(Util.MillimeterToPoint(Height), radiusy+Util.MillimeterToPoint(BorderWidth)) - 0.1);

          gfx.DrawRoundedRectangle(pen,
     Util.MillimeterToPoint(offsetLeft + Left + (BorderWidth / 2.0)),
      Util.MillimeterToPoint(offsetTop + Top + (BorderWidth / 2.0)),
      Util.MillimeterToPoint(width),
      Util.MillimeterToPoint(height),
      Math.Min(Util.MillimeterToPoint(width), radiusx) - 0.1,
      Math.Min(Util.MillimeterToPoint(height), radiusy) - 0.1);

        
      }

     

      /**/

      /*if (radiusx == 0.0 && radiusy == 0.0)
      {
        if (Fill)
        {
          gfx.DrawRectangle(FillColor,
            Util.MillimeterToPoint(offsetLeft + Left),
            Util.MillimeterToPoint(offsetTop + Top),
            Util.MillimeterToPoint(Width * scaleX),
            Util.MillimeterToPoint(Height * scaleY));
        }
        if (BorderWidth > 0.0 && DrawBorder) {
          gfx.DrawRectangle(pen,
          Util.MillimeterToPoint(offsetLeft + Left + (borderWidth / 2f)),
          Util.MillimeterToPoint(offsetTop + Top + (borderWidth / 2f)),
          Util.MillimeterToPoint((Width - (borderWidth / 2f)) * scaleX),
          Util.MillimeterToPoint((Height - (borderWidth / 2f)) * scaleY));
        }
      }
      else
      {
        if (Fill)
        {
          gfx.DrawRoundedRectangle(FillColor,
            Util.MillimeterToPoint(offsetLeft + Left + borderWidth),
            Util.MillimeterToPoint(offsetTop + Top + borderWidth),
            Util.MillimeterToPoint((Width - (borderWidth * 2.0)) * scaleX),
            Util.MillimeterToPoint((Height - (borderWidth * 2.0)) * scaleY),
            radiusx,
            radiusy);
        }
        if (BorderWidth > 0.0)
        {
          gfx.DrawRoundedRectangle(pen,
          Util.MillimeterToPoint(offsetLeft + Left+(BorderRadius/2f)),
          Util.MillimeterToPoint(offsetTop + Top + (BorderRadius / 2f)),
          Util.MillimeterToPoint((Width - (BorderRadius / 2f)) * scaleX),
          Util.MillimeterToPoint((Height - (BorderRadius / 2f)) * scaleY),
          radiusx,
          radiusy);
        }
      }*/
      
      base.Render(ref gfx, offsetLeft, offsetTop, scaleX, scaleY);
    }
  }
}
