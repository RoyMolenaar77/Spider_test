using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FileHelpers;
using System.Globalization;

namespace Concentrator.Vendors.PFA.Helpers
{
  class SignConverter : ConverterBase
  {
     public string _sign { get; private set; }
     public int _padLeft { get; private set; }
     public SignConverter(string sign, int padLeft)
    {
      _sign = sign;
      _padLeft = padLeft;
    }

     public override string FieldToString(object from)
     {
       return from.ToString().PadLeft(_padLeft,'0') + _sign;
     }
 
     public override object StringToField(string from)
     {
       return from.Substring(0, from.Length - _sign.Length);
     }
  }
}
