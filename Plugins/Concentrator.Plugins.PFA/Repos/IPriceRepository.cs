using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Plugins.PFA.Models;

namespace Concentrator.Plugins.PFA.Repos
{
  public interface IPriceRepository
  {
    /// <summary>
    /// Retrieves product prices
    /// </summary>
    /// <param name="productCode">The artikel code/vendor item number</param>
    /// <param name="currencyCode">THe currency code in which to retrieve the prices</param>
    /// <returns></returns>
    List<PriceResult> GetProductPriceRules(string productCode, string currencyCode);
  }
}
