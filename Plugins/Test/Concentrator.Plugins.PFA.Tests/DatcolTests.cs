using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Concentrator.Plugins.PFA.Helpers;
using Concentrator.Objects.Models.Orders;

namespace Concentrator.Plugins.PFA.Tests
{
	[TestClass]
	public class DatcolTests
	{
		[TestMethod]
		public void Refund_Revenue_With_SpecialPrice_Should_Be_Correct_With_10_Euro_Discount()
		{
			OrderLine testLine = new OrderLine();
			testLine.BasePrice = 24.95;
			testLine.UnitPrice = 5;
			testLine.Quantity = 1;

			var revenue = DatcolHelper.GetNegativeRevenue(testLine, 1);
			Assert.AreEqual(-500, revenue);
		}

		[TestMethod]
		public void Refund_Revenue_With_Special_Price_Should_Be_Correct()
		{
			OrderLine testLine = new OrderLine();
			testLine.BasePrice = 39.95;
			testLine.UnitPrice = 15;
			testLine.Quantity = 1;

			var revenue = DatcolHelper.GetNegativeRevenue(testLine, 1);
			Assert.AreEqual(-1500, revenue);
		}

		[TestMethod]
		public void Refund_Revenue_With_Special_Price_Should_Be_Correct_With_1_Refunded_And_2_Ordered()
		{
			OrderLine testLine = new OrderLine();
			testLine.BasePrice = 39.95;
			testLine.UnitPrice = 15;
			testLine.Quantity = 2;

			var revenue = DatcolHelper.GetNegativeRevenue(testLine, 1);
			Assert.AreEqual(-1500, revenue);
		}

		[TestMethod]
		public void Refung_Should_Calculate_Discount_Correctly()
		{
			OrderLine testLine = new OrderLine();
			testLine.BasePrice = 12.95;
			testLine.UnitPrice = 12.95;
			testLine.Quantity = 1;
			testLine.LineDiscount = 1.295;

			var revenue = DatcolHelper.GetNegativeRevenue(testLine, 1);
			Assert.AreEqual(-1166, revenue);
		}

		[TestMethod]
		public void Refung_Should_Calculate_0_ShipmentCosts_Correctly()
		{
			OrderLine testLine = new OrderLine();
			testLine.BasePrice = 3.95;
			testLine.UnitPrice = 3.95;
			testLine.Quantity = 1;
			testLine.LineDiscount = 3.95;
			testLine.Price = 0;

			var revenue = DatcolHelper.GetNegativeRevenue(testLine, 1);
			Assert.AreEqual(0, revenue);
		}

		[TestMethod]
		public void Revenue_Should_Not_Be_Negative_With_0_Shippment_Costs()
		{
			OrderLine testLine = new OrderLine();
			testLine.BasePrice = 3.95;
			testLine.UnitPrice = 3.95;
			testLine.Price = 0;
			testLine.Quantity = 1;

			var revenue = DatcolHelper.GetRevenue(testLine, 1, 1);

			Assert.AreEqual(0, revenue);
		}

		[TestMethod]
		public void Get_BTW_Code_Should_Retrieve_1_For_21_Taxed_Items()
		{
			Assert.AreEqual(1, DatcolHelper.GetBTWCode("1234"));
		}

		[TestMethod]
		public void Get_BTW_Code_Should_Retrieve_2_For_Below_21_Items()
		{
			Assert.AreEqual(2, DatcolHelper.GetBTWCode("7864"));
		}

		[TestMethod]
		public void Set_Discount_Order_Line_Without_Any_Discounts_Should_Return_False()
		{
			OrderLine line = new OrderLine();

			Assert.AreEqual(false, DatcolHelper.IsSetSale(line));
		}

		[TestMethod]
		public void Set_Discount_Order_Line_With_A_Set_Discount_Should_Return_True()
		{
			OrderLine line = new OrderLine()
			{
				OrderLineAppliedDiscountRules = new List<OrderLineAppliedDiscountRule>() { 
					new OrderLineAppliedDiscountRule{
						IsSet = true
					}
				}
			};

			Assert.AreEqual(true, DatcolHelper.IsSetSale(line));
		}

		[TestMethod]
		public void Set_Discount_Order_Line_With_Any_Discounts_But_No_Set_Discount_Should_Return_False()
		{
			OrderLine line = new OrderLine()
			{
				OrderLineAppliedDiscountRules = new List<OrderLineAppliedDiscountRule>() { 
					new OrderLineAppliedDiscountRule{
						IsSet = false
					}
				}
			};

			Assert.AreEqual(false, DatcolHelper.IsSetSale(line));
		}

		[TestMethod]
		public void CJP_Discount_Order_Line_Without_Discounts_Should_Return_False()
		{
			OrderLine line = new OrderLine();
			string cjpName = "CJP";

			Assert.AreEqual(false, ATDatcolHelper.IsCJPSale(line, cjpName));
		}

		[TestMethod]
		public void CJP_Discount_Order_Line_With_Discounts_And_No_CJP_Should_Return_False()
		{
			OrderLine line = new OrderLine()
			{
				OrderLineAppliedDiscountRules = new List<OrderLineAppliedDiscountRule>() { 
					new OrderLineAppliedDiscountRule{
						IsSet = true
					}
				}
			};
			string cjpName = "CJP";

			Assert.AreEqual(false, ATDatcolHelper.IsCJPSale(line, cjpName));
		}

		[TestMethod]
		public void CJP_Discount_Order_Line_With_Discounts_And_CJP_Should_Return_True()
		{
			OrderLine line = new OrderLine()
			{
				OrderLineAppliedDiscountRules = new List<OrderLineAppliedDiscountRule>() { 
					new OrderLineAppliedDiscountRule{
						Code = "CJP"
					}
				}
			};
			string cjpName = "CJP";

			Assert.AreEqual(true, ATDatcolHelper.IsCJPSale(line, cjpName));
		}
	}
}
