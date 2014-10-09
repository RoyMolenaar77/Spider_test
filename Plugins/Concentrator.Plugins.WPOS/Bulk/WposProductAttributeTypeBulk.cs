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
  class WposProductAttributeTypeBulk : VendorImportLoader<ConcentratorDataContext>
  {
    #region SQL strings

    private string _concentratorProductAttributeTypeTable = "[dbo].[Concentrator_ProductAttributeTypes]";

    private string _concentratorProductAttributeTypeQuery = @"CREATE TABLE [dbo].[Concentrator_ProductAttributeTypes](
	  [Name] [nvarchar](max) NULL,
    [Unit] [nvarchar](max) NULL,
    [IsVisible] [nvarchar](max) NOT NULL,
    [IsSearchable] [nvarchar](max) NOT NULL,
    [CategoryID] [int] NOT NULL,
    [BackendID] [nvarchar](max) NULL
    ) ON [PRIMARY]";

    #endregion

    IEnumerable<WposProductAttributeTypes> _products = null;
    string _connectionString = string.Empty;
    public WposProductAttributeTypeBulk(IEnumerable<WposProductAttributeTypes> products, string wposConnectionString)
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

          using (SqlCommand command = new SqlCommand(_concentratorProductAttributeTypeQuery, connection))
          {
            int result = command.ExecuteNonQuery();

            using (GenericCollectionReader<WposProductAttributeTypes> reader = new GenericCollectionReader<WposProductAttributeTypes>(_products))
            {
              BulkLoad(_concentratorProductAttributeTypeTable, 500, reader, _connectionString);
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
      Log.Debug("Starting productmerge");

      string productMergeQuery = @"merge productattributetypes p
      using (select distinct
              c.Name,
              c.Unit,
              c.IsVisible,
              c.IsSearchable,
              b.ID as CategoryID,
              c.BackendID
              from
              concentrator_productattributetypes c
              inner join productattributecategories b on c.CategoryID = b.BackendID) cpat
      ON p.backendid = cpat.backendid
      WHEN NOT Matched by target then insert
                 ([Name]
                 ,[Unit]
                 ,[IsVisible]
                 ,[IsSearchable]
                 ,[CategoryID]
                 ,[BackendID])
           VALUES
                 (cpat.name
                 ,cpat.unit
                 ,cpat.isvisible
                 ,cpat.issearchable
                 ,cpat.categoryid
                 ,cpat.BackendID);";

      using (SqlConnection connection = new SqlConnection(_connectionString))
      {
        connection.Open();

        using (SqlCommand command = new SqlCommand(productMergeQuery, connection))
        {
          command.CommandTimeout = 3600;
          int result = command.ExecuteNonQuery();

          Log.DebugFormat("Finished productmerge, result: {0} rows", result);
        }
      }
    }

    protected override void TearDown(ConcentratorDataContext context)
    {
      using (SqlConnection connection = new SqlConnection(_connectionString))
      {
        connection.Open();

        using (SqlCommand command = new SqlCommand(String.Format("DROP TABLE {0}", _concentratorProductAttributeTypeTable), connection))
        {
          int result = command.ExecuteNonQuery();
        }
      }
    }
  }

  public class WposProductAttributeTypes
  {
    public string Name { get; set; }
    public string Unit { get; set; }
    public string IsVisible { get; set; }
    public string IsSearchable { get; set; }
    public string CategoryID { get; set; }
    public string BackendID { get; set; }
  }
}