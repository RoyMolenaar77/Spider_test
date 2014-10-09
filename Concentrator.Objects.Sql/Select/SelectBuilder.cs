using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Concentrator.Objects.Sql.Select
{
  /// <summary>
  /// Represents an SQL statement.
  /// </summary>
  public class SelectBuilder : StatementBuilder<SelectBuilder>
  {
    internal List<String> Columns
    {
      get;
      private set;
    }

    internal FromBuilder FromBuilder
    {
      get;
      private set;
    }

    internal List<GroupByClause> GroupByColumns
    {
      get;
      private set;
    }

    internal IDictionary<String, SortingDirection> OrderByColumns
    {
      get;
      private set;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SelectBuilder"/> class.
    /// </summary>
    internal SelectBuilder(QueryBuilder queryBuilder, FromBuilder fromBuilder, params String[] columns)
      : base(queryBuilder)
    {
      Columns = new List<String>(columns);
      FromBuilder = fromBuilder;
      GroupByColumns = new List<GroupByClause>();
      OrderByColumns = new Dictionary<String, SortingDirection>();
    }

    /// <summary>
    /// Adds columns to SELECT
    /// </summary>
    public SelectBuilder Column(String column)
    {
      Columns.Add(column);

      return this;
    }

    /// <summary>
    /// Adds a column to the GROUP BY clause.
    /// </summary>
    public SelectBuilder GroupBy(String column)
    {
      GroupByColumns.Add(column);

      return this;
    }

    /// <summary>
    /// Adds a column to the ORDER BY clause.
    /// </summary>
    public SelectBuilder OrderBy(String column, SortingDirection sortingDirection = SortingDirection.Ascending)
    {
      OrderByColumns[column] = sortingDirection;

      return this;
    }


    /// <summary>
    /// Returns the statement as an SQL string.
    /// </summary>
    public override String ToString()
    {
      var stringBuilder = new StringBuilder("SELECT ");

      if (!Columns.Any())
      {
        Columns.Add("*");
      }

      stringBuilder.AppendLine(String.Join(QueryBuilder.ColumnSeparator, Columns));
      stringBuilder.AppendLine(FromBuilder.ToString());
    
      if (GroupByColumns.Any())
      {
        stringBuilder.AppendLine("GROUP BY " + String.Join(QueryBuilder.ColumnSeparator, GroupByColumns));
      }
    
      if (OrderByColumns.Any())
      {
        var orderByColumns = OrderByColumns
          .Select(pair => pair.Key + " " + (pair.Value == SortingDirection.Ascending ? "ASC" : "DESC"))
          .ToArray();

        stringBuilder.AppendLine("ORDER BY " + String.Join(QueryBuilder.ColumnSeparator, orderByColumns));
      }

      return stringBuilder.ToString().Trim();
    }
  }
}
