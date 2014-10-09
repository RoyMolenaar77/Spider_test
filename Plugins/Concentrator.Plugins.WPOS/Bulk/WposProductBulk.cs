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
  public class WposProductBulk : VendorImportLoader<ConcentratorDataContext>
  {
    #region SQL strings

    private string _concentratorProductTable = "[dbo].[Concentrator_Product]";

    private string _concentratorProductQuery = @"CREATE TABLE [dbo].[Concentrator_Product](
    [BrandID] [nvarchar](50) NULL,
	  [BackendProductID] [nvarchar](max) NULL,
    [CustomItemNumber] [nvarchar](max) NULL,
    [VendorProductID] [nvarchar](max) NULL,
	  [ShortDescription] [nvarchar](max) NULL,
	  [LongDescription] [nvarchar](max) NULL,
	  [LongContentDescription] [nvarchar](max) NULL,
    [UnitCost] [nvarchar](50) NULL,
    [UnitPrice] [nvarchar](50) NULL,
    [TaxRate] [nvarchar](50) NOT NULL,
    [PromisedDeliveryDate] [nvarchar](50) NULL,
    [Status] [nvarchar](max) NULL,
    [CreatedByID] [int] NOT NULL,
    [CreationTime] [datetime] NOT NULL,
    [Rating] [nvarchar](max) NOT NULL,
    [NrOfReviews] [int] NOT NULL,
    [Popularity] [nvarchar](max) NOT NULL
    ) ON [PRIMARY]";

    #endregion

    IEnumerable<WposProducts> _products = null;
    string _connectionString = string.Empty;
    public WposProductBulk(IEnumerable<WposProducts> products, string wposConnectionString)
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

          using (SqlCommand command = new SqlCommand(_concentratorProductQuery, connection))
          {
            int result = command.ExecuteNonQuery();

            using (GenericCollectionReader<WposProducts> reader = new GenericCollectionReader<WposProducts>(_products))
            {
              BulkLoad(_concentratorProductTable, 500, reader, _connectionString);
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
      Log.Debug("Starting merging products");

      string productMergeQuery = @"merge products p
      using (select 
              b.ID,
              c.BackendProductID,
              c.CustomItemNumber,
              c.VendorProductID,
              c.shortdescription,
              c.longdescription,
              c.longcontentdescription,
              c.unitcost,
              c.unitprice,
              c.taxrate,
              c.promiseddeliverydate,
              c.status,
              c.rating,
              c.nrofreviews,
              c.popularity,
              c.createdbyid,
              c.creationtime
             from
             concentrator_product c
             inner join brands b on c.brandid = b.backendid) cp
      ON p.BackendProductID = cp.BackendProductID
      WHEN NOT Matched by target then insert
                 ([BrandID]
                 ,[BackendProductID]
                  ,[CustomItemNumber]
                  ,[VendorProductID]
                 ,[ShortDescription]
                 ,[LongDescription]
                 ,[UnitCost]
                 ,[UnitPrice]
                 ,[TaxRate]
                 ,[PromisedDeliveryDate]
                 ,[Status]
                 ,[CreatedByID]
                 ,[CreationTime]
                 ,[Rating]
                 ,[NrOfReviews]
                 ,[Popularity])
           VALUES
                 (cp.id
                 ,cp.BackendProductID
                 ,cp.CustomItemNumber
                  ,cp.VendorProductID
                 ,cp.shortdescription
                 ,cp.LongDescription           
                 ,cast (cp.unitcost as decimal(18,4))
                 ,cast (cp.unitprice as decimal(18,4))
                 ,cast (cp.taxrate as decimal(18,4)) / 100 + 1
                 ,case when cp.promiseddeliverydate = '' then NULL else cast(cp.promiseddeliverydate as datetime) end
                 ,cp.status
                 ,cp.createdbyid
                 ,cp.creationtime
                 ,cast (cp.rating as decimal(18,4))
                 ,cp.nrofreviews
                 ,cast (cp.popularity as decimal(18,4)))
      WHEN not matched by source then 
	      update set
	      p.deletiontime = getdate(),
	      p.lastmodifiedbyid = 1,
	      p.lastmodificationtime = getdate();";

      using (SqlConnection connection = new SqlConnection(_connectionString))
      {
        connection.Open();

        using (SqlCommand command = new SqlCommand(productMergeQuery, connection))
        {
          command.CommandTimeout = 3600;
          int result = command.ExecuteNonQuery();

          Log.DebugFormat("Finished merging products, result: {0} rows", result);
        }
      }
    }

    protected override void TearDown(ConcentratorDataContext context)
    {
      using (SqlConnection connection = new SqlConnection(_connectionString))
      {
        connection.Open();

        using (SqlCommand command = new SqlCommand(String.Format("DROP TABLE {0}", _concentratorProductTable), connection))
        {
          int result = command.ExecuteNonQuery();
        }
      }
    }
  }

  public class WposProducts
  {
    public string BrandID { get; set; }
    public string BackendProductID { get; set; }
    public string CustomItemNumber { get; set; }
    public string VendorProductID { get; set; }
    public string ShortDescription { get; set; }
    public string LongDescription { get; set; }
    public string LongContentDescription { get; set; }
    public string UnitCost { get; set; }
    public string UnitPrice { get; set; }
    public string TaxRate { get; set; }
    public string PromisedDeliveryDate { get; set; }
    public string Status { get; set; }
    public int CreatedByID { get; set; }
    public DateTime CreationTime { get; set; }
    public string Rating { get; set; }
    public int NrOfReviews { get; set; }
    public string Popularity { get; set; }
  }
}