using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Enumerations;
using PetaPoco;
using Concentrator.Objects.Models.Connectors;
using System.Data;
using Concentrator.Objects.Models.Contents;
using Concentrator.Objects.Utility;
using Concentrator.Objects.Logic;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Models.Media;
using Concentrator.Objects.Models.Brands;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.Models.Attributes;
using Concentrator.Objects.Models.Magento;
using System.Configuration;
using Concentrator.Objects.Models.MastergroupMapping;
using Concentrator.Objects.AssortmentService.DBModels;

namespace Concentrator.Objects.AssortmentService
{
  public class Service : IDisposable
  {
    private Connector _connector;
    private readonly int? _productID;
    private Database _db;

    private int connectorID { get { return _connector.ConnectorID; } }

    public Service(int connectorID, int? productID = null)
    {
      _productID = productID;
      _db = new PetaPoco.Database(Environments.Environments.Current.Connection, "System.Data.SqlClient");
      _db.CommandTimeout = 1200;

      _connector = _db.Single<Connector>("SELECT * FROM Connector WHERE ConnectorID = @0", connectorID);
      _connector.ThrowIfNull("Connector cannot be null");
    }

    public List<AssortmentProductInfo> GetAssortment(bool importFullContent, bool shopInformation, string customerID, bool showProductGroups, int languageID, bool showRelatedProducts = true)
    {
      var inactiveGroups = GetInactiveGroupsAndTheirChilds(connectorID);

      string mappingsToExcludeStatement = string.IsNullOrEmpty(inactiveGroups) ? "" : string.Format(" and MasterGroupMappingID not in ({0})", inactiveGroups);

      var productGroupsQuery = string.Format("SELECT * FROM ContentProductGroup WHERE ConnectorID = {0} and IsExported = 1 AND MasterGroupMappingID is NOT NULL {1}", connectorID, mappingsToExcludeStatement);

      var productGroups = _db.Query<ContentProductGroup>(productGroupsQuery).GroupBy(c => c.ProductID).ToDictionary(c => c.Key, c => c.ToList());
      var cStatuses = _db.Query<ConnectorProductStatus>("SELECT * FROM ConnectorProductStatus WHERE ConnectorID = @0", connectorID).ToList();

      ProductStatusConnectorMapper mapper = new ProductStatusConnectorMapper(connectorID, cStatuses);
      ContentLogic logic = new ContentLogic(_db, connectorID);
      logic.FillRetailStock(_db);

      var calculatedPrices = _db.Query<Concentrator.Objects.Models.Prices.CalculatedPriceView>(
               @"EXECUTE GetCalculatedPriceView @0", connectorID);
      logic.FillPriceInformation(calculatedPrices);


      var connectorSystem = _db.Single<ConnectorSystem>("SELECT * FROM ConnectorSystem WHERE ConnectorSystemID = @0", _connector.ConnectorSystemID.Value);

      bool magento = _connector.ConnectorSystemID.HasValue && connectorSystem.Name == "Magento";
      var defaultLanguage = _db.FirstOrDefault<ConnectorLanguage>("SELECT * FROM ConnectorLanguage WHERE ConnectorID = @0", connectorID);

      defaultLanguage.ThrowIfNull("No default Language specified for connector");

      if (languageID == 0)
      {
        //languageID = EntityExtensions.GetValueByKey(con.ConnectorSettings, "LanguageID", 2);
        var language = _db.FirstOrDefault<ConnectorLanguage>("SELECT * FROM ConnectorLanguage WHERE ConnectorID = @0", connectorID);
        language.ThrowIfNull("No Language specified for connector");

        languageID = language.LanguageID;
      }

      var recordsSource = _db.Query<AssortmentContentView>("EXECUTE GetAssortmentContentView @0", connectorID).Where(x => x.BrandID > 0);

      if (_productID.HasValue)
      {
        int id = _productID.Value;

        if (id > 0)
          recordsSource = recordsSource.Where(x => x.ProductID == id);
      }

      var records = recordsSource.ToList();

      var barcodes = _db.Query<ProductBarcodeView>("SELECT * FROM ProductBarcodeView WHERE ConnectorID = @0 and BarcodeType = @1", connectorID, (int)BarcodeTypes.Default).ToList();


      var brands = _db.Query<Brand>("SELECT * FROM Brand").ToList();


      List<ImageView> productImages = new List<ImageView>();
      if (importFullContent)
      {
        productImages = _db.Query<ImageView>("SELECT * FROM ImageView WHERE ConnectorID = @0 AND ImageType = @1", connectorID, "Product").ToList();
      }

      int totalRecords = records.Count;


      List<CrossLedgerclass> crossLedgerClasses = null;
      if (shopInformation)
      {
        crossLedgerClasses = _db.Query<CrossLedgerclass>("SELECT * FROM CrossLedgerClass WHERE ConnectorID = @0", connectorID).ToList();
      }

      var vendorstocktypes = _db.Query<VendorStockType>("SELECT * FROM VendorStockTypes").ToList();

      List<MasterGroupMapping> mappingslist = null;
      Dictionary<int, List<MasterGroupMappingMedia>> mappingMediaList = new Dictionary<int, List<MasterGroupMappingMedia>>();
      List<MasterGroupMapping> parentMappingList = new List<MasterGroupMapping>();
      if (showProductGroups)
      {
        string mappingListQuery = string.Format("SELECT * FROM MasterGroupMapping WHERE ConnectorID = {0} {1}", connectorID, mappingsToExcludeStatement);
        mappingslist = _db.Query<MasterGroupMapping>(mappingListQuery).ToList();

        string mappingMediaQuery = @"SELECT MGMM.* 
                                     FROM MasterGroupMapping MGM 
                                     LEFT JOIN MasterGroupMappingMedia MGMM ON MGM.MasterGroupMappingID = MGMM.MasterGroupMappingID 
                                     WHERE MasterGroupMappingMediaID IS NOT NULL AND MGM.ConnectorID = {0} {1}";

        foreach (var masterGroupMappingMedia in _db.Query<MasterGroupMappingMedia>(string.Format(mappingMediaQuery, connectorID, mappingsToExcludeStatement)))
        {
          if (!mappingMediaList.ContainsKey(masterGroupMappingMedia.MasterGroupMappingID))
            mappingMediaList.Add(masterGroupMappingMedia.MasterGroupMappingID, new List<MasterGroupMappingMedia>());

          mappingMediaList[masterGroupMappingMedia.MasterGroupMappingID].Add(masterGroupMappingMedia);
        }

        if (_connector.ParentConnectorID.HasValue)
        {
          string parentMappingListQuery = string.Format("SELECT * FROM MasterGroupMapping WHERE ConnectorID = {0} {1}", _connector.ParentConnectorID.Value, mappingsToExcludeStatement);
          parentMappingList = _db.Query<MasterGroupMapping>(parentMappingListQuery).ToList();

          foreach (var masterGroupMappingMedia in _db.Query<MasterGroupMappingMedia>(string.Format(mappingMediaQuery, _connector.ParentConnectorID.Value, mappingsToExcludeStatement)))
          {
            if (!mappingMediaList.ContainsKey(masterGroupMappingMedia.MasterGroupMappingID))
              mappingMediaList.Add(masterGroupMappingMedia.MasterGroupMappingID, new List<MasterGroupMappingMedia>());

            mappingMediaList[masterGroupMappingMedia.MasterGroupMappingID].Add(masterGroupMappingMedia);
          }
        }
      }

      var preferredVendor = _db.FirstOrDefault<PreferredConnectorVendor>("SELECT * FROM PreferredConnectorVendor WHERE ConnectorID = @0 AND IsPreferred = 1", connectorID);
      int? vendorid = null;
      if (preferredVendor != null) vendorid = preferredVendor.VendorID;

      var relatedProductTypeList = _db.Query<RelatedProductType>("SELECT * FROM RelatedProductType").ToDictionary(x => x.RelatedProductTypeID, y => y);

      var styleType = relatedProductTypeList.Where(c => c.Value.Type == "Style").FirstOrDefault();

      var relatedProductsList = GetRelatedProducts();

      var configAttributesListSource = _db.Query<ProductAttributeMetaData>(
               @"SELECT ProductID, PAMD.*
											FROM ProductAttributeConfiguration PAC
											INNER JOIN ProductAttributeMetaData PAMD ON (PAC.AttributeID = PAMD.AttributeID)"
               );

      var configAttributesList = (from f in configAttributesListSource
                                  group f by f.ProductID into grouped
                                  select grouped).ToDictionary(x => x.Key, y => y.ToList());

      var productGroupNames = _db.Query<MasterGroupMappingLanguage>("SELECT * FROM MasterGroupMappingLanguage WHERE LanguageID = @0", languageID).Select(c => new { c.MasterGroupMappingID, c.Name }).ToDictionary(c => c.MasterGroupMappingID, c => c.Name);
      var productGroupNamesDefault = _db.Query<MasterGroupMappingLanguage>("SELECT * FROM MasterGroupMappingLanguage WHERE LanguageID = @0", defaultLanguage.LanguageID).Select(c => new { c.MasterGroupMappingID, c.Name }).ToDictionary(c => c.MasterGroupMappingID, c => c.Name);
      var productGroupDescriptions = _db.Query<MasterGroupMappingDescription>("SELECT * FROM MasterGroupMappingDescription WHERE LanguageID = @0", languageID).Select(c => new { c.MasterGroupMappingID, c.Description }).ToDictionary(c => c.MasterGroupMappingID, c => c.Description);

      var productGroupCustomLabelsForThisConnector
        = _db.Query<MasterGroupMappingCustomLabel>("SELECT * FROM MasterGroupMappingCustomLabel WHERE LanguageID = @0 AND ConnectorID = @1", languageID, connectorID).Select(c => new { c.MasterGroupMappingID, c.CustomLabel }).ToDictionary(c => c.MasterGroupMappingID, c => c.CustomLabel);

      var productGroupCustomLabelsForDefaultConnector = _db.Query<MasterGroupMappingCustomLabel>(@"SELECT * FROM MasterGroupMappingCustomLabel WHERE LanguageID = @0 AND ConnectorID is null", languageID).Select(c => new { c.MasterGroupMappingID, c.CustomLabel }).ToDictionary(c => c.MasterGroupMappingID, c => c.CustomLabel);

      var magentoLayouts = _db.Query<MagentoPageLayout>("SELECT * FROM MagentoPageLayout").Select(c => new { c.LayoutID, c.LayoutCode }).ToDictionary(c => c.LayoutID, c => c.LayoutCode);
      var magentoSettingModels = _db.Query<MagentoProductGroupSetting>("SELECT * FROM MagentoProductGroupSetting WHERE MasterGroupMappingID is NOT NULL").ToList();
      var seoTextsModel = new SortedList<int, List<SeoTexts>>(_db.Query<SeoTexts>("SELECT * FROM SeoTexts WHERE MasterGroupMappingID is NOT NULL").GroupBy(c => c.MasterGroupMappingID).ToDictionary(c => c.Key, c => c.ToList()));


      //refactor in a nicer way
      var presaleProducts = _db.Query<int>("select distinct productid from productattributevalue where attributeid in (select attributeid from productattributemetadata where attributecode= 'btf') and value = 'true'").ToList();
      presaleProducts.Sort();

      var supportsCompleteTheLookFunctionality = _db.FirstOrDefault<bool>("Select * from connectorsetting where connectorid = @0 and settingkey = 'CompleteTheLookConnector'", connectorID);

      var masterGroupMappingSettingsQuery = @";WITH OptionValues AS 
                                              (
                                              	SELECT MGMS.MasterGroupMappingSettingID, MGMSV.MasterGroupMappingID, MGMSO.Value
                                              	FROM MasterGroupMappingSetting MGMS
                                              	INNER JOIN MasterGroupMappingSettingValue MGMSV ON MGMS.MasterGroupMappingSettingID = MGMSV.MasterGroupMappingSettingID
                                              	INNER JOIN MasterGroupMappingSettingOption MGMSO ON MGMSV.Value = MGMSO.OptionID
                                              	WHERE MGMS.Type = 'option'
                                              )
                                              SELECT MGMS.Name, MGMS.[Group], MGMS.[Type], MGMSV.MasterGroupMappingID
                                                , CASE WHEN OV.MasterGroupMappingSettingID IS NOT NULL THEN OV.Value ELSE MGMSV.Value END AS Value
                                              FROM MasterGroupMappingSettingValue MGMSV
                                              INNER JOIN MasterGroupMappingSetting MGMS ON MGMS.MasterGroupMappingSettingID = MGMSV.MasterGroupMappingSettingID
                                              INNER JOIN MasterGroupMapping MGM ON MGMSV.MasterGroupMappingID = MGM.MasterGroupMappingID
                                              LEFT JOIN OptionValues OV ON MGMS.MasterGroupMappingSettingID = OV.MasterGroupMappingSettingID AND MGMSV.MasterGroupMappingID = OV.MasterGroupMappingID
                                              WHERE MGM.ConnectorID = {0} {1}";

      var parentConnectorFilter = string.Empty;
      if (_connector.ParentConnectorID.HasValue)
        parentConnectorFilter = string.Format("OR MGM.ConnectorID = {0}", _connector.ParentConnectorID);

      var masterGroupMappingSettings = _db.Query<MasterGroupMappingSettingResult>(string.Format(masterGroupMappingSettingsQuery, connectorID, parentConnectorFilter));
      var masterGroupMappingSettingsDictionary = new Dictionary<int, List<MasterGroupMappingSettingResult>>();

      foreach (var masterGroupMappingSetting in masterGroupMappingSettings)
      {
        if (!masterGroupMappingSettingsDictionary.ContainsKey(masterGroupMappingSetting.MasterGroupMappingID))
          masterGroupMappingSettingsDictionary.Add(masterGroupMappingSetting.MasterGroupMappingID, new List<MasterGroupMappingSettingResult>());

        masterGroupMappingSettingsDictionary[masterGroupMappingSetting.MasterGroupMappingID].Add(masterGroupMappingSetting);
      }

      SynchronizedCollection<AssortmentProductInfo> assortmentList = new SynchronizedCollection<AssortmentProductInfo>();
      ParallelOptions options = new ParallelOptions() { MaxDegreeOfParallelism = 8 };

#if DEBUG
      options = new ParallelOptions() { MaxDegreeOfParallelism = 8 };
#endif

      Parallel.ForEach(Partitioner.Create(0, totalRecords), options, (range, loopState) =>
      {

        List<AssortmentProductInfo> list = new List<AssortmentProductInfo>();
        for (int idx = range.Item1; idx < range.Item2; idx++)
        {
          var a = records[idx];

          if (a.IsConfigurable)
          {
            var lis = relatedProductsList[a.ProductID].Where(c => c.IsConfigured).Select(c => c.RelatedProductID).ToList();

            if (!records.Any(rp => lis.Contains(rp.ProductID))) continue;
          }

          var productBarcodes = barcodes.Where(x => x.ProductID == a.ProductID).Select(c => c.Barcode).ToList();
          var images = productImages.Where(x => x.ProductID == a.ProductID).ToList();

          var configAttributes = configAttributesList.ContainsKey(a.ProductID) ? configAttributesList[a.ProductID] : new List<ProductAttributeMetaData>();
          var relatedProducts = relatedProductsList.ContainsKey(a.ProductID) ? relatedProductsList[a.ProductID] : new List<RelatedProduct>();
          if (supportsCompleteTheLookFunctionality)
          {
            if (a.IsConfigurable && a.ParentProductID != null)
            {
              //this is color level
              //get from the relatedProductsList the ParentProduct related products.
              var parentProductRelatedProducts = relatedProductsList.ContainsKey(a.ParentProductID.Value) ? relatedProductsList[a.ParentProductID.Value] : new List<RelatedProduct>();
              parentProductRelatedProducts = parentProductRelatedProducts.Where(c => c.RelatedProductTypeID == 9).ToList();

              //foreach related product:
              //if related product is artikel (IsConfigurable && ParentProductID == null) then find all colors of that artikel (get products where parentproductid is productid of artikel item)
              //  the found color items are the related products.
              //else
              //  if related product is color level than this is the related product
              foreach (var prp in parentProductRelatedProducts)
              {
                if (prp.PP_IsConfigurable && prp.PP_ParentProductID == null)
                {
                  //Is Artikel product
                  var artikelRelatedProducts = relatedProductsList.ContainsKey(prp.ProductID)
                                                 ? relatedProductsList[prp.ProductID]
                                                 : new List<RelatedProduct>();

                  //TODO: fix the types
                  foreach (var rp in artikelRelatedProducts.Where(c => c.RelatedProductTypeID == 9 || c.RelatedProductTypeID == 10).ToList())
                  {
                    var colorItem = relatedProductsList[rp.RelatedProductID].Where(t => t.RelatedProductTypeID == styleType.Key);
                    var rangeRelatedProducts = (from ci in colorItem
                                                where !relatedProducts.Any(c => c.RelatedProductID == ci.RelatedProductID)
                                                select new RelatedProduct()
                                                  {
                                                    ProductID = a.ProductID,
                                                    RelatedProductID = ci.RelatedProductID,
                                                    RelatedProductTypeID = 9,
                                                    Index = rp.Index,
                                                    IsConfigured = false

                                                  }).ToList();
                    relatedProducts.AddRange(rangeRelatedProducts);

                  }

                  //relatedProducts.AddRange(artikelRelatedProducts.Distinct());
                }
                else
                {
                  relatedProducts.Add(prp);
                }
              }

            }
          }

          var prices = logic.CalculatePrice(a.ProductID);
          var stockRetail = logic.RetailStock(a.ProductID, _connector);
          var productProductGroups = productGroups.ContainsKey(a.ProductID) ? productGroups[a.ProductID] : new List<ContentProductGroup>();

          list.Add(new AssortmentProductInfo
          {
            Visible = a.Visible,
            Content = new AssortmentContent()
            {
              ProductName = !string.IsNullOrEmpty(a.ProductName.ToString()) ? a.ProductName.ToString() : a.ShortDescription,
              ShortDescription = a.ShortDescription ?? string.Empty,
              LongDescription = a.LongDescription ?? string.Empty
            },
            ProductID = a.ProductID,
            ManufacturerID = a.VendorItemNumber.Trim(),
            IsNonAssortmentItem = a.IsNonAssortmentItem ?? false,
            RelatedProducts = (from rp in relatedProducts.Distinct().OrderBy(c => c.RelatedProductID)
                               select new AssortmentRelatedProduct()
                               {
                                 IsConfigured = rp.IsConfigured,
                                 RelatedProductID = rp.RelatedProductID,
                                 TypeID = rp.RelatedProductTypeID,
                                 Type = relatedProductTypeList[rp.RelatedProductTypeID].Type,
                                 Index = rp.Index,
                                 MapsToMagentoTypeID = relatedProductTypeList[rp.RelatedProductTypeID].TypeMapsToMagentoTypeID
                               }).ToList(),

            IsConfigurable = a.IsConfigurable,
            CustomProductID = a.CustomItemNumber,
            PresaleProduct = presaleProducts.Contains(a.ProductID),
            Stock = new AssortmentStock()
            {
              QuantityToReceive = a.QuantityToReceive ?? 0,
              InStock = a.QuantityOnHand,
              StockStatus = a.ConnectorStatus,
              PromisedDeliveryDate = a.PromisedDeliveryDate
            },
            RetailStock = (from rs in stockRetail.OrderBy(c => c.VendorStockTypeID)
                           select new AssortmentRetailStock()
                           {
                             CostPrice = rs.UnitCost ?? 0,
                             VendorCode = rs.BackendVendorCode,
                             InStock = rs.QuantityOnHand,
                             StockStatus = mapper.GetForConnector(rs.ConcentratorStatusID),
                             Name = rs.VendorStockTypeID == 1 ? rs.vendorName : vendorstocktypes.Where(x => x.VendorStockTypeID == rs.VendorStockTypeID).Select(x => x.StockType).FirstOrDefault(),
                             PromisedDeliveryDate = rs.PromisedDeliveryDate,
                             QuantityToReceive = rs.QuantityToReceive ?? 0
                           }).ToList(),
            Prices = (from p in prices.OrderBy(c => c.VendorAssortmentID)
                      select new AssortmentPrice()
                      {
                        CommercialStatus = p.CommercialStatus,
                        CostPrice = p.CostPrice ?? 0,
                        SpecialPrice = p.SpecialPrice,
                        MinimumQuantity = p.MinimumQuantity ?? 0,
                        TaxRate = p.TaxRate ?? 19,
                        UnitPrice = p.Price
                      }).ToList(),
            ConfigurableAttributes = (from ca in configAttributes
                                      select new AssortmentConfigurableAttribute()
                                      {
                                        AttributeCode = ca.AttributeCode,
                                        AttributeID = ca.AttributeID,
                                        ConfigurablePosition = ca.ConfigurablePosition ?? 0
                                      }).ToList(),
            Barcodes = productBarcodes,
            Images = (from img in images.OrderBy(c => c.mediaid)
                      select new AssortmentProductImage()
                      {
                        Sequence = img.Sequence ?? 0,
                        Path = img.ImagePath
                      }).ToList(),
            Brand = GetAssortmentBrandHierarchy(a.BrandID, a.VendorBrandCode, brands),

            ProductGroups =
              (from p in productProductGroups.OrderBy(c => c.ContentProductGroupID)
               select GetAssortmentProductGroupHierarchy(
               p.MasterGroupMappingID.Value


               , languageID
               , defaultLanguage.LanguageID


               , mappingslist
               , mappingMediaList
               , masterGroupMappingSettingsDictionary
               , productGroupNames
               , productGroupNamesDefault
               , productGroupDescriptions
               , productGroupCustomLabelsForThisConnector
                 //, productGroupModels
               , magentoLayouts
               , magentoSettingModels
               , parentMappingList
               , productGroupCustomLabelsForDefaultConnector
               , seoTextsModel))
               .ToList().OrderBy(x => x.Index).ToList()
            ,
            LineType = a.LineType
            ,
            ShopInfo = new AssortmentShopInformation()
              {
                LedgerClass = a.LedgerClass,
                ProductDesk = a.ProductDesk,
                ExtendedCatalog = a.ExtendedCatalog ?? false,
                OriginalLedgerClass = a.LedgerClass
              },
            DeliveryHours = a.DeliveryHours,
            CutOffTime = a.CutOffTime
          });
        }

        foreach (var item in list)
        {
          assortmentList.Add(item);
        }

      });
      return assortmentList.Distinct().ToList();
    }

