using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Plugins.Magento.Models
{
  public class eav_attribute_group
  {
    public int attribute_group_id { get; set; }
    public int attribute_set_id { get; set; }
    public short sort_order{ get; set; }
    public int default_id{ get; set; }
    public string attribute_group_name { get; set; }
  }
}
