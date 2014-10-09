using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Web;
using System.Data.Objects.SqlClient;

namespace Concentrator.Objects
{
  public static class QueryExtensionMethods
  {
    private static readonly MethodInfo ContainsMethodInfo = typeof(String).GetMethod("Contains", new[] { typeof(string) });
    private static readonly MethodInfo EqualsMethodInfo = typeof(String).GetMethod("Equals", new[] { typeof(string) });
    private const string lessThan = "lt";
    private const string greaterThan = "gt";

    public static IQueryable<T> Filter<T>(this IQueryable<T> query, HttpRequestBase request)
    {
      return Filter<T>(query, request.Params);
    }

    public static IQueryable<T> Filter<T>(this IQueryable<T> query, NameValueCollection requestParameters)
    {
      var expressionList = new List<Expression>();
      var type = typeof(T);
      var queryParam = Expression.Parameter(type, "e");

      for (var index = 0; index < requestParameters.Count && Objects.Filter.CheckExistence(index, requestParameters); index++)
      {
        var filter = new Filter(index, requestParameters);        
        var field = filter.Field;
        var property = type.GetProperty(field);

        switch (filter.DataType)
        {
          case "string":
            /**
             * Adding a reserved search term "/o/" here to support filtering for empty strings.
             * The normally used String.Contains does not process the empty string parameter correctly,
             * so need to distinct between it and String.Equals here.
             * - Coen, Diract-IT
             */
            if (filter.DataValue == "/o/")
            {
              filter.DataValue = String.Empty; 
              expressionList.Add(Expression.Call(Expression.MakeMemberAccess(queryParam, type.GetProperty(field)), EqualsMethodInfo, Expression.Constant(filter.DataValue)));
            }
            else
            {
              var memberAccess = Expression.MakeMemberAccess(queryParam, type.GetProperty(field));
              var toUpperCall = Expression.Call(memberAccess, typeof(string).GetMethod("ToUpper", Type.EmptyTypes));
              var contains = Expression.Call(toUpperCall, ContainsMethodInfo, Expression.Constant(filter.DataValue.ToUpper()));

              expressionList.Add(contains);
            }
            break;

          case "list":
            var expression = filter.DataValue
              .Split(',')
              .Select(part => CreateFilterExpression(type, filter, part, field, queryParam))
              .Aggregate(Expression.Or);

            if (expression != null)
            {
              expressionList.Add(expression);
            }
            break;

          case "date":
            // Dates by the grid filters are supplied in 'mm-dd-yy'
            var culture = new CultureInfo("en-US", true);
            var date = DateTime.Parse(filter.DataValue, culture);

            // PDW: 2013-09-18 - Changed to yyyy-MM-dd because the filtering on Timestamp in the Coolcat DATCOL isn't working when date is send as MM-dd-yyyy 
            //_expressions.Add(CreateFilterExpression(t, f, date.ToString("MM-dd-yyyy"), field, queryParam));
            expressionList.Add(CreateFilterExpression(type, filter, date.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss"), field, queryParam));
            break;

          case "custom": // Don't handle this automatically, used for Order Inquiry status filter
            break;

          default:
            expressionList.Add(CreateFilterExpression(type, filter, filter.DataValue, field, queryParam));
            break;
        }
      }

      return expressionList.Count > 0
        ? query.Where(Expression.Lambda<Func<T, Boolean>>(expressionList.Aggregate(Expression.And), queryParam))
        : query;
    }

    private static Expression CreateFilterExpression(Type t, Filter f, string dataValue, string field, ParameterExpression queryParam)
    {
      object value = null;
      MemberExpression memberAccess = Expression.MakeMemberAccess(queryParam, t.GetProperty(field));
      Type prop = t.GetProperty(field).PropertyType;
      string val = dataValue.ToLower().Trim();

      if (prop.IsNullableValueType())
      {
        if (val != "null")
        {
          Type underlyingType = new NullableConverter(prop).UnderlyingType;
          value = Convert.ChangeType(dataValue, underlyingType, CultureInfo.InvariantCulture);
          memberAccess = Expression.Property(memberAccess, prop.GetProperty("Value"));
        }
      }

      // The field is not nullable, but for the 'emptyfilter' we want to compare to a value that can be considered 'empty' for this datatype, 
      // which isn't null.
      else if (val == "null")
      {
        //prop
        switch (f.DataType)
        {
          case "numeric":
            value = default(Int32);
            break;
          case "boolean":
            value = default(Boolean);
            break;
          case "date":
            value = default(DateTime);
            break;
        }
      }
      else
      {
        value = Convert.ChangeType(dataValue, t.GetProperty(field).PropertyType);
      }
      Expression result = null;


      switch (f.DataComparison)
      {
        case lessThan:
          result = Expression.LessThan(memberAccess, Expression.Constant(value));
          break;
        case greaterThan:
          result = Expression.GreaterThan(memberAccess, Expression.Constant(value));
          break;
        default:
          if (f.DataType == "date")
          {
            // When the object's DateTime property is actually nullable, we have to get to the .Value property to compare it
            if (prop.IsNullableValueType())
            {
              memberAccess = Expression.Property(memberAccess, prop.GetProperty("Value").PropertyType.GetProperty("Date"));
              result = Expression.Equal(memberAccess, Expression.Constant(value));
            }
            else
            {
              var casted = Convert.ToDateTime(value);
              result = Expression.And(
                        Expression.And(
                                   Expression.Equal(
                                                    Expression.Property(memberAccess, prop.GetProperty("Year")),
                                                    Expression.Constant(casted.Year)
                                                    ),
                                   Expression.Equal(
                                                    Expression.Property(memberAccess, prop.GetProperty("Month")),
                                                    Expression.Constant(casted.Month)
                                                    )
                                       ),
                                    Expression.Equal(
                                                    Expression.Property(memberAccess, prop.GetProperty("Day")),
                                                    Expression.Constant(casted.Day)
                                                    )
                                     );


            }
          }
          result = Expression.Equal(memberAccess, Expression.Constant(value));

          break;
      }

      return result;
    }

    /// <summary>
    /// Sorts a query based on Request parameters.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="query"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    public static IQueryable<T> Sort<T>(this IQueryable<T> query, string property, string direction)
    {
      if (!string.IsNullOrEmpty(property) && !string.IsNullOrEmpty(direction))
      {
        string orderMethod = direction == "DESC" ? "OrderByDescending" : "OrderBy";

        // The below expression is equal to:
        // query = Queryable.OrderBy(query, source => source.Property);

        var currentExpression = query.Expression;
        var param = Expression.Parameter(query.ElementType, "source");

        //TODO: Remove this
        if (property.Contains(","))
        {
          string[] properties = property.Split(',');
          property = properties[0];
        }

        var prop = Expression.MakeMemberAccess(param, query.ElementType.GetProperty(property));

        var selector = Expression.Lambda(prop, param);

        currentExpression = Expression.Call(typeof(Queryable),
                                            orderMethod,
                                            new[] { query.ElementType, prop.Type },
                                            currentExpression, Expression.Quote(selector));

        return (IQueryable<T>)query.Provider.CreateQuery(currentExpression);
      }

      return query;
    }
  }
}
