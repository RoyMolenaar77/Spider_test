using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Plugins.PFA.Models.CC;

namespace Concentrator.Plugins.PFA
{
  public class AttributeVendorMetaData
  {
    public string Code { get; set; }

    public bool Configurable { get; set; }

    public bool Searchable { get; set; }

    public int VendorID { get; set; }

    public bool UsedOnConfigurableLevel { get; set; }

    public Func<ProductInfoResult, string> GetAttributeValue { get; set; } 
  }
}
