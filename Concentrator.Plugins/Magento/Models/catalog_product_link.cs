using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Plugins.Magento.Models
{
  public class catalog_product_link
  {
    public int link_id { get; set; }
    public int product_id { get; set; }
    public int linked_product_id { get; set; }
    public int link_type_id { get; set; }
  }
}
