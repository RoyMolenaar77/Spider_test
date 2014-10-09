using System;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Linq.Expressions;

namespace System
{
  public static class PredicateBuilder
  {
    public static Expression<Func<T, bool>> True<T>() { return f => true; }
    public static Expression<Func<T, bool>> False<T>() { return f => false; }

    public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> expr1,
                                                        Expression<Func<T, bool>> expr2)
    {
      var invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
      return Expression.Lambda<Func<T, bool>>
            (Expression.OrElse(expr1.Body, invokedExpr), expr1.Parameters);
    }

    public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> expr1,
                                                         Expression<Func<T, bool>> expr2)
    {
      var invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
      return Expression.Lambda<Func<T, bool>>
            (Expression.AndAlso(expr1.Body, invokedExpr), expr1.Parameters);
    }


    public static IQueryable<T> Search<T>(this IQueryable<T> sourceQuery, string query)
            where T : class
    {
      if (query == null) query = string.Empty;
      var exp = False<T>();
      return sourceQuery.Where(Search<T>(exp, query));
    }

    public static IQueryable<T> Search<T>(this IQueryable<T> sourceQuery, string query, Func<PropertyInfo, bool> columnFilter)
        where T : class
    {
      if (query == null) query = string.Empty;
      var exp = False<T>();
      return sourceQuery.Where(Search(exp, query, columnFilter));
    }

    public static Expression<Func<T, bool>> Search<T>(this Expression<Func<T, bool>> exp, string query, Func<PropertyInfo, bool> columnFilter)
        where T : class
    {
      var properties = typeof(T).GetProperties().Where(columnFilter);

      foreach (var prop in properties)
      {
        var currentProp = prop;

        var par = exp.Parameters.First();

        Expression propExp = Expression.Property(par, currentProp);

        if (prop.PropertyType != typeof(string))
        { // Cast to string where necessary
          propExp = Expression.Call(propExp, "ToString", null);
          //propExp = Expression.Convert(propExp, typeof(string));
        }


        var mi = typeof(string).GetMethod("Contains");

        var containsExp = Expression.Call(propExp, mi, Expression.Constant(query));

        exp = exp.Or(Expression.Lambda<Func<T, bool>>(containsExp, par));
      }

      return exp;
    }

    public static Expression<Func<T, bool>> Search<T>(this Expression<Func<T, bool>> exp, string query)
        where T : class
    {

      Func<PropertyInfo, bool> columnFilters = p => p.PropertyType == typeof(string);

      if (typeof(T).IsDefined(typeof(TableAttribute), false))
        return Search<T>(exp, query, p => columnFilters(p) && p.IsDefined(typeof(ColumnAttribute), false));

      return Search<T>(exp, query, columnFilters);


    }

  }
}
