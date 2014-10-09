using System;
using System.Collections.Generic;
using System.Configuration;

namespace Concentrator.Configuration
{
  public sealed class CommercialElement : ConfigurationElement
  {
    private const String DisplayAveragePricePropertyName = "displayAveragePrice";

    [ConfigurationProperty(DisplayAveragePricePropertyName, DefaultValue = true)]
    public Boolean DisplayAveragePrice
    {
      get
      {
        return Convert.ToBoolean(this[DisplayAveragePricePropertyName]);
      }
    }

    private const string DisplayPriceColorsPropertyName = "displayPriceColors";

    [ConfigurationProperty(DisplayPriceColorsPropertyName, DefaultValue = true)]
    public Boolean DisplayPriceColors
    {
      get
      {
        return Convert.ToBoolean(this[DisplayPriceColorsPropertyName]);
      }
    }

    private const String DisplayCurrencySymbolPropertyName = "displayCurrencySymbol";

    [ConfigurationProperty(DisplayCurrencySymbolPropertyName, DefaultValue = true)]
    public Boolean DisplayCurrencySymbol
    {
      get
      {
        return Convert.ToBoolean(this[DisplayCurrencySymbolPropertyName]);
      }
    }

    private const String FixedPriceRequiresProductPropertyName = "fixedPriceRequiresProduct";

    [ConfigurationProperty(FixedPriceRequiresProductPropertyName, DefaultValue = true)]
    public Boolean FixedPriceRequiresProduct
    {
      get
      {
        return Convert.ToBoolean(this[FixedPriceRequiresProductPropertyName]);
      }
    }

    private const String CurrencySymbolPropertyName = "currencySymbol";

    [ConfigurationProperty(CurrencySymbolPropertyName, DefaultValue = "€")]
    public String CurrencySymbol
    {
      get
      {
        return (string)this[CurrencySymbolPropertyName];
      }
    }

  }
}
