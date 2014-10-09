using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Plugins.Magento.Models
{
  public class eav_attribute_option_value
  {
    public int attribute_id { get; set; }
    public int sort_order { get; set; }
    public int option_id { get; set; }
    public int store_id { get; set; }
    public string value { get; set; }

    public int? concentrator_attribute_id { get; set; }
  }
}
