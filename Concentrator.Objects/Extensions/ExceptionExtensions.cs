using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System
{
  public static class ExceptionExtensions
  {
    /// <summary>
    /// Throws an exception if instance is null
    /// </summary>
    /// <param name="instance"></param>
    /// <param name="message">Message to be used if parameter e is null</param>
    /// <param name="e"></param>
    public static void ThrowIfNull(this object instance, string message = "Object cannot be null", Exception e = null)
    {
      if (instance == null)
        throw e ?? new NullReferenceException(message);
    }

    /// <summary>
    /// Throws an argumentnullexception if the instance is null
    /// </summary>
    /// <param name="instance"></param>
    /// <param name="argName"></param>
    public static void ThrowArgNull(this object instance, string argName)
    {
      if (instance == null)
        throw new ArgumentNullException(argName);
    }

    /// <summary>
    /// Throws an exception if predicate is true
    /// </summary>
    /// <typeparam name="TObject"></typeparam>
    /// <param name="instance"></param>
    /// <param name="predicate">Predicate to check</param>
    /// <param name="message">Message to include in exception. If exception param is specified it will be with precedence</param>
    /// <param name="e">Exception. Message defaults to message param</param>
    public static void ThrowIf<TObject>(this TObject instance, Func<TObject, bool> predicate, string message = "Not a valid value", Exception e = null)
    {
      if (predicate.Invoke(instance))
        throw e ?? new Exception(message);
    }
  }
}
