using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Data;
using System.Net;
using System.Data.Objects;
using System.Xml.Serialization;
using Concentrator.Objects.DataAccess.EntityFramework;
using Concentrator.Objects.Vendors.Base;
using System.Data.SqlClient;

namespace Concentrator.Plugins.WPOS.Bulk
{
  public class WposBrandBulk : VendorImportLoader<ConcentratorDataContext>
  {
    #region SQL strings

    private string _concentratorBrandTable = "[dbo].[Concentrator_Brand]";

    private string _concentratorBrandQuery = @"CREATE TABLE [dbo].[Concentrator_Brand](
    [Name] [nvarchar](50) NULL,
    [BackendID] [nvarchar](max) NULL
    ) ON [PRIMARY]";

    #endregion

    IEnumerable<WposBrands> _brands = null;
    string _connectionString = string.Empty;
    public WposBrandBulk(IEnumerable<WposBrands> brands, string wposConnectionString)
    {
      _brands = brands;
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

          using (SqlCommand command = new SqlCommand(_concentratorBrandQuery, connection))
          {
            int result = command.ExecuteNonQuery();

            using (GenericCollectionReader<WposBrands> reader = new GenericCollectionReader<WposBrands>(_brands))
            {
              BulkLoad(_concentratorBrandTable, 500, reader, _connectionString);
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
      Log.Debug("Starting merging brands");

      string brandMergeQuery = @"merge Brands b
      using(
      select distinct
      name, BackendID
      from concentrator_brand cb
      ) cb
      on b.name = cb.name and b.BackendID = cb.BackendID
      WHEN NOT Matched by target then insert
                 ([Name]
                 ,[BackendID])
           VALUES
                 (cb.name
                 ,cb.BackendID);";

      using (SqlConnection connection = new SqlConnection(_connectionString))
      {
        connection.Open();

        using (SqlCommand command = new SqlCommand(brandMergeQuery, connection))
        {
          command.CommandTimeout = 3600;
          int result = command.ExecuteNonQuery();

          Log.DebugFormat("Finished merging brands, result: {0} rows", result);
        }
      }
    }

    protected override void TearDown(ConcentratorDataContext context)
    {
      using (SqlConnection connection = new SqlConnection(_connectionString))
      {
        connection.Open();

        using (SqlCommand command = new SqlCommand(String.Format("DROP TABLE {0}", _concentratorBrandTable), connection))
        {
          int result = command.ExecuteNonQuery();
        }
      }
    }
  }

  public class WposBrands
  {
    public string Name { get; set; }
    public string BackendID { get; set; }
  }
}