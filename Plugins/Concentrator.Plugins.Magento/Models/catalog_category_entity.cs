using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.Types;

namespace Concentrator.Plugins.Magento.Models
{
  public class catalog_category_entity
  {

    public catalog_category_entity()
    {
			created_at = DateTime.Now;
      updated_at = DateTime.Now;
    }
    public int entity_id { get; set; }
    public int entity_type_id { get; set; }
    public int attribute_set_id { get; set; }
    public int parent_id { get; set; }
    public DateTime created_at { get; set; }
    public DateTime updated_at { get; set; }
    public string path { get; set; }
    public int position { get; set; }
    public int level { get; set; }
    public int children_count { get; set; }


    public int name_attribute_id { get; set; }
    public string name { get; set; }
    public int icecat_attribute_id { get; set; }
    public string icecat_value { get; set; }
    public int store_id { get; set; }
    public int website_id { get; set; }
  }
}
