using System;

namespace Concentrator.Tasks
{
  [AttributeUsage(AttributeTargets.Property)]
  public sealed class ConnectorSettingAttribute : Attribute
  {
    public Object DefaultValue
    {
      get;
      private set;
    }

    public Boolean IsRequired 
    { 
      get; 
      private set; 
    }

    public String SettingKey
    {
      get;
      private set;
    }

    public ConnectorSettingAttribute(String settingKey, Boolean isRequired = true)
      : this(settingKey, null, isRequired)
    {
    }

    public ConnectorSettingAttribute(String settingKey, Object defaultValue, Boolean isRequired = true)
    {
      DefaultValue = defaultValue;
      IsRequired = isRequired;
      SettingKey = settingKey;
    }
  }
}
