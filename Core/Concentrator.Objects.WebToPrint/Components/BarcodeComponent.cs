using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PdfSharp.Drawing;
using System.Drawing;

namespace Concentrator.Objects.WebToPrint.Components
{
  public class BarcodeComponent : PrintableComponent
  {
    class Pattern
    {
      private string lhOdd;

      public string LhOdd
      {
        get { return lhOdd; }
        set { lhOdd = value; }
      }

      private string lhEven;

      public string LhEven
      {
        get { return lhEven; }
        set { lhEven = value; }
      }

      private string rh;

      public string Rh
      {
        get { return rh; }
        set { rh = value; }
      }
    }
    class Parity
    {
      private string par;

      internal Parity(string par)
      {
        this.par = par;
      }

      internal bool IsOdd(int i)
      {
        return par[i] == 'o';
      }

      internal bool IsEven(int i)
      {
        return par[i] == 'e';
      }
    }
    static class EAN13
    {
      public static Dictionary<int, Pattern> Codes = new Dictionary<int, Pattern>();
      public static Dictionary<int, Parity> Parity = new Dictionary<int, Parity>();
      static EAN13()
      {
        //      #  LEFT ODD   LEFT EVEN  RIGHT
        AddCode(0, "0001101", "0100111", "1110010");
        AddCode(1, "0011001", "0110011", "1100110");
        AddCode(2, "0010011", "0011011", "1101100");
        AddCode(3, "0111101", "0100001", "1000010");
        AddCode(4, "0100011", "0011101", "1011100");
        AddCode(5, "0110001", "0111001", "1001110");
        AddCode(6, "0101111", "0000101", "1010000");
        AddCode(7, "0111011", "0010001", "1000100");
        AddCode(8, "0110111", "0001001", "1001000");
        AddCode(9, "0001011", "0010111", "1110100");

        AddParity(0, "ooooo");
        AddParity(1, "oeoee");
        AddParity(2, "oeeoe");
        AddParity(3, "oeeeo");
        AddParity(4, "eooee");
        AddParity(5, "eeooe");
        AddParity(6, "eeeoo");
        AddParity(7, "eoeoe");
        AddParity(8, "eoeeo");
        AddParity(9, "eeoeo");
      }
      static void AddCode(int digit, string lhOdd, string lhEven, string rh)
      {
        Pattern p = new Pattern();
        p.LhOdd = lhOdd; p.LhEven = lhEven; p.Rh = rh;
        Codes.Add(digit, p);
      }

      static void AddParity(int digit, string par)
      {
        Parity.Add(digit, new Parity(par));
      }
    }

    private string code;
    private XFont font;
    double BarWidth;
    private double offsetTop, offsetLeft;

    public BarcodeComponent(double left, double top, double width, double height, double angle)
      : base(left,top,width,height, angle)
    {

    }

    public override void Render(ref PdfSharp.Drawing.XGraphics gfx, double _offsetLeft, double _offsetTop, double scaleX, double scaleY)
    {
      PreRender(ref gfx, offsetLeft, offsetTop, scaleX, scaleY);
      if (code != null && code.Length == 13)
      {
        double left = 0.0;
        offsetTop = _offsetTop;
        offsetLeft = _offsetLeft;
        Width*= scaleX;
        Height*= scaleY;
        font = new XFont(new Font("World",(float)Util.MillimeterToPoint((float)Math.Min(Height*0.2,Width*0.1)*0.8),GraphicsUnit.World),new XPdfFontOptions(false,false));
        gfx.DrawString(code[0].ToString(), font, Brushes.Black, new PointF((float)(Util.MillimeterToPoint(offsetLeft + Left + left)), (float)(Util.MillimeterToPoint(offsetTop + Top + (Height*0.95)))));
      left += 4;
      BarWidth = (Width - left) / (3 + 6 * 7 + 5 + 6 * 7 + 3);
      left = DrawLeftGuard(ref gfx, code[0].ToString(), left);

      int first = int.Parse(code[0].ToString());
      Parity par = EAN13.Parity[first];
      string digit = code[1].ToString();
      left = Draw(ref gfx, left, digit, EAN13.Codes[int.Parse(digit)].LhOdd);
      for (int i = 2; i <= 6; i++)
      {
        digit = code[i].ToString();
        Pattern p = EAN13.Codes[int.Parse(digit)];
        left = Draw(ref gfx, left, digit, par.IsOdd(i - 2) ? p.LhOdd : p.LhEven);
      }
      left = DrawCenterGuard(ref gfx, left);
      for (int i = 7; i <= 12; i++)
      {
        digit = code[i].ToString();
        Pattern p = EAN13.Codes[int.Parse(digit)];
        left = Draw(ref gfx, left, digit, p.Rh);
      }

      left = DrawRightGuard(ref gfx, left);
        Width/= scaleX;
        Height/= scaleY;
      }
      base.Render(ref gfx, offsetLeft, offsetTop, scaleX, scaleY);
    }

