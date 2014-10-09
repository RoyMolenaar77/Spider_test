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
  class WposProductGroupBulk : VendorImportLoader<ConcentratorDataContext>
  {
    #region SQL strings

    private string _concentratorProductGroupTable = "[dbo].[Concentrator_ProductGroups]";

    private string _concentratorProductGroupQuery = @"CREATE TABLE [dbo].[Concentrator_ProductGroups](
    [ConcentratorProductGroupID] [nvarchar](50) NULL,
  	[ParentConcentratorProductGroupID] [nvarchar](50) NULL,
	  [Name] [nvarchar](max) NULL,
	  [CreatedByID] [int] NOT NULL,
	  [CreationTime] [datetime] NOT NULL,
    [Level] [int] NOT NULL
    ) ON [PRIMARY]";

    #endregion

    IEnumerable<WposProductGroups> _products = null;
    string _connectionString = string.Empty;
    public WposProductGroupBulk(IEnumerable<WposProductGroups> products, string wposConnectionString)
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

          using (SqlCommand command = new SqlCommand(_concentratorProductGroupQuery, connection))
          {
            int result = command.ExecuteNonQuery();

            using (GenericCollectionReader<WposProductGroups> reader = new GenericCollectionReader<WposProductGroups>(_products))
            {
              BulkLoad(_concentratorProductGroupTable, 500, reader, _connectionString);
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

      #region Cleanup duplicates
      Log.Debug("Start cleanup duplicates");
      string addImportID = @"ALTER TABLE Concentrator_ProductGroups ADD ImportID int identity(1,1) PRIMARY KEY";
      
      using (SqlConnection connection = new SqlConnection(_connectionString))
      {
        connection.Open();

        using (SqlCommand command = new SqlCommand(addImportID, connection))
        {
          command.CommandTimeout = 3600;
          int result = command.ExecuteNonQuery();
        }
      }

      string deleteDuplicates = @"delete T1
          from Concentrator_ProductGroups T1, Concentrator_ProductGroups T2
          where T1.concentratorproductgroupid = T2.concentratorproductgroupid
          and T1.ImportID > T2.ImportID";

      using (SqlConnection connection = new SqlConnection(_connectionString))
      {
        connection.Open();

        using (SqlCommand command = new SqlCommand(deleteDuplicates, connection))
        {
          command.CommandTimeout = 3600;
          int result = command.ExecuteNonQuery();

          Log.DebugFormat("Finished delete duplicates, result: {0} rows", result);
        }
      }
      Log.Debug("Finish cleanup duplicates");
      #endregion

      #region Merge ProductGroups
      Log.Debug("Start merge productgroups");
      string mergeProductGroups = @"DECLARE @level INT
      DECLARE @getLevel CURSOR

      SET @getLevel = CURSOR FOR
      SELECT distinct level
      FROM concentrator_productgroups

      OPEN @getLevel
      FETCH NEXT
      FROM @getLevel INTO @level
      WHILE @@FETCH_STATUS = 0
      BEGIN
      PRINT @level

      merge productgroups pg
      using(
	      select 
	      pg.id,
	      (select id from productgroups pg2
	      inner join Concentrator_ProductGroups cpg2 on pg2.backendid = cpg2.concentratorproductgroupid and cpg2.level = (@level - 1)
	      where BackendID = cpg.parentconcentratorproductgroupid) as parentid,
	      cpg.name,
	      cpg.createdbyid,
	      cpg.creationtime,
	      cpg.concentratorproductgroupid as BackendID	
	      from Concentrator_ProductGroups cpg 
	      left join productgroups pg on cpg.concentratorproductgroupid = pg.BackendID
	      where level = @level
      ) cp
      ON pg.BackendID = cp.BackendID 
	      and ((cp.parentid is null and pg.parentid is null) or	
	      pg.parentid = cp.parentid)
          when matched 
	            then update set 
		            pg.Name = cp.name,
		            pg.LastmodificationTime = getdate()
	      WHEN NOT Matched by target then insert
                       ([Name]
                       ,[CreatedByID]
                       ,[CreationTime]
                       ,backendid
                       ,parentid)
                 VALUES
                       (cp.name
                       ,cp.createdbyid
                       ,cp.creationtime, cp.BackendID, cp.parentid);
      FETCH NEXT
      FROM @getLevel INTO @level
      END
      CLOSE @getLevel
      DEALLOCATE @getLevel";

      using (SqlConnection connection = new SqlConnection(_connectionString))
      {
        connection.Open();

        using (SqlCommand command = new SqlCommand(mergeProductGroups, connection))
        {
          command.CommandTimeout = 3600;
          int result = command.ExecuteNonQuery();

          Log.DebugFormat("Finished merge productgroups, result: {0} rows", result);
        }
      }
      Log.Debug("Finish merge productgroups");
      #endregion

      #region cleanup wpos productgroups
      Log.Debug("Start update backendid's in productgroups");
      string updateBackendID = @"update pg set pg.backendid = null
      from productgroups pg 
      where pg.backendid not in (
      select concentratorproductgroupid from Concentrator_ProductGroups
      )";

      using (SqlConnection connection = new SqlConnection(_connectionString))
      {
        connection.Open();

        using (SqlCommand command = new SqlCommand(updateBackendID, connection))
        {
          command.CommandTimeout = 3600;
          int result = command.ExecuteNonQuery();

          Log.DebugFormat("Finished update backendid's, result: {0} rows", result);
        }
      }
      Log.Debug("Finish update backendid's in productgroups");

      Log.Debug("Start delete unused ProductProductGroups");
      string deleteProductGroupProducts = @"delete from ProductProductGroups where
      productgroupid in (
      select id from productgroups
      where backendid is null
      )";

      using (SqlConnection connection = new SqlConnection(_connectionString))
      {
        connection.Open();

        using (SqlCommand command = new SqlCommand(deleteProductGroupProducts, connection))
        {
          command.CommandTimeout = 3600;
          int result = command.ExecuteNonQuery();

          Log.DebugFormat("Finished delete  unused productgroupproduct, result: {0} rows", result);
        }
      }
      Log.Debug("Finish delete unused productgroupproducts");

      Log.Debug("Start delete unused productgroups");
      string deleteProductGroups = @"delete from productgroups where backendid is null";

      using (SqlConnection connection = new SqlConnection(_connectionString))
      {
        connection.Open();

        using (SqlCommand command = new SqlCommand(deleteProductGroups, connection))
        {
          command.CommandTimeout = 3600;
          int result = command.ExecuteNonQuery();

          Log.DebugFormat("Finished delete unused productgroups, result: {0} rows", result);
        }
      }
      Log.Debug("Finish delete unused productgroups");

      #endregion
    }

    protected override void TearDown(ConcentratorDataContext context)
    {
      using (SqlConnection connection = new SqlConnection(_connectionString))
      {
        connection.Open();

        using (SqlCommand command = new SqlCommand(String.Format("DROP TABLE {0}", _concentratorProductGroupTable), connection))
        {
          int result = command.ExecuteNonQuery();
        }
      }
    }
  }

  public class WposProductGroups
  {
    public string ConcentratorProductGroupID { get; set; }
    public string ParentConcentratorProductGroupID { get; set; }
    public string Name { get; set; }
    public int CreatedByID { get; set; }
    public DateTime CreationTime { get; set; }
    public int Level { get; set;}
  }
}