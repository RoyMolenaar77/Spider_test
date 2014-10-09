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
  public class WposImagesBulk : VendorImportLoader<ConcentratorDataContext>
  {
    #region SQL strings
    private string _concentratorImageTable = "[dbo].[Concentrator_ProductImages]";

    private string _concentratorImageQuery = @"CREATE TABLE [dbo].[Concentrator_ProductImages](
    [ImageType] [nvarchar](max) NULL,
    [ImagePath] [nvarchar](max) NULL,
    [SequenceNo] [nvarchar](max) NULL,
    [CreatedByID] [nvarchar](max) NULL,
    [CreationTime] [datetime] NOT NULL,
    [Product_ID] [nvarchar](max) NULL,
    [Brand_ID] [nvarchar](max) NULL
    ) ON [PRIMARY]";
    #endregion

    IEnumerable<WposImages> _images = null;
    string _connectionString = string.Empty;
    public WposImagesBulk(IEnumerable<WposImages> images, string wposConnectionString)
    {
      _images = images;
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

          using (SqlCommand command = new SqlCommand(_concentratorImageQuery, connection))
          {
            int result = command.ExecuteNonQuery();

            using (GenericCollectionReader<WposImages> reader = new GenericCollectionReader<WposImages>(_images))
            {
              BulkLoad(_concentratorImageTable, 500, reader, _connectionString);
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
      Log.Debug("Starting merging images");

      string imageMergeQuery = @"merge productimages p
      using (  select 
              c.ImageType,
              c.ImagePath,
              c.SequenceNo,
              c.CreatedByID,
              c.CreationTime,
              p.id as Product_ID,
              b.id as Brand_ID
             from
             Concentrator_ProductImages c
             inner join brands b on b.backendid = c.brand_id
             inner join products p on p.backendproductid = c.product_id) cp
      ON p.ImagePath = cp.ImagePath
      WHEN NOT Matched by target then insert
                 ([ImageType]
                 ,[ImagePath]
                 ,[SequenceNumber]
                 ,[CreatedByID]
                 ,[CreationTime]
                 ,[ProductID]
                 ,[BrandID])
           VALUES
                 (cp.ImageType
                 ,cp.ImagePath
                 ,cp.SequenceNo
                 ,cp.CreatedByID
                 ,cp.CreationTime
                 ,cp.Product_ID
                 ,cp.Brand_ID)

      WHEN not matched by source then 
	      update set
	      p.lastmodifiedbyid = 1,
	      p.lastmodificationtime = getdate();";

      using (SqlConnection connection = new SqlConnection(_connectionString))
      {
        connection.Open();

        using (SqlCommand command = new SqlCommand(imageMergeQuery, connection))
        {
          command.CommandTimeout = 3600;
          int result = command.ExecuteNonQuery();

          Log.DebugFormat("Finished merging images, result: {0} rows", result);
        }
      }
    }

    protected override void TearDown(ConcentratorDataContext context)
    {
      using (SqlConnection connection = new SqlConnection(_connectionString))
      {
        connection.Open();

        using (SqlCommand command = new SqlCommand(String.Format("DROP TABLE {0}", _concentratorImageTable), connection))
        {
          int result = command.ExecuteNonQuery();
        }
      }
    }
  }

  public class WposImages
  {
    public string ImageType { get; set; }
    public string ImagePath { get; set; }
    public string SequenceNo { get; set; }
    public string CreatedByID { get; set; }
    public DateTime CreationTime { get; set; }
    public string Product_ID { get; set; }
    public string Brand_ID { get; set; }
  }
}