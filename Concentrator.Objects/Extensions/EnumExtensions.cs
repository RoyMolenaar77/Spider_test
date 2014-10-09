using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Data.Linq.Mapping;
using Concentrator.Objects.Models.Connectors;

namespace System
{

  public class DescriptionAttribute : Attribute
  {
    public string Description;
    public DescriptionAttribute(string text)
    {
      Description = text;
    }
  }

  public static class Enums
  {
    public static IEnumerable<T> Get<T>()
    {
      return System.Enum.GetValues(typeof(T)).Cast<T>();
    }
  }



  public static class EnumerationExtensions
  {

    public static TAttribute GetAttribute<TAttribute>(this Enum source)
    {
      var sourceType = source.GetType();
      var name = source.ToString();
      var memberInfo = sourceType.GetMember(name);

      var attributes = memberInfo[0].GetCustomAttributes(false)
                              .Cast<Attribute>()
                              .ToArray();

      return attributes.OfType<TAttribute>().FirstOrDefault();
    }

    public static Expression<Func<int, bool>> Has2(int value)
    {

      return (x => (((int)x & (int)value) == (int)value)
        );

      //return (((int)(object)type & (int)(object)value) == (int)(object)value);
    }

    /// <summary>
    /// Use in linq-to-sql context
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="type"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool HasD<T>(this Enum type, T value)
    {
      Expression<Func<int, int, bool>> has = (p, q) => (p & q) == q;
      var a = has.Compile();
      return a.Invoke((int)(object)type, (int)(object)value);
    }


    public static bool Has<T>(this System.Enum type, T value)
    {
      try
      {
        return ((Convert.ToInt32(type) & Convert.ToInt32(value)) == Convert.ToInt32(value));
      }
      catch
      {
        return false;
      }
    }

    public static List<int> GetActiveList(this System.Enum type, Concentrator.Objects.Models.Connectors.Connector connector)
    {
      List<int> eList = new List<int>();

      foreach (var e in Enum.GetValues(type.GetType()))
      {
        if (((ConnectorType)connector.ConnectorType).Has(e))
          eList.Add((int)e);
      }

      return eList;
    }

    public static List<string> GetList<T>(this System.Enum type)
    {
      return Enum.GetNames(type.GetType()).ToList();
    }

    public static bool Is<T>(this System.Enum type, T value)
    {
      try
      {
        return (int)(object)type == (int)(object)value;
      }
      catch
      {
        return false;
      }
    }


    public static T Add<T>(this System.Enum type, T value)
    {
      try
      {
        return (T)(object)(((int)(object)type | (int)(object)value));
      }
      catch (Exception ex)
      {
        throw new ArgumentException(
            string.Format(
                "Could not append value from enumerated type '{0}'.",
                typeof(T).Name
                ), ex);
      }
    }


    public static T Remove<T>(this System.Enum type, T value)
    {
      try
      {
        return (T)(object)(((int)(object)type & ~(int)(object)value));
      }
      catch (Exception ex)
      {
        throw new ArgumentException(
            string.Format(
                "Could not remove value from enumerated type '{0}'.",
                typeof(T).Name
                ), ex);
      }
    }

    public static T Max<T>(this Enum type, T value)
    {
      try
      {
        var one = (int)(object)type;
        var two = (int)(object)value;
        return (T)(object)(one > two ? one : two);
      }
      catch (Exception ex)
      {
        throw new ArgumentException(string.Format("Could not set max of enumerated type '{0}'.", typeof(T).Name), ex);
      }
    }

    public static Dictionary<T, string> EnumToList<T>()
    {
      Type enumType = typeof(T);

      // Can't use type constraints on value types, so have to do check like this
      if (enumType.BaseType != typeof(Enum))
        throw new ArgumentException("T must be of type System.Enum");

      Array enumValArray = Enum.GetNames(enumType);

      Dictionary<T, string> enumValList = new Dictionary<T, string>(enumValArray.Length);

      foreach (string val in enumValArray)
      {
        var fi = typeof(T).GetField(val);
        var da = (DescriptionAttribute)Attribute.GetCustomAttribute(fi,
                           typeof(DescriptionAttribute));

        enumValList.Add((T)Enum.Parse(typeof(T), val), da.Description);
      }

      return enumValList;
    }


  }

}
