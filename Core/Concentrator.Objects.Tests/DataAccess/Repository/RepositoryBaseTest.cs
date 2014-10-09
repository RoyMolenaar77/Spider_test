using System;
using System.Configuration;
using System.Data;
using System.Data.Common;

namespace Concentrator.Objects.DataAccess.Repository
{
  public abstract class RepositoryBaseTest
  {
    private const String ConnectionStringName = "Test";

    protected IDbConnection Connection
    {
      get;
      private set;
    }

    protected RepositoryBaseTest()
    {
      var connectionConfiguration = ConfigurationManager.ConnectionStrings[ConnectionStringName];

      if (connectionConfiguration == null)
      {
        throw new Exception(String.Format("No connection string could be found by the name '{0}'", ConnectionStringName));
      }

      var providerFactory = DbProviderFactories.GetFactory(connectionConfiguration.ProviderName);

      if (providerFactory == null)
      {
        throw new Exception(String.Format("No factory could be found with the name '{0}'", connectionConfiguration.ProviderName));
      }
      
      Connection = providerFactory.CreateConnection();
      Connection.ConnectionString = connectionConfiguration.ConnectionString;
      Connection.Open();
    }

    public void Dispose()
    {
      Connection.Dispose();
    }

    protected IDbCommand CreateCommand(String sql, params Object[] arguments)
    {
      var command = Connection.CreateCommand();

      command.CommandText = String.Format(sql, arguments);
      
      return command;
    }

    protected Int32 ExecuteCommand(String sql, params Object[] arguments)
    {
      using (var command = CreateCommand(sql, arguments))
      {
        return command.ExecuteNonQuery();
      }
    }

    protected IDataReader ReaderCommand(String sql, params Object[] arguments)
    {
      using (var command = CreateCommand(sql, arguments))
      {
        return command.ExecuteReader();
      }
    }
  }
}
