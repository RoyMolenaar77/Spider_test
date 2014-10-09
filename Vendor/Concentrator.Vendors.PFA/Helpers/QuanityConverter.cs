using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FileHelpers;

namespace Concentrator.Vendors.PFA.Helpers
{
   [DelimitedRecord("|")]
  public class QuantityConverter : ConverterBase
  {
    private readonly int _padding = 10;
    public QuantityConverter() { }

     public QuantityConverter(int padding)
    {
      this._padding = padding;
    }

    public override object StringToField(string from)
    {
      from = from.TrimStart('0');

      var number = int.Parse(from.Substring(0, from.Length - 1));

      var sign = from[from.Length - 1];

      if (sign == '-')
      {
        number *= -1;
      }

      return number;

    }

    public override string FieldToString(object from)
    {
      var nr = Convert.ToInt32(from);

      var str = Math.Abs(nr) + (nr < 0 ? "-" : "+");

      str = str.PadLeft(this._padding, '0');

      return str;

    }



  }
}
