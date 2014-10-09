using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Concentrator.Plugins.PFA.Helpers;

namespace Concentrator.Plugins.PFA.Tests
{
	[TestClass]
	public class ATMediaTests
	{
		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		public void Sequence_Should_Throw_An_Exception_If_Extension_Is_Invalid()
		{
			var sequence = ATMediaHelper.GetImageSequence("asqwe");
		}

		[TestMethod]
		public void Sequence_Should_Be_0_If_FileName_Ends_In_F()
		{
			Assert.AreEqual(0, ATMediaHelper.GetImageSequence("f"));
		}

		[TestMethod]
		public void Sequence_Should_Be_1_If_FileName_Ends_In_B()
		{
			Assert.AreEqual(1, ATMediaHelper.GetImageSequence("b"));
		}

		[TestMethod]
		public void Sequence_Should_Be_3_If_FileName_Ends_In_L()
		{
			Assert.AreEqual(3, ATMediaHelper.GetImageSequence("l"));
		}

		[TestMethod]
		public void Sequence_Should_Be_4_If_FileName_Ends_In_H()
		{
			Assert.AreEqual(4, ATMediaHelper.GetImageSequence("h"));
		}
	}
}
