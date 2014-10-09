using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security;
using System.Linq.Expressions;
using System.Reflection;

namespace System
{
  public static class Extensions
  {
    public static void ForEach<T>(this IEnumerable<T> collection, Action<T, int> action)
    {
      if (collection == null) return;
      
      var e = collection.GetEnumerator();
      int i = 0;

      while (e.MoveNext())
      {
        action(e.Current, i);
        i++;
      }
    }

    public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action)
    {
      collection.ForEach((x, y) => action(x));
    }

    public static string Try<T>(this T instance, Func<T, string> action)
    {
      return Try(instance, action, String.Empty);
    }

    public static R Try<T, R>(this T instance, Func<T, R> action)
    {
      return Try(instance, action, default(R));
    }

    public static R Try<T, R>(this T instance, Func<T, R> action, R defaultValue)
    {
      return Try<NullReferenceException, T, R>(instance, action, defaultValue);
    }

    public static R Try<TException, T, R>(this T instance, Func<T, R> action, R defaultValue)
        where TException : Exception
    {
      if (instance == null)
        return defaultValue;

      var rType = typeof(R);
      try
      {
        if (rType.IsValueType)
          return action(instance);
        else
          return (R)((object)action(instance) ?? defaultValue);
      }
      catch (TException)
      {
        return defaultValue;
      }
      catch (Exception)
      {
        return defaultValue;
      }
    }

    public static IEnumerable<T> RemoveDuplicates<T>(this IEnumerable<T> instance, Func<T, object> selector)
    {
      return instance.Distinct();
    }


    public static IEnumerable<T> CleanDuplicates<T, TProperty>(this IEnumerable<T> instance, Func<T, TProperty> selector)
    {
      var result = new List<T>();

      var list = new List<TProperty>();


      foreach (var o in instance)
      {
        var prop = selector.Invoke(o);
        var val = (TProperty)o.GetType().GetProperty(prop.GetType().Name).GetValue(o, null);

        if (!list.Contains(val))
        {
          list.Add(val);
          result.Add(o);
        }
      }
      return result;
    }
  }
}