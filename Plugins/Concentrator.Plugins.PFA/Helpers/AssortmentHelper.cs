using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Plugins.PFA.Models;
using System.Globalization;


namespace Concentrator.Plugins.PFA.Helpers
{
  public class AssortmentHelper
  {
    private Dictionary<string, decimal> VatRateStorage;

    public AssortmentHelper()
    {
      VatRateStorage = new Dictionary<string, decimal>();
    }

    private DateTime? GetLatestColorRuleDate(List<PriceResult> priceRules, DateTime currentDate, string size_code, string color_code)
    {
      return (from pr in priceRules
              where pr.start_date <= currentDate
              && pr.color_code == color_code
              && (string.IsNullOrEmpty(pr.discount_code) || pr.discount_code.ToLower() == "omp")
              && (string.IsNullOrEmpty(pr.size_code) || (!string.IsNullOrEmpty(pr.size_code) && pr.size_code == size_code))
              select pr.start_date).Max();
    }

    private DateTime? GetLatestProductRuleDate(List<PriceResult> priceRules, DateTime currentDate, string size_code, string color_code)
    {
      return (from pr in priceRules
              where pr.start_date <= currentDate
              && string.IsNullOrEmpty(pr.color_code)
              && (string.IsNullOrEmpty(pr.discount_code) || pr.discount_code.ToLower() == "omp")
             && (string.IsNullOrEmpty(pr.size_code) || (!string.IsNullOrEmpty(pr.size_code) && pr.size_code == size_code))
             && pr.price != 0
              select pr.start_date).Max();
    }

    /// <summary>
    /// Gets the active price from the SKU
    /// from the list of price rules
    /// </summary>
    /// <param name="product_number"></param>
    /// <param name="color_code"></param>
    /// <param name="size_code"></param>
    /// <param name="priceRules"></param>
    /// <returns></returns>
    public decimal GetPrice(string product_number, string color_code, string size_code, List<PriceResult> priceRules, DateTime? currentDate = null)
    {
      decimal price = 0;

      var currentDateTime = currentDate.GetValueOrDefault(DateTime.Now.Date);

      var latestColorRuleDate = GetLatestColorRuleDate(priceRules, currentDateTime, size_code, color_code);

      var latestProductRuleDate = GetLatestProductRuleDate(priceRules, currentDateTime, size_code, color_code);

      if (latestColorRuleDate != null)
      {
        if (latestProductRuleDate == null || latestColorRuleDate >= latestProductRuleDate)
        {

          var foundRule = priceRules.FirstOrDefault(c => c.start_date == latestColorRuleDate && c.color_code == color_code && (string.IsNullOrEmpty(c.size_code) || (!string.IsNullOrEmpty(c.size_code) && c.size_code == size_code)));

          if (foundRule != null)
            return foundRule.price;

          return price;
        }

      }
      if (latestProductRuleDate != null)
      {
        var foundRule = priceRules.FirstOrDefault(c => c.start_date == latestProductRuleDate && string.IsNullOrEmpty(c.color_code) && string.IsNullOrEmpty(c.size_code));

        if (foundRule != null)
          price = foundRule.price;

      }

      return price;
    }

