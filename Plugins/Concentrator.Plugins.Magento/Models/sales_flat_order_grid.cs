using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Plugins.Magento.Models
{
  public class sales_flat_order_grid
  {
    public int entity_id { get; set; }
    public string increment_id { get; set; }
    public string status { get; set; }
    public int store_id { get; set; }
  }
}
