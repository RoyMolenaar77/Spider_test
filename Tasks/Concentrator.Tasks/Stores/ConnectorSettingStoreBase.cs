using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Concentrator.Tasks
{
  using Concentrator.Tasks.Stores;
  using Objects.DataAccess.UnitOfWork;
  using Objects.Models.Connectors;

  public abstract class ConnectorSettingStoreBase : StoreBase
  {
    public Connector Connector
    {
      get;
      private set;
    }

    public ConnectorSettingStoreBase(Connector connector, TraceSource traceSource = null)
      : base(traceSource)
    {
      if (connector == null)
      {
        throw new ArgumentNullException("connector");
      }

      Connector = connector;
    }

    public override Boolean Load()
    {
      foreach (var property in GetProperties())
      {
        var connectorAttribute = property
          .GetCustomAttributes(false)
          .OfType<ConnectorSettingAttribute>()
          .SingleOrDefault();

        if (connectorAttribute != null)
        {
          var connectorSetting = Connector
            .ConnectorSettings
            .FirstOrDefault(item => item.SettingKey == connectorAttribute.SettingKey);

          if (connectorSetting == null && connectorAttribute.IsRequired)
          {
            TraceSource.TraceEvent(TraceEventType.Error, 0, "Unable to find connector setting '{0}' for connector '{1}'."
              , connectorAttribute.SettingKey
              , Connector.Name);

            return false;
          }

          var typeDescriptor = TypeDescriptor.GetConverter(property.PropertyType);

          if (typeDescriptor == null || !typeDescriptor.CanConvertFrom(typeof(String)))
          {
            TraceSource.TraceEvent(TraceEventType.Error, 0, "Unable to find a type converter for type '{0}' for connector setting '{1}'."
              , property.PropertyType.FullName
              , connectorAttribute.SettingKey);

            return false;
          }

          var value = typeDescriptor.ConvertFrom(connectorSetting.Value);

          property.SetValue(this, value, null);
        }
      }

      return true;
    }
  }
}
