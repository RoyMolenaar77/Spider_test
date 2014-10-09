using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace Concentrator.ui.Configuration
{
  public class CommercialConfigurationElement : ConfigurationElement
  {
    private const String DisplayAveragePricePropertyName = "displayAveragePrice";

    [ConfigurationProperty(DisplayAveragePricePropertyName, DefaultValue = true)]
    public Boolean DisplayAveragePrice
    {
      get
      {
        return (Boolean)this[DisplayAveragePricePropertyName];
      }
    }

    private const String DisplayCurrencySymbolPropertyName = "displayCurrencySymbol";

    [ConfigurationProperty(DisplayCurrencySymbolPropertyName, DefaultValue = true)]
    public Boolean DisplayCurrencySymbol
    {
      get
      {
        return (Boolean)this[DisplayCurrencySymbolPropertyName];
      }
    }
  }
}
