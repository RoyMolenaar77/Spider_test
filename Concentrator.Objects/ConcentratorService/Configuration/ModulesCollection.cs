using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Concentrator.Objects.Configuration
{
  public class ModulesCollection : ConfigurationElementCollection
  {
    protected override ConfigurationElement CreateNewElement()
    {
      return new Module();
    }

    protected override object GetElementKey(ConfigurationElement element)
    {
      return ((Module)element).Name;
    }

    public Module this[int index]
    {
      get
      {
        return (Module)BaseGet(index);
      }
    }

    public new Module this[string Name]
    {
      get
      {
        return (Module)BaseGet(Name);
      }
    }

    public int IndexOf(Module environment)
    {
      return BaseIndexOf(environment);
    }

  }
}
