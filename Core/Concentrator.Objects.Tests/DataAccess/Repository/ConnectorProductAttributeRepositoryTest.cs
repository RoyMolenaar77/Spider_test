using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Linq;

using Xunit;

namespace Concentrator.Objects.DataAccess.Repository.Tests
{
  public class ConnectorProductAttributeRepositoryTest : IDisposable
  {
    private const String ConnectionStringName = "Test";

    private IDbConnection Connection
    {
      get;
      set;
    }

    public ConnectorProductAttributeRepositoryTest()
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

    [Fact(DisplayName = "Connector Product Attribute Repository: Initialization with valid connection parameter")]
    public void InitializeWithValidConnection()
    {
      new ConnectorProductAttributeRepository(Connection);
    }

    [Fact(DisplayName = "Connector Product Attribute Repository: Initialization with invalid connection parameter")]
    public void InitializeWithInvalidConnection()
    {
      Assert.Throws<ArgumentNullException>(delegate
      {
        new ConnectorProductAttributeRepository(null);
      });
    }
  }
}
