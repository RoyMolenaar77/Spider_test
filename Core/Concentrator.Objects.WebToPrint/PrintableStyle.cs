using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using PdfSharp.Drawing;
using PdfSharp.Drawing.Layout;

namespace Concentrator.Objects.WebToPrint
{
  public class PrintableStyle
  {
    string RefName;
    public double FontSize;
    public FontFamily Font;
    public Color PrimaryColor;
    public Color SecondaryColor;
    public XParagraphAlignment FontAlignment;

    public bool Bold;
    public bool Italic;
    public bool Underlined;

    public PrintableStyle()
      : this("")
    {

    }

    public PrintableStyle(string name)
    {
      RefName = name;
      FontSize = 18.0;
      Font = FontFamily.GenericSansSerif;
      PrimaryColor = Color.Black;
    }

    public XFont AsXFont()
    {
      return new XFont(Font.Name, FontSize, Underlined ? XFontStyle.Underline : (Bold ? (Italic ? XFontStyle.BoldItalic : XFontStyle.Bold): (Italic ? XFontStyle.Italic : XFontStyle.Regular)));
    }

    public PrintableStyle Clone()
    {
      return new PrintableStyle()
      {
        Bold = this.Bold,
        Font = this.Font,
        FontAlignment = this.FontAlignment,
        FontSize = this.FontSize,
        Italic = this.Italic,
        PrimaryColor = this.PrimaryColor,
        SecondaryColor = this.SecondaryColor,
        Underlined = this.Underlined
      };
    }


  }
}
