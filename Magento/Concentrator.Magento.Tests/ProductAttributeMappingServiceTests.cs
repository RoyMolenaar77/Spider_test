using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;

using Xunit;

namespace Concentrator.Magento.Tests
{
  using Contracts;
  using Services;

  public class ProductAttributeMappingServiceTests
  {
    private const String ConnectionString = "Data Source=.\\SQL2012; Initial Catalog=CCAT; Integrated Security=True";

    public ProductAttributeMappingServiceTests()
    {
    }
    
    [Fact(DisplayName = "Product Attribute Mapping Service: Create")]
    public void CreateTest()
    {
      using (var connection = new SqlConnection(ConnectionString))
      {
        connection.Open();

        var productAttributeMappingService = new ProductAttributeMappingService(connection);

       // using (var transaction = new TransactionScope(TransactionScopeOption.RequiresNew))
        {
          productAttributeMappingService.Create(1, 3, false, new Dictionary<String, Object>
          {
            { "Test Property 1", Math.PI  },
            { "Test Property 2", 42       },
            { "Test Property 3", "Blaat"  }
          });
        }
      }
    }

    [Fact(DisplayName = "Product Attribute Mapping Service: Get")]
    public void GetTest()
    {
      //var mockCommand = new Mock<IDbCommand>(MockBehavior.Default)
      //  .SetupAllProperties();

      //var mockDataReader = new Mock<IDataReader>(MockBehavior.Default)
      //  .SetupAllProperties();

      //mockDataReader.Setup(reader => reader.FieldCount).Callback<Int32>(value => value

      //mockCommand.Setup<IDataReader>(command => command.ExecuteReader()).Returns(

      //var mockConnection = new Mock<IDbConnection>(MockBehavior.Default);

      //mockConnection
      //  .Setup(connection => connection.CreateCommand())
      //  .Returns(mockCommand.Object);

      //var productAttributeMappingService = new ProductAttributeMappingService(mockConnection.Object);
      //var result = productAttributeMappingService.Get(null, null, null).ToArray();
    }
  }
}
