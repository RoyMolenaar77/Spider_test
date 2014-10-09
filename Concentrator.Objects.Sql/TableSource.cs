using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Concentrator.Objects.Sql
{
  /// <summary>
  /// A table or view reference.
  /// </summary>
  public class TableSource
  {
    /// <summary>
    /// Performs an implicit conversion from <see cref="System.String"/> to <see cref="Concentrator.Objects.Sql.TableSource"/>.
    /// </summary>
    public static implicit operator TableSource(String name)
    {
      return new TableSource(name);
    }

    /// <summary>
    /// Gets the alias of the from clause.
    /// </summary>
    public String Alias
    {
      get;
      private set;
    }

    /// <summary>
    /// The table name.
    /// </summary>
    public String Name
    {
      get;
      private set;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TableSource"/> class.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="alias">The alias.</param>
    public TableSource(String name, String alias = null)
    {
      Alias = alias;
      Name = name;
    }

    /// <summary>
    /// Returns a <see cref="System.String"/> that represents this instance.
    /// </summary>
    /// <returns>
    /// A <see cref="System.String"/> that represents this instance.
    /// </returns>
    public override String ToString()
    {
      return !String.IsNullOrWhiteSpace(Alias)
        ? Name + " AS " + Alias
        : Name;
    }
  }
}
