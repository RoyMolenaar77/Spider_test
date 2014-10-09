using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Plugins.Magento.Models
{
  public class eav_attribute_set
  {
    public int attribute_set_id { get; set; }
    public int entity_type_id { get; set; }
    public string attribute_set_name { get; set; }
    public short sort_order { get; set; }
  }
}
