using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace Concentrator.Configuration
{
  public class GeneralElementCollection : ConfigurationElementCollection
  {
    private const String DisplaySummaryFieldsPropertyName = "displaySummaryFields";
    private const String CreateAttributeByVendorPropertyName = "createAttributeByVendor";

    [ConfigurationProperty(DisplaySummaryFieldsPropertyName, DefaultValue = false)]
    public Boolean DisplaySummaryFields
    {
      get
      {
        return Convert.ToBoolean(base[DisplaySummaryFieldsPropertyName]);
      }
    }

    [ConfigurationProperty(CreateAttributeByVendorPropertyName, DefaultValue = true)]
    public Boolean CreateAttributeByVendor
    {
      get
      {
        return Convert.ToBoolean(base[CreateAttributeByVendorPropertyName]);
      }
    }

    protected override ConfigurationElement CreateNewElement()
    {
      return new GeneralAttributeElement();
    }

    protected override object GetElementKey(ConfigurationElement element)
    {
      return ((GeneralAttributeElement)element).Code;
    }

    public GeneralAttributeElement this[int index]
    {
      get
      {
        return (GeneralAttributeElement)BaseGet(index);
      }
    }

    public new GeneralAttributeElement this[string Name]
    {
      get
      {
        return (GeneralAttributeElement)BaseGet(Name);
      }
    }

    public int IndexOf(GeneralAttributeElement attribute)
    {
      return BaseIndexOf(attribute);
    }
  }
}
