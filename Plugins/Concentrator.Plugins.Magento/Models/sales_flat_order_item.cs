using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Plugins.Magento.Models
{
  public class sales_flat_order_item
  {
    public int item_id { get; set; }
    public int order_id { get; set; }
    public int parent_item_id { get; set; }
    public int quote_item_id { get; set; }
    public int store_id { get; set; }

    public int product_id { get; set; }
    public string sku { get; set; }
    

  }
}
