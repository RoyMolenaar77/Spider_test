using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Concentrator.Plugins.PFA.Helpers;

namespace Concentrator.Plugins.PFA.Tests
{
	[TestClass]
	public class TaxRateTests
	{
		[TestMethod]
		public void TaxRate_Parsing_Should_Take_Rate_Of_Active_Date()
		{
			AssortmentHelper help = new AssortmentHelper();
			var dates = "10/01/2012;01/01/2001;01/01/1980;08/07/1996;08/07/1996;08/07/1996;08/07/1996;08/07/1996;08/07/1996;08/07/1996";
			var rates = "21;19;17.5;0;0;0;0;0;0;0";
			var code = "1";
			var rate = help.GetCurrentBTWRate(rates, dates, 18, code);
			Assert.AreEqual(21, rate);
		}

		[TestMethod]
		public void TaxRate_Parsing_Should_Return_Default_Rate_If_Empty_Params()
		{
			AssortmentHelper help = new AssortmentHelper();
			var dates = "";
			var rates = "";
			var code = "1";

			var rate = help.GetCurrentBTWRate(rates, dates, 18, code);
			Assert.AreEqual(18, rate);
		}

		[TestMethod]
		public void TaxRate_Parsing_Should_Return_Default_Rate_If_Exception()
		{
			AssortmentHelper help = new AssortmentHelper();
			var dates = "3123q2rt3q";
			var rates = "";
			var code = "1";

			var rate = help.GetCurrentBTWRate(rates, dates, 18, code);
			Assert.AreEqual(18, rate);
		}

		[TestMethod]
		public void TaxRate_Parsing_Should_Still_Work_Without_A_Rate_Code()
		{
			AssortmentHelper help = new AssortmentHelper();
			var dates = "3123q2rt3q";
			var rates = "";
			var code = "";

			var rate = help.GetCurrentBTWRate(rates, dates, 18, code);
			Assert.AreEqual(18, rate);
		}
	}
}
