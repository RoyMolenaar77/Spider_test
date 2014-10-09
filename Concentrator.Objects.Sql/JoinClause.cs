using System;
using System.Collections.Generic;
using System.Text;

namespace Concentrator.Objects.Sql
{
  /// <summary>
  /// An SQL Join clause.
  /// </summary>
  public class JoinClause
  {
    internal TableSource JoinTableSource
    {
      get;
      private set;
    }

    internal JoinType JoinType
    {
      get;
      private set;
    }

    private static readonly IDictionary<JoinType, String> JoinTypeLookup = new Dictionary<JoinType, String>
    {
      { JoinType.Cross,       "CROSS JOIN "       },
      { JoinType.CrossApply,  "CROSS APPLY "      },
      { JoinType.Inner,       "INNER JOIN "       },
      { JoinType.OuterApply,  "OUTER APPLY "      },
      { JoinType.OuterFull,   "FULL OUTER JOIN "  },
      { JoinType.OuterLeft,   "LEFT OUTER JOIN "  },
      { JoinType.OuterRight,  "RIGHT OUTER JOIN " }
    };

    /// <summary>
    /// The ON clause to join the two tables.
    /// </summary>
    internal String OnClause
    {
      get;
      private set;
    }

    internal JoinClause(JoinType joinType, String objectName, String onClause = null)
    {
      JoinType = joinType;
      JoinTableSource = objectName;
      OnClause = onClause;
    }

    /// <summary>
    /// Returns the SQL for the current object.
    /// </summary>
    public override String ToString()
    {
      var builder = new StringBuilder(JoinTypeLookup[JoinType] + JoinTableSource);

      if (JoinType == JoinType.Inner || JoinType == JoinType.OuterFull || JoinType == JoinType.OuterLeft || JoinType == JoinType.OuterRight)
      {
        builder.Append(" ON " + OnClause);
      }

      return builder.ToString();
    }
  }
}
