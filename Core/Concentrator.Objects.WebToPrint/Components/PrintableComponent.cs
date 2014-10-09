using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PdfSharp.Drawing;
using System.Drawing;

namespace Concentrator.Objects.WebToPrint.Components
{
  public class PrintableComponent
  {
    public double Left, Top, Width, Height, Angle;
    public string DataBindingSource;
    private bool hasExecutedPreRender = false;

    public bool IsIndexComponent = false;

    public CompositeComponent Parent;

    public PrintableComponent() : this(0,0,0,0,0)
    {

    }

    public PrintableComponent(double left, double top, double width, double height, double angle)
    {
      Left = left;
      Top = top;
      Width = width;
      Height = height;
      Angle = angle;
    }

    /// <summary>
    /// Renderfunction to draw the object to the Xgraphics object
    /// </summary>
    /// <param name="gfx">object to draw to</param>
    /// <param name="offsetLeft">x offset</param>
    /// <param name="offsetTop">y offset</param>
    /// <param name="scaleX">x scale</param>
    /// <param name="scaleY">y scale</param>
    public virtual void Render(ref XGraphics gfx, double offsetLeft, double offsetTop, double scaleX, double scaleY)
    {
      if (hasExecutedPreRender)
      {
        gfx.Restore();
      }
    }

    protected void PreRender(ref XGraphics gfx, double offsetLeft, double offsetTop, double scaleX, double scaleY)
    {
      gfx.Save();
      gfx.RotateAtTransform(Angle, new XPoint(Util.MillimeterToPoint(offsetLeft + Left + (Width / 2.0)),
         Util.MillimeterToPoint(offsetTop + Top + (Height / 2.0))));
      hasExecutedPreRender = true;
    }

    /// <summary>
    /// Sets the data of the component, to allow for deferred data fetching
    /// </summary>
    /// <param name="data">string with data, either an URL for an image, or text for a textfield</param>
    public virtual void SetData(string data)
    {

    }

    public virtual PrintableComponent Clone()
    {
      return new PrintableComponent(Left, Top, Width, Height, Angle);
    }
  }
}
