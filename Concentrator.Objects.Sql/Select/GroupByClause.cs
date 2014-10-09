using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Objects.Sql.Select
{
  /// <summary>
  /// Represents an GROUP BY column in an SQL statement.
  /// </summary>
  public class GroupByClause
  {
    /// <summary>
    /// The column to group by
    /// </summary>
    public String Column
    {
      get;
      private set;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GroupByClause"/> class.
    /// </summary>
    /// <param name="column">The column.</param>
    public GroupByClause(String column)
    {
      Column = column;
    }

    /// <summary>
    /// Performs an implicit conversion from <see cref="System.String"/> to <see cref="Concentrator.Objects.Sql.Select.GroupByClause"/>.
    /// </summary>
    /// <param name="s">The s.</param>
    /// <returns>
    /// The result of the conversion.
    /// </returns>
    public static implicit operator GroupByClause(string s)
    {
      return new GroupByClause(s);
    }
  }
}
