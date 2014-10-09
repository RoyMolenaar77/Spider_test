using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Concentrator.Objects.Environments
{
  internal class EnvironmentConfigSection : ConfigurationSection
  {
    [ConfigurationProperty("Current", DefaultValue = "false", IsRequired = false)]
    public string Current
    {
      get
      {
        return (string)this["Current"];
      }
      set
      {
        this["Current"] = value;
      }
    }

    [ConfigurationProperty("Environments", IsDefaultCollection = true)]
    [ConfigurationCollection(typeof(EnvironmentElementCollection),
        AddItemName = "Env",
        ClearItemsName = "Clear",
        RemoveItemName = "Remove")]
    public EnvironmentElementCollection Environments
    {
      get { return (EnvironmentElementCollection)this["Environments"]; }
    }
  }

  internal class EnvironmentElement : ConfigurationElement
  {
    [ConfigurationProperty("Name", IsRequired = true)]
    [StringValidator(InvalidCharacters = "~!@#$%^&*()[]{}/;'\"|\\", MinLength = 0, MaxLength = 20)]
    public string Name
    {
      get
      {
        return (string)this["Name"];
      }
      set
      {
        this["Name"] = value;
      }
    }

    [ConfigurationProperty("Connection", IsRequired = true)]
    public string Connection
    {
      get
      {
        return (string)this["Connection"];
      }
      set
      {
        this["Connection"] = value;
      }
    }

    [ConfigurationProperty("IdentificationMethod", IsRequired = true)]
    public IdentificationMethodType IdentificationMethod
    {
      get
      {
        return (IdentificationMethodType)this["IdentificationMethod"];
      }
      set
      {
        this["IdentificationMethod"] = value;
      }
    }
  }

  public enum IdentificationMethodType
  {
    IdCode = 0,
    UserName = 1
  }

  internal class EnvironmentElementCollection : ConfigurationElementCollection
  {
    protected override ConfigurationElement CreateNewElement()
    {
      return new EnvironmentElement();
    }

    protected override object GetElementKey(ConfigurationElement element)
    {
      return ((EnvironmentElement)element).Name;
    }

    public EnvironmentElement this[int index]
    {
      get
      {
        return (EnvironmentElement)BaseGet(index);
      }
    }

    public new EnvironmentElement this[string Name]
    {
      get
      {
        return (EnvironmentElement)BaseGet(Name);
      }
    }

    public int IndexOf(EnvironmentElement environment)
    {
      return BaseIndexOf(environment);
    }


  }
}