    private Dictionary<int, List<RelatedProduct>> GetRelatedProducts()
    {
      var preferredvendors = _db.Fetch<int>("SELECT vendorid FROM PreferredConnectorVendor WHERE ConnectorID = @0 ", connectorID);

      var sql = String.Format(@"SELECT DISTINCT [RP].*,[P].[IsConfigurable] as PP_IsConfigurable,[P].[ParentProductID] as PP_ParentProductID 
                                FROM [RelatedProduct] RP 
                                INNER JOIN Product P ON RP.RelatedProductID = P.ProductID 
                                WHERE RP.IsActive = 1 and (RP.VendorID IS NULL OR RP.VendorID in ({0}))", string.Join(" ,", preferredvendors));

      return (from rp in _db.Query<RelatedProduct>(sql)
              group rp by rp.ProductID into grouped
              select grouped).ToDictionary(x => x.Key, y => y.ToList());
    }

    public List<AssortmentStockPriceProduct> GetStockAndPrices()
    {
      var cStatuses = _db.Query<ConnectorProductStatus>("SELECT * FROM ConnectorProductStatus WHERE ConnectorID = @0", connectorID).ToList();
      ProductStatusConnectorMapper mapper = new ProductStatusConnectorMapper(connectorID, cStatuses);
      ContentLogic logic = new ContentLogic(_db, connectorID);
      logic.FillRetailStock(_db);

      var relatedProductTypeList = _db.Query<RelatedProductType>("SELECT * FROM RelatedProductType").ToDictionary(x => x.RelatedProductTypeID, y => y);

      var calculatedPrices = _db.Query<Concentrator.Objects.Models.Prices.CalculatedPriceView>(
         @"EXECUTE GetCalculatedPriceView @0", connectorID);
      logic.FillPriceInformation(calculatedPrices);

      var recordsSource = _db.Query<AssortmentContentView>("EXECUTE GetAssortmentContentView @0", connectorID).Where(x => x.BrandID > 0);

      if (_productID.HasValue)
      {
        int id = _productID.Value;

        if (id > 0)
          recordsSource = recordsSource.Where(x => x.ProductID == id);
      }

      var records = recordsSource.ToList();
      int totalRecords = records.Count;

      var vendorstocktypes = _db.Query<VendorStockType>("SELECT * FROM VendorStockTypes").ToList();
      var relatedProductsList = GetRelatedProducts();

      SynchronizedCollection<AssortmentStockPriceProduct> assortmentList = new SynchronizedCollection<AssortmentStockPriceProduct>();
      ParallelOptions options = new ParallelOptions() { MaxDegreeOfParallelism = 8 };

      Parallel.ForEach(Partitioner.Create(0, totalRecords), options, (range, loopState) =>
      {
        List<AssortmentStockPriceProduct> list = new List<AssortmentStockPriceProduct>();
        for (int idx = range.Item1; idx < range.Item2; idx++)
        {
          var a = records[idx];
          var stockRetail = logic.RetailStock(a.ProductID, _connector);
          var prices = logic.CalculatePrice(a.ProductID);
          var relatedProducts = relatedProductsList.ContainsKey(a.ProductID) ? relatedProductsList[a.ProductID] : new List<RelatedProduct>();


          list.Add(new AssortmentStockPriceProduct
          {
            RelatedProducts = (from rp in relatedProducts.Distinct()
                               where rp.IsConfigured
                               select new AssortmentRelatedProduct()
                               {
                                 IsConfigured = rp.IsConfigured,
                                 RelatedProductID = rp.RelatedProductID,
                                 TypeID = rp.RelatedProductTypeID,
                                 Type = relatedProductTypeList[rp.RelatedProductTypeID].Type,
                                 Index = rp.Index,
                                 MapsToMagentoTypeID = relatedProductTypeList[rp.RelatedProductTypeID].TypeMapsToMagentoTypeID
                               }).ToList(),
            Visible = a.Visible,
            ProductID = a.ProductID,
            ManufacturerID = a.VendorItemNumber.Trim(),
            IsNonAssortmentItem = a.IsNonAssortmentItem ?? false,
            IsConfigurable = a.IsConfigurable,
            CustomProductID = a.CustomItemNumber,

            Stock = new AssortmentStock()
            {
              QuantityToReceive = a.QuantityToReceive ?? 0,
              InStock = a.QuantityOnHand,
              StockStatus = a.ConnectorStatus,
              PromisedDeliveryDate = a.PromisedDeliveryDate
            },

            RetailStock = (from rs in stockRetail
                           select new AssortmentRetailStock()
                           {
                             CostPrice = rs.UnitCost ?? 0,
                             VendorCode = rs.BackendVendorCode,
                             InStock = rs.QuantityOnHand,
                             StockStatus = mapper.GetForConnector(rs.ConcentratorStatusID),
                             Name = rs.VendorStockTypeID == 1 ? rs.vendorName : vendorstocktypes.Where(x => x.VendorStockTypeID == rs.VendorStockTypeID).Select(x => x.StockType).FirstOrDefault(),
                             PromisedDeliveryDate = rs.PromisedDeliveryDate,
                             QuantityToReceive = rs.QuantityToReceive ?? 0
                           }).ToList(),

            Prices = (from p in prices
                      select new AssortmentPrice()
                      {
                        CommercialStatus = p.CommercialStatus,
                        CostPrice = p.CostPrice ?? 0,
                        SpecialPrice = p.SpecialPrice,
                        MinimumQuantity = p.MinimumQuantity ?? 0,
                        TaxRate = p.TaxRate ?? 21,
                        UnitPrice = p.Price
                      }).ToList()
          });
        }

        foreach (var item in list)
        {
          assortmentList.Add(item);
        }

      }); return assortmentList.Distinct().ToList();
    }

    private string GetMasterGroupMappingImage(MasterGroupMapping mapping, Dictionary<int, List<MasterGroupMappingMedia>> mappingMedia, MasterGroupMappingMediaType masterGroupMappingMediaType, int languageID)
    {
      var imageUrl = ConfigurationManager.AppSettings["ConcentratorImageUrl"];
      var groupImageUrl = ConfigurationManager.AppSettings["FTPMasterGroupMappingMediaPath"];

      if (mappingMedia.ContainsKey(mapping.MasterGroupMappingID))
      {
        var masterGroupMappingMedia = mappingMedia[mapping.MasterGroupMappingID];
        var connectorLanguageMedia = masterGroupMappingMedia.FirstOrDefault(media => media.ConnectorID == connectorID && media.LanguageID == languageID && media.ImageTypeID == (int)masterGroupMappingMediaType);

        if (connectorLanguageMedia == null)
          connectorLanguageMedia = masterGroupMappingMedia.FirstOrDefault(media => media.ImageTypeID == (int)masterGroupMappingMediaType);

        var image = connectorLanguageMedia == null ? string.Empty : connectorLanguageMedia.ImagePath;

        if (!string.IsNullOrEmpty(imageUrl) && !string.IsNullOrEmpty(image))
        {
          imageUrl = new Uri(new Uri(imageUrl), groupImageUrl + "/" + image).ToString();
        }
        else
          imageUrl = image;
      }
      else
      {
        return string.Empty;
      }
      return imageUrl;
    }

    private AssortmentProductGroup GetAssortmentProductGroupHierarchy(int masterGroupMappingID, int languageID, int defaultLanguageID
      , List<MasterGroupMapping> mappings, Dictionary<int, List<MasterGroupMappingMedia>> mappingMedia, Dictionary<int, List<MasterGroupMappingSettingResult>> mappingSettings
      , Dictionary<int, string> productGroupNames, Dictionary<int, string> productGroupNamesDefault, Dictionary<int, string> mappingDescriptions
      , Dictionary<int, string> mappingCustomLabels, Dictionary<int, string> magentoLayouts, List<MagentoProductGroupSetting> magentoSettings
      , List<MasterGroupMapping> parentList, Dictionary<int, string> mappingCustomLabelsForDefaultConnector, SortedList<int, List<SeoTexts>> seoTexts)
    {
      var mapping = mappings.FirstOrDefault(x => x.MasterGroupMappingID == masterGroupMappingID);

      if (mapping == null) mapping = parentList.FirstOrDefault(x => x.MasterGroupMappingID == masterGroupMappingID); // check for parentt mapping

      string name = string.Empty;
      productGroupNames.TryGetValue(mapping.MasterGroupMappingID, out name);

      name = name.IfNullOrEmpty("Unknown");

      string setName = string.Empty;
      productGroupNamesDefault.TryGetValue(mapping.MasterGroupMappingID, out setName);
      setName = setName.IfNullOrEmpty("Unknown");

      string customLabel = null;
      if (mappingCustomLabelsForDefaultConnector.ContainsKey(mapping.MasterGroupMappingID))
        customLabel = mappingCustomLabelsForDefaultConnector[mapping.MasterGroupMappingID];

      if (mappingCustomLabels.ContainsKey(mapping.MasterGroupMappingID))
        customLabel = mappingCustomLabels[mapping.MasterGroupMappingID];

      string description = null;
      mappingDescriptions.TryGetValue(mapping.MasterGroupMappingID, out description);

      //var productGroup = productGroups.FirstOrDefault(c => c.MasterGroupMappingID == mapping.MasterGroupMappingID);
      var assortmentGroup = new AssortmentProductGroup()
      {
        AttributeSet = setName,
        CustomName = "",//TODO figure out what to put here
        Name = string.IsNullOrEmpty(customLabel) ? name : customLabel,
        ID = mapping.ProductGroupID,
        Description = description,
        Index = mapping.Score ?? 0,
        MappingID = mapping.ExportID ?? mapping.MasterGroupMappingID,
        Image = GetMasterGroupMappingImage(mapping, mappingMedia, MasterGroupMappingMediaType.Image, languageID),
        ThumbnailImage = GetMasterGroupMappingImage(mapping, mappingMedia, MasterGroupMappingMediaType.Thumbnail, languageID),
        MenuIconImage = GetMasterGroupMappingImage(mapping, mappingMedia, MasterGroupMappingMediaType.MenuIcon, languageID)
      };

      var magentoSettingModel = magentoSettings.FirstOrDefault(c => c.MasterGroupMappingID == mapping.MasterGroupMappingID);

      if (magentoSettingModel != null)
      {
        string codeLayout = string.Empty;
        if (mapping.MagentoPageLayoutID.HasValue)
          magentoLayouts.TryGetValue(mapping.MagentoPageLayoutID.Value, out codeLayout);

        assortmentGroup.MagentoSetting = new AssortmentMagentoSetting()
        {
          HideInMenu = magentoSettingModel.ShowInMenu,
          DisableMenu = magentoSettingModel.DisabledMenu,
          IsAnchor = magentoSettingModel.IsAnchor,
          PageLayoutCode = codeLayout
        };
      }

      assortmentGroup.ProductGroupSettings = new List<ProductGroupSetting>();
      if (mappingSettings.ContainsKey(masterGroupMappingID))
      {
        foreach (var setting in mappingSettings[masterGroupMappingID])
        {
          assortmentGroup.ProductGroupSettings.Add(new ProductGroupSetting
          {
            Name = setting.Name,
            Value = setting.Value
          });
        }
      }

      if (mapping.ParentMasterGroupMappingID.HasValue)
      {
        assortmentGroup.ParentProductGroup = GetAssortmentProductGroupHierarchy(
          mapping.ParentMasterGroupMappingID.Value
          , languageID
          , defaultLanguageID
          , mappings
          , mappingMedia
          , mappingSettings
          , productGroupNames
          , productGroupNamesDefault
          , mappingDescriptions
          , mappingCustomLabels
          , magentoLayouts
          , magentoSettings
          , parentList
          , mappingCustomLabelsForDefaultConnector
          , seoTexts
          );
      }

      if (seoTexts.ContainsKey(masterGroupMappingID))
      {
        var searchEngineOptimalizationTexts = seoTexts[masterGroupMappingID].Where(c => c.LanguageID == languageID && c.ConnectorID == connectorID);

        if (searchEngineOptimalizationTexts.Count() > 0)
        {
          List<AssortmentSeoTexts> assortmentSeoTexts = new List<AssortmentSeoTexts>();

          foreach (var seo in searchEngineOptimalizationTexts)
          {
            AssortmentSeoTexts seoText = new AssortmentSeoTexts()
            {
              Description = seo.Description,
              DescriptionType = seo.DescriptionType
            };
            assortmentSeoTexts.Add(seoText);
          }
          assortmentGroup.AssortmentSeoTexts = assortmentSeoTexts;
        }
      }
      return assortmentGroup;
    }

    private AssortmentBrand GetAssortmentBrandHierarchy(int brandID, string vendorBrandCode, List<Brand> brands)
    {

      var brand = brands.FirstOrDefault(c => c.BrandID == brandID);
      var b = new AssortmentBrand()
      {
        BrandID = brand.BrandID,
        Code = vendorBrandCode,
        Name = brand.Name
      };

      if (brand.ParentBrandID.HasValue)
      {
        b.ParentBrand = GetAssortmentBrandHierarchy(brand.ParentBrandID.Value, string.Empty, brands);
      }

      return b;
    }

    public List<AssortmentProductMedia> GetImages()
    {
      var assortment = _db.Query<ImageView>("SELECT * FROM ImageView WHERE ConnectorID = @0", connectorID); //unit.Scope.Repository<ImageView>().GetAll(x => x.ConnectorID == connectorID).ToList();

      var imageUrl = new Uri(ConfigurationManager.AppSettings["ConcentratorImageUrl"].ToString());
      var thumbUrl = new Uri(ConfigurationManager.AppSettings["ConcentratorThumbImageUrl"].ToString());

      return new List<AssortmentProductMedia>();
    }

    private string GetInactiveGroupsAndTheirChilds(int connectorID)
    {
      List<int> inactiveProductGroupMappingsToReturn = new List<int>();

      List<int> inactiveGroupsForConnector = _db.Query<int>("select MasterGroupMappingID from ProductGroupMappingConnectorNotActive where ConnectorID = @0", connectorID).ToList();

      foreach (var inactiveGroup in inactiveGroupsForConnector)
      {
        AddInactiveChildMapping(inactiveGroup, inactiveProductGroupMappingsToReturn);
      }

      string idsToReturn = string.Join(",", inactiveProductGroupMappingsToReturn);

      return idsToReturn;
    }

    private void AddInactiveChildMapping(int masterGroupMappingID, List<int> MappingIdsNotToInclude)
    {
      MappingIdsNotToInclude.Add(masterGroupMappingID);

      var childMappings = _db.Query<int>(@"select MasterGroupMappingID from MasterGroupMapping where ParentMasterGroupMappingID = @0", masterGroupMappingID).ToList();

      foreach (var childMappingOfChild in childMappings)
      {
        AddInactiveChildMapping(childMappingOfChild, MappingIdsNotToInclude);
      }
    }


    public void Dispose()
    {
      _db.Dispose();
    }
  }
}
