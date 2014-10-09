using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Plugins.Magento.Models
{
  public class eav_entity_attribute
  {
    public int entity_attribute_id { get; set; }
    public int entity_type_id { get; set; }
    public int attribute_set_id { get; set; }
    public int attribute_group_id { get; set; }
    public int attribute_id { get; set; }
    public short sort_order { get; set; }
  }
}
