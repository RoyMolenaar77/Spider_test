using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Concentrator.Tasks
{
  using Objects.DataAccess.Repository;
  using Objects.Models;
  using Objects.Models.Connectors;

  /// <summary>
  /// Represents the base for a typical exporter task.
  /// </summary>
  public abstract class ConnectorTaskBase : ContextTaskBase<Connector>
  {
    /// <summary>
    /// Returns the connector repository including the connectorsettings
    /// </summary>
    protected override IRepository<Connector> ContextRepository
    {
      get
      {
        return Unit.Scope
          .Repository<Connector>()
          .Include(connector => connector.ConnectorSettings)
          .Include(connector => connector.ConnectorSystem);
      }
    }

    /// <summary>
    /// Apply the connector setting values to each field or property decorated with the <see cref="Concentrator.Tasks.ConnectorSettingAttribute"/> using the current connector.
    /// </summary>
    protected virtual Boolean ApplyConnectorSettings()
    {
      var connectorSettingDictionary = Context.ConnectorSettings.ToDictionary(
        connectorSetting => connectorSetting.SettingKey,
        connectorSetting => connectorSetting.Value,
        StringComparer.CurrentCultureIgnoreCase);

      var result = true;

      TraverseConnectorSettingAttributeMembers((memberInfo, connectorSettingAttribute) =>
      {
        var memberType = default(Type);

        switch (memberInfo.MemberType)
        {
          case MemberTypes.Field:
            memberType = ((FieldInfo)memberInfo).FieldType;
            break;

          case MemberTypes.Property:
            memberType = ((PropertyInfo)memberInfo).PropertyType;
            break;
        }

        var connectorSettingValue = String.Empty;
        var memberValue = connectorSettingAttribute.DefaultValue;

        if (connectorSettingDictionary.TryGetValue(connectorSettingAttribute.SettingKey, out connectorSettingValue))
        {
          try
          {
            memberValue = TypeConverterService.ConvertFromString(memberType, connectorSettingValue);
          }
          catch (Exception exception)
          {
            result = false;

            TraceCritical(exception);
          }
        }
        else if (connectorSettingAttribute.IsRequired)
        {
          result = false;

          TraceError("{0}: Connector setting '{1}' is required, but is not defined.", Context.Name, connectorSettingAttribute.SettingKey);
        }

        switch (memberInfo.MemberType)
        {
          case MemberTypes.Field:
            ((FieldInfo)memberInfo).SetValue(this, memberValue);
            break;

          case MemberTypes.Property:
            ((PropertyInfo)memberInfo).SetValue(this, memberValue, null);
            break;
        }
      });

      return result;
    }

    protected virtual void PersistConnectorSettings()
    {
      var connectorSettingDictionary = Context.ConnectorSettings.ToDictionary(connectorSetting => connectorSetting.SettingKey, StringComparer.CurrentCultureIgnoreCase);

      TraverseConnectorSettingAttributeMembers((memberInfo, connectorSettingAttribute) =>
      {
        var connectorSetting = default(ConnectorSetting);

        if (connectorSettingDictionary.TryGetValue(connectorSettingAttribute.SettingKey, out connectorSetting))
        {
          var memberValue = default(Object);

          switch (memberInfo.MemberType)
          {
            case MemberTypes.Field:
              memberValue = ((FieldInfo)memberInfo).GetValue(this);
              break;

            case MemberTypes.Property:
              memberValue = ((PropertyInfo)memberInfo).GetValue(this, null);
              break;
          }

          connectorSetting.Value = TypeConverterService.ConvertToString(memberValue);
        }
      });

      Unit.Save();
    }

    private void TraverseConnectorSettingAttributeMembers(Action<MemberInfo, ConnectorSettingAttribute> action)
    {
      if (action != null)
      {
        foreach (var memberInfo in GetType().GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
          if (memberInfo.MemberType == MemberTypes.Field || memberInfo.MemberType == MemberTypes.Property)
          {
            var connectorSettingAttribute = memberInfo.GetCustomAttribute<ConnectorSettingAttribute>(true);

            if (connectorSettingAttribute != null)
            {
              action.Invoke(memberInfo, connectorSettingAttribute);
            }
          }
        }
      }
    }

    protected override void ExecuteContextTask()
    {
      if (ApplyConnectorSettings())
      {
        ExecuteConnectorTask();
        PersistConnectorSettings();
      }
    }

    protected abstract void ExecuteConnectorTask();

    /// <summary>
    /// Fires the ValidateMandatorySettings and base.ValidateContext methods.
    /// </summary>
    /// <returns>True if ValidateMandatorySettings and base.ValidateContext return true</returns>
    protected override bool ValidateContext()
    {
      return base.ValidateContext();
    }
  }
}
