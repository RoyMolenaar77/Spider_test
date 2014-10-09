using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web;

namespace Concentrator.Objects
{
  /// THIS IS THE INTERESTING BIT
  public static class QueryableChunked
  {
    public static IEnumerable<T> InRange<T, TValue>(
            this IQueryable<T> source,
            Expression<Func<T, TValue>> selector,
            int blockSize,
            IEnumerable<TValue> values)
    {
      MethodInfo method = null;
      foreach (MethodInfo tmp in typeof(Enumerable).GetMethods(
              BindingFlags.Public | BindingFlags.Static))
      {
        if (tmp.Name == "Contains" && tmp.IsGenericMethodDefinition
                && tmp.GetParameters().Length == 2)
        {
          method = tmp.MakeGenericMethod(typeof(TValue));
          break;
        }
      }
      if (method == null) throw new InvalidOperationException(
             "Unable to locate Contains");
      foreach (TValue[] block in values.GetBlocks(blockSize))
      {
        var row = Expression.Parameter(typeof(T), "row");
        var member = Expression.Invoke(selector, row);
        var keys = Expression.Constant(block, typeof(TValue[]));
        var predicate = Expression.Call(method, keys, member);
        var lambda = Expression.Lambda<Func<T, bool>>(
              predicate, row);
        foreach (T record in source.Where(lambda))
        {
          yield return record;
        }
      }
    }
    public static IEnumerable<T[]> GetBlocks<T>(
            this IEnumerable<T> source, int blockSize)
    {
      List<T> list = new List<T>(blockSize);
      foreach (T item in source)
      {
        list.Add(item);
        if (list.Count == blockSize)
        {
          yield return list.ToArray();
          list.Clear();
        }
      }
      if (list.Count > 0)
      {
        yield return list.ToArray();
      }
    }
  }

  public class LambdaComparer<T> : IEqualityComparer<T>
  {
    private readonly Func<T, T, bool> _lambdaComparer;
    private readonly Func<T, int> _lambdaHash;

    public LambdaComparer(Func<T, T, bool> lambdaComparer) :
      this(lambdaComparer, o => 0)
    {
    }

    public LambdaComparer(Func<T, T, bool> lambdaComparer, Func<T, int> lambdaHash)
    {
      if (lambdaComparer == null)
        throw new ArgumentNullException("lambdaComparer");
      if (lambdaHash == null)
        throw new ArgumentNullException("lambdaHash");

      _lambdaComparer = lambdaComparer;
      _lambdaHash = lambdaHash;
    }

    public bool Equals(T x, T y)
    {
      return _lambdaComparer(x, y);
    }

    public int GetHashCode(T obj)
    {
      return _lambdaHash(obj);
    }
  }
}
