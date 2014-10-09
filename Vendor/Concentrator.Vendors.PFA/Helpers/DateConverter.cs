using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FileHelpers;
using System.Globalization;

namespace Concentrator.Vendors.PFA.Helpers
{
  [DelimitedRecord("")]
  class DateConverter : ConverterBase
  {
    private CultureInfo culture = new CultureInfo("en-US");

    public override string FieldToString(object from)
    {
      if (from is DateTime?)
      {
        if (((DateTime?)from).HasValue)
          return ((DateTime?)from).Value.ToString("yyyyMMdd");
        else
          return "00000000";
      }
      else
        return "00000000";

    }

    //string format =20110603
    public override object StringToField(string from)
    {
      DateTime dt;
      if (DateTime.TryParseExact(from, "yyyyMMdd", culture, DateTimeStyles.None, out dt))
        return dt;
      else
        return null;

    }
  }
}
