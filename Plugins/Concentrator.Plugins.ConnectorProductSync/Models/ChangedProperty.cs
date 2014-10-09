using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Plugins.ConnectorProductSync.Models
{
  public class ChangedProperty
  {
    public ChangedProperty(string propertyName, object newValue)
    {
      PropertyName = propertyName;
      NewValue = newValue;
    }

    public string PropertyName { get; private set; }
    public object NewValue { get; private set; }
  }
}
