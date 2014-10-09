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
  class WposProductAttributeCategoryBulk : VendorImportLoader<ConcentratorDataContext>
  {
    #region SQL strings

    private string _concentratorProductAttributeCategoryTable = "[dbo].[Concentrator_ProductAttributeCategories]";

    private string _concentratorProductAttributeCategoryQuery = @"CREATE TABLE [dbo].[Concentrator_ProductAttributeCategories](
    [Name] [nvarchar](50) NULL,
	  [BackendID] [nvarchar](max) NULL,
    ) ON [PRIMARY]";

    #endregion

    IEnumerable<WposProductAttributeCategories> _productattributes = null;
    string _connectionString = string.Empty;
    public WposProductAttributeCategoryBulk(IEnumerable<WposProductAttributeCategories> productattributes, string wposConnectionString)
    {
      _productattributes = productattributes;
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

          using (SqlCommand command = new SqlCommand(_concentratorProductAttributeCategoryQuery, connection))
          {
            int result = command.ExecuteNonQuery();

            using (GenericCollectionReader<WposProductAttributeCategories> reader = new GenericCollectionReader<WposProductAttributeCategories>(_productattributes))
            {
              BulkLoad(_concentratorProductAttributeCategoryTable, 500, reader, _connectionString);
            }
          }
        }
      }
      catch (Exception ex)
      {
        _log.Error("Error executing bulk copy");
      }
    }

    public override void Sync(ConcentratorDataContext context)
    {
      Log.Debug("Start merge products");

      string productMergeQuery = @"merge productattributecategories p
      using( 
      select distinct
	    name, BackendID
      from concentrator_productattributecategories cpac
      ) cpac
      on p.name = cpac.name and p.BackendID = cpac.BackendID
      WHEN NOT Matched by target then insert
                 ([Name]
                 ,[BackendID])
           VALUES
                 (cpac.name
                 ,cpac.BackendID);";

      using (SqlConnection connection = new SqlConnection(_connectionString))
      {
        connection.Open();

        using (SqlCommand command = new SqlCommand(productMergeQuery, connection))
        {
          command.CommandTimeout = 3600;
          int result = command.ExecuteNonQuery();

          Log.DebugFormat("Finish merge products, result: {0} rows", result);
        }
      }
    }

    protected override void TearDown(ConcentratorDataContext context)
    {
      using (SqlConnection connection = new SqlConnection(_connectionString))
      {
        connection.Open();

        using (SqlCommand command = new SqlCommand(String.Format("DROP TABLE {0}", _concentratorProductAttributeCategoryTable), connection))
        {
          int result = command.ExecuteNonQuery();
        }
      }
    }
  }

  public class WposProductAttributeCategories
  {
    public string Name { get; set; }
    public string BackendID { get; set; }
  }
}