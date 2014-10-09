using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Objects.Extensions
{
  public static class ObjectExtensions
  {
    public static bool IsNullableValueType(this Type type)
    {
      return (type.IsGenericType && type.
        GetGenericTypeDefinition().Equals
        (typeof(Nullable<>)));
    }
  }
}
