using System;

namespace Concentrator.Tasks
{
  [AttributeUsage(AttributeTargets.Property)]
  public sealed class VendorSettingAttribute : Attribute
  {
    public Object DefaultValue
    {
      get;
      private set;
    }

    public String SettingKey
    {
      get;
      private set;
    }

    public Boolean IsRequired 
    { 
      get; 
      private set; 
    }

    public VendorSettingAttribute(String settingKey)
      : this(settingKey, null)
    {
    }

    public VendorSettingAttribute(String settingKey, Object defaultValue)
    {
      DefaultValue = defaultValue;
      SettingKey = settingKey;
    }

    public VendorSettingAttribute(String settingKey, Boolean isRequired)
    {
      SettingKey = settingKey;
      IsRequired = isRequired;
    }
  }
}
