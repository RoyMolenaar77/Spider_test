using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace Concentrator.Configuration
{
  public class GeneralAttributeElement : ConfigurationElement
  {
    private const string CodePropertyName = "code";
    private const string TypePropertyName = "type";
    private const string DisplayNamePropertyName = "displayName";
    private const string DefaultValuePropertyName = "defaultValue";

    [ConfigurationProperty(CodePropertyName, IsRequired = true, DefaultValue = null)]    
    public string Code
    {
      get
      {
        return (string)this[CodePropertyName];
      }
    }

    [ConfigurationProperty(DisplayNamePropertyName, IsRequired = true, DefaultValue = null)]
    public string DisplayName
    {
      get
      {
        return (string)this[DisplayNamePropertyName];
      }
    }

    [ConfigurationProperty(TypePropertyName, IsRequired = false, DefaultValue = null)]
    public string Type
    {
      get
      {
        return (string)this[TypePropertyName];
      }
    }

    [ConfigurationProperty(DefaultValuePropertyName, IsRequired = false, DefaultValue = null)]
    public string DefaultValue
    {
      get
      {
        return (string)this[DefaultValuePropertyName];
      }
    } 
  }
}
