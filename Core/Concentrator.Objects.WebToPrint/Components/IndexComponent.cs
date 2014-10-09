using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Objects.WebToPrint.Components
{
  public class IndexComponent: PrintableComponent
  {
    public string LeftText, RightText;
    private TextComponent leftTextComponent, rightTextComponent;
    public IndexComponent(double left, double top, double width, double height, double angle, PrintableStyle ps)
      : base(left, top, width, height, angle)
    {
      PrintableStyle leftStyle = ps.Clone();
      leftStyle.FontAlignment = PdfSharp.Drawing.Layout.XParagraphAlignment.Left;
      PrintableStyle rightStyle = ps.Clone();
      rightStyle.FontAlignment = PdfSharp.Drawing.Layout.XParagraphAlignment.Right;

      leftTextComponent = new TextComponent(left, top, width, height, angle)
      {
        Style = leftStyle
      };

      rightTextComponent = new TextComponent(left, top, width, height, angle)
      {
        Style = rightStyle
      };
    }

    public override void Render(ref PdfSharp.Drawing.XGraphics gfx, double offsetLeft, double offsetTop, double scaleX, double scaleY)
    {
      leftTextComponent.SetData(LeftText);
      leftTextComponent.Render(ref gfx, offsetLeft, offsetTop, scaleX, scaleY);

      rightTextComponent.SetData(RightText);
      rightTextComponent.Render(ref gfx, offsetLeft, offsetTop, scaleX, scaleY);

      base.Render(ref gfx, offsetLeft, offsetTop, scaleX, scaleY);
    }

    public static IndexComponent FromTextComponent(TextComponent tc)
    {
      return new IndexComponent(tc.Left, tc.Top, tc.Width, tc.Height, tc.Angle, tc.Style);
    }
  }
}
