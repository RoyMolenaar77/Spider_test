using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FileHelpers;
using System.Globalization;

namespace Concentrator.Vendors.PFA.Helpers
{
  class DefaultValueConverter : ConverterBase
  {
  public string _value { get; private set; }
  public DefaultValueConverter(string value)
    {
      _value = value;
    }
    
    public override string FieldToString(object from)
    {
      return _value;
    }

    //string format =20110603
    public override object StringToField(string from)
    {
      return _value;
    }
  }
}
