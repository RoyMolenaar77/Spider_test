using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Objects.Sql
{
  using Select;

  public sealed class FromBuilder : StatementBuilder<FromBuilder>
  {
    internal TableSource TableSource
    {
      get;
      private set;
    }

    internal List<JoinClause> JoinClauses
    {
      get;
      private set;
    }

    internal List<String> WhereClauses
    {
      get;
      private set;
    }

    internal FromBuilder(QueryBuilder queryBuilder, String objectName)
      : base(queryBuilder)
    {
      TableSource = objectName;
      JoinClauses = new List<JoinClause>();
      WhereClauses = new List<String>();
    }

    /// <summary>
    /// Adds a table or view to the FROM list of the statement.
    /// </summary>
    public FromBuilder Join(JoinType joinType, String objectName, String onClause = null, params Object[] arguments)
    {
      JoinClauses.Add(new JoinClause(joinType, objectName, onClause != null ? String.Format(onClause, arguments) : null));

      return this;
    }

    /// <summary>
    /// Creates a SELECT statement.
    /// </summary>
    /// <param name="columns">The columns to select.</param>
    public SelectBuilder Select(params String[] columns)
    {
      return new SelectBuilder(QueryBuilder, this, columns);
    }

    /// <summary>
    /// Adds a WHERE clause to the statement
    /// </summary>
    public FromBuilder Where(String clause, params Object[] arguments)
    {
      WhereClauses.Add(String.Format(clause, arguments));

      return this;
    }

    public override String ToString()
    {
      var stringBuilder = new StringBuilder("FROM ");

      stringBuilder.AppendLine(TableSource.ToString());

      foreach (var joinClause in JoinClauses)
      {
        stringBuilder.AppendLine(joinClause.ToString());
      }

      if (WhereClauses.Any())
      {
        stringBuilder.AppendLine("WHERE " + String.Join(" AND ", WhereClauses));
      }

      return stringBuilder.ToString();
    }
  }
}
