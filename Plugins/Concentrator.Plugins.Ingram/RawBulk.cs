using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.DataAccess.EntityFramework;
using Concentrator.Objects.Vendors.Base;

namespace Concentrator.Plugins.Ingram
{
  class RawBulk : VendorImportLoader<ConcentratorDataContext>
  {
    #region SQL strings

    private string _tempDescriptionsTable = "[VendorTemp].[Concentrator_tempDescriptions]";
    private string _tempSeriesTable = "[VendorTemp].[Concentrator_tempSeries]";
    private string _tempCategoriesTable = "[VendorTemp].[Concentrator_tempCategories]";
    private string _tempRelatedProductsTable = "[VendorTemp].[Concentrator_tempRelated]";
    private string _tempStockandPriceTable = "[VendorTemp].[Concentrator_tempStockPrice]";

    private string _tempDescriptionsQuery = @"CREATE TABLE [VendorTemp].[Concentrator_tempDescriptions](
    [EAN] [nvarchar](255)  NULL,
    [Text] [nvarchar] (MAX) NULL,
    ) ON [PRIMARY]";

    private string _tempSeriesQuery = @"CREATE TABLE [VendorTemp].[Concentrator_tempSeries](
    [SeriesID] [nvarchar] (255) NULL,
    [Text] [nvarchar] (MAX) NULL,
    ) ON [PRIMARY]";

    private string _tempCategoriesQuery = @"CREATE TABLE [VendorTemp].[Concentrator_tempCategories](
    [IngramSubjectCode] [nvarchar] (255) NULL,
    [IngramSubjectDescription] [nvarchar] (MAX) NULL,
	  ) ON [PRIMARY]";

    private string _tempRelatedProductsQuery = @"CREATE TABLE [VendorTemp].[Concentrator_tempRelated](
    [EAN] [nvarchar] (255) NULL,
    [FamilyID] [nvarchar] (255) NULL,
	  [ItemID] [nvarchar] (255) NULL
    ) ON [PRIMARY]";

    private string _tempStockandPriceQuery = @"CREATE TABLE [VendorTemp].[Concentrator_tempStockPrice](
     [EAN] [nvarchar] (255) NULL,
	  [Price] [nvarchar] (100) NULL,
    [CostPrice] [nvarchar] (100) NULL,
 [QuantityOnHand] [nvarchar](50)  NULL,
