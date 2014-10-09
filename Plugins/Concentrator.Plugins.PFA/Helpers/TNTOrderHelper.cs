using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Concentrator.Objects.Models.Orders;

namespace Concentrator.Plugins.PFA.Helpers
{
	public static class TNTOrderHelper
	{
		public static bool IsTotalOrderReturned(Order order)
		{
			var d = order.OrderLines.Count - order.OrderLines.Count(c => c.Product.IsNonAssortmentItem.GetValueOrDefault());

			return (
							from or in order.OrderResponses.SelectMany(c => c.OrderResponseLines).Where(c => c.Remark == "Returned")
							let sku = or.OrderLine.Product.VendorItemNumber
							group or by sku into groups
							let qty = Convert.ToInt32(groups.Sum(c => c.Delivered)) //get all returned count for this item
							where order.OrderLines.FirstOrDefault(c => c.Product.VendorItemNumber == groups.Key && c.Quantity == qty) != null
							select groups).Count() == d;
		}
	}
}
