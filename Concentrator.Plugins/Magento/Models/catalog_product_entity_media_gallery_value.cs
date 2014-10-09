using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Plugins.Magento.Models
{
  public class catalog_product_entity_media_gallery_value
  {
    public int value_id { get; set; }
    public int store_id { get; set; }
    public string label { get; set; }
    public int position { get; set; }
    public bool disabled { get; set; }
  }
}