[CommercialStatus] [nvarchar] (100) NULL
    ) ON [PRIMARY]";


    #endregion

    RawBulkModel _data = null;

    int _vendorID;
    //string _connectionString = string.Empty;

    public RawBulk(RawBulkModel data,
      int vendorID)
    {
      _data = data;
      _vendorID = vendorID;
      // _connectionString = ConnectionString;
    }

    public override void Init(ConcentratorDataContext context)
    {
      base.Init(context);

      try
      {
        var _tempDescriptions = _data.Descriptions;

        context.ExecuteStoreCommand(_tempDescriptionsQuery);
        using (GenericCollectionReader<tempDescriptionsModel> reader = new GenericCollectionReader<tempDescriptionsModel>(_tempDescriptions))
        {
          BulkLoad(_tempDescriptionsTable, 500, reader);
        }

        var _tempSeries = _data.Series;

        context.ExecuteStoreCommand(_tempSeriesQuery);
        using (GenericCollectionReader<tempSeriesModel> reader = new GenericCollectionReader<tempSeriesModel>(_tempSeries))
        {
          BulkLoad(_tempSeriesTable, 500, reader);
        }
        context.ExecuteStoreCommand(_tempCategoriesQuery);

        var _tempCategories = _data.Categories;

        using (GenericCollectionReader<tempCategoriesModel> reader = new GenericCollectionReader<tempCategoriesModel>(_tempCategories))
        {
          BulkLoad(_tempCategoriesTable, 500, reader);
        }
        context.ExecuteStoreCommand(_tempRelatedProductsQuery);

        var _tempRelatedProducts = _data.RelatedProducts;

        using (GenericCollectionReader<tempRelatedProductsModel> reader = new GenericCollectionReader<tempRelatedProductsModel>(_tempRelatedProducts))
        {
          BulkLoad(_tempRelatedProductsTable, 500, reader);
        }

        context.ExecuteStoreCommand(_tempStockandPriceQuery);

        var _tempStockandPrice = _data.StockandPrice;
        using (GenericCollectionReader<tempStockandPriceModel> reader = new GenericCollectionReader<tempStockandPriceModel>(_tempStockandPrice))
        {
          BulkLoad(_tempStockandPriceTable, 500, reader);
        }
      }
      catch (Exception ex)
      {
        _log.Error("Error executing bulk copy");
      }
    }

    public override void Sync(ConcentratorDataContext context)
    {
      Log.Debug("Starting RawBulkTemp Import");

      string _RawBulktempDescriptionsQuery = @"merge productdescription p
      using (select 
              va.ProductID,
              1 as LanguageID,
              va.VendorID,
              c.Text as Longcontentdescription,
              1 as CreatedBy,
              Getdate() as CreationTime
             from
             [VendorTemp].[Concentrator_tempDescriptions] c
             inner join vendorassortment va on c.ean = va.customitemnumber) cp
             ON p.productid = cp.productid
             and p.languageid = cp.languageid
             and p.vendorid = cp.vendorid
      WHEN Matched then update set
			p.longcontentdescription = cp.longcontentdescription           
      WHEN NOT Matched by target then insert
                 ([ProductID]
                 ,[LanguageID]
                 ,[VendorID]
                 ,[LongContentDescription]
                 ,[CreatedBy]
                 ,[CreationTime])
           VALUES
                 (cp.ProductID
                 ,cp.LanguageID
                 ,cp.VendorID
                 ,cp.LongContentDescription
                 ,cp.CreatedBy
                 ,cp.CreationTime);";

      string _RawBulktempSeriesQuery = @"merge ProductGroupVendor p
      using (select 
              pgv.ProductGroupVendorID,
              pgv.ProductGroupID,
              pgv.VendorID,
              c.Text as VendorName
             from
             [VendorTemp].[Concentrator_tempSeries] c
             inner join ProductGroupVendor pgv on c.SeriesID = pgv.VendorProductGroupCode1) cp
             ON p.ProductGroupVendorID = cp.ProductGroupVendorID
      WHEN Matched then update set
			p.VendorName = cp.VendorName;";

      string _RawBulktempCategoriesQuery = @"merge ProductGroupVendor p
      using (select 
              pgv.ProductGroupVendorID,
              pgv.ProductGroupID,
              pgv.VendorID,
              c.IngramSubjectDescription as VendorName
             from
             [VendorTemp].[Concentrator_tempCategories] c
             inner join ProductGroupVendor pgv on c.IngramSubjectCode = pgv.VendorProductGroupCode2) cp
             ON p.ProductGroupVendorID = cp.ProductGroupVendorID
      WHEN Matched then update set
			p.VendorName = cp.VendorName;";

      //CHECKEN MET TIM
      string _RawBulktempStockPriceQuery = @"merge VendorPrice p
      using (select 
              va.VendorAssortmentID,
              0 as MinimumQuantity,
              c.Price,
              c.CostPrice,
              c.CommercialStatus
             from
             [VendorTemp].[Concentrator_tempStockPrice] c
             inner join VendorAssortment va on c.EAN = va.CustomItemNumber) cp
             ON p.VendorAssortmentID = cp.VendorAssortmentID
             and p.MinimumQuantity = cp.MinimumQuantity
      WHEN Matched then update set
			p.Price = cp.Price,
		 p.CostPrice = cp.CostPrice,
		 p.CommercialStatus = cp.CommercialStatus;";

      string _RawBulktempStockPrice2Query = string.Format(@"merge VendorStock p
      using (SELECT c.QuantityOnHand, vs.ProductID 
			FROM [VendorTemp].[Concentrator_tempStockPrice] c
			inner join vendorassortment va ON va.customitemnumber = c.EAN 
			inner join vendorstock vs on va.vendorid = vs.vendorid and va.ProductID = vs.ProductID ) cp
             ON p.ProductID = cp.ProductID and p.vendorID = {0}
             
      WHEN Matched then update set
			p.QuantityOnHand = cp.QuantityOnHand;",_vendorID);


      context.ExecuteStoreCommand(_RawBulktempDescriptionsQuery);
      context.ExecuteStoreCommand(_RawBulktempSeriesQuery);
      context.ExecuteStoreCommand(_RawBulktempCategoriesQuery);
      context.ExecuteStoreCommand(_RawBulktempStockPriceQuery);
      context.ExecuteStoreCommand(_RawBulktempStockPrice2Query);
    }

    protected override void TearDown(ConcentratorDataContext context)
    {
      try
      {
        context.ExecuteStoreCommand(String.Format("DROP TABLE {0}", _tempDescriptionsTable));
      }
      catch { }
      try
      {
        context.ExecuteStoreCommand(String.Format("DROP TABLE {0}", _tempSeriesTable));
      }
      catch { } try
      {
        context.ExecuteStoreCommand(String.Format("DROP TABLE {0}", _tempCategoriesTable));
      }
      catch { } try
      {
        context.ExecuteStoreCommand(String.Format("DROP TABLE {0}", _tempRelatedProductsTable));
      }
      catch { } try
      {
        context.ExecuteStoreCommand(String.Format("DROP TABLE {0}", _tempStockandPriceTable));
      }
      catch { }
    }
  }

  class RawBulkModel
  {
    public IEnumerable<tempDescriptionsModel> Descriptions { get; set; }
    public IEnumerable<tempSeriesModel> Series { get; set; }
    public IEnumerable<tempCategoriesModel> Categories { get; set; }
    public IEnumerable<tempRelatedProductsModel> RelatedProducts { get; set; }
    public IEnumerable<tempStockandPriceModel> StockandPrice { get; set; }
  }

  class tempDescriptionsModel
  {
    public string EAN { get; set; }
    public string Text { get; set; }
  }

  class tempSeriesModel
  {
    public string SeriesID { get; set; }
    public string Text { get; set; }
  }

  class tempCategoriesModel
  {
    public string IngramSubjectCode { get; set; }
    public string IngramSubjectDescription { get; set; }
  }

  class tempRelatedProductsModel
  {
    public string EAN { get; set; }
    public string FamilyID { get; set; }
    public string ItemID { get; set; }
  }

  public class tempStockandPriceModel
  {
    public string EAN { get; set; }
    public string Price { get; set; }
    public string CostPrice { get; set; }
    public string QuantityOnHand { get; set; }
    public string CommercialStatus { get; set; }
  }
}
