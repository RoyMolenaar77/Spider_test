using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Plugins.PFA.Models
{
	public class StoreOrderLineModel
	{
		public string Art_Number { get; set; }
		public string Color_Code { get; set; }
		public string Size_Code { get; set; }
		public int Quantity { get; set; }
	}
}
