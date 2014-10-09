using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.Types;

namespace Concentrator.Plugins.Magento.Models
{
  public class catalog_product_entity
  {
    public catalog_product_entity()
    {
      created_at = DateTime.Now;
      updated_at = DateTime.Now;
    }

    public int entity_id { get; set; }
    public int entity_type_id { get; set; }
    public int attribute_set_id { get; set; }
    public string type_id { get; set; }
    public string sku { get; set; }
    public bool has_options { get; set; }
    public bool required_options { get; set; }
		public DateTime created_at { get; set; }
		public DateTime updated_at { get; set; }

  }
}
