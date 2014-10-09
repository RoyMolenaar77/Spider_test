using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Plugins.PFA.Models
{
	public class StockResult
	{
		public string ItemNumber { get; set; }
		public string ColorCode { get; set; }
		public string SizeCode { get; set; }
		public int Quantity { get; set; }
	}
}
