using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Plugins.Magento.Models
{
  public class customer_address_entity
  {
    public int entity_id { get; set; }
    public int entity_type_id { get; set; }
    public int attribute_set_id { get; set; }
    public string increment_id { get; set; }
    public int parent_id { get; set; }
    public DateTime created_at { get; set; }
    public DateTime updated_at { get; set; }
    public bool is_active { get; set; }

    public customer_address_entity()
    {
      created_at = updated_at = DateTime.Now;

    }
  }
}
