using System;

namespace Concentrator.Objects.Sql
{
  /// <summary>
  /// The basics of an SQL statement.
  /// </summary>
  public abstract class StatementBuilder<T> where T : StatementBuilder<T>
  {
    /// <summary>
    /// Performs an implicit conversion from <see cref="System.String"/> to <see cref="Concentrator.Objects.Sql.TableSource"/>.
    /// </summary>
    public static implicit operator String(StatementBuilder<T> statementBuilder)
    {
      return statementBuilder.ToString();
    }

    protected QueryBuilder QueryBuilder
    {
      get;
      private set;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StatementBuilder{T}"/> class.
    /// </summary>
    protected StatementBuilder(QueryBuilder queryBuilder)
    {
      if (queryBuilder == null)
      {
        throw new ArgumentNullException("queryBuilder");
      }

      QueryBuilder = queryBuilder;
    }
  }
}
