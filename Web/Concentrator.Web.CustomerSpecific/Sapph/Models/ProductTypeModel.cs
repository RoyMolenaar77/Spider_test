using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Web.CustomerSpecific.Sapph.Models
{
  public class ProductTypeModel
  {
    public string Type { get; set; }

    public string Translation { get; set; }

    public bool IsBra { get; set; }

    /// <summary>
    /// int: LanguageID
    /// string: Value
    /// </summary>
    public Dictionary<int, string> Translations { get; set; }    

    public ProductTypeEnum? ProductType { get; set; }
  }
}
