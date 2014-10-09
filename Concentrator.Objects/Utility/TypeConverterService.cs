using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace System.ComponentModel
{
  /// <summary>
  /// Represents a thread-safe type conversion service.
  /// </summary>
  public class TypeConverterService
  {
    public static TypeConverterService Default
    {
      get;
      private set;
    }

    static TypeConverterService()
    {
      Default = new TypeConverterService();
    }

    private ConcurrentDictionary<Type, TypeConverter> Cache
    {
      get;
      set;
    }

    private TypeConverterService()
    {
      Cache = new ConcurrentDictionary<Type, TypeConverter>();
    }

    public TypeConverter this[Type type]
    {
      get
      {
        return Cache.GetOrAdd(type, delegate
        {
          return TypeDescriptor.GetConverter(type);
        });
      }
    }

    public static Boolean CanConvertFrom(Type sourceType, Type destinationType)
    {
      return Default[destinationType].CanConvertFrom(sourceType);
    }

    public static Boolean CanConvertTo(Type sourceType, Type destinationType)
    {
      return Default[sourceType].CanConvertTo(destinationType);
    }

    public static Object ConvertFrom(Type type, Object value, CultureInfo culture = null)
    {
      return Default[type].ConvertFrom(null, culture, value);
    }

    public static TResult ConvertFrom<TResult>(Object value, CultureInfo culture = null)
    {
      return (TResult)ConvertFrom(typeof(TResult), value, culture);
    }

    public static Object ConvertFromString(Type type, String value, CultureInfo culture = null)
    {
      return Default[type].ConvertFromString(null, culture, value);
    }

    public static TResult ConvertFromString<TResult>(String value, CultureInfo culture = null)
    {
      return (TResult)ConvertFromString(typeof(TResult), value, culture);
    }
    
    public static String ConvertToString(Object value, CultureInfo culture = null)
    {
      if (value == null)
      {
        throw new ArgumentNullException("value");
      }

      return Default[value.GetType()].ConvertToString(null, culture, value);
    }
  }
}
