using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Concentrator.Plugins.PFA.Models;
using Concentrator.Plugins.PFA.Helpers;

namespace Concentrator.Plugins.PFA.Tests
{
	[TestClass]
	public class StoreOrderImport
	{
		[TestMethod]
		public void Order_Line_With_Quantity_When_WMS_Has_No_Quantity_Should_Return_Null()
		{
			StoreOrderLineModel model = new StoreOrderLineModel()
			{
				Size_Code = "10",
				Art_Number = "111111",
				Color_Code = "100",
				Quantity = 1
			};

			StoreOrderHelper help = new StoreOrderHelper();
			Assert.IsNull(help.GetOrderLine(model, 0, 1, 10m, null), "Should return null");
		}

		[TestMethod]
		public void Order_Line_With_No_Quantity_When_WMS_Has_No_Quantity_Should_Return_Null()
		{
			StoreOrderLineModel model = new StoreOrderLineModel()
			{
				Size_Code = "10",
				Art_Number = "111111",
				Color_Code = "100",
				Quantity = 0
			};

			StoreOrderHelper help = new StoreOrderHelper();
			Assert.IsNull(help.GetOrderLine(model, 1, 1, 10m, null), "Should return null");
		}

		[TestMethod]
		public void Order_Line_Quantity_Should_Decrease_To_Wms_Quantity_If_Wms_Quantity_Is_Less_Than_Requested()
		{
			StoreOrderLineModel model = new StoreOrderLineModel()
			{
				Size_Code = "10",
				Art_Number = "111111",
				Color_Code = "100",
				Quantity = 4
			};

			StoreOrderHelper help = new StoreOrderHelper();
			Assert.AreEqual(help.GetOrderLine(model, 3, 1, 10m, null).Quantity, 3, "Should return null");
		}
	}
}
