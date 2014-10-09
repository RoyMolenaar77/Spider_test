using FileHelpers;
using System;
using System.Globalization;

namespace Concentrator.Vendors.PFA.Helpers
{
  public class PaddedNumberConverter : ConverterBase
  {
    private CultureInfo culture = new CultureInfo("en-US");

    public int Length { get; private set; }

    public PaddedNumberConverter(int length)
    {
      Length = length;
    }

    public override string FieldToString(object from)
    {
      return from.ToString().PadLeft(Length, '0');
    }

    //string format =20110603
    public override object StringToField(string from)
    {
      int result = 0;

      if (Int32.TryParse(from.Trim().TrimStart('0'), out result))
        return result;

      return null;
    }
  }
}
