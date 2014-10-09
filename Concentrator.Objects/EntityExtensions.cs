using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Data.Linq;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.Models.Configuration;
using Concentrator.Objects.Models.Contents;
using Concentrator.Objects.Models;


namespace Concentrator.Objects
{
  public static class EntityExtensions
  {
    public static T GetValueByKey<T>(this ICollection<VendorSetting> set, string key, T defaultValue)
    {
      string value = set.Where(d => d.SettingKey == key.ToString()).Select(e => e.Value).FirstOrDefault();

      if (String.IsNullOrEmpty(value))
        return defaultValue;

      try
      {
        TypeConverter conv = TypeDescriptor.GetConverter(typeof(T));
        return (T)conv.ConvertFrom(value);
      }
      catch
      {
        return defaultValue;
      }
    }
    public static T GetValueByKey<T>(this ICollection<ConnectorSetting> set, string key, T defaultValue)
    {
      string value = set.Where(d => d.SettingKey == key.ToString()).Select(e => e.Value).FirstOrDefault();

      if (String.IsNullOrEmpty(value))
        return defaultValue;

      try
      {
        TypeConverter conv = TypeDescriptor.GetConverter(typeof(T));
        return (T)conv.ConvertFrom(value);
      }
      catch
      {
        return defaultValue;
      }
    }

    public static T GetValueByKey<T>(this ICollection<Config> set, string key, T defaultValue)
    {
      string value = set.Where(d => d.Name == key.ToString()).Select(e => e.Value).FirstOrDefault();

      if (String.IsNullOrEmpty(value))
        return defaultValue;

      try
      {
        TypeConverter conv = TypeDescriptor.GetConverter(typeof(T));
        return (T)conv.ConvertFrom(value);
      }
      catch
      {
        return defaultValue;
      }
    }

    private static decimal CalculateContentPrice(this ICollection<Content> set, int connectorID)
    {
      return 0;
    }
  }
}
