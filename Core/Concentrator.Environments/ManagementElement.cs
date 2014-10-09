using System;
using System.Configuration;

namespace Concentrator.Configuration
{
  public sealed class ManagementElement : ConfigurationElement
  {
    private const String CommercialPropertyName = "commercial";

    [ConfigurationProperty(CommercialPropertyName, IsRequired = false)]
    public CommercialElement Commercial
    {
      get
      {
        return base[CommercialPropertyName] as CommercialElement ?? new CommercialElement();
      }
    }

    private const String ProductBrowserPropertyName = "productBrowser";

    [ConfigurationProperty(ProductBrowserPropertyName, IsRequired = false)]
    public ProductBrowserElement ProductBrowser
    {
      get
      {
        return base[ProductBrowserPropertyName] as ProductBrowserElement ?? new ProductBrowserElement();
      }
    }
  }
}