    /// <summary>
    /// Retrieves the discount of a product
    /// </summary>
    /// <param name="productNumber"></param>
    /// <param name="color_code"></param>
    /// <param name="size_code"></param>
    /// <param name="priceRules">A list of price rules retrieved by PFA</param>
    /// <param name="currentDate">Optional time for controlled tests </param>
    /// <returns></returns>
    public decimal? GetDiscount(string productNumber, string color_code, string size_code, List<PriceResult> priceRules, DateTime? currentDate = null)
    {
      decimal? discount = null;

      var currentDateTime = currentDate.GetValueOrDefault(DateTime.Now.Date);

      var latestColorRuleDate = (from pr in priceRules
                                 where pr.start_date <= currentDateTime
                                 && pr.color_code == color_code
                                 && (!pr.end_date.HasValue || (pr.end_date.HasValue && pr.end_date.Value >= currentDateTime))
                                 && (!string.IsNullOrEmpty(pr.discount_code) && pr.discount_code.ToLower() != "omp")
                                 select pr.start_date).Max();

      var latestProductRuleDate = (from pr in priceRules
                                   where pr.start_date <= currentDateTime
                                   && string.IsNullOrEmpty(pr.color_code)
                                   && (!pr.end_date.HasValue || (pr.end_date.HasValue && pr.end_date.Value >= currentDateTime))
                                   && (!string.IsNullOrEmpty(pr.discount_code) && pr.discount_code.ToLower() != "omp")
                                   select pr.start_date).Max();


      //check for an override newer price. UGLY. RE-WORK
      var latestColorRulePriceDate = GetLatestColorRuleDate(priceRules, currentDateTime, size_code, color_code);
      var latestProductRulePriceDate = GetLatestProductRuleDate(priceRules, currentDateTime, size_code, color_code);



      var latestDiscountDate = latestColorRuleDate.Try(c => c.Value, DateTime.MinValue) >= latestProductRuleDate.Try(c => c.Value, DateTime.MinValue) ? latestColorRuleDate : latestProductRuleDate;
      var latestPriceDate = latestColorRulePriceDate.Try(c => c.Value, DateTime.MinValue) >= latestProductRulePriceDate.Try(c => c.Value, DateTime.MinValue) ? latestColorRulePriceDate : latestProductRulePriceDate;

      var discountEndDate = GetDiscountEndDate(latestDiscountDate, priceRules);

      if (latestPriceDate > latestDiscountDate && (discountEndDate == null || discountEndDate < latestPriceDate)) return null;

      if (latestColorRuleDate != null)
      {
        if (latestProductRuleDate == null || latestColorRuleDate >= latestProductRuleDate)
        {
          discount = priceRules.FirstOrDefault(c => c.start_date == latestColorRuleDate && c.color_code == color_code).price;

          return discount; //short circuit here. We dont need to go deeper
        }
      }

      if (latestProductRuleDate != null)
        discount = priceRules.FirstOrDefault(c => c.start_date == latestProductRuleDate && string.IsNullOrEmpty(c.color_code)).price;

      return discount;
    }

    private DateTime? GetDiscountEndDate(DateTime? latestDiscountDate, List<PriceResult> prices)
    {
      return (from p in prices
              where !string.IsNullOrEmpty(p.discount_code) && p.discount_code != "omp"
              && p.start_date == latestDiscountDate
              select p.end_date).Max();
    }

    /// <summary>
    /// Generate the current date under which stock is maintained
    /// </summary>
    /// <returns></returns>
    public DateTime GetCurrentStockDate()
    {
      var currentStockDate = DateTime.Now;

      while (currentStockDate.DayOfWeek != DayOfWeek.Saturday)
        currentStockDate = currentStockDate.AddDays(1);
      return currentStockDate;

    }

    /// <summary>
    /// Retrieves the current BTW rate by parsing the PFA btw rules
    /// </summary>
    /// <param name="pfaBtwRates">A ; separated string of rates</param>
    /// <param name="pfaBtwDates">A ; separated string of start dates</param>
    /// /// <param name="pfaBtwCode">The code of the btw rate. Used for caching of the results</param>
    /// <returns></returns>
    public decimal GetCurrentBTWRate(string pfaBtwRates, string pfaBtwDates, decimal defaultRate, string pfaBtwCode)
    {
      if (!string.IsNullOrEmpty(pfaBtwCode))
      {
        if (VatRateStorage.ContainsKey(pfaBtwCode)) return VatRateStorage[pfaBtwCode];
      }

      try
      {
        var rates = (from p in pfaBtwRates.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                     select decimal.Parse(p, CultureInfo.InvariantCulture)).ToArray();

        var dates = (from ds in pfaBtwDates.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                     select DateTime.ParseExact(ds, "MM/dd/yyyy", CultureInfo.InvariantCulture)).ToArray();
        decimal currentBtwRate = defaultRate;

        for (int i = 0; i < rates.Count(); i++)
        {
          if (DateTime.Now >= dates[i])
          {
            currentBtwRate = rates[i];
            break;
          }
        }
        if (!string.IsNullOrEmpty(pfaBtwCode))
        {
          VatRateStorage[pfaBtwCode] = currentBtwRate;
        }
        return currentBtwRate;
      }
      catch (Exception e)
      {
        return defaultRate;
      }
    }
  }
}
