using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Data;
using System.Net;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.ComponentModel;
using System.Diagnostics;
using System.Data.Objects;
using System.Xml.Serialization;

using Concentrator.Objects;
using Concentrator.Objects.DataAccess.EntityFramework;
using Concentrator.Objects.Vendors.Base;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.Models.Brands;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Models.Attributes;

namespace Concentrator.Objects.Vendors.Bulk
{
  public class VendorAssortmentBulk : VendorImportLoader<ConcentratorDataContext>
  {
    #region SQL strings

    private int VendorID
    {
      get;
      set;
    }
    private int DefaultVendorID
    {
      get;
      set;
    }

    private string _vendorAssortImportTableName = "[VendorTemp].[VendorImport_VendorAssortment_{0}]";

    //[VendorAssortmentID] [int] NULL,
    //[ProductID] [int] NULL,
    //[VendorID] [int] NULL,
    private string _vendorAssortmentTableQuery = @"CREATE TABLE {0} (
          [VendorID] [int] NULL,
          [DefaultVendorID] [int] NULL,
          [CustomItemNumber] [nvarchar](255) NOT NULL,
          [VendorItemNumber] [nvarchar](255) NOT NULL,
          [VendorBrandCode] [nvarchar](255) NULL,
          [ShortDescription] [nvarchar](255) NULL,
          [LongDescription] [nvarchar](1000) NULL,
          [LineType] [nvarchar](10) NULL,
          [LedgerClass] [nvarchar](50) NULL,
          [ExtendedCatalog] [nvarchar](10) NULL,
          [ProductDesk] [nvarchar](150) NULL,
          [Barcode] [nvarchar](150) NULL,
          [IsConfigurable] int not null,
          [VendorProductGroupCode1] [nvarchar](50) NULL,
          [VendorProductGroupCodeName1] [nvarchar](200) NULL,
          [VendorProductGroupCode2] [nvarchar](50) NULL,
          [VendorProductGroupCodeName2] [nvarchar](200) NULL,
          [VendorProductGroupCode3] [nvarchar](50) NULL,
          [VendorProductGroupCodeName3] [nvarchar](200) NULL,
          [VendorProductGroupCode4] [nvarchar](50) NULL,
          [VendorProductGroupCodeName4] [nvarchar](200) NULL,
          [VendorProductGroupCode5] [nvarchar](50) NULL,
          [VendorProductGroupCodeName5] [nvarchar](200) NULL,
          [VendorProductGroupCode6] [nvarchar](50) NULL,
          [VendorProductGroupCodeName6] [nvarchar](200) NULL,
          [VendorProductGroupCode7] [nvarchar](50) NULL,
          [VendorProductGroupCodeName7] [nvarchar](200) NULL,
          [VendorProductGroupCode8] [nvarchar](50) NULL,
          [VendorProductGroupCodeName8] [nvarchar](200) NULL,
          [VendorProductGroupCode9] [nvarchar](50) NULL,
          [VendorProductGroupCodeName9] [nvarchar](200) NULL,
          [VendorProductGroupCode10] [nvarchar](50) NULL,
          [VendorProductGroupCodeName10] [nvarchar](200) NULL,
          [ParentProductCustomItemNumber] [nvarchar](200) NULL,
          [VendorAssortmentID] [int] NULL,
          [ProductID] [int] NULL,
          [ParentProductID] [int] NULL,
          [BrandID] [int] NULL,
          [ProductGroupVendorID1] [int] NULL,
          [ProductGroupVendorID2] [int] NULL,
          [ProductGroupVendorID3] [int] NULL
          
) ON [PRIMARY]";
    //[ActivationKey] [nvarchar](255) NULL,
    //[ZoneReferenceID] [nvarchar](255) NULL,
    //[ShipmentRateTableReferenceID] [nvarchar](255) NULL,


    private string _vendorBrandImportTableName = "[VendorTemp].[VendorImport_VendorBrandCode_{0}]";
    private string _vendorBrandTableQuery = @"CREATE TABLE {0} (
	[VendorID] [int] NOT NULL,
	[VendorBrandCode] [nvarchar](150) NOT NULL,
[ParentBrandCode] [nvarchar](150) NULL,
	[Name] [nvarchar](150) NULL,
	[Logo] [nvarchar](255) NULL,
[BrandID] [int] NULL) ON [PRIMARY]";

    private string _relatedProductImportTableName = "[VendorTemp].[VendorImport_RelatedProduct_{0}]";
    private string _relatedProductTableQuery = @"CREATE TABLE {0} (
	[VendorID] [int] NOT NULL,
	[DefaultVendorID] [int] NOT NULL,
	[CustomItemNumber] [nvarchar](255) NOT NULL,
	[RelatedProductType] [nvarchar](100) NULL,
  [RelatedCustomItemNumber] [nvarchar](255) NULL,
  [IsConfigured] int not null) 
ON [PRIMARY]";

    private string _vendorAttributeImportTableName = "[VendorTemp].[VendorImport_ProductAttribute_{0}]";
    private string _vendorAttributeTableQuery = @"CREATE TABLE {0} (
	[VendorID] [int] NOT NULL,
	[DefaultVendorID] [int] NOT NULL,
[CustomItemNumber] [nvarchar](255) NOT NULL,
	[AttributeID] [int] NULL,
	[Value] [nvarchar](3000) NULL,
[LanguageID] [nvarchar](50) NULL,
[AttributeCode] [nvarchar](150) NULL) 
ON [PRIMARY]";


    private string _vendorPriceImportTableName = "[VendorTemp].[VendorImport_VendorPrice_{0}]";
    private string _vendorPriceTableQuery = @"CREATE TABLE {0} (
	[VendorID] [int] NOT NULL,
	[DefaultVendorID] [int] NOT NULL,
[CustomItemNumber] [nvarchar](255) NOT NULL,
	[Price] [nvarchar](50) NULL,
	[CostPrice] [nvarchar](50) NULL,  
[SpecialPrice] [nvarchar](50) NULL,
[TaxRate] [nvarchar](50) NULL,
[MinimumQuantity] [int] NULL,
[CommercialStatus] [nvarchar](50) NULL) 
ON [PRIMARY]";

    private string _vendorStockImportTableName = "[VendorTemp].[VendorImport_VendorStock_{0}]";
    private string _vendorStockTableQuery = @"CREATE TABLE {0} (
	[VendorID] [int] NOT NULL,
	[DefaultVendorID] [int] NOT NULL,
[CustomItemNumber] [nvarchar](255) NOT NULL,
	[QuantityOnHand] [int] NOT NULL,
	[StockType] [nvarchar](50) NULL, 
[StockStatus] [nvarchar](50) NULL) 
ON [PRIMARY]";


    private string _productImageImportTableName = "[VendorTemp].[VendorImport_ProductImage_{0}]";
    private string _productImageTableQuery = @"CREATE TABLE {0} (
	[VendorID] [int] NOT NULL,
	[DefaultVendorID] [int] NOT NULL,
[CustomItemNumber] [nvarchar](255) NOT NULL)";



    private string _vendorDescriptionImportTableName = "[VendorTemp].[VendorImport_Descriptions_{0}]";
    private string _vendorDescriptionTableQuery = @"CREATE TABLE {0} (
	[VendorID] [int] NOT NULL,
	[DefaultVendorID] [int] NOT NULL,
[CustomItemNumber] [nvarchar](255) NOT NULL,
  [LanguageID] [int] NOT NULL,
	[ProductName] [nvarchar](255) NOT NULL,
	[ShortContentDescription] [nvarchar](1000) NULL, 
