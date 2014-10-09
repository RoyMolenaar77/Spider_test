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
  public class WposBarcodeBulk : VendorImportLoader<ConcentratorDataContext>
  {
    #region SQL strings

    private string _concentratorBarcodeTable = "[dbo].[Concentrator_Barcodes]";

    private string _concentratorBarcodeQuery = @"CREATE TABLE [dbo].[Concentrator_Barcodes](
    [BarcodeStandardID] [int] NOT NULL,
    [BarcodeTypeID] [int] NOT NULL,
	  [Value] [nvarchar](max) NULL,
    [Product_ID] [int] NOT NULL
    ) ON [PRIMARY]";

    #endregion

    IEnumerable<WposBarcodes> _barcodes = null;
    string _connectionString = string.Empty;
    public WposBarcodeBulk(IEnumerable<WposBarcodes> barcodes, string wposConnectionString)
    {
      _barcodes = barcodes;
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

          using (SqlCommand command = new SqlCommand(_concentratorBarcodeQuery, connection))
          {
            int result = command.ExecuteNonQuery();

            using (GenericCollectionReader<WposBarcodes> reader = new GenericCollectionReader<WposBarcodes>(_barcodes))
            {
              BulkLoad(_concentratorBarcodeTable, 500, reader, _connectionString);
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
      Log.Debug("Starting Barcodemerge");

      string productMergeQuery = @"merge barcodes p
      using (select
              c.BarcodeStandardID,
              c.BarcodeTypeID,
              c.Value,
              b.ID as Product_ID
              from
              concentrator_barcodes c
              inner join products b on c.Product_ID = b.backendProductID
              ) cbar
      ON p.ProductID = cbar.Product_ID
      WHEN NOT Matched by target then insert
                 ([StandardID]
                 ,[TypeID]
	               ,[Value]
                 ,[ProductID])
           VALUES
                 (cbar.BarcodeStandardID
                 ,cbar.BarcodeTypeID
                 ,cbar.value
                 ,cbar.Product_ID);";

      using (SqlConnection connection = new SqlConnection(_connectionString))
      {
        connection.Open();

        using (SqlCommand command = new SqlCommand(productMergeQuery, connection))
        {
          command.CommandTimeout = 3600;
          int result = command.ExecuteNonQuery();

          Log.DebugFormat("Finished merging barcodes, result: {0} rows", result);
        }
      }
    }

    protected override void TearDown(ConcentratorDataContext context)
    {
      using (SqlConnection connection = new SqlConnection(_connectionString))
      {
        connection.Open();

        using (SqlCommand command = new SqlCommand(String.Format("DROP TABLE {0}", _concentratorBarcodeTable), connection))
        {
          int result = command.ExecuteNonQuery();
        }
      }
    }
  }

  public class WposBarcodes
  {
    public string BarcodeStandardID { get; set; }
    public string BarcodeTypeID { get; set; }
    public string Value { get; set; }
    public string Product_ID { get; set; }
  }
}