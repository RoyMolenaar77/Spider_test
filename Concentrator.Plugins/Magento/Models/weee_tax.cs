using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Plugins.Magento.Models
{
  public class weee_tax
  {

    public weee_tax()
    {
      state = "*";
    }

    public int value_id { get; set; }
    public int website_id { get; set; }
    public int entity_id { get; set; }
    public string country { get; set; }
    public decimal value { get; set; }
    public string state { get; set; }
    public int attribute_id { get; set; }
    public int entity_type_id { get; set; }

  }
}
