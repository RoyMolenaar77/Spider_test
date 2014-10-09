using System;
using System.ComponentModel;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace System.Xml.Linq
{
  /// <summary>
  /// Provides a collection of extension-methods.
  /// </summary>
  public static class XElementExtensions
  {
    // A static, thread-safe dictionary for fast-lookup of type converters.
    [Browsable(false)]
    internal static readonly IDictionary<Type, TypeConverter> TypeConverters = new ConcurrentDictionary<Type, TypeConverter>();

    static XElementExtensions()
    {
      TypeConverters.Add(typeof(Boolean), new BooleanConverter());
      TypeConverters.Add(typeof(Char), new CharConverter());
      TypeConverters.Add(typeof(Byte), new ByteConverter());
      TypeConverters.Add(typeof(Decimal), new DecimalConverter());
      TypeConverters.Add(typeof(Double), new DoubleConverter());
      TypeConverters.Add(typeof(Single), new SingleConverter());
      TypeConverters.Add(typeof(Int16), new Int16Converter());
      TypeConverters.Add(typeof(Int32), new Int32Converter());
      TypeConverters.Add(typeof(Int64), new Int64Converter());
      TypeConverters.Add(typeof(UInt16), new UInt16Converter());
      TypeConverters.Add(typeof(UInt32), new UInt32Converter());
      TypeConverters.Add(typeof(UInt64), new UInt64Converter());
      TypeConverters.Add(typeof(String), new StringConverter());
    }

    /// <summary>
    /// Returns the value of an xml-attribute if it exists or else returns the default value.
    /// </summary>
    /// <param name="attributeName">The name of the attribute.</param>
    /// <param name="defaultValue">The fallback value if the attribute doesn't exists.</param>
    public static String AttributeValue(this XElement element, String attributeName, String defaultValue = null)
    {
      return AttributeValue<String>(element, attributeName, defaultValue);
    }

    /// <summary>
    /// Returns the value of an xml-attribute if it exists or else returns the default value.
    /// </summary>
    /// <typeparam name="TValue">The type of the value to return.</typeparam>
    /// <param name="attributeName">The name of the attribute.</param>
    /// <param name="defaultValue">The fallback value if the attribute doesn't exists.</param>
    public static TValue AttributeValue<TValue>(this XElement element, String attributeName)
    {
      return AttributeValue(element, attributeName, default(TValue));
    }

    /// <summary>
    /// Returns the value of an xml-attribute if it exists or else returns the specified default value.
    /// </summary>
    /// <typeparam name="TValue">The type of the value to return.</typeparam>
    /// <param name="attributeName">The name of the attribute.</param>
    /// <param name="defaultValue">The fallback value if the attribute doesn't exists.</param>
    public static TValue AttributeValue<TValue>(this XElement element, String attributeName, TValue defaultValue)
    {
      element.ThrowIfNull("element");

      var attribute = element.Attribute(attributeName);

      if (attribute != null)
      {
        var typeConverter = default(TypeConverter);

        if (!TypeConverters.TryGetValue(typeof(TValue), out typeConverter))
        {
          typeConverter = TypeDescriptor.GetConverter(typeof(TValue));

          TypeConverters[typeof(TValue)] = typeConverter;
        }

        return (TValue)typeConverter.ConvertFromString(attribute.Value);
      }

      return defaultValue;
    }

    /// <summary>
    /// Returns the value of an xml-element if it exists or else returns the specified default value.
    /// </summary>
    /// <typeparam name="TValue">The type of the value to return.</typeparam>
    /// <param name="elementName">The name of the attribute.</param>
    /// <param name="defaultValue">The fallback value if the attribute doesn't exists.</param>
    public static String ElementValue(this XElement element, String elementName, String defaultValue = null)
    {
      return ElementValue<String>(element, elementName, defaultValue);
    }

    /// <summary>
    /// Returns the value of an xml-element if it exists or else returns the default value.
    /// </summary>
    /// <typeparam name="TValue">The type of the value to return.</typeparam>
    /// <param name="attributeName">The name of the attribute.</param>
    /// <param name="defaultValue">The fallback value if the attribute doesn't exists.</param>
    public static TValue ElementValue<TValue>(this XElement element, String elementPath)
    {
      return ElementValue(element, elementPath, default(TValue));
    }

    /// <summary>
    /// Returns the value of an xml-element if it exists or else returns the specified default value.
    /// </summary>
    /// <typeparam name="TValue">The type of the value to return.</typeparam>
    /// <param name="elementPath">The name of the attribute.</param>
    /// <param name="defaultValue">The fallback value if the attribute doesn't exists.</param>
    public static TValue ElementValue<TValue>(this XElement element, String elementPath, TValue defaultValue)
    {
      element.ThrowIfNull("element");

      var childNames = elementPath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
      var childElement = element;

      for (var index = 0; index < childNames.Length && childElement != null; index++)
      {
        childElement = childElement.Element(childNames[index]);
      }

      if (childElement != null)
      {
        var typeConverter = default(TypeConverter);

        if (!TypeConverters.TryGetValue(typeof(TValue), out typeConverter))
        {
          typeConverter = TypeDescriptor.GetConverter(typeof(TValue));

          TypeConverters[typeof(TValue)] = typeConverter;
        }

        return (TValue)typeConverter.ConvertFromString(childElement.Value);
      }

      return defaultValue;
    }
  }
}