using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Plugins.Magento.Models
{
  public class salesrule
  {
    public int rule_id { get; set; }
    public string name { get; set; }
    public string description { get; set; }
    public DateTime? from_date { get; set; }
    public DateTime? to_date { get; set; }
    public int uses_per_customer { get; set; }
    public bool is_active { get; set; }

    public string customer_group_ids { get; set; }
    public string conditions_serialized { get; set; }
    public string actions_serialized { get; set; }
    public bool is_advanced { get; set; }
    public string product_ids { get; set; }
    public int sort_order { get; set; }
    public string simple_action { get; set; }
    public decimal discount_amount { get; set; }
    public decimal discount_qty { get; set; }
    public int discount_step { get; set; }

    public bool simple_free_shipping { get; set; }
    public bool apply_to_shipping { get; set; }
    public int times_used { get; set; }
    public bool is_rss { get; set; }
    public string website_ids { get; set; }
    public int coupon_type { get; set; }

		public bool is_discountset { get; set; }

    //stub
    public string label { get; set; }	
  }
}