[LongContentDescription] [nvarchar](MAX) NULL, 
[ModelName] [nvarchar](300) NULL, 
[ShortSummaryDescription] [nvarchar](500) NULL, 
[LongSummaryDescription] [nvarchar](2500) NULL) 
ON [PRIMARY]";

    #endregion

    VendorAssortmentBulkConfiguration Configuration { set; get; }

    IEnumerable<VendorAssortmentItem> _assortment = null;

    public VendorAssortmentBulk(IEnumerable<VendorAssortmentItem> assortment, int vendorID, int? defaultVendorid, VendorAssortmentBulkConfiguration config)
    {
      //TODO: apply EmbeddedResourceHelper.Bind(this); NB: This requires a migration of the helper class to a more common library
      _assortment = assortment;
      Configuration = new VendorAssortmentBulkConfiguration
      {
         IncludeBrandMapping = config.IncludeBrandMapping, 
         IsPartialAssortment = config.IsPartialAssortment, 
         ProcessProductBarcode = config.ProcessProductBarcode
      };

      DefaultVendorID = defaultVendorid.HasValue ? defaultVendorid.Value : vendorID;

      VendorID = vendorID;
      _vendorAssortImportTableName = string.Format(_vendorAssortImportTableName, vendorID);
      _vendorAssortmentTableQuery = string.Format(_vendorAssortmentTableQuery, _vendorAssortImportTableName);

      _vendorBrandImportTableName = string.Format(_vendorBrandImportTableName, vendorID);
      _vendorBrandTableQuery = string.Format(_vendorBrandTableQuery, _vendorBrandImportTableName);

      _relatedProductImportTableName = string.Format(_relatedProductImportTableName, vendorID);
      _relatedProductTableQuery = string.Format(_relatedProductTableQuery, _relatedProductImportTableName);

      _vendorAttributeImportTableName = string.Format(_vendorAttributeImportTableName, vendorID);
      _vendorAttributeTableQuery = string.Format(_vendorAttributeTableQuery, _vendorAttributeImportTableName);

      _vendorPriceImportTableName = string.Format(_vendorPriceImportTableName, vendorID);
      _vendorPriceTableQuery = string.Format(_vendorPriceTableQuery, _vendorPriceImportTableName);

      _vendorStockImportTableName = string.Format(_vendorStockImportTableName, vendorID);
      _vendorStockTableQuery = string.Format(_vendorStockTableQuery, _vendorStockImportTableName);

      _productImageImportTableName = string.Format(_productImageImportTableName, vendorID);
      _productImageTableQuery = string.Format(_productImageTableQuery, _productImageImportTableName);

      _vendorDescriptionImportTableName = string.Format(_vendorDescriptionImportTableName, vendorID);
      _vendorDescriptionTableQuery = string.Format(_vendorDescriptionTableQuery, _vendorDescriptionImportTableName);
    }

    public override void Init(ConcentratorDataContext context)
    {
      base.Init(context);

      try
      {
        context.ExecuteStoreCommand(_vendorAssortmentTableQuery);

        using (GenericCollectionReader<VendorProduct> reader = new GenericCollectionReader<VendorProduct>(_assortment.Where(x => x.VendorProduct != null).Select(x => x.VendorProduct)))
        {
          BulkLoad(_vendorAssortImportTableName, 1000, reader);
        }

        context.ExecuteStoreCommand(_vendorPriceTableQuery);

        var vendorPrices = (from bv in _assortment.Select(x => x.VendorImportPrices)
                            from b in bv
                            select b
                               );

        using (GenericCollectionReader<VendorImportPrice> reader = new GenericCollectionReader<VendorImportPrice>(vendorPrices))
        {
          BulkLoad(_vendorPriceImportTableName, 1000, reader);
        }

        context.ExecuteStoreCommand(_vendorStockTableQuery);

        var vendorStocks = (from bv in _assortment.Select(x => x.VendorImportStocks)
                            from b in bv
                            select b
                               );

        using (GenericCollectionReader<VendorImportStock> reader = new GenericCollectionReader<VendorImportStock>(vendorStocks))
        {
          BulkLoad(_vendorStockImportTableName, 1000, reader);
        }

        context.ExecuteStoreCommand(_vendorBrandTableQuery);

        var brandVendors = _assortment
          .SelectMany(x => x.BrandVendors)
          .GroupBy(brandVendor => brandVendor.VendorBrandCode)
          .Select(grouping => new VendorImportBrand
          {
            VendorID = grouping.First().VendorID,
            VendorBrandCode = grouping.Key,
            ParentBrandCode = grouping.First().ParentBrandCode,
            Name = grouping.First().Name,
            Logo = grouping.First().Logo
          });

        using (var reader = new GenericCollectionReader<VendorImportBrand>(brandVendors))
        {
          BulkLoad(_vendorBrandImportTableName, 100, reader);
        }

        context.ExecuteStoreCommand(_relatedProductTableQuery);

        var relatedProductList = (from bv in _assortment.Where(x => x.RelatedProducts != null).Select(x => x.RelatedProducts)
                                  from b in bv
                                  where !string.IsNullOrEmpty(b.RelatedCustomItemNumber)
                                  select b
                               );

        using (GenericCollectionReader<VendorImportRelatedProduct> reader = new GenericCollectionReader<VendorImportRelatedProduct>(relatedProductList))
        {
          BulkLoad(_relatedProductImportTableName, 1000, reader);
        }

        context.ExecuteStoreCommand(_vendorAttributeTableQuery);

        //var pr = _assortment.First().VendorImportAttributeValues.Where(c => c.AttributeCode == "Productgroup").ToList();

        var attributevalues = (from bv in _assortment.SelectMany(x => x.VendorImportAttributeValues)

                               where !string.IsNullOrEmpty(bv.Value)
                               select bv
                               );

        //var prAfter = attributevalues.Where(c => c.AttributeCode == "Productgroup").ToList();

        using (GenericCollectionReader<VendorImportAttributeValue> reader = new GenericCollectionReader<VendorImportAttributeValue>(attributevalues))
        {
          BulkLoad(_vendorAttributeImportTableName, 1000, reader);
        }

        context.ExecuteStoreCommand(_vendorDescriptionTableQuery);

        Log.Info("Created vendor description table");
        var productDescriptions = _assortment.SelectMany(x => x.VendorProductDescriptions);

        using (GenericCollectionReader<VendorProductDescription> reader = new GenericCollectionReader<VendorProductDescription>(productDescriptions))
        {
          BulkLoad(_vendorDescriptionImportTableName, 1000, reader);
        }
      }
      catch (Exception ex)
      {
        _log.Error("Error execute bulk copy", ex);
      }
    }

    public override void Sync(ConcentratorDataContext context)
    {
      context.CommandTimeout = 7200;

      #region Brands

      Log.DebugFormat("Start merge brands for vendor {0}", VendorID);

      var mergeBrands = (String.Format(@"
MERGE {0} AS ass
USING [dbo].[BrandVendor] AS BV
ON BV.[VendorID] = ass.[DefaultVendorID] AND LOWER(bv.[VendorBrandCode]) = LOWER(ass.[VendorBrandCode])
WHEN MATCHED THEN
  UPDATE SET ass.[BrandID] = bv.[BrandID];", _vendorAssortImportTableName));
      context.ExecuteStoreCommand(mergeBrands);

      var newBrands = String.Format(@"
INSERT INTO [dbo].[BrandVendor] ([BrandID], [VendorID], [VendorBrandCode])
SELECT DISTINCT ISNULL([BrandID], -1), [VendorID], [VendorBrandCode]
FROM {0}
WHERE [BrandID] IS NULL", _vendorAssortImportTableName);

      context.ExecuteStoreCommand(newBrands);

      Log.DebugFormat("Finish merge brands for vendor {0}", VendorID);

      #endregion

      #region Products
      Log.DebugFormat("Start merge products for vendor {0}", VendorID);
      string removeVendorstock = (String.Format(@"delete VendorStock where ProductID in(
select p.ProductID
from Product p
inner join {0} ass on rtrim(upper(p.VendorItemNumber)) = rtrim(upper(ass.VendorItemNumber))
inner join Product p2 on rtrim(upper(p.VendorItemNumber)) = rtrim(upper(ass.VendorItemNumber)) and ass.BrandID = p2.BrandID and p2.ProductID != p.ProductID
where ass.BrandID is not null and ass.BrandID > 0 and p.BrandID < 0 and p2.BrandID > 0
)", _vendorAssortImportTableName));

      context.ExecuteStoreCommand(removeVendorstock);

      string removeProductMedia = (String.Format(@"delete ProductMedia where ProductID in(
select p.ProductID
from Product p
inner join {0} ass on rtrim(upper(p.VendorItemNumber)) = rtrim(upper(ass.VendorItemNumber))
inner join Product p2 on rtrim(upper(p2.VendorItemNumber)) = rtrim(upper(ass.VendorItemNumber)) and ass.BrandID = p2.BrandID and p2.ProductID != p.ProductID
where ass.BrandID is not null and ass.BrandID > 0 and p.BrandID < 0 and p2.BrandID > 0
)", _vendorAssortImportTableName));

      context.ExecuteStoreCommand(removeProductMedia);

      string removeProductMatch = (String.Format(@"delete productmatch where ProductID in(
select p.ProductID
from Product p
inner join {0} ass on rtrim(upper(p.VendorItemNumber)) = rtrim(upper(ass.VendorItemNumber))
inner join Product p2 on rtrim(upper(p2.VendorItemNumber)) = rtrim(upper(ass.VendorItemNumber)) and ass.BrandID = p2.BrandID and p2.ProductID != p.ProductID
where ass.BrandID is not null and ass.BrandID > 0 and p.BrandID < 0 and p2.BrandID > 0
)", _vendorAssortImportTableName));

      context.ExecuteStoreCommand(removeProductMatch);

      string removeMissingContent = (String.Format(@"delete MissingContent where ConcentratorProductID in(
select p.ProductID
from Product p
inner join {0} ass on rtrim(upper(p.VendorItemNumber)) = rtrim(upper(ass.VendorItemNumber))
inner join Product p2 on rtrim(upper(p2.VendorItemNumber)) = rtrim(upper(ass.VendorItemNumber)) and ass.BrandID = p2.BrandID and p2.ProductID != p.ProductID
where ass.BrandID is not null and ass.BrandID > 0 and p.BrandID < 0 and p2.BrandID > 0
)", _vendorAssortImportTableName));

      context.ExecuteStoreCommand(removeMissingContent);

      string removeRelatedProducts = (String.Format(@"delete RelatedProduct where ProductID in(
select p.ProductID
from Product p
inner join {0} ass on rtrim(upper(p.VendorItemNumber)) = rtrim(upper(ass.VendorItemNumber))
inner join Product p2 on rtrim(upper(p2.VendorItemNumber)) = rtrim(upper(ass.VendorItemNumber)) and ass.BrandID = p2.BrandID and p2.ProductID != p.ProductID
where ass.BrandID is not null and ass.BrandID > 0 and p.BrandID < 0 and p2.BrandID > 0
)", _vendorAssortImportTableName));

      context.ExecuteStoreCommand(removeRelatedProducts);

      string removeRelatedProductsR = (String.Format(@"delete RelatedProduct where RelatedProductID in(
select p.ProductID
from Product p
inner join {0} ass on rtrim(upper(p.VendorItemNumber)) = rtrim(upper(ass.VendorItemNumber))
inner join Product p2 on rtrim(upper(p2.VendorItemNumber)) = rtrim(upper(ass.VendorItemNumber)) and ass.BrandID = p2.BrandID and p2.ProductID != p.ProductID
where ass.BrandID is not null and ass.BrandID > 0 and p.BrandID < 0 and p2.BrandID > 0
)", _vendorAssortImportTableName));

      context.ExecuteStoreCommand(removeRelatedProductsR);

      string removeContentLedger = (String.Format(@"delete ContentLedger where ProductID in(
select p.ProductID
from Product p
inner join {0} ass on rtrim(upper(p.VendorItemNumber)) = rtrim(upper(ass.VendorItemNumber))
inner join Product p2 on rtrim(upper(p2.VendorItemNumber)) = rtrim(upper(ass.VendorItemNumber)) and ass.BrandID = p2.BrandID and p2.ProductID != p.ProductID
where ass.BrandID is not null and ass.BrandID > 0 and p.BrandID < 0 and p2.BrandID > 0
)", _vendorAssortImportTableName));

      //context.ExecuteStoreCommand(removeContentLedger);

      string removeProducts = (String.Format(@"
DELETE Product 
WHERE ProductID in
(
  SELECT p.ProductID
  FROM Product p
  JOIN {0} ass on rtrim(upper(p.VendorItemNumber)) = rtrim(upper(ass.VendorItemNumber))
  JOIN Product p2 on rtrim(upper(p2.VendorItemNumber)) = rtrim(upper(ass.VendorItemNumber)) and ass.BrandID = p2.BrandID and p2.ProductID != p.ProductID
  WHERE ass.BrandID is not null and ass.BrandID > 0 and p.BrandID < 0 and p2.BrandID > 0
)", _vendorAssortImportTableName));

      context.ExecuteStoreCommand(removeProducts);

      string updateProductBrands = (String.Format(@"
UPDATE P SET P.BrandID = ass.BrandID
from Product p
inner join {0} ass on rtrim(upper(p.VendorItemNumber)) = rtrim(upper(ass.VendorItemNumber))
where ass.BrandID is not null and ass.BrandID > 0 and p.BrandID < 0 "
        , _vendorAssortImportTableName));

      context.ExecuteStoreCommand(updateProductBrands);

      string mergeProducts = (String.Format(@"
MERGE {0} ass
USING product p
ON (RTrim(upper(p.VendorItemNumber)) = RTrim(upper(ass.VendorItemNumber)) and p.BrandID = isnull(ass.BrandID,-1))
WHEN MATCHED THEN
  UPDATE SET ass.ProductID = p.ProductID;"
        , _vendorAssortImportTableName));

      context.ExecuteStoreCommand(mergeProducts);

      string mergeParentProductIDsQuery = (String.Format(@"
MERGE {0} ass
USING product p
ON (RTrim(upper(p.VendorItemNumber)) = RTrim(upper(ass.ParentProductCustomItemNumber)) and p.BrandID = isnull(ass.BrandID,-1))
WHEN MATCHED THEN
  UPDATE SET ass.ParentProductID = p.ProductID;"
        , _vendorAssortImportTableName));

      context.ExecuteStoreCommand(mergeParentProductIDsQuery);

      string insertNewProducts = (String.Format(@"
INSERT INTO Product (BrandID,SourceVendorID,VendorItemNumber,CreatedBy, IsConfigurable)
SELECT DISTINCT ISNULL(ass.BrandID, -1), {1}, RTRIM(ASS.Vendoritemnumber), 1, CAST(ass.IsConfigurable AS BIT)
FROM {0} ass
WHERE ass.ProductID IS NULL", _vendorAssortImportTableName, DefaultVendorID));

      context.ExecuteStoreCommand(insertNewProducts);

      string updateProductConfigurability = (string.Format(@"
UPDATE Product
SET Product.IsConfigurable = {0}.IsConfigurable
FROM Product, {0}
WHERE Product.ProductID = {0}.ProductID AND Product.ProductID IS NOT NULL", _vendorAssortImportTableName));

      context.ExecuteStoreCommand(updateProductConfigurability);

      string mergeNewProducts = (String.Format(@"
MERGE {0} ass
USING product p
ON (RTRIM(upper(p.VendorItemNumber)) = RTRIM(upper(ass.VendorItemNumber)) and p.BrandID = isnull(ass.BrandID,-1))
WHEN MATCHED THEN
  UPDATE SET ass.ProductID = P.Productid;"
        , _vendorAssortImportTableName));

      context.ExecuteStoreCommand(mergeNewProducts);

      string updateProductRelations = (string.Format(@"
UPDATE Product
SET Product.ParentProductID = {0}.ParentProductID
FROM Product, {0}
WHERE Product.ProductID = {0}.ProductID and product.ProductID IS NOT NULL", _vendorAssortImportTableName));

      context.ExecuteStoreCommand(updateProductRelations);

      Log.DebugFormat("Finish merge products for vendor {0}", VendorID);
      #endregion

      #region productGroupVendor
      Log.DebugFormat("Start merge productgroupvendors for vendor {0}", VendorID);

      for (var i = 1; i <= 10; i++)
      {
        string newProductGroupVendorCode = String.Format(@"
UPDATE  [PGV]
SET     [PGV].[VendorName] = [ASS].[VendorProductGroupCodeName{1}]
FROM    [dbo].[ProductGroupVendor]  AS [PGV]
JOIN    {0}                         AS [ASS] ON [PGV].[VendorID] = [ASS].[DefaultVendorID] AND [PGV].[VendorProductGroupCode{1}] = [ASS].[VendorProductGroupCode{1}]
WHERE   [PGV].[VendorProductGroupCode{1}] IS NOT NULL 
  AND   [ASS].[VendorProductGroupCode{1}] IS NOT NULL

INSERT [dbo].[ProductGroupVendor] ([VendorID], [VendorProductGroupCode{1}], [VendorName])
SELECT DISTINCT [DefaultVendorID], [ASS].[VendorProductGroupCode{1}], [ASS].[VendorProductGroupCodeName{1}]
FROM {0} AS [ASS]
LEFT JOIN [dbo].[ProductGroupVendor] AS [PGV] ON [PGV].[VendorID] = [ASS].[DefaultVendorID] AND [PGV].[VendorProductGroupCode{1}] = [ASS].[VendorProductGroupCode{1}]
WHERE [PGV].[VendorProductGroupCode{1}] IS NULL
  AND [ASS].[VendorProductGroupCode{1}] IS NOT NULL"
          , _vendorAssortImportTableName
          , i);

        context.ExecuteStoreCommand(newProductGroupVendorCode);
      }

      if (Configuration.IncludeBrandMapping)
      {
        string newProductGroupVendorBrandCode = String.Format(@"
MERGE ProductGroupVendor AS pgv
USING 
(
  SELECT DISTINCT VendorBrandCode, DefaultVendorID 
  FROM {0}
) AS ass
ON    pgv.VendorID = ass.DefaultVendorID and pgv.BrandCode = ass.VendorBrandCode
  AND pgv.BrandCode IS NOT NULL 
  AND ass.VendorBrandCode IS NOT NULL
WHEN NOT MATCHED BY TARGET THEN
  INSERT (ProductGroupID, VendorID, VendorName, BrandCode)
  VALUES (-1, ass.DefaultVendorID, '', ass.VendorBrandCode);"
          , _vendorAssortImportTableName);

        context.ExecuteStoreCommand(newProductGroupVendorBrandCode);
      }

      Log.DebugFormat("Finish merge productgroupvendor for vendor {0}", VendorID);
      #endregion

      #region VendorAssortment
      Log.DebugFormat("Start merge vendorassortment for vendor {0}", VendorID);


      string deleteDuplicateCustomItemNumbers = (String.Format(@"delete from VendorAssortment where CustomItemNumber in (
select CustomItemNumber
 from VendorAssortment va
 where va.VendorID = {0}
group by CustomItemNumber
having COUNT(*) > 1
) and VendorID = {0}", VendorID));

      //context.ExecuteStoreCommand(deleteDuplicateCustomItemNumbers);

      string addImportField = String.Format("ALTER TABLE {0} ADD ImportID int identity(1,1) PRIMARY KEY;", _vendorAssortImportTableName);
      context.ExecuteStoreCommand(addImportField);

      string removeDuplicateImportProducts = String.Format(@"delete T1
from {0} T1, {0} T2
where T1.ProductID = T2.ProductID and T1.VendorID = T2.VendorID
and T1.ImportID > T2.ImportID", _vendorAssortImportTableName);
      context.ExecuteStoreCommand(removeDuplicateImportProducts);


      string vendorassortmentMerge = (String.Format(@"
MERGE [dbo].[VendorAssortment] AS VA
USING 
(
  SELECT DISTINCT ProductID, CustomItemNumber, VendorID, ShortDescription, LongDescription, LineType, LedgerClass, ExtendedCatalog, ProductDesk
  FROM {0}
) AS Ass
ON va.VendorID = ass.VendorID and va.ProductID = ass.ProductID
WHEN MATCHED THEN
  UPDATE SET 
      va.CustomItemNumber = ass.CustomItemNumber,
      va.ShortDescription = ass.ShortDescription,
      va.LongDescription = ass.LongDescription,
      va.LineType = ass.LineType,
      va.LedgerClass = ass.LedgerClass,
      va.ExtendedCatalog = cast(ass.ExtendedCatalog as bit),
      va.ProductDesk = ass.ProductDesk,
      va.IsActive = 1
WHEN NOT MATCHED BY TARGET THEN
  INSERT ([ProductID],[CustomItemNumber],[VendorID],[ShortDescription],[LongDescription],[LineType],[LedgerClass],[ExtendedCatalog],[ProductDesk],[IsActive])
  VALUES (ass.ProductID,ass.CustomItemNumber,ass.VendorID, ass.ShortDescription,ass.LongDescription,ass.LineType,ass.LedgerClass,cast(ass.ExtendedCatalog as bit), ass.ProductDesk,1)
WHEN NOT MATCHED BY SOURCE AND va.VendorID = {1} AND 1 = {2} THEN
  UPDATE SET va.IsActive = 0;"
        , _vendorAssortImportTableName
        , VendorID
        , Configuration.IsPartialAssortment
          ? 0
          : 1));

      context.ExecuteStoreCommand(vendorassortmentMerge);
      Log.DebugFormat("Finish merge products for vendor {0}", VendorID);
      #endregion

      #region vendorproductgroupassortment
      Log.DebugFormat("Start merge vendorproductgroupassortment for vendor {0}", VendorID);
      //      string deletevpga = String.Format(@"delete vpga from VendorProductGroupAssortment vpga
      //inner join vendorassortment va on vpga.vendorassortmentid = va.vendorassortmentid
      //where va.VendorID = {0}",VendorID);
      //      context.ExecuteStoreCommand(deletevpga);

      for (int i = 1; i <= 10; i++)
      {
        Log.DebugFormat("Start merge VendorProductGroupAssortment code {1} for vendor {0}", VendorID, i);

        string vendorproductgroupassortmentMerge = (String.Format(@"Merge VendorProductGroupAssortment vpga
using(
select va.vendorassortmentid, pgv.productgroupVendorID
from {1} ass
inner join vendorassortment va on ass.ProductID = va.ProductID and va.VendorID = ass.VendorID
inner join productgroupvendor pgv on 
pgv.vendorproductgroupcode{0} = ass.vendorproductgroupcode{0}
and pgv.VendorID = ass.defaultVendorID
where ass.VendorProductGroupCode{0} is not null
) ass
on vpga.vendorassortmentid = ass.vendorassortmentid and vpga.productgroupVendorID = ass.productgroupVendorID
WHEN NOT Matched by target
THEN
INSERT (vendorassortmentid,productgroupVendorID)
values (ass.vendorassortmentid, ass.productgroupVendorID)
WHEN NOT Matched by source 
  and vpga.vendorassortmentid in (select vendorassortmentid from vendorassortment where VendorID = {2})
	and vpga.productgroupVendorID in (select productgroupVendorID from productgroupvendor where VendorID = {3} and vendorproductgroupcode{0} is not null)
  and 1 = {4}
THEN delete;", i, _vendorAssortImportTableName, VendorID, DefaultVendorID, Configuration.IsPartialAssortment ? 0 : 1));
        context.ExecuteStoreCommand(vendorproductgroupassortmentMerge);

        Log.DebugFormat("Finish merge VendorProductGroupAssortment code {1} for vendor {0}", VendorID, i);
      }

      Log.DebugFormat("Start merge VendorProductGroupAssortment brandcode for vendor {0}", VendorID);

      string vendorproductgroupassortmentbrandMerge = (String.Format(@"Merge VendorProductGroupAssortment vpga
using(
select va.vendorassortmentid, pgv.productgroupVendorID
from {0} ass
inner join vendorassortment va on ass.ProductID = va.ProductID and va.VendorID = ass.VendorID
inner join productgroupvendor pgv on 
pgv.BrandCode = ass.VendorBrandCode
and pgv.VendorID = ass.defaultVendorID
where ass.VendorBrandCode is not null
) ass
on vpga.vendorassortmentid = ass.vendorassortmentid and vpga.productgroupVendorID = ass.productgroupVendorID
WHEN NOT Matched by target
THEN
INSERT (vendorassortmentid,productgroupVendorID)
values (ass.vendorassortmentid, ass.productgroupVendorID)
WHEN NOT Matched by source 
  and vpga.vendorassortmentid in (select vendorassortmentid from vendorassortment where VendorID = {1})
	and vpga.productgroupVendorID in (select productgroupVendorID from productgroupvendor where VendorID = {1} and BrandCode is not null)
  and 1 = {2}
THEN delete;", _vendorAssortImportTableName, VendorID, Configuration.IsPartialAssortment ? 0 : 1));

      context.ExecuteStoreCommand(vendorproductgroupassortmentbrandMerge);

      Log.DebugFormat("Finish merge VendorProductGroupAssortment brandcode for vendor {0}", VendorID);

      Log.DebugFormat("Finish merge vendorproductgroupassortment for vendor {0}", VendorID);
      #endregion

      #region ProductBarcode

      if (Configuration.ProcessProductBarcode)
      {
        Log.DebugFormat("Start merge product barcode for vendor {0}", VendorID);

        var productBarcodeMerge = String.Format(@"
MERGE [dbo].[ProductBarcode] AS PB
USING 
(
  SELECT DISTINCT [ProductID], [DefaultVendorID], [Barcode]
  FROM {0}
  WHERE [Barcode] IS NOT NULL
) AS ASS
ON  ass.[ProductID] = pb.[ProductID] and ass.[Barcode] = pb.[Barcode]
WHEN MATCHED and pb.[VendorID] IS NULL THEN
  UPDATE SET PB.VendorID = ass.defaultVendorID
WHEN NOT MATCHED BY TARGET THEN
  INSERT ([ProductID], [Barcode], [VendorID], [BarcodeType])
  VALUES (ass.[ProductID],ass.barcode, ass.[DefaultVendorID], 0);"
          , _vendorAssortImportTableName
          , VendorID
          , DefaultVendorID);

        context.ExecuteStoreCommand(productBarcodeMerge);
        Log.DebugFormat("Finish merge product barcode for vendor {0}", VendorID);
      }
      #endregion

      #region RelatedProducts

      Log.DebugFormat("Start merge relatedproducts for vendor {0}", VendorID);

      string relatedProductTypes = String.Format(@"
                                                    insert into RelatedProductType (Type)
                                                    select distinct RelatedProductType from {0}
                                                    where RelatedProductType not in (Select type from RelatedProductType);"
                                                 , _relatedProductImportTableName);

      context.ExecuteStoreCommand(relatedProductTypes);

      var createRelatedProductClusterdIndex = string.Format(@"
CREATE CLUSTERED INDEX [ClusteredIndex-20131024-125720] ON {0}
(
	[RelatedProductType] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]"
                                                            , _relatedProductImportTableName);

      context.ExecuteStoreCommand(createRelatedProductClusterdIndex);

      string alterRelatedProductsTempTable = string.Format(@"
                                                                  alter table {0} add 

	                                                                ProductID int null,
	                                                                relatedProductID int null, 
	                                                                TypeID int null
                                                            ", _relatedProductImportTableName);

      context.ExecuteStoreCommand(alterRelatedProductsTempTable);

      //update type
      context.ExecuteStoreCommand(string.Format(@" update 
		rpt
		set rpt.TypeID = rp.RelatedProductTypeID
	  from {0} rpt, RelatedProductType rp
	  where rpt.RelatedProductType = rp.Type", _relatedProductImportTableName));

      //update ProductIDs
      context.ExecuteStoreCommand(string.Format(@"update rpt
	set rpt.ProductID = va.ProductID
	from {0} rpt, vendorassortment va
	where va.CustomItemNumber = rpt.CustomItemNumber
", _relatedProductImportTableName));

      //update ProductIDs
      context.ExecuteStoreCommand(string.Format(@"update rpt
	set rpt.relatedProductID = va.ProductID
	from {0} rpt, vendorassortment va
	where va.CustomItemNumber = rpt.RelatedCustomItemNumber
", _relatedProductImportTableName));

      string relatedProducts = (String.Format(@"MERGE 
relatedproduct rp
USING
  (select 
distinct vrd.ProductID,vrd.RelatedProductID,vrd.TypeID as RelatedProductTypeID, vrd.IsConfigured
from {0} vrd ) as vrp
ON vrp.productID = rp.ProductID and vrp.relatedProductID = rp.relatedProductID and vrp.relatedproducttypeid = rp.relatedproducttypeid 
and rp.VendorID = {1}
WHEN Not Matched By Target
  THEN
      Insert (ProductID,relatedProductID,relatedproducttypeid,creationtime,createdby,vendorID, isconfigured)
      values (vrp.ProductID,vrp.relatedProductID,vrp.RelatedProductTypeID,getdate(),1,{1}, cast(vrp.IsConfigured as bit));", _relatedProductImportTableName, VendorID));

      context.ExecuteStoreCommand(relatedProducts);
      Log.DebugFormat("Finish merge relatedproducts for vendor {0}", VendorID);
      #endregion

      #region ProductAttribute
      Log.DebugFormat("Start merge productattributes for vendor {0}", VendorID);
      string setAttributeValueLanguage = (String.Format(@"update {0} set languageID = null where languageID < 0;", _vendorAttributeImportTableName));

      context.ExecuteStoreCommand(setAttributeValueLanguage);

      string addAttributeImportField = String.Format("ALTER TABLE {0} ADD ImportID int identity(1,1) PRIMARY KEY;", _vendorAttributeImportTableName);
      context.ExecuteStoreCommand(addAttributeImportField);

      string removeDuplicateImportProductAttributes = String.Format(@"delete T1
from {0} T1, {0} T2
where T1.CustomItemNumber = T2.CustomItemNumber
and (T1.LanguageID is null and T2.languageID is null or T1.languageID = T2.languageID) and T1.AttributeID = T2.AttributeID
and T1.ImportID > T2.ImportID", _vendorAttributeImportTableName);

      //context.ExecuteStoreCommand(removeDuplicateImportProductAttributes);

      context.ExecuteStoreCommand(string.Format(@"	alter table {0}
				add ProductID int null", _vendorAttributeImportTableName));

      context.ExecuteStoreCommand(string.Format(@"
				update 
					at
				set 
					at.ProductID = va.ProductID
				from vendorassortment va, {0} at
				where va.CustomItemNumber = at.CustomItemNumber", _vendorAttributeImportTableName));

      string ProductAttributeValue = (String.Format(@"
MERGE ProductAttributeValue AS pav
USING
(
	SELECT DISTINCT pa.ProductID, pa.AttributeID, CAST(LanguageID AS INT) AS LanguageID, Value, PAMD.NeedsUpdate
  FROM {0} AS pa
  JOIN ProductAttributeMetaData pamd ON pamd.AttributeID = pa.AttributeID	        
) AS vpav
ON vpav.ProductID = pav.ProductID
	AND vpav.AttributeID = pav.AttributeID
  AND 
  (
    (vpav.languageID IS NULL AND pav.languageID IS NULL) 
    OR vpav.languageID = pav.languageID 
  )
WHEN MATCHED AND pav.Value != vpav.Value AND ISNULL(vpav.NeedsUpdate, 1) = 1 THEN
	UPDATE SET pav.Value = vpav.Value, pav.lastmodificationTime = getdate()
WHEN NOT MATCHED BY TARGET THEN
	INSERT (AttributeID, productID, Value, LanguageID, CreatedBy, CreationTime, LastModifiedBy )
	VALUES (vpav.AttributeID, vpav.ProductID, vpav.value, vpav.languageID, 1, getdate(),1 )
WHEN NOT MATCHED BY SOURCE AND pav.AttributeValueID IN 
  (
		SELECT AttributeValueID
		FROM ProductAttributeMetaData pamd
		JOIN ProductAttributeValue pav1 ON pamd.AttributeID = pav1.AttributeID AND pamd.VendorID = {1}
	) AND 1 = {2} THEN
	DELETE;"
        , _vendorAttributeImportTableName
        , VendorID
        , Configuration.IsPartialAssortment ? 0 : 1));

      context.ExecuteStoreCommand(ProductAttributeValue);
      Log.DebugFormat("Finish merge productattributes for vendor {0}", VendorID);
      #endregion

      #region VendorPrice
      Log.DebugFormat("Start merge vendorprice for vendor {0}", VendorID);
      string addCommercialStatus = String.Format(@"Merge vendorproductstatus vp
Using (select distinct defaultvendorID,isnull(commercialstatus,'') as commercialstatus from {0}) vvp
on vp.vendorID = vvp.defaultvendorID and vp.vendorstatus = vvp.commercialstatus
WHEN Not Matched By Target
  THEN
      Insert (vendorID,vendorstatus,concentratorstatusID)
      values (vvp.defaultvendorID,vvp.commercialstatus,-1);", _vendorPriceImportTableName);
      context.ExecuteStoreCommand(addCommercialStatus);

      string addVendorPriceImportField = String.Format("ALTER TABLE {0} ADD ImportID int identity(1,1) PRIMARY KEY;", _vendorPriceImportTableName);
      context.ExecuteStoreCommand(addVendorPriceImportField);

      string addDescriptionImportField = String.Format("ALTER TABLE {0} ADD ImportID int identity(1,1) PRIMARY KEY;", _vendorDescriptionImportTableName);
      context.ExecuteStoreCommand(addDescriptionImportField);

      string removeDuplicateProductDescriptions = String.Format(@"delete T1
from {0} T1, {0} T2
where T1.CustomItemNumber = T2.CustomItemNumber
and   T1.LanguageID = T2.LanguageID
and   T1.VendorID = T2.VendorID  
and T1.ImportID > T2.ImportID", _vendorDescriptionImportTableName);

      context.ExecuteStoreCommand(removeDuplicateProductDescriptions);



      string removeDuplicateImportVendorPrices = String.Format(@"delete T1
from {0} T1, {0} T2
where T1.CustomItemNumber = T2.CustomItemNumber
and T1.minimumquantity = T2.minimumquantity
and T1.ImportID > T2.ImportID", _vendorPriceImportTableName);
      context.ExecuteStoreCommand(removeDuplicateImportVendorPrices);


      //add ProductID 
      context.ExecuteStoreCommand(string.Format(@"

alter table {0} add ProductID int null


", _vendorPriceImportTableName));

      context.ExecuteStoreCommand(string.Format(@"update vrt
	set vrt.ProductID = va.ProductID 
	from {0} vrt, vendorassortment va
	where va.CustomItemNumber = vrt.CustomItemNumber", _vendorPriceImportTableName));


      string vendorPriceMerge = (String.Format(@"MERGE 
vendorprice vp
USING
  (select va.vendorassortmentID,cast(vvp.price as decimal(18,4)) as price,cast(vvp.costprice as decimal(18,4)) as costprice, cast(vvp.specialprice as decimal(18,4)) as SpecialPrice,cast(vvp.taxrate as decimal(18,4)) as taxrate,
  vvp.minimumquantity,vvp.CommercialStatus, isnull(pvs.concentratorstatusid,-1) as ConcentratorStatusID
from {0} vvp
inner join VendorAssortment va on vvp.VendorID = va.VendorID and vvp.ProductID = va.ProductID
left join VendorProductStatus pvs on pvs.vendorstatus = vvp.commercialStatus and pvs.VendorID = vvp.DefaultVendorID and pvs.concentratorstatusID > 0) vvp
ON vp.vendorassortmentID = vvp.vendorassortmentID and vp.minimumquantity = vvp.minimumquantity

WHEN Matched 
THEN UPDATE SET 
	vp.price = vvp.price,
	vp.costprice = vvp.costprice,
  vp.SpecialPrice = vvp.SpecialPrice,
	vp.concentratorstatusID = vvp.concentratorStatusID,
	vp.taxrate = vvp.taxrate,
	vp.commercialstatus = vvp.commercialstatus
WHEN Not Matched By Target
  THEN
      Insert (VendorAssortmentID,Price,CostPrice, SpecialPrice, Taxrate,minimumquantity,commercialstatus,concentratorstatusID)
      values (vvp.vendorassortmentID,vvp.price,vvp.costprice, vvp.SpecialPrice, vvp.taxrate,vvp.minimumquantity,vvp.commercialstatus,vvp.concentratorstatusid);", _vendorPriceImportTableName));

      context.ExecuteStoreCommand(vendorPriceMerge);
      Log.DebugFormat("Finish merge vendorprice for vendor {0}", VendorID);
      #endregion

      #region VendorStock
      Log.DebugFormat("Start merge vendorstock for vendor {0}", VendorID);

      string addStockStatus = String.Format(@"Merge vendorproductstatus vps
Using (select distinct defaultvendorID,stockstatus from {0}) vvps
on vps.vendorID = vvps.defaultvendorID and vps.vendorstatus = vvps.stockstatus
WHEN Not Matched By Target
  THEN
      Insert (vendorID,vendorstatus,concentratorstatusID)
      values (vvps.defaultvendorID,vvps.stockstatus,-1);", _vendorStockImportTableName);
      context.ExecuteStoreCommand(addStockStatus);


      string vendorStockTypes = (String.Format(@"insert into VendorStockTypes (StockType)
select stocktype from {0}
where stocktype not in (Select StockType from VendorStockTypes);", _vendorStockImportTableName));
      context.ExecuteStoreCommand(relatedProductTypes);

      string addVendorStrockImportField = String.Format("ALTER TABLE {0} ADD ImportID int identity(1,1) PRIMARY KEY;", _vendorStockImportTableName);
      context.ExecuteStoreCommand(addVendorStrockImportField);

      string removeDuplicateImportVendorStocks = String.Format(@"delete T1
from {0} T1, {0} T2
where T1.CustomItemNumber = T2.CustomItemNumber
and T1.StockType = T2.StockType
and T1.ImportID > T2.ImportID", _vendorStockImportTableName);
      context.ExecuteStoreCommand(removeDuplicateImportVendorStocks);

      context.ExecuteStoreCommand(string.Format(@"alter table {0} add ProductID int null", _vendorStockImportTableName));

      context.ExecuteStoreCommand(string.Format(@"update vs
	  set vs.ProductID = va.ProductID
	  from {0} vs , vendorassortment va 
	  where vs.CustomItemNumber = va.CustomItemNumber", _vendorStockImportTableName));

      string vendorStockMerge = (String.Format(@"MERGE 
vendorStock vp
USING
  (select vvp.ProductID,vvp.VendorID,vvp.quantityOnHand,pvs.concentratorstatusID,vst.vendorstocktypeID,vvp.stockstatus
from {0} vvp
left join VendorProductStatus pvs on pvs.vendorstatus = vvp.stockstatus and pvs.VendorID = vvp.DefaultVendorID and pvs.concentratorstatusID > 0
left join VendorStockTypes vst on vst.stocktype = vvp.stocktype
) vvp
ON vp.productID = vvp.productID and vp.vendorstocktypeID = vvp.vendorstocktypeID and vp.vendorID = vvp.vendorID
WHEN Matched 
THEN UPDATE SET 
	vp.quantityOnHand = vvp.quantityOnHand,
	vp.ConcentratorStatusID = vvp.concentratorstatusID,
	vp.stockstatus = vvp.stockstatus
WHEN Not Matched By Target
  THEN
      Insert (ProductID,VendorID,QuantityOnHand,VendorStockTypeID,StockStatus,ConcentratorStatusID)
      values (vvp.productID,vvp.VendorID,vvp.quantityOnHand,vvp.vendorstocktypeid,vvp.stockstatus,vvp.concentratorstatusID);", _vendorStockImportTableName));

      context.ExecuteStoreCommand(vendorStockMerge);
      Log.DebugFormat("Finish merge vendorStock for vendor {0}", VendorID);
      #endregion

      #region ProductDescription

      context.ExecuteStoreCommand(string.Format(@"alter table {0} add ProductID int null", _vendorDescriptionImportTableName));

      context.ExecuteStoreCommand(string.Format(@"update pdt
set pdt.ProductID = va.ProductID
from {0} pdt, vendorassortment va
where va.CustomItemNumber = pdt.CustomItemNumber", _vendorDescriptionImportTableName));

      Log.DebugFormat("Start merge productdescription for vendor {0}", VendorID);
      string productDescriptionMerge = (String.Format(@"merge productdescription pd
using (
select 
vpd.ProductID,
vpd.vendorID,
vpd.languageID,
vpd.productname,
vpd.modelname,
vpd.shortcontentdescription,
vpd.longcontentdescription,
vpd.shortsummarydescription,
vpd.longsummarydescription
from {0} vpd

) vc on pd.ProductID = vc.ProductID and pd.VendorID = vc.VendorID and pd.languageID = vc.languageID
WHEN MATCHED
then update set 
	pd.productname = vc.productname,
pd.modelname = vc.modelname,
pd.shortcontentdescription = vc.shortcontentdescription,
pd.longcontentdescription = vc.longcontentdescription,
pd.shortsummarydescription = vc.shortsummarydescription,
pd.longsummarydescription = vc.longsummarydescription,
pd.LastModificationTime = Getdate()
WHEN NOT Matched by target
THEN
INSERT (ProductID,languageID,VendorID,shortcontentdescription,longcontentdescription,shortsummarydescription,longsummarydescription,modelname,productname, createdby, creationtime)
values (vc.ProductID,vc.languageID,vc.VendorID,vc.shortcontentdescription,vc.longcontentdescription,vc.shortsummarydescription, vc.longsummarydescription, vc.modelname, vc.productname, 1, getdate());", _vendorDescriptionImportTableName));

      context.ExecuteStoreCommand(productDescriptionMerge);
      Log.DebugFormat("Finish merge productdescription for vendor {0}", VendorID);
      #endregion

      #region MasterGroupMapping

      Log.DebugFormat("Started merging Master Group Mapping for vendor {0}", VendorID);

      var query = new StringBuilder();
      var vendorQuery = new StringBuilder();

      // the query which will be built up for the disabling of the mastergroupmappingproduct records
      // the query needs to take the full set of product/mgm combinations spanning over all VendorProductGroupCodes
      // the using clause is being built up below - the rest of the query is formatted afterwards
      var disableQueryMGMP = new StringBuilder();

      for (int i = 1; i <= 10; i++)
      {
        Log.DebugFormat("Start mapping Products in Product Group Vendor {0} to Master Group Mapping", i);

        if (i != 1)
          disableQueryMGMP.AppendLine("UNION");

        disableQueryMGMP.AppendLine(string.Format(@"SELECT p.ProductID, mg.MasterGroupMappingID
                                  FROM {0} tempTable
                                  INNER JOIN ProductGroupVendor pgv ON tempTable.VendorProductGroupCode{1} = pgv.VendorProductGroupCode{1}  and pgv.VendorID = {2}
                                  INNER JOIN MasterGroupMappingProductGroupVendor mg ON pgv.ProductGroupVendorID = mg.ProductGroupVendorID
                                  INNER JOIN Product p ON tempTable.VendorItemNumber = p.VendorItemNumber", _vendorAssortImportTableName, i, DefaultVendorID));


        var mergeMasterGroupMappingProductsVendorsQuery = @"
MERGE MasterGroupMappingProductVendor mp
USING (SELECT DISTINCT p.ProductID, mg.MasterGroupMappingID, tempTable.VendorID
        FROM {0} tempTable
        INNER JOIN ProductGroupVendor pgv ON tempTable.VendorProductGroupCode{1} = pgv.VendorProductGroupCode{1} and pgv.VendorID = {3}
        INNER JOIN MasterGroupMappingProductGroupVendor mg ON pgv.ProductGroupVendorID = mg.ProductGroupVendorID
        INNER JOIN Product p ON tempTable.VendorItemNumber = p.VendorItemNumber
        ) AS tempTable
ON
  tempTable.ProductID = mp.ProductID AND tempTable.MasterGroupMappingID = mp.MasterGroupMappingID and tempTable.VendorID = mp.VendorID
WHEN NOT MATCHED BY target
THEN
  INSERT (MasterGroupMappingID, ProductID, VendorID)
  VALUES (tempTable.mastergroupmappingid, tempTable.ProductID, tempTable.VendorID)
;";

        var mergeMasterProductVendors = (string.Format(mergeMasterGroupMappingProductsVendorsQuery, _vendorAssortImportTableName, i, VendorID, DefaultVendorID));
        vendorQuery.AppendLine(mergeMasterProductVendors);
        vendorQuery.AppendLine("");

        var mergeMasterGroupMappingProductsQuery = @"
MERGE MasterGroupMappingProduct mp
USING (SELECT DISTINCT p.ProductID, mg.MasterGroupMappingID
        FROM {0} tempTable
        INNER JOIN ProductGroupVendor pgv ON tempTable.VendorProductGroupCode{1} = pgv.VendorProductGroupCode{1} and pgv.VendorID = {2}
        INNER JOIN MasterGroupMappingProductGroupVendor mg ON pgv.ProductGroupVendorID = mg.ProductGroupVendorID
        INNER JOIN Product p ON tempTable.VendorItemNumber = p.VendorItemNumber
        ) AS tempTable
ON
  tempTable.ProductID = mp.ProductID AND tempTable.MasterGroupMappingID = mp.MasterGroupMappingID
WHEN NOT MATCHED BY target
THEN
  INSERT (MasterGroupMappingID, ProductID, IsProductMapped)
  VALUES (tempTable.mastergroupmappingid, tempTable.ProductID, 1)

when matched then update
	set mp.isproductmapped = 1
;
";
        string mergeMasterProducts = (String.Format(mergeMasterGroupMappingProductsQuery, _vendorAssortImportTableName, i, DefaultVendorID));

        Log.DebugFormat("Finished mapping Products in Product Group Vendor {0} to Master Group Mapping", i);

        query.AppendLine(mergeMasterProducts);
        query.AppendLine("");
      }


      #region Unused MasterGroupMappingProduct

      try
      {
        context.ExecuteStoreCommand(string.Format("if exists (select * from sys.tables where name = 'Temp_Combi{0}') begin drop table Temp_Combi{0} end", VendorID));
        context.ExecuteStoreCommand(string.Format("if exists (select * from sys.tables where name = 'Temp_Combi_Except{0}') begin drop table Temp_Combi_Except{0} end", VendorID));
      }
      catch
      {
      }

      context.ExecuteStoreCommand(string.Format(@"
            select distinct ProductID, mastergroupmappingid into Temp_Combi{0}
            from (
              {1}
            ) a
          ", VendorID, disableQueryMGMP));

      context.ExecuteStoreCommand(string.Format(@"
          select mp.ProductID, mp.mastergroupmappingid into Temp_Combi_Except{0}
          from Temp_Combi{0} t
	        right join Mastergroupmappingproduct mp on mp.ProductID = t.ProductID and mp.mastergroupmappingid = t.mastergroupmappingid
	        where t.ProductID is null
        ", VendorID));

      context.ExecuteStoreCommand(string.Format(@"
        MERGE MasterGroupMappingProductVendor mp
	      using
	      (
		      select e.ProductID, e.mastergroupmappingid, va.VendorID from Temp_Combi_Except{0} e
		      inner join {1} va on e.ProductID = va.ProductID
	      ) as src
	      on src.ProductID = mp.ProductID and src.mastergroupmappingid = mp.mastergroupmappingid and mp.Vendorid = src.VendorID
	      when matched then delete;
      ", VendorID, _vendorAssortImportTableName));

      #endregion

      context.ExecuteStoreCommand(query.ToString());
      context.ExecuteStoreCommand(vendorQuery.ToString());

      try
      {
        context.ExecuteStoreCommand(string.Format("drop table Temp_Combi{0}", VendorID));
        context.ExecuteStoreCommand(string.Format("drop table Temp_Combi_Except{0}", VendorID));
      }
      catch
      {
      }

      Log.DebugFormat("Start mapping Products in Brand Groups to Master Group Mapping");

      Log.DebugFormat("Finished mapping Products in Brand Groups to Master Group Mapping");
      #endregion
    }

    protected override void TearDown(ConcentratorDataContext context)
    {
      foreach (var tableName in new[] 
      { 
        _vendorAssortImportTableName,
        _vendorPriceImportTableName,
        _vendorStockImportTableName,
        _vendorBrandImportTableName,
        _relatedProductImportTableName,
        _vendorAttributeImportTableName,
        _vendorDescriptionImportTableName,
        _productImageImportTableName
      })
      {
        try
        {
          context.ExecuteStoreCommand(String.Format("IF OBJECT_ID('{0}') IS NOT NULL DROP TABLE {0}", tableName));
        }
        catch
        {
        }
      }
    }

    private class ProductMappingFileReader : XmlDataReader
    {
      private const string Tag = "ProductMapping";
      private const int _fieldCount = 3;

      public override bool IsElement(XmlReader reader)
      {
        return reader.Name == Tag;
      }

      public override int FieldCount
      {
        get
        {
          return _fieldCount;
        }
      }

      protected override void FillRow(System.Xml.Linq.XElement element, object[] row)
      {
        row[0] = (int)element.Attribute("supplier_id");
        row[1] = element.Attribute("m_prod_id").Value;
        row[2] = element.Attribute("prod_id").Value;
      }
    }

    public class VendorAssortmentItem : ValidationAttribute
    {
      [Required]
      public List<VendorImportBrand> BrandVendors
      {
        get;
        set;
      }

      [Required]
      public VendorProduct VendorProduct
      {
        get;
        set;
      }

      public VendorBarcode VendorBarcode
      {
        get;
        set;
      }

      [Required]
      public List<ProductGroupVendor> ProductGroupVendors
      {
        get;
        set;
      }

      public List<VendorImportRelatedProduct> RelatedProducts
      {
        get;
        set;
      }

      public List<VendorImportAttributeValue> VendorImportAttributeValues
      {
        get;
        set;
      }

      [Required]
      public List<VendorImportPrice> VendorImportPrices
      {
        get;
        set;
      }

      [Required]
      public List<VendorImportStock> VendorImportStocks
      {
        get;
        set;
      }

      public List<VendorImportImage> VendorImportImages
      {
        get;
        set;
      }

      public List<VendorProductDescription> VendorProductDescriptions
      {
        get;
        set;
      }

      public VendorAssortmentItem()
      {
        BrandVendors = new List<VendorImportBrand>();
        ProductGroupVendors = new List<ProductGroupVendor>();
        RelatedProducts = new List<VendorImportRelatedProduct>();
        VendorImportAttributeValues = new List<VendorImportAttributeValue>();
        VendorImportPrices = new List<VendorImportPrice>();
        VendorImportStocks = new List<VendorImportStock>();
        VendorProductDescriptions = new List<VendorProductDescription>();
      }
    }

    public class VendorAttributeLabel
    {
      [Required()]
      public int LanguageID
      {
        get;
        set;
      }

      [Required()]
      public string Label
      {
        get;
        set;
      }
    }

    public class VendorBarcode
    {
      [Required()]
      [System.ComponentModel.Description("VendorID is the import vendor identifier")]
      public int VendorID
      {
        get;
        set;
      }

      [Required()]
      [System.ComponentModel.Description("DefaultVendorID is the parentVendorID if the vendor has a parent, else fill in the VendorID")]
      public int DefaultVendorID
      {
        get;
        set;
      }

      [Required()]
      [System.Description("Unique productnumber Vendor")]
      public string CustomItemNumber
      {
        get;
        set;
      }

      [Required()]
      [System.ComponentModel.Description("Product Barcode Vendor")]
      public string Barcode
      {
        get;
        set;
      }

      [Required()]
      [System.ComponentModel.Description("Product Barcode Type")]
      public Int32 BarcodeType
      {
        get;
        set;
      }
    }

    public class VendorProduct
    {
      [Required()]
      [System.ComponentModel.Description("VendorID is the import vendor identifier")]
      public int VendorID
      {
        get;
        set;
      }

      [Required()]
      [System.ComponentModel.Description("DefaultVendorID is the parentVendorID if the vendor has a parent, else fill in the VendorID")]
      public int DefaultVendorID
      {
        get;
        set;
      }

      [Required()]
      [System.Description("Unique productnumber Vendor")]
      public string CustomItemNumber
      {
        get;
        set;
      }

      [Required()]
      [System.ComponentModel.Description("ManufacturerID / VendorPartnumer, the general product identifier")]
      public string VendorItemNumber
      {
        get;
        set;
      }

      [Required()]
      [System.ComponentModel.Description("Brandcode Vendor")]
      public string VendorBrandCode
      {
        get;
        set;
      }

      [Required()]
      [System.ComponentModel.Description("Short product description or Productname Vendor")]
      public string ShortDescription
      {
        get;
        set;
      }

      [DefaultValue(null)]
      [System.ComponentModel.Description("Long product description Vendor, Nullable")]
      public String LongDescription
      {
        get;
        set;
      }

      [DefaultValue(null)]
      [System.ComponentModel.Description("Product LineType Vendor, S = Stock / DL = Download, Nullable")]
      public String LineType
      {
        get;
        set;
      }

      [DefaultValue(null)]
      [System.ComponentModel.Description("LedgerClass Vendor, Nullable")]
      public String LedgerClass
      {
        get;
        set;
      }

      [DefaultValue(null)]
      [System.ComponentModel.Description("Extended Catalog Vendor, Nullable")]
      public String ExtendedCatalog
      {
        get;
        set;
      }

      [DefaultValue(null)]
      [System.ComponentModel.Description("ProductDesk Vendor, Nullable")]
      public String ProductDesk
      {
        get;
        set;
      }

      [Required()]
      [System.ComponentModel.Description("Product Barcode Vendor")]
      public string Barcode
      {
        get;
        set;
      }

      [DefaultValue(0)]
      [System.ComponentModel.Description("Is a product configurable")]
      public int IsConfigurable
      {
        get;
        set;
      }

      [Required()]
      [System.ComponentModel.Description("Productgroup identifier 1 Vendor")]
      public string VendorProductGroupCode1
      {
        get;
        set;
      }

      [Required()]
      [DefaultValue(null)]
      [System.ComponentModel.Description("ProductgroupName identifier 1 Vendor")]
      public string VendorProductGroupCodeName1
      {
        get;
        set;
      }

      [DefaultValue(null)]
      [System.ComponentModel.Description("Productgroup identifier 2 Vendor")]
      public string VendorProductGroupCode2
      {
        get;
        set;
      }

      [DefaultValue(null)]
      [System.ComponentModel.Description("ProductgroupName identifier 2 Vendor")]
      public string VendorProductGroupCodeName2
      {
        get;
        set;
      }

      [DefaultValue(null)]
      [System.ComponentModel.Description("Productgroup identifier 3 Vendor")]
      public string VendorProductGroupCode3
      {
        get;
        set;
      }

      [DefaultValue(null)]
      [System.ComponentModel.Description("ProductgroupName identifier 3 Vendor")]
      public string VendorProductGroupCodeName3
      {
        get;
        set;
      }

      [DefaultValue(null)]
      [System.ComponentModel.Description("Productgroup identifier 4 Vendor")]
      public string VendorProductGroupCode4
      {
        get;
        set;
      }

      [DefaultValue(null)]
      [System.ComponentModel.Description("ProductgroupName identifier 4 Vendor")]
      public string VendorProductGroupCodeName4
      {
        get;
        set;
      }

      [DefaultValue(null)]
      [System.ComponentModel.Description("Productgroup identifier 5 Vendor")]
      public string VendorProductGroupCode5
      {
        get;
        set;
      }

      [DefaultValue(null)]
      [System.ComponentModel.Description("ProductgroupName identifier 5 Vendor")]
      public string VendorProductGroupCodeName5
      {
        get;
        set;
      }

      [DefaultValue(null)]
      [System.ComponentModel.Description("Productgroup identifier 6 Vendor")]
      public string VendorProductGroupCode6
      {
        get;
        set;
      }

      [DefaultValue(null)]
      [System.ComponentModel.Description("ProductgroupName identifier 6 Vendor")]
      public string VendorProductGroupCodeName6
      {
        get;
        set;
      }

      [DefaultValue(null)]
      [System.ComponentModel.Description("Productgroup identifier 7 Vendor")]
      public string VendorProductGroupCode7
      {
        get;
        set;
      }

      [DefaultValue(null)]
      [System.ComponentModel.Description("ProductgroupName identifier 7 Vendor")]
      public string VendorProductGroupCodeName7
      {
        get;
        set;
      }

      [DefaultValue(null)]
      [System.ComponentModel.Description("Productgroup identifier 8 Vendor")]
      public string VendorProductGroupCode8
      {
        get;
        set;
      }

      [DefaultValue(null)]
      [System.ComponentModel.Description("ProductgroupName identifier 8 Vendor")]
      public string VendorProductGroupCodeName8
      {
        get;
        set;
      }

      [DefaultValue(null)]
      [System.ComponentModel.Description("Productgroup identifier 9 Vendor")]
      public string VendorProductGroupCode9
      {
        get;
        set;
      }

      [DefaultValue(null)]
      [System.ComponentModel.Description("ProductgroupName identifier 9 Vendor")]
      public string VendorProductGroupCodeName9
      {
        get;
        set;
      }

      [DefaultValue(null)]
      [System.ComponentModel.Description("Productgroup identifier 10 Vendor")]
      public string VendorProductGroupCode10
      {
        get;
        set;
      }

      [DefaultValue(null)]
      [System.ComponentModel.Description("ProductgroupName identifier 10 Vendor")]
      public string VendorProductGroupCodeName10
      {
        get;
        set;
      }

      [DefaultValue(null)]
      [System.ComponentModel.Description("Parent product custom item number, Nullable")]
      public String ParentProductCustomItemNumber
      {
        get;
        set;
      }
    }

    public class VendorImportRelatedProduct
    {
      [Required()]
      [System.ComponentModel.Description("VendorID is the import vendor identifier")]
      public int VendorID
      {
        get;
        set;
      }

      [Required()]
      [System.ComponentModel.Description("DefaultVendorID is the parentVendorID if the vendor has a parent, else fill in the VendorID")]
      public int DefaultVendorID
      {
        get;
        set;
      }

      [Required()]
      [System.ComponentModel.Description("Unique productnumber Vendor")]
      public string CustomItemNumber
      {
        get;
        set;
      }

      [Required()]
      [System.ComponentModel.Description("Related product Type Vendor")]
      public string RelatedProductType
      {
        get;
        set;
      }

      [Required()]
      [System.ComponentModel.Description("Unique related productnumber Vendor")]
      public string RelatedCustomItemNumber
      {
        get;
        set;
      }

      [DefaultValue(false)]
      [System.ComponentModel.Description("Is the relationship configured")]
      public int IsConfigured
      {
        get;
        set;
      }
    }

    public class VendorImportAttributeValue
    {
      [Required()]
      [System.ComponentModel.Description("VendorID is the import vendor identifier")]
      public int VendorID
      {
        get;
        set;
      }

      [Required()]
      [System.ComponentModel.Description("DefaultVendorID is the parentVendorID if the vendor has a parent, else fill in the VendorID")]
      public int DefaultVendorID
      {
        get;
        set;
      }

      [Required()]
      [System.ComponentModel.Description("Unique productnumber Vendor")]
      public string CustomItemNumber
      {
        get;
        set;
      }

      [DefaultValue(null)]
      [System.ComponentModel.Description("If you have a list of custom defined attributes, fill in the AttributeID")]
      public Int32 AttributeID
      {
        get;
        set;
      }

      [Required()]
      [System.ComponentModel.Description("Attribute Value vendor")]
      public String Value
      {
        get;
        set;
      }

      [DefaultValue(null)]
      [System.ComponentModel.Description("Attribute Value Language vendor")]
      public String LanguageID
      {
        get;
        set;
      }

      [DefaultValue(null)]
      [System.ComponentModel.Description("Unique attribute Identifier vendor")]
      public String AttributeCode
      {
        get;
        set;
      }
    }

    public class VendorImportPrice
    {
      [Required()]
      [System.ComponentModel.Description("VendorID is the import vendor identifier")]
      public int VendorID
      {
        get;
        set;
      }

      [Required()]
      [System.ComponentModel.Description("DefaultVendorID is the parentVendorID if the vendor has a parent, else fill in the VendorID")]
      public int DefaultVendorID
      {
        get;
        set;
      }

      [Required()]
      [System.ComponentModel.Description("Unique productnumber Vendor")]
      public String CustomItemNumber
      {
        get;
        set;
      }

      [DefaultValue("0")]
      [System.ComponentModel.Description("Sales price Vendor")]
      public String Price
      {
        get;
        set;
      }

      [DefaultValue("0")]
      [System.ComponentModel.Description("Cost price Vendor")]
      public String CostPrice
      {
        get;
        set;
      }

      [DefaultValue("0")]
      [System.ComponentModel.Description("Cost price Vendor")]
      public String SpecialPrice
      {
        get;
        set;
      }

      [DefaultValue("19")]
      [System.ComponentModel.Description("Taxrate Vendor")]
      public String TaxRate
      {
        get;
        set;
      }

      [DefaultValue(0)]
      [System.ComponentModel.Description("Minimum quantity or staffel quantity Vendor")]
      public Int32 MinimumQuantity
      {
        get;
        set;
      }

      [Required()]
      [System.ComponentModel.Description("Commercial product status Vendor or same as stock status")]
      public String CommercialStatus
      {
        get;
        set;
      }
    }

    public class VendorImportBrand
    {
      [Required()]
      [System.ComponentModel.Description("VendorID is the import vendor identifier")]
      public int VendorID
      {
        get;
        set;
      }

      [Required()]
      [System.ComponentModel.Description("Brandcode Vendor")]
      public String VendorBrandCode
      {
        get;
        set;
      }

      [DefaultValue(null)]
      [System.ComponentModel.Description("Parent Brandcode Vendor")]
      public String ParentBrandCode
      {
        get;
        set;
      }

      [DefaultValue(null)]
      [System.ComponentModel.Description("Brandname Vendor")]
      public String Name
      {
        get;
        set;
      }

      [DefaultValue(null)]
      [System.ComponentModel.Description("Brand logo Vendor")]
      public String Logo
      {
        get;
        set;
      }
    }

    public class VendorImportStock
    {
      [Required()]
      [System.ComponentModel.Description("VendorID is the import vendor identifier")]
      public int VendorID
      {
        get;
        set;
      }

      [Required()]
      [System.ComponentModel.Description("DefaultVendorID is the parentVendorID if the vendor has a parent, else fill in the VendorID")]
      public int DefaultVendorID
      {
        get;
        set;
      }

      [Required()]
      [System.ComponentModel.Description("Unique productnumber Vendor")]
      public string CustomItemNumber
      {
        get;
        set;
      }

      [DefaultValue(0)]
      [System.ComponentModel.Description("Availible stock Vendor")]
      public int QuantityOnHand
      {
        get;
        set;
      }

      [DefaultValue("Assortment")]
      [System.ComponentModel.Description("Stock type Vendor")]
      public String StockType
      {
        get;
        set;
      }

      [Required()]
      [System.ComponentModel.Description("Stock product status Vendor or same as commercial status")]
      public String StockStatus
      {
        get;
        set;
      }
    }

    public class VendorImportImage
    {
      [Required()]
      [System.ComponentModel.Description("VendorID is the import vendor identifier")]
      public int VendorID
      {
        get;
        set;
      }

      [Required()]
      [System.ComponentModel.Description("DefaultVendorID is the parentVendorID if the vendor has a parent, else fill in the VendorID")]
      public int DefaultVendorID
      {
        get;
        set;
      }

      [Required()]
      [System.ComponentModel.Description("Unique productnumber Vendor")]
      public string CustomItemNumber
      {
        get;
        set;
      }

      [System.ComponentModel.Description("Sequence indicates the image level(prio), in case of multiple images")]
      public Int32 Sequence
      {
        get;
        set;
      }

      [Required()]
      [System.ComponentModel.Description("ImageUrl contains the url to the image")]
      public String ImageUrl
      {
        get;
        set;
      }

      [DefaultValue(null)]
      [System.ComponentModel.Description("ImagePath contains the local path to the image")]
      public String ImagePath
      {
        get;
        set;
      }

    }

    public class VendorProductDescription
    {
      [Required()]
      [System.ComponentModel.Description("VendorID is the import vendor identifier")]
      public int VendorID
      {
        get;
        set;
      }

      [Required()]
      [System.ComponentModel.Description("DefaultVendorID is the parentVendorID if the vendor has a parent, else fill in the VendorID")]
      public int DefaultVendorID
      {
        get;
        set;
      }

      [Required()]
      [System.ComponentModel.Description("Unique productnumber Vendor")]
      public string CustomItemNumber
      {
        get;
        set;
      }

      [DefaultValue(null)]
      [System.ComponentModel.Description("Language")]
      public int LanguageID
      {
        get;
        set;
      }

      [DefaultValue(null)]
      [System.ComponentModel.Description("Product Name")]
      public String ProductName
      {
        get;
        set;
      }

      [DefaultValue(null)]
      [System.ComponentModel.Description("Short Content description")]
      public String ShortContentDescription
      {
        get;
        set;
      }

      [DefaultValue(null)]
      [System.ComponentModel.Description("Long Content description")]
      public String LongContentDescription
      {
        get;
        set;
      }

      [DefaultValue(null)]
      [System.ComponentModel.Description("ModelName")]
      public String ModelName
      {
        get;
        set;
      }

      [DefaultValue(null)]
      [System.ComponentModel.Description("Short summary description")]
      public String ShortSummaryDescription
      {
        get;
        set;
      }

      [DefaultValue(null)]
      [System.ComponentModel.Description("Long summary description")]
      public String LongSummaryDescription
      {
        get;
        set;
      }
    }
  }

}
