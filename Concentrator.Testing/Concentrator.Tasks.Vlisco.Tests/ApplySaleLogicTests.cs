using System;
using Concentrator.Objects.Models.Contents;
using Concentrator.Tasks.Vlisco.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Concentrator.Tasks.Vlisco.Tests
{
  [TestClass]
  public class ApplySaleLogicTests
  {
    [TestMethod]
    public void ApplySaleLogic_Changes_ProFrom_And_ProTo_Dates_And_CountryCode_When_Supplied_ContentPrice()
    {
      var newArticle = new Article();
      var fromDate = DateTime.Now;
      var toDate = DateTime.Now.AddDays(7);

      var newPriceRule = new ContentPrice
      {
        UnitPriceIncrease = 0.8M,
        FromDate = fromDate,
        ToDate = toDate
      };

      newArticle.ApplySaleLogic(newArticle, newPriceRule);

      Assert.IsTrue(
        newArticle.ProFrom == fromDate.ToString(Constants.SaleDateFormat) &&
        newArticle.ProTo == toDate.ToString(Constants.SaleDateFormat) &&
        newArticle.CountryCode == Constants.SaleTarrif
      );
    }

    [TestMethod]
    public void ApplySaleLogic_DoesNotChange_ProFrom_And_ProTo_Dates_When_Supplied_ContentPrice_Without_ProFrom_Or_ProTo_Dates()
    {
      var newArticle = new Article();
      var newPriceRule = new ContentPrice
      {
        UnitPriceIncrease = 0.8M
      };

      newArticle.ApplySaleLogic(newArticle, newPriceRule);

      Assert.IsTrue(
        String.IsNullOrEmpty(newArticle.ProFrom) &&
        String.IsNullOrEmpty(newArticle.ProTo) &&
        newArticle.CountryCode != Constants.SaleTarrif
      );
    }
  }
}
