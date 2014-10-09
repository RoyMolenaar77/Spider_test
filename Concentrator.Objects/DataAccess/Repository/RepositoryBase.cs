using System;
using System.Data;

namespace Concentrator.Objects.DataAccess.Repository
{
  /// <summary>
  /// Represents a base-class for a repository implementation.
  /// </summary>
  public abstract class RepositoryBase
  {
    /// <summary>
    /// Gets the connection-instance to the database.
    /// </summary>
    protected IDbConnection Connection
    {
      get;
      private set;
    }

    protected RepositoryBase(IDbConnection connection)
    {
      if (connection == null)
      {
        throw new ArgumentNullException("connection");
      }

      Connection = connection;
    }

    private void DoNothing()
    {
    }

    protected IDisposable EnsureOpenStatus()
    {
      switch (Connection.State)
      {
        case ConnectionState.Closed:
        case ConnectionState.Connecting:
          return new ActionDisposable(Connection.Close);

        default:
          return new ActionDisposable(DoNothing);
      }
    }
  }
}
