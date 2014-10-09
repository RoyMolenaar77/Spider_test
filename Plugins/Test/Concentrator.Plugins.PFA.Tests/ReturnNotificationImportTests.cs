using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Concentrator.Objects.Models.Orders;
using Concentrator.Objects.Models.Products;
using Concentrator.Plugins.PFA.Helpers;

namespace Concentrator.Plugins.PFA.Tests
{
	[TestClass]
	public class ReturnNotificationImportTests
	{
		private string vendorItemNumberLine1 = "123 101 S";
		private string vendorItemNumberLine2 = "123 102 S";

		private OrderLine ConstructOrderLine(int productID, string vendorItemNumber, int quantity, bool? isNonAssortment = null)
		{
			return new OrderLine()
			{
				ProductID = productID,
				Product = new Product()
				{
					VendorItemNumber = vendorItemNumber,
					IsNonAssortmentItem = isNonAssortment
				},
				Quantity = quantity
			};
		}

		[TestMethod]
		public void Is_Whole_Order_Returned_Should_Return_True_If_There_Is_A_Order_Response_Line_For_Each_Ordered_Line_With_Same_Qty()
		{
			var line1 = ConstructOrderLine(123, vendorItemNumberLine1, 2);
			var line2 = ConstructOrderLine(124, vendorItemNumberLine2, 2);

			#region test case setup
			var order = new Order()
			{
				OrderLines = new List<OrderLine>(){
					line1, line2
				},
				OrderResponses = new List<OrderResponse>()
				{
					new OrderResponse(){
						OrderResponseLines = new List<OrderResponseLine>(){
							new OrderResponseLine(){
								Remark = "Returned",
								OrderLine = line1,
								Delivered = 2
							},
							new OrderResponseLine(){
								Remark = "Returned",
								OrderLine = line2,
								Delivered = 2
							}
						}
					}
				}

			};
			#endregion

			Assert.IsTrue(TNTOrderHelper.IsTotalOrderReturned(order));
		}

		[TestMethod]
		public void Is_Whole_Order_Returned_Should_Return_False_If_There_Is_A_Order_Response_Line_For_Each_Ordered_Line_With_Less_Qty()
		{
			var line1 = ConstructOrderLine(123, vendorItemNumberLine1, 2);
			var line2 = ConstructOrderLine(124, vendorItemNumberLine2, 2);

			#region test case setup
			var order = new Order()
			{
				OrderLines = new List<OrderLine>(){
					line1, line2
				},
				OrderResponses = new List<OrderResponse>()
				{
					new OrderResponse(){
						OrderResponseLines = new List<OrderResponseLine>(){
							new OrderResponseLine(){
								Remark = "Returned",
								OrderLine = line1,
								Delivered = 2
							},
							new OrderResponseLine(){
								Remark = "Returned",
								OrderLine = line2,
								Delivered = 1 
							}
						}
					}
				}

			};
			#endregion

			Assert.IsFalse(TNTOrderHelper.IsTotalOrderReturned(order));
		}

		[TestMethod]
		public void Is_Whole_Order_Returned_Should_Return_True_If_There_Is_A_Order_Response_Line_For_Each_Ordered_Line_With_Same_Qty_And_Shipment_Costs()
		{
			var line1 = ConstructOrderLine(123, vendorItemNumberLine1, 2);
			var line2 = ConstructOrderLine(124, vendorItemNumberLine2, 2);
			var line3 = ConstructOrderLine(125, "shipment", 1, true);

			#region test case setup
			var order = new Order()
			{
				OrderLines = new List<OrderLine>(){
					line1, line2
				},
				OrderResponses = new List<OrderResponse>()
				{
					new OrderResponse(){
						OrderResponseLines = new List<OrderResponseLine>(){
							new OrderResponseLine(){
								Remark = "Returned",
								OrderLine = line1,
								Delivered = 2
							},
							new OrderResponseLine(){
								Remark = "Returned",
								OrderLine = line2,
								Delivered = 2
							}
						}
					}
				}

			};
			#endregion

			Assert.IsTrue(TNTOrderHelper.IsTotalOrderReturned(order));
		}
	}
}
