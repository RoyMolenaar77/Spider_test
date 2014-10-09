using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Plugins.Magento.Models
{
  public class catalog_product_super_attribute
  {
    public int product_super_attribute_id { get; set; }
    public int product_id { get; set; }
    public int attribute_id { get; set; }
    public int position { get; set; }


    public string label { get; set; }
    public bool use_default { get; set; }

  }
}
