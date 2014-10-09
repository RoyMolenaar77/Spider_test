using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PdfSharp.Drawing;
using System.Drawing;
using PdfSharp.Drawing.Layout;

namespace Concentrator.Objects.WebToPrint.Components
{
  public class TextComponent : PrintableComponent
  {
    public enum SpecialBinding
    {
      None,
      PageNumber,
      Index
    }

    public PrintableStyle Style;
    private string text = "";

    public SpecialBinding BindingOption = SpecialBinding.None;

    public TextComponent(double left, double top, double width, double height, double angle)
      : base(left, top, width, height, angle)
    {
      Style = new PrintableStyle();
    }

    public override void Render(ref PdfSharp.Drawing.XGraphics gfx, double offsetLeft, double offsetTop, double scaleX, double scaleY)
    {
        PreRender(ref gfx, offsetLeft, offsetTop, scaleX, scaleY);
        XRect rectangle = new XRect(
            Util.MillimeterToPoint(offsetLeft + Left),
            Util.MillimeterToPoint(offsetTop + Top),
            Util.MillimeterToPoint(Width * scaleX),
            Util.MillimeterToPoint(Height * scaleY));

        if (BindingOption == SpecialBinding.PageNumber)
        {
          PrintableComponent pc = Parent;
          while (pc != null && !(pc is PrintablePage))
            pc = pc.Parent;

          if (pc != null)
            text = ((PrintablePage)pc).PageNumber.ToString();
        }

        if (text != null && text != "Null")
        {
          XTextFormatter xtf = new XTextFormatter(gfx);
          xtf.Alignment = Style.FontAlignment;

          xtf.DrawString(text, Style.AsXFont(), (XBrush)new SolidBrush(Style.PrimaryColor), rectangle);
        }
        base.Render(ref gfx, offsetLeft, offsetTop, scaleX, scaleY);
    }

    public override void SetData(string data)
    {
      if (data == null) { data = ""; }
      text = data;
      base.SetData(data);
    }

    public string GetDataValue()
    {
      return text ?? "";
    }

    public override PrintableComponent Clone()
    {
      TextComponent tc = new TextComponent(Left, Top, Width, Height, Angle)
      {
        BindingOption = this.BindingOption,
        DataBindingSource = this.DataBindingSource
      };
      tc.Style = this.Style.Clone();
      return tc;
      
    }
  }
}
