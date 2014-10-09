using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace System
{
	public static class EnumerableExtensions
  {
    /// <summary>
    /// Appends the specified elements after the source elements.
    /// </summary>
    public static IEnumerable<TElement> Append<TElement>(this IEnumerable<TElement> source, params TElement[] elements)
    {
      foreach (var element in source)
      {
        yield return element;
      }

      foreach (var element in elements)
      {
        yield return element;
      }
    }

    /// <summary>
    /// Prepends the specified elements before the source elements.
    /// </summary>
    public static IEnumerable<TElement> Prepend<TElement>(this IEnumerable<TElement> source, params TElement[] elements)
    {
      foreach (var element in elements)
      {
        yield return element;
      }

      foreach (var element in source)
      {
        yield return element;
      }
    }

    /// <summary>
    /// Creates a <see cref="System.Data.DataTable"/> from the source, using reflection to define the data columns.
    /// </summary>
    /// <param name="predicate">
    /// A predicate that allows you to exclude or include specific properties.
    /// </param>
    [DebuggerStepThrough]
    public static DataTable ToDataTable<TElement>(this IEnumerable<TElement> source, Func<PropertyInfo, Boolean> predicate = null)
    {
      if (source == null)
      {
        throw new ArgumentNullException("source");
      }

      var result = new DataTable();
      var elementType = typeof(TElement);

      if (elementType.IsPrimitive || elementType.IsNullableValueType() && Nullable.GetUnderlyingType(elementType).IsPrimitive)
      {
        result.Columns.Add(new DataColumn("Value", elementType.IsNullableValueType()
          ? Nullable.GetUnderlyingType(elementType)
          : elementType)
        {
          AllowDBNull = elementType.IsNullableValueType() || !elementType.IsValueType
        });

        foreach (var element in source)
        {
          result.Rows.Add(element);
        }
      }
      else
      {
        var properties = elementType
          .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty)
          .ToArray();

        if (predicate != null)
        {
          properties = properties
            .Where(item => item.PropertyType.IsPrimitive || item.PropertyType == typeof(String))
            .Where(predicate)
            .ToArray();
        }

        foreach (var property in properties)
        {
          result.Columns.Add(new DataColumn(property.Name, property.PropertyType.IsNullableValueType()
            ? Nullable.GetUnderlyingType(property.PropertyType)
            : property.PropertyType)
          {
            AllowDBNull = property.PropertyType.IsNullableValueType() || !property.PropertyType.IsValueType
          });
        }

        foreach (var element in source)
        {
          result.LoadDataRow(properties.Select(property => property.GetValue(element, null)).ToArray(), true);
        }
      }

      return result;
    }

		/// <summary>
		/// Compares each T of the list on property compareValue and returns a List with T with distinct compareValue
		/// </summary>
		/// <typeparam name="T">Type of object</typeparam>
		/// <typeparam name="TCompareValue">The property on which to compare</typeparam>
		/// <param name="instance"></param>
		/// <param name="compareValue"></param>
		/// <returns>A list of T with distinct TCompareValue</returns>
		public static IEnumerable<T> UniqueOn<T, TCompareValue>(this IEnumerable<T> instance, Func<T, TCompareValue> compareValue)
		{
			List<TCompareValue> history = new List<TCompareValue>();

			foreach (var i in instance)
			{
				var value = compareValue(i);
				if (!history.Contains(value))
				{
					history.Add(value);
					yield return i;
				}
			}
		}

		/// <summary>
		/// Returns a random element of the collection
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="instance"></param>
		/// <returns></returns>
		public static T Random<T>(this IEnumerable<T> instance)
		{
			instance.ThrowIfNull();

			var random = new Random();

			if (instance.Any())
			{
				return default(T);
			}

			return instance.ElementAtOrDefault(random.Next(0, instance.Count() - 1));
		}
	}
}
