using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Orders;
using Concentrator.Plugins.PFA.Models;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Models.Vendors;

namespace Concentrator.Plugins.PFA.Helpers
{
	public class StoreOrderHelper
	{
		private int vendorID = 1;

		public OrderLine GetOrderLine(StoreOrderLineModel orderLineModel, int wmsQuantityOnHand, int productID, decimal price, decimal? specialPrice, Product p = null)
		{
			OrderLine line = null;

			if (wmsQuantityOnHand == 0) return line; //shortcircuit here because no stock is available in the wms

			int quantityToShip = Math.Min(orderLineModel.Quantity, wmsQuantityOnHand);

			if (quantityToShip == 0) return line; //shortcircuit here in case of input mistakes


			line = new OrderLine()
					{
						ProductID = productID,
						Product = p,
						Price = (double)Math.Min(price, specialPrice.Try(c => c.Value, decimal.MaxValue)) * orderLineModel.Quantity,
						Quantity = quantityToShip,
						OrderLedgers = new List<OrderLedger>()
					};

			return line;
		}
	}
}
