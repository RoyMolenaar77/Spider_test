using System;
using System.Diagnostics;
using System.Linq;

namespace System.Collections.Generic
{
  public static class DictionaryExtensions
  {
    /// <summary>
    /// Gets the value associated with the key or if the key does not exists returns the default value.
    /// </summary>
    [DebuggerNonUserCode]
    public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
    {
      return dictionary.GetValueOrDefault(key, default(TValue));
    }

    /// <summary>
    /// Gets the value associated with the key or if the key does not exists it returns the specified default value.
    /// </summary>
    [DebuggerNonUserCode]
    public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue)
    {
      if (dictionary == null)
      {
        throw new NullReferenceException("dictionary");
      }

      var value = default(TValue);

      return dictionary.TryGetValue(key, out value) ? value : defaultValue;
    }

    /// <summary>
    /// Gets the value associated with the key or if the key does not exists it invokes the specified function to return a default value.
    /// </summary>
    [DebuggerNonUserCode]
    public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TValue> defaultValueFactory)
    {
      if (dictionary == null)
      {
        throw new NullReferenceException("dictionary");
      }

      if (defaultValueFactory == null)
      {
        throw new ArgumentNullException("defaultValueFactory");
      }

      var value = default(TValue);

      return dictionary.TryGetValue(key, out value) ? value : defaultValueFactory();
    }
  }
}
