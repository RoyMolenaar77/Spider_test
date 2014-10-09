using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.WebToPrint.Components;

namespace Concentrator.Objects.WebToPrint
{
  public class PrintablePage: CompositeComponent
  {
    public int PageNumber;
    public PrintablePage(double width, double height): base(0,0,width,height,0) {
    }

    public void Render(ref PdfSharp.Drawing.XGraphics gfx)
    {
      foreach (PrintableComponent pc in Children)
      {
        // voor scaling/offset later
        pc.Render(ref gfx, 0.0, 0.0, 1.0, 1.0);
      }
    }
  }
}
