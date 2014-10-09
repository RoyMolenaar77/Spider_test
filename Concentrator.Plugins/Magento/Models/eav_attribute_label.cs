using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Plugins.Magento.Models
{
  public class eav_attribute_label
  {
    public int attribute_label_id { get; set; }
    public int attribute_id { get; set; }
    public int store_id { get; set; }
    public string value { get; set; }
  }
}
