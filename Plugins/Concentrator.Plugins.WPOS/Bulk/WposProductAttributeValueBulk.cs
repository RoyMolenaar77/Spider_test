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
  class WposProductAttributeValueBulk : VendorImportLoader<ConcentratorDataContext>
  {
    #region SQL strings

    private string _concentratorProductAttributeValueTable = "[dbo].[Concentrator_ProductAttributeValues]";

    private string _concentratorProductAttributeCategoryQuery = @"CREATE TABLE [dbo].[Concentrator_ProductAttributeValues](
    [ProductID] [nvarchar](max) NULL,
    [TypeID] [nvarchar](max) NULL,
    [Value] [nvarchar](max) NULL,
    [CreatedByID] [nvarchar](max) NULL,
    [CreationTime] [datetime] NOT NULL,
    ) ON [PRIMARY]";

    #endregion

    IEnumerable<WposProductAttributeValues> _products = null;
    string _connectionString = string.Empty;
    public WposProductAttributeValueBulk(IEnumerable<WposProductAttributeValues> products, string wposConnectionString)
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

          using (SqlCommand command = new SqlCommand(_concentratorProductAttributeCategoryQuery, connection))
          {
            int result = command.ExecuteNonQuery();

            using (GenericCollectionReader<WposProductAttributeValues> reader = new GenericCollectionReader<WposProductAttributeValues>(_products))
            {
              BulkLoad(_concentratorProductAttributeValueTable, 500, reader, _connectionString);
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

      string productMergeQuery = @"merge productattributevalues p
      using (select
			  b.ID as ProductID,
              d.ID as TypeID,
              c.Value,
              c.CreatedByID,
              c.CreationTime
              from
              concentrator_productattributevalues c
              inner join products b on c.ProductID = b.backendproductid
              inner join productattributetypes d on c.TypeID = d.BackendID
              ) cpav      
      ON p.ProductID = cpav.ProductID
      WHEN NOT Matched by target then insert
                 ([ProductID]
                 ,[TypeID]
                 ,[Value]
                 ,[CreatedByID]
                 ,[CreationTime])
           VALUES
                 (cpav.ProductID
                 ,cpav.TypeID
                 ,cpav.Value
                 ,cpav.createdbyid
                 ,cpav.creationtime)
      WHEN not matched by source then 
	      update set
	      p.lastmodifiedbyid = 1,
	      p.lastmodificationtime = getdate();";

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

      string productGroupAttributeTypesMerge = @"merge [ProductGroupAttributeTypes] pgat
using( 
SELECT DISTINCT pat.issearchable as IsFilter, pg.ID as ProductGroup_ID, pat.ID as ProductAttributeType_ID, 1 as CreatedByID, GETDATE() AS CreationTime
--pg.Name as ProductGroup_Name, pat.Name as ProductAttributeType_Name, pac.Name as ProductAttributeCategory_Name 
FROM ProductGroups pg 
      LEFT JOIN ProductProductGroups pgp ON pgp.ProductGroupID = pg.ID
      LEFT JOIN Products p ON pgp.ProductID = p.ID
      LEFT JOIN ProductAttributeValues pav ON pav.ProductID = p.ID
      LEFT JOIN ProductAttributeTypes pat ON pav.TypeID = pat.ID
      LEFT JOIN ProductAttributeCategories pac ON pac.ID = pat.CategoryID
      WHERE pat.ID IS NOT NULL
) i on i.productgroup_id = pgat.productgroupid and i.productattributetype_id = pgat.productattributetypeid
WHEN Matched and pgat.isfilter != i.isfilter then update set
	pgat.isfilter = i.isfilter,
	pgat.lastmodificationtime = getdate()
when not matched by target then insert
 (IsFilter, ProductGroupID, ProductAttributeTypeID, CreatedByID, CreationTime)
values
 (i.isfilter,i.productgroup_id,i.productattributetype_id,1,getdate())
when not matched by source
then delete;";

      using (SqlConnection connection = new SqlConnection(_connectionString))
      {
        connection.Open();

        using (SqlCommand command = new SqlCommand(productGroupAttributeTypesMerge, connection))
        {
          command.CommandTimeout = 3600;
          int result = command.ExecuteNonQuery();

          Log.DebugFormat("Finished productGroupAttributeTypes, result: {0} rows", result);
        }
      }


      string setAttributeType = @"update pat set FilterType = pav.FilterType
  from 
  ProductAttributeTypes pat 
  inner join (select *,
      case when Value in ('Y','N') then 'Boolean' else
      case when ISNUMERIC(value) = 1 then 'Range' else
      'String' end
   end as FilterType
  from dbo.ProductAttributeValues pav
  ) pav on pav.TypeID = pat.ID";

      using (SqlConnection connection = new SqlConnection(_connectionString))
      {
        connection.Open();

        using (SqlCommand command = new SqlCommand(setAttributeType, connection))
        {
          command.CommandTimeout = 3600;
          int result = command.ExecuteNonQuery();

          Log.DebugFormat("Finished set AttributeValue Types, result: {0} rows", result);
        }
      }

    }

    protected override void TearDown(ConcentratorDataContext context)
    {
      using (SqlConnection connection = new SqlConnection(_connectionString))
      {
        connection.Open();

        using (SqlCommand command = new SqlCommand(String.Format("DROP TABLE {0}", _concentratorProductAttributeValueTable), connection))
        {
          int result = command.ExecuteNonQuery();
        }
      }
    }
  }

  public class WposProductAttributeValues
  {
    public string ProductID { get; set; }
    public string TypeID { get; set; }
    public string Value { get; set; }
    public int CreatedByID { get; set; }
    public DateTime CreationTime { get; set; }
  }
}