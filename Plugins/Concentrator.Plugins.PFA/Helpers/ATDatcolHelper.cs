using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Orders;

namespace Concentrator.Plugins.PFA.Helpers
{
	public static class ATDatcolHelper
	{
		public static bool IsCJPSale(OrderLine line, string cjpDiscountName)
		{
			bool isCjp = false;

			if (line.OrderLineAppliedDiscountRules != null && line.OrderLineAppliedDiscountRules.Count > 0)
			{
				isCjp = line.OrderLineAppliedDiscountRules.Any(c => c.Code == cjpDiscountName);
			}

			return isCjp;
		}
	}
}
