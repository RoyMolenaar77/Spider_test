using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Vendors.Base;
using Concentrator.Objects.DataAccess.EntityFramework;
using System.Xml;
using System.Data.SqlClient;

namespace Concentrator.Plugins.WPOS.Bulk
{
  class WposProductGroupProductsBulk : VendorImportLoader<ConcentratorDataContext>
  {
    #region SQL strings

    private string _concentratorProductGroupProductsTable = "[dbo].[Concentrator_ProductGroupProducts]";

    private string _concentratorProductGroupProductsQuery = @"CREATE TABLE [dbo].[Concentrator_ProductGroupProducts](
    [ConcentratorProductID] [int] NOT NULL,
  	[ConcentratorProductGroupID] int NOT NULL
    ) ON [PRIMARY]";

    #endregion

    IEnumerable<WposProductGroupProducts> _products = null;
    string _connectionString = string.Empty;

    public WposProductGroupProductsBulk(IEnumerable<WposProductGroupProducts> products, string wposConnectionString)
    {
      _products = products;
      _connectionString = wposConnectionString;
    }

    public override void Init(ConcentratorDataContext context)
    {
      base.Init(context);

      try
      {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
          connection.Open();

          using (SqlCommand command = new SqlCommand(_concentratorProductGroupProductsQuery, connection))
          {
            int result = command.ExecuteNonQuery();

            using (GenericCollectionReader<WposProductGroupProducts> reader = new GenericCollectionReader<WposProductGroupProducts>(_products))
            {
              BulkLoad(_concentratorProductGroupProductsTable, 500, reader, _connectionString);
            }
          }
        }
      }
      catch (Exception ex)
      {
        _log.Error("Error executing bulk copy");
      }
    }

    #region sync
    public override void Sync(ConcentratorDataContext context)
    {
      Log.Debug("Starting productmerge");

      #region Cleanup duplicates
      Log.Debug("Start cleanup duplicates");
      string addImportID = @"merge ProductProductGroups pgp
      using(
      select distinct
      p.id as product_ID,
      pg.id as productGroup_ID
      from dbo.Concentrator_ProductGroupProducts cpgp
      inner join products p on cast(cpgp.concentratorproductid as varchar) = p.backendproductid
      inner join productgroups pg on cpgp.concentratorproductgroupid = pg.BackendID
      ) cpgp
      on pgp.productID = cpgp.Product_ID and pgp.ProductGroupID = cpgp.ProductGroup_ID

	      WHEN NOT Matched by target then insert
                             (ProductID
                             ,ProductGroupID)
                       VALUES
                             (cpgp.Product_ID
                             ,cpgp.ProductGroup_ID)
	      WHEN NOT Matched by source then delete;";

      using (SqlConnection connection = new SqlConnection(_connectionString))
      {
        connection.Open();

        using (SqlCommand command = new SqlCommand(addImportID, connection))
        {
          command.CommandTimeout = 3600;
          int result = command.ExecuteNonQuery();
        }
      }
      
      Log.Debug("Finish delete unused productgroups");

      #endregion
    }
    #endregion

    protected override void TearDown(ConcentratorDataContext context)
    {
      using (SqlConnection connection = new SqlConnection(_connectionString))
      {
        connection.Open();

        using (SqlCommand command = new SqlCommand(String.Format("DROP TABLE {0}", _concentratorProductGroupProductsTable), connection))
        {
          int result = command.ExecuteNonQuery();
        }
      }
    }
  }

  public class WposProductGroupProducts
  {
    public int ConcentratorProductID { get; set; }
    public int ConcentratorProductGroupID { get; set; }
  }
}