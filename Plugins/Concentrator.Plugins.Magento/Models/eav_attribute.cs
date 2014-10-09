using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Plugins.Magento.Models
{
	public class eav_attribute
	{

		public eav_attribute()
		{
			used_in_product_listing = false;
			is_searchable = false;
		}

		public string attribute_code { get; set; }
		public int attribute_id { get; set; }
		public string attribute_model { get; set; }

		public string backend_model { get; set; }
		public string backend_type { get; set; }
		public string backend_table { get; set; }
		public string default_value { get; set; }

		public int entity_type_id { get; set; }

		public string frontend_class { get; set; }
		public string frontend_input { get; set; }
		public string frontend_label { get; set; }
		public string frontend_model { get; set; }

		public bool is_required { get; set; }
		public bool is_unique { get; set; }
		public bool is_user_defined { get; set; }
		public string note { get; set; }
		public string source_model { get; set; }


		public bool is_comparable { get; set; }
		public bool is_filterable { get; set; }
		public bool is_visible { get; set; }

		// stubs
		public bool is_searchable { get; set; }
		public bool used_in_product_listing { get; set; }
		public bool is_configurable { get; set; }
		public bool is_used_for_promo_rules { get; set; }

		public int position { get; set; }
	}
}