using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Concentrator.Objects.Extensions;

namespace Concentrator.Objects.Tests
{
	[TestClass]
	public class StringExtensionsTests
	{
		[TestMethod]
		public void DateTimeNull_TryParse_On_Empty_String_Should_Return_Null() {
			string s = string.Empty;

			Assert.AreEqual(null, s.ParseToDateTime());
		}

		[TestMethod]
		public void DateTimeNull_TryParse_On_Null_String_Should_Return_Null()
		{
			string s = null;

			Assert.AreEqual(null, s.ParseToDateTime());
		}

		[TestMethod]
		public void DateTimeNull_TryParse_On_Valid_Date_String_String_Should_Not_Return_Null()
		{
			string s = "2012-08-15";
			var date = s.ParseToDateTime();
			Assert.IsNotNull(date);
		}

		[TestMethod]
		public void DateTimeNull_TryParse_On_Valid_Date_String_String_Should_Return_Valid_Date()
		{
			string s = "2012-08-15";
			var date = s.ParseToDateTime();
			
			Assert.AreEqual(2012, date.Value.Year);
			Assert.AreEqual(8, date.Value.Month);
			Assert.AreEqual(15, date.Value.Day);
		}
		
	}
}