    private double Draw(ref XGraphics g, double left, string digit, string s)
    {
      int h = (int)(Height * 0.8);

      g.DrawString(digit, font, XBrushes.Black, new PointF((float)Util.MillimeterToPoint(offsetLeft + Left + left), (float)Util.MillimeterToPoint(offsetTop + Top + (Height * 0.95))));
      foreach (char c in s)
      {
        if (c == '1')
          g.DrawRectangle(null, XBrushes.Black, new XRect(Util.MillimeterToPoint(left + Left + offsetLeft),Util.MillimeterToPoint(offsetTop + Top),Util.MillimeterToPoint(BarWidth), Util.MillimeterToPoint(h)));
        left += BarWidth;
      }
      return left;
    }

    private double DrawLeftGuard(ref PdfSharp.Drawing.XGraphics g, string digit, double left)
    {
      g.DrawRectangle(null, XBrushes.Black, new XRect(Util.MillimeterToPoint(left + Left + offsetLeft), Util.MillimeterToPoint(offsetTop + Top), Util.MillimeterToPoint(BarWidth), Util.MillimeterToPoint(Height)));
      left += BarWidth;
      left += BarWidth; //0
      g.DrawRectangle(null, XBrushes.Black, new XRect(Util.MillimeterToPoint(left + Left + offsetLeft), Util.MillimeterToPoint(offsetTop + Top), Util.MillimeterToPoint(BarWidth), Util.MillimeterToPoint(Height)));
      left += BarWidth;
      return left;
    }

    private double DrawRightGuard(ref PdfSharp.Drawing.XGraphics g, double left)
    {
      g.DrawRectangle(null, XBrushes.Black, new XRect(Util.MillimeterToPoint(left + Left + offsetLeft), Util.MillimeterToPoint(offsetTop + Top),Util.MillimeterToPoint(BarWidth), Util.MillimeterToPoint(Height)));
      left += BarWidth;
      left += BarWidth;
      g.DrawRectangle(null, XBrushes.Black, new XRect(Util.MillimeterToPoint(left + Left + offsetLeft), Util.MillimeterToPoint(offsetTop + Top), Util.MillimeterToPoint(BarWidth), Util.MillimeterToPoint(Height)));
      left += BarWidth;
      return left;
    }

    private double DrawCenterGuard(ref PdfSharp.Drawing.XGraphics g, double left)
    {
      left += BarWidth;
      g.DrawRectangle(null, XBrushes.Black, new XRect(Util.MillimeterToPoint(left + Left + offsetLeft), Util.MillimeterToPoint(offsetTop + Top), Util.MillimeterToPoint(BarWidth), Util.MillimeterToPoint(Height)));
      left += BarWidth;
      left += BarWidth;
      g.DrawRectangle(null, XBrushes.Black, new XRect(Util.MillimeterToPoint(left + Left + offsetLeft), Util.MillimeterToPoint(offsetTop + Top), Util.MillimeterToPoint(BarWidth), Util.MillimeterToPoint(Height)));
      left += BarWidth;
      left += BarWidth;
      return left;
    }

    public override void SetData(string data)
    {
      code = data;
    }

  }
}
