using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Plugins.PFA.Models
{
	public class StoreOrderModel
	{
		public List<StoreOrderLineModel> OrderLines { get; set; }

		public string Store_Number { get; set; }
		public string Address1 { get; set; }
		public string Address2 { get; set; }
		public string Postcode { get; set; }
		public string City { get; set; }
		public string Country { get; set; }
	}
}
