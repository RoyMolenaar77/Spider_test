using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Plugins.Magento.Models
{
  public class customer_entity
  {
    public int entity_id { get; set; }
    public int entity_type_id { get; set; }
    public int attribute_set_id { get; set; }
    public int website_id { get; set; }
    public string email { get; set; }
    public short group_id { get; set; }
    public string increment_id { get; set; }
    public int store_id { get; set; }
    public DateTime created_at { get; set; }
    public DateTime updated_at { get; set; }
    public bool is_active { get; set; }

  }
}
