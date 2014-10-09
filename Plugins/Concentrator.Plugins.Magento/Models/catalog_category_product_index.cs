using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Plugins.Magento.Models
{
  class catalog_category_product_index
  {
    public int category_id { get; set; }
    public int product_id { get; set; }
    public int position { get; set; }
    public int is_parent { get; set; }
    public int store_id { get; set; }
    public int visibility { get; set; }

  }
}
