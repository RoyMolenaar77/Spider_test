using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Concentrator.Objects.Configuration
{
  internal class PluginsCollection : ConfigurationElementCollection
  {
    protected override ConfigurationElement CreateNewElement()
    {
      return new Plugin();
    }

    protected override object GetElementKey(ConfigurationElement element)
    {
      return ((Plugin)element).Name;
    }

    public Plugin this[int index]
    {
      get
      {
        return (Plugin)BaseGet(index);
      }
    }

    public new Plugin this[string Name]
    {
      get
      {
        return (Plugin)BaseGet(Name);
      }
    }

    public int IndexOf(Plugin environment)
    {
      return BaseIndexOf(environment);
    }

  }
}
