using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace Concentrator.Configuration
{
  public class ProductBrowserElement : ConfigurationElement
  {
    private const String GeneralElementName = "general";

    [ConfigurationProperty(GeneralElementName, IsDefaultCollection = true)]
    [ConfigurationCollection(typeof(GeneralElementCollection), AddItemName = "attribute")]
    public GeneralElementCollection General
    {
      get
      {
        return this[GeneralElementName] as GeneralElementCollection ?? new GeneralElementCollection();
      }
    }
  }
}
