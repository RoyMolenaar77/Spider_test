using System;
using System.Collections.Generic;
using System.Linq;

namespace System.Reflection
{
  public static class MemberInfoExtensions
  {
    /// <summary>
    /// When overridden in a derived class, returns a custom attribute applied to this member and identified by System.Type.
    /// </summary>
    public static TCustomAttribute GetCustomAttribute<TCustomAttribute>(this MemberInfo memberInfo, Boolean inherit)
      where TCustomAttribute : System.Attribute
    {
      return (TCustomAttribute)memberInfo.GetCustomAttributes(typeof(TCustomAttribute), inherit).SingleOrDefault();
    }

    /// <summary>
    /// When overridden in a derived class, returns an array of custom attributes applied to this member and identified by System.Type.
    /// </summary>
    public static IEnumerable<TCustomAttribute> GetCustomAttributes<TCustomAttribute>(this MemberInfo memberInfo, Boolean inherit)
      where TCustomAttribute : System.Attribute
    {
      return memberInfo.GetCustomAttributes(typeof(TCustomAttribute), inherit).Cast<TCustomAttribute>();
    }
  }
}
