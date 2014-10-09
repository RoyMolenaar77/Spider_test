using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Plugins.Magento.Models
{
  public class eav_attribute_option
  {
    public int option_id { get; set; }
    public short attribute_id { get; set; }
    public short sort_order { get; set; }
  }
}
