using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Globalization;
using System.Linq;
using Concentrator.Objects.ConcentratorService;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects.Models.Attributes;
using Concentrator.Objects.Models.Localization;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.Vendors.Bulk;
using Concentrator.Objects;
using Concentrator.Plugins.PFA.Repos;
using Concentrator.Plugins.PFA.Helpers;
using System.Xml.Linq;
using Concentrator.Plugins.PFA.Models;
using Concentrator.Plugins.PFA.Models.CC;
using Concentrator.Plugins.PFA.Configuration;
using Concentrator.Plugins.PFA.Objects.Helper;

namespace Concentrator.Plugins.PFA
{
  public class PFAImport : BaseCCATImport
  {
    private readonly Monitoring.Monitoring _monitoring = new Monitoring.Monitoring();

    public override string Name
    {
      get { return "PFA import CC"; }
    }

    private List<int> _vendorIDs = new List<int> { 1, 13, 14, 15 };

    private List<int> _connectorIDs = new List<int> { 5, 7, 8 };

    private List<int> _languageIDs = new List<int> { 1, 2, 3 };

    private Dictionary<string, string> _genderTranslations = new Dictionary<string, string>() 
    {
        {"1",   "Men"},
        {"81",  "Men"},
        {"2",   "Women"},
        {"82",  "Women"},
        {"3",   "Men"},
        {"83",  "Men"},
        {"4",   "Women"},
        {"84",  "Women"},
        {"5",   "No gender"}
    };

    private const string COOLCAT_BRAND_CODE = "CC";
    private const string COOLCAT_BRAND_NAME = "Coolcat";
    private const string STOCK_ENABLED_STATUS = "ENA";
    private const string STOCK_DISABLED_STATUS = "NoStock";
    private const string SHOP_WEEK_ATTRIBUTE_CODE = "ShopWeek";
    public const int CONCENTRATOR_VENDOR_ID = 48;
    private const string LEVEL_NOT_EXPORTABLE_STATUS = "Level not exportable";

    protected void SetupAttributes(IUnitOfWork unit, List<AttributeVendorMetaData> attributeMapping, out List<ProductAttributeMetaData> attributes)
    {
      int generalAttributegroupID = GetGeneralAttributegroupID(unit);
      var languages = unit.Scope.Repository<Language>().GetAll().ToList();
      var attributeRepo = unit.Scope.Repository<ProductAttributeMetaData>();
      var attributeNameRepo = unit.Scope.Repository<ProductAttributeName>();

      List<ProductAttributeMetaData> res = new List<ProductAttributeMetaData>();

      foreach (var attr in attributeMapping)
      {
        var concAttr = attributeRepo.GetSingle(c => c.AttributeCode.ToLower() == attr.Code.ToLower() && c.VendorID == attr.VendorID);

        if (concAttr == null)
        {
          concAttr = new ProductAttributeMetaData()
          {
            AttributeCode = attr.Code,
            IsVisible = true,
            VendorID = attr.VendorID,
            ProductAttributeGroupID = generalAttributegroupID,
            Index = 0,
            IsConfigurable = attr.Configurable,
            NeedsUpdate = true,
            IsSearchable = attr.Searchable,
            Sign = String.Empty
          };

          attributeRepo.Add(concAttr);
        }

        res.Add(concAttr);
        unit.Save();

        foreach (var lang in languages)
        {
          var productAttributeName = attributeNameRepo.GetSingle(c => c.LanguageID == lang.LanguageID && c.AttributeID == concAttr.AttributeID);

          if (productAttributeName == null)
          {
            productAttributeName = new ProductAttributeName()
            {
              LanguageID = lang.LanguageID,
              ProductAttributeMetaData = concAttr,
              Name = attr.Code
            };

            attributeNameRepo.Add(productAttributeName);
          }

        }
      }
      unit.Save();

      attributes = res;
    }

    private int GetGeneralAttributegroupID(IUnitOfWork unit)
    {
      int generalAttributegroupID;

      var attributeGroupNameRepo = unit.Scope.Repository<ProductAttributeGroupName>();

      generalAttributegroupID = (from pa in attributeGroupNameRepo.GetAll()
                                 where pa.LanguageID == (int)LanguageTypes.Netherlands &&
                                 pa.Name == GeneralProductGroupName
                                 && !pa.ProductAttributeGroupMetaData.ConnectorID.HasValue
                                 && pa.ProductAttributeGroupMetaData.VendorID == 2
                                 select pa.ProductAttributeGroupID).FirstOrDefault();

      if (generalAttributegroupID == default(int))
      {
        var attributeGroup = new ProductAttributeGroupMetaData
        {
          Index = 0,
          VendorID = 2
        };
        unit.Scope.Repository<ProductAttributeGroupMetaData>().Add(attributeGroup);

        var groupNameEng = new ProductAttributeGroupName
        {
          LanguageID = (int)LanguageTypes.Netherlands,
          Name = GeneralProductGroupName,
          ProductAttributeGroupMetaData = attributeGroup
        };
        attributeGroupNameRepo.Add(groupNameEng);

        var groupNameDut = new ProductAttributeGroupName
        {
          LanguageID = (int)LanguageTypes.Netherlands,
          Name = GeneralProductGroupName,
          ProductAttributeGroupMetaData = attributeGroup
        };
        attributeGroupNameRepo.Add(groupNameDut);
        unit.Save();
        generalAttributegroupID = attributeGroup.ProductAttributeGroupID;
      }
      return generalAttributegroupID;
    }

    protected override void Process()
    {
      _monitoring.Notify(Name, 0);

      var vendorOverridesSetting = GetConfiguration().AppSettings.Settings["VendorIDOverrides"].Try(c => c.Value, string.Empty);
      if (!string.IsNullOrEmpty(vendorOverridesSetting))
      {
        _vendorIDs = (from p in vendorOverridesSetting.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                      select Convert.ToInt32(p)).ToList();

      }

      var cultureInfoAmerican = CultureInfo.InvariantCulture;
      List<ProductAttributeMetaData> attributes = new List<ProductAttributeMetaData>();

      string connectionString = string.Empty;
      string stockConnectionString = string.Empty;
      var pluginConfig = GetConfiguration();
      var currentYear = DateTime.Now.Year;

      using (var unit = GetUnitOfWork())
      {
        connectionString = VendorHelper.GetConnectionStringForPFA(1); //todo: fix vendorid

        CoolcatPFARepository repository = new CoolcatPFARepository(connectionString, stockConnectionString, log);
        AssortmentHelper helper = new AssortmentHelper();

        try
        {
          SetupAttributes(unit, CCAttributeHelper.Attributes, out attributes);
        }
        catch (Exception e)
        {
          log.Debug(e.InnerException);
        }

        var shopWeekLookup = repository.GetShopWeekLookup();
        var sizeCodeLookup = repository.GetSizeCodeLookup();
        var validArtCodes = repository.GetValidItemNumbers();
        var colorNameLookup = repository.GetColorLookup();
        Dictionary<string, string> colorCodesDict = new Dictionary<string, string>();

        foreach (var vendorID in _vendorIDs)
        {
          try
          {
            _monitoring.Notify(Name, vendorID);
#if DEBUG
            if (vendorID != 15) continue;
#endif

            Dictionary<string, int> justArrivedCounters = new Dictionary<string, int>();
            var vendor = unit.Scope.Repository<Vendor>().GetSingle(c => c.VendorID == vendorID);
            var shouldNotCheckForSolden = vendor.VendorSettings.GetValueByKey<bool>("NonSoldenVendor", false);
            var vendorDoesNotImportDescription = vendor.VendorSettings.GetValueByKey<bool>("VendorDoesNotImportDescriptions", false);
            var countryCode = VendorHelper.GetCountryCode(vendorID);
            log.Info("Starting import for vendor " + vendor.Name);

            var connectorID = vendor.ContentProducts.Select(x => x.ConnectorID).FirstOrDefault();

            bool isSolden;

            if (shouldNotCheckForSolden)
            {
              isSolden = true;
            }
            else if (connectorID == null || !GetSoldenPeriod(connectorID, out isSolden))
            {
              throw new Exception("Solden period values are corrupt.");
            }

            var parentVendorID = vendor.ParentVendorID.HasValue ? vendor.ParentVendorID.Value : vendor.VendorID;

            var currencyCode = vendor.VendorSettings.FirstOrDefault(c => c.SettingKey == "PFACurrencyCode").Try(c => c.Value, string.Empty);
            currencyCode.ThrowIfNullOrEmpty(new InvalidOperationException("Missing PFA currency code"));


            List<Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorAssortmentItem> items = new List<Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorAssortmentItem>();
            List<Concentrator.Objects.Vendors.Bulk.VendorBarcodeBulk.VendorImportBarcode> vendorBarcodes = new List<VendorBarcodeBulk.VendorImportBarcode>();

            log.Info("Found products in total: " + validArtCodes.Count());

            int counter = 0;

            foreach (var itemNumber in validArtCodes)
            {
              var productInfo = repository.GetGeneralProductInformation(itemNumber, countryCode);
              //productInfo.TaxRate = productInfo.TaxRate.Replace("%", string.Empty);
              var rate = helper.GetCurrentBTWRate(productInfo.TaxRatePercentage, productInfo.TaxRateDates, 21, productInfo.TaxCode);

              counter++;
              if (counter % 100 == 0) log.Info("Found " + counter);

              if (!justArrivedCounters.ContainsKey(productInfo.GroupName1))
                justArrivedCounters.Add(productInfo.GroupName1, 0);

              var priceRules = repository.GetProductPriceRules(itemNumber, currencyCode);

              if (!string.IsNullOrEmpty(productInfo.override_tax_code))
                rate = helper.GetCurrentBTWRate(productInfo.override_tax_rates, productInfo.override_tax_dates, 21, productInfo.override_tax_code);


              DateTime? weekShop = null;

              shopWeekLookup.TryGetValue(itemNumber, out weekShop);

              Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorAssortmentItem itemToAdd = null;
              if (itemNumber != PfaCoolcatConfiguration.Current.ShipmentCostsProduct && itemNumber != PfaCoolcatConfiguration.Current.ReturnCostsProduct
                && itemNumber != PfaCoolcatConfiguration.Current.KialaShipmentCostsProduct && itemNumber != PfaCoolcatConfiguration.Current.KialaReturnCostsProduct)
              {

                itemToAdd = new Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorAssortmentItem
                                                    {
                                                      BrandVendors = new List<VendorAssortmentBulk.VendorImportBrand>(){
                                                      GetVendorBrand(vendorID)
                                                    },
                                                      VendorProduct = GetVendorProduct(itemNumber, productInfo.ShortDescription, productInfo.Material, vendorID, parentVendorID, true, productInfo.GroupCode1, productInfo.GroupName1, productInfo.GroupCode2, productInfo.GroupName2, productInfo.GroupCode3, productInfo.GroupName3),
                                                      VendorProductDescriptions = vendorDoesNotImportDescription ? new List<Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorProductDescription>() : GetProductDescriptions(itemNumber, vendorID, parentVendorID, productInfo, _languageIDs),
                                                      VendorImportAttributeValues = GetAttributesOnConfigurableLevel(productInfo, itemNumber, attributes, CONCENTRATOR_VENDOR_ID, vendorID),
                                                      RelatedProducts = new List<VendorAssortmentBulk.VendorImportRelatedProduct>(),
                                                      VendorImportPrices = new List<Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportPrice>()
                                                    {
                                                      GetVendorPrice(vendorID, parentVendorID, itemNumber, "0", rate.ToString(cultureInfoAmerican), "0", LEVEL_NOT_EXPORTABLE_STATUS)
                                                    },
                                                      VendorImportStocks = new List<Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportStock>()
                                                    };
                items.Add(itemToAdd);

              }
              int? shopWeekNumber = null;
              if (weekShop.HasValue)
              {
                Calendar cal = new CultureInfo("nl-NL").Calendar;
                shopWeekNumber = cal.GetWeekOfYear(weekShop.Value, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Sunday);
              }

              if (itemToAdd != null && shopWeekNumber.HasValue)
              {
                itemToAdd.VendorImportAttributeValues.Add(new Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportAttributeValue()
                {
                  AttributeCode = SHOP_WEEK_ATTRIBUTE_CODE,
                  Value = weekShop.Value.Year.ToString() + shopWeekNumber.ToString().PadLeft(2, '0'),
                  AttributeID = attributes.FirstOrDefault(c => c.AttributeCode == SHOP_WEEK_ATTRIBUTE_CODE).AttributeID,
                  CustomItemNumber = itemNumber,
                  DefaultVendorID = vendorID,
                  LanguageID = null,
                  VendorID = CONCENTRATOR_VENDOR_ID
                });
              }

              var skus = repository.GetValidSkus(itemNumber);
              var skuSpecs = repository.GetSkuSpecifications(itemNumber);

              //process color level
              foreach (var colorLevel in skus.GroupBy(c => c.ColorCode))
              {
                var colorItemNumber = string.Format("{0} {1}", itemNumber, colorLevel.Key).Trim();

                Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorAssortmentItem colorLevelProduct = null;

                if (itemNumber != PfaCoolcatConfiguration.Current.ShipmentCostsProduct && itemNumber != PfaCoolcatConfiguration.Current.ReturnCostsProduct
                  && itemNumber != PfaCoolcatConfiguration.Current.KialaShipmentCostsProduct && itemNumber != PfaCoolcatConfiguration.Current.KialaReturnCostsProduct)
                {
                  colorLevelProduct = new Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorAssortmentItem()
                  {
                    BrandVendors = new List<VendorAssortmentBulk.VendorImportBrand>(){
                                                      GetVendorBrand(vendorID)
                                                    },
                    VendorProduct = GetVendorProduct(colorItemNumber, productInfo.ShortDescription, productInfo.Material, vendorID, parentVendorID, true, productInfo.GroupCode1, productInfo.GroupName1, productInfo.GroupCode2, productInfo.GroupName2, productInfo.GroupCode3, productInfo.GroupName3, parentCustomItemNumber: itemNumber),
                    VendorProductDescriptions = vendorDoesNotImportDescription ? new List<Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorProductDescription>() : GetProductDescriptions(colorItemNumber, vendorID, parentVendorID, productInfo, _languageIDs),
                    VendorImportAttributeValues = GetAttributesOnConfigurableLevel(productInfo, colorItemNumber, attributes, CONCENTRATOR_VENDOR_ID, vendorID),
                    RelatedProducts = new List<VendorAssortmentBulk.VendorImportRelatedProduct>(
                            (from rp in skus.GroupBy(c => c.ColorCode)
                             where rp.Key != colorLevel.Key
                             select new Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportRelatedProduct()
                             {
                               CustomItemNumber = colorItemNumber,
                               DefaultVendorID = vendorID,
                               VendorID = vendorID,
                               IsConfigured = 0,
                               RelatedCustomItemNumber = string.Format("{0} {1}", itemNumber, rp.Key).Trim(),
                               RelatedProductType = "Related Color Level"

                             }).ToList()
                        ),
                    VendorImportPrices = new List<Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportPrice>()
                                                    {
                                                      GetVendorPrice(vendorID, parentVendorID, colorItemNumber, "0", rate.ToString(cultureInfoAmerican), "0")
                                                    },
                    VendorImportStocks = new List<Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportStock>()

                  };

                  if (colorLevelProduct != null && shopWeekNumber.HasValue)
                  {
                    colorLevelProduct.VendorImportAttributeValues.Add(new Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportAttributeValue()
                    {
                      AttributeCode = SHOP_WEEK_ATTRIBUTE_CODE,
                      Value = weekShop.Value.Year.ToString() + shopWeekNumber.ToString().PadLeft(2, '0'),
                      AttributeID = attributes.FirstOrDefault(c => c.AttributeCode == SHOP_WEEK_ATTRIBUTE_CODE).AttributeID,
                      CustomItemNumber = colorItemNumber,
                      DefaultVendorID = vendorID,
                      LanguageID = null,
                      VendorID = CONCENTRATOR_VENDOR_ID
                    });
                  }

                  items.Add(colorLevelProduct);

                  if (itemToAdd != null)
                  {
                    itemToAdd.RelatedProducts.Add(new VendorAssortmentBulk.VendorImportRelatedProduct()
                    {
                      CustomItemNumber = itemNumber,
                      RelatedCustomItemNumber = colorItemNumber,
                      IsConfigured = 0,
                      DefaultVendorID = parentVendorID,
                      VendorID = vendorID,
                      RelatedProductType = "Style"
                    });
                  }
                }

                if (justArrivedCounters[productInfo.GroupName1] < 60)
                {
                  if (weekShop.HasValue)
                  {
                    var daysInRange = (DateTime.Now - weekShop.Value).Days;
                    if (daysInRange >= 0 && daysInRange <= 14)
                    {
                      if (colorLevelProduct != null)
                      {
                        colorLevelProduct.VendorProduct.VendorProductGroupCode4 = "Just arrived";

                        colorLevelProduct.VendorProduct.VendorProductGroupCodeName4 = "Just arrived";
                        justArrivedCounters[productInfo.GroupName1]++;
                      }
                    }
                  }
                }

                foreach (var sku in colorLevel)
                {

                  var skuBarcode = CompleteBarcode(sku.Barcode);

                  int webshopStock = 0;
                  int ceyenneStock = 0;
                  int transferStock = 0;
                  int wmsStock = 0;

                  string sizeCodePfa = sku.SizeCode;

                  foreach (var lookupT in new List<string>() { productInfo.MtbCode1, productInfo.MtbCode2, productInfo.MtbCode3, productInfo.MtbCode4 })
                  {
                    var lookup = sizeCodeLookup.FirstOrDefault(c => c.SizeCode.ToLower() == sku.SizeCode.ToLower() && c.MtbCode.ToLower() == lookupT.ToLower());
                    if (lookup != null)
                    {
                      sizeCodePfa = lookup.PfaCode;
                      break;

                    }
                  }

                  var product = new Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorAssortmentItem();

                  string skuNew = string.Empty;
                  if (itemNumber == PfaCoolcatConfiguration.Current.ShipmentCostsProduct || itemNumber == PfaCoolcatConfiguration.Current.ReturnCostsProduct
                    || itemNumber == PfaCoolcatConfiguration.Current.KialaShipmentCostsProduct || itemNumber == PfaCoolcatConfiguration.Current.KialaReturnCostsProduct)
                    skuNew = itemNumber;
                  else
                    skuNew = string.Format("{0} {1} {2}", itemNumber, sku.ColorCode, sku.SizeCode);

                  Concentrator.Objects.Vendors.Bulk.VendorBarcodeBulk.VendorImportBarcode bc = new VendorBarcodeBulk.VendorImportBarcode()
                                  {
                                    CustomItemNumber = skuNew,
                                    Barcode = sizeCodePfa,
                                    Type = 4,
                                    VendorID = 1
                                  };

                  vendorBarcodes.Add(bc);

                  var price = helper.GetPrice(itemNumber, sku.ColorCode, sku.SizeCode, priceRules);
                  var discount = helper.GetDiscount(itemNumber, sku.ColorCode, sku.SizeCode, priceRules);

                  if (!isSolden)
                  {
                    if (discount.HasValue)
                    {
                      price = discount.Value;
                      discount = null;
                    }
                  }

                  var item = items.Where(c => c.VendorProduct.CustomItemNumber == itemNumber);
                  if (item == null) continue;


                  product = new Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorAssortmentItem()
                  {
                    VendorProduct = GetVendorProduct(skuNew, productInfo.ShortDescription, productInfo.LongDescription, vendorID, parentVendorID, false, productInfo.GroupCode1, productInfo.GroupName1, productInfo.GroupCode2, productInfo.GroupName2, productInfo.GroupCode3, productInfo.GroupName3, parentCustomItemNumber: colorItemNumber, barcode: skuBarcode),
                    VendorProductDescriptions = vendorDoesNotImportDescription ? new List<Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorProductDescription>() : GetProductDescriptions(skuNew, vendorID, parentVendorID, productInfo, _languageIDs),
                    BrandVendors = new List<VendorAssortmentBulk.VendorImportBrand>(){
                  GetVendorBrand(vendorID)
                },
                    VendorImportAttributeValues = (from m in CCAttributeHelper.Attributes
                                                   where m.Configurable
                                                   let aM = attributes.FirstOrDefault(c => c.AttributeCode.ToLower() == m.Code.ToLower())
                                                   select new Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportAttributeValue
                                                   {
                                                     AttributeCode = aM.AttributeCode,
                                                     AttributeID = aM.AttributeID,
                                                     CustomItemNumber = skuNew,
                                                     DefaultVendorID = vendorID,
                                                     LanguageID = null,
                                                     VendorID = CONCENTRATOR_VENDOR_ID,
                                                     Value = (m.Code.ToLower() == "color" ? sku.ColorCode : sku.SizeCode)
                                                   }).ToList(),

                    RelatedProducts = new List<VendorAssortmentBulk.VendorImportRelatedProduct>(),
                    VendorImportPrices = new List<Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportPrice>()
                          {
                            GetVendorPrice(vendorID, parentVendorID, skuNew, price.ToString(cultureInfoAmerican), rate.ToString(cultureInfoAmerican), (discount.HasValue && discount.Value < price)? discount.Value.ToString(cultureInfoAmerican) :null)                           
                          },

                    VendorImportStocks = new List<Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportStock>()
                  };


                  if ((discount.HasValue && discount.Value > 0) && (price != discount))
                  {
                    if (itemToAdd != null)
                    {
                      //That is because code4 might be filled with just arrived
                      if (string.IsNullOrEmpty(colorLevelProduct.VendorProduct.VendorProductGroupCode4))
                      {
                        colorLevelProduct.VendorProduct.VendorProductGroupCode4 = "SALE";
                        colorLevelProduct.VendorProduct.VendorProductGroupCodeName4 = "SALE";
                      }
                      else
                      {
                        colorLevelProduct.VendorProduct.VendorProductGroupCode5 = "SALE";
                        colorLevelProduct.VendorProduct.VendorProductGroupCodeName5 = "SALE";
                      }

                      if (string.IsNullOrEmpty(itemToAdd.VendorProduct.VendorProductGroupCode4))
                      {
                        itemToAdd.VendorProduct.VendorProductGroupCode4 = "SALE";
                        itemToAdd.VendorProduct.VendorProductGroupCodeName4 = "SALE";
                      }
                      else
                      {
                        itemToAdd.VendorProduct.VendorProductGroupCode5 = "SALE";
                        itemToAdd.VendorProduct.VendorProductGroupCodeName5 = "SALE";
                      }

                      if (string.IsNullOrEmpty(product.VendorProduct.VendorProductGroupCode4))
                      {
                        product.VendorProduct.VendorProductGroupCode4 = "SALE";
                        product.VendorProduct.VendorProductGroupCodeName4 = "SALE";
                      }
                      else
                      {
                        product.VendorProduct.VendorProductGroupCode5 = "SALE";
                        product.VendorProduct.VendorProductGroupCodeName5 = "SALE";
                      }
                    }
                  }

                  var attributeColorRows = skuSpecs.Where(c => c.ColorCode == sku.ColorCode).ToList();

                  var mentality = GetAttributeValue(attributeColorRows, "Mentality");
                  var input = GetAttributeValue(attributeColorRows, "Input");
                  var Style = GetAttributeValue(attributeColorRows, "Style");
                  var Usermoment = GetAttributeValue(attributeColorRows, "Usermoment");
                  var Module = GetAttributeValue(attributeColorRows, "Module");

                  AddAttributeValue(itemToAdd, "Mentality", mentality, attributes, itemNumber, CONCENTRATOR_VENDOR_ID, vendorID);
                  AddAttributeValue(itemToAdd, "Style", Style, attributes, itemNumber, CONCENTRATOR_VENDOR_ID, vendorID);
                  AddAttributeValue(itemToAdd, "Usermoment", Usermoment, attributes, itemNumber, CONCENTRATOR_VENDOR_ID, vendorID);
                  AddAttributeValue(product, "Module", Module, attributes, skuNew, CONCENTRATOR_VENDOR_ID, vendorID);
                  AddAttributeValue(itemToAdd, "InputCode", input, attributes, itemNumber, CONCENTRATOR_VENDOR_ID, vendorID);
                  AddAttributeValue(itemToAdd, "Gender", DetermineGenderByVendorItemNumber(itemNumber), attributes, itemNumber, CONCENTRATOR_VENDOR_ID, vendorID);

                  items.Add(product);
                  if (colorLevelProduct != null)
                  {
                    //relate to main product
                    if (colorLevelProduct.RelatedProducts.Where(c => c.CustomItemNumber == colorItemNumber && c.RelatedCustomItemNumber == skuNew).FirstOrDefault() == null)
                    {
                      colorLevelProduct.RelatedProducts.Add(new VendorAssortmentBulk.VendorImportRelatedProduct()
                      {
                        CustomItemNumber = colorItemNumber,
                        DefaultVendorID = parentVendorID,
                        IsConfigured = 1,
                        RelatedCustomItemNumber = skuNew,
                        RelatedProductType = "Configured product",
                        VendorID = vendorID
                      });
                    }
                  }
                }
              }
            }

            var bulkConfig = new VendorAssortmentBulkConfiguration();
            using (var vendorAssortmentBulk = new VendorAssortmentBulk(items, vendorID, parentVendorID, bulkConfig))
            {
              vendorAssortmentBulk.Init(unit.Context);
              vendorAssortmentBulk.Sync(unit.Context);
            }

            using (var barcodeBulk = new VendorBarcodeBulk(vendorBarcodes, vendorID, 4))
            {
              barcodeBulk.Init(unit.Context);
              barcodeBulk.Sync(unit.Context);
            }


          }
          catch (Exception e)
          {
            log.AuditCritical("Import failed for vendor " + vendorID, e);
            _monitoring.Notify(Name, -vendorID);
          }
        }
        AddValueLabels(unit, colorCodesDict);
        ConfigureProductAttributes(_vendorIDs, unit);
      }
      _monitoring.Notify(Name, 1);
    }

    protected void AddValueLabels(IUnitOfWork work, Dictionary<string, string> colorCodeLabels)
    {
      var attributeColors = work.Scope.Repository<ProductAttributeMetaData>().GetSingle(c => c.AttributeCode.ToLower() == "color");

      var values = attributeColors.ProductAttributeValues.Select(c => c.Value).Distinct().ToList();
      var allLanguages = work.Scope.Repository<Language>().GetAll();

      foreach (var value in values)
      {
        var label = colorCodeLabels.FirstOrDefault(c => c.Key == value);
        if (string.IsNullOrEmpty(label.Value)) continue;

        foreach (var lang in allLanguages)
        {

          foreach (var cID in _connectorIDs)
          {

            var translation = work.Scope.Repository<ProductAttributeValueLabel>().GetSingle(c => c.Value == value && c.AttributeID == attributeColors.AttributeID && c.ConnectorID == cID && c.LanguageID == lang.LanguageID);

            if (translation == null)
            {
              translation = new ProductAttributeValueLabel
              {
                AttributeID = attributeColors.AttributeID,
                ConnectorID = cID,
                Label = label.Value,
                Value = value,
                LanguageID = lang.LanguageID
              };

              work.Scope.Repository<ProductAttributeValueLabel>().Add(translation);
            }
          }
        }
      }
      work.Save();
    }

    public string GetAttributeValue(List<SkuSpecification> specifications, string key)
    {
      var Row = (from r in specifications
                 where r.KmsDescription == key
                 select r).FirstOrDefault();

      string value = string.Empty;

      if (Row == null) return value;

      if (Row != null)
      {
        if (key == "input") value = Row.KmkDescription;
        else value = Row.KmkCode;
      }

      return value;
    }

    public void AddAttributeValue(Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorAssortmentItem item, string key, string value, List<ProductAttributeMetaData> items, string customItemNumber, int vendorID, int defaultVendorID)
    {
      if (item == null) return;



      if (!string.IsNullOrEmpty(value))
      {
        //prevent null refs
        if (item.VendorImportAttributeValues == null) item.VendorImportAttributeValues = new List<VendorAssortmentBulk.VendorImportAttributeValue>();

        if (item.VendorImportAttributeValues.Any(c => c.AttributeCode == key)) return; //if already set 

        item.VendorImportAttributeValues.Add(new VendorAssortmentBulk.VendorImportAttributeValue()
        {
          AttributeCode = key,
          AttributeID = items.First(c => c.AttributeCode.ToLower() == key.ToLower()).AttributeID,
          CustomItemNumber = customItemNumber,
          DefaultVendorID = defaultVendorID,
          LanguageID = null,
          Value = value,
          VendorID = vendorID
        });
      }
    }

    private string CompleteBarcode(string s)
    {
      return BarcodeHelper.AddCheckDigitToBarcode(s);
    }

    private Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportBrand GetVendorBrand(int vendorID)
    {
      return new VendorAssortmentBulk.VendorImportBrand()
      {
        VendorID = vendorID,
        VendorBrandCode = COOLCAT_BRAND_CODE,
        ParentBrandCode = null,
        Name = COOLCAT_BRAND_NAME,
      };
    }

    private Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorProduct GetVendorProduct(string itemNumber, string shortDescription, string longDescription, int vendorID, int parentVendorID, bool isConfigurable, string code1, string name1, string code2, string name2, string code3, string name3, string parentCustomItemNumber = "", string barcode = "")
    {
      return new VendorAssortmentBulk.VendorProduct
                                                    {
                                                      VendorItemNumber = itemNumber,
                                                      CustomItemNumber = itemNumber,
                                                      ShortDescription = shortDescription,
                                                      LongDescription = longDescription,
                                                      LineType = null,
                                                      LedgerClass = null,
                                                      ProductDesk = null,
                                                      ExtendedCatalog = null,
                                                      VendorID = vendorID,
                                                      DefaultVendorID = parentVendorID,
                                                      VendorBrandCode = COOLCAT_BRAND_CODE,
                                                      Barcode = barcode,
                                                      IsConfigurable = isConfigurable ? 1 : 0,
                                                      VendorProductGroupCode1 = code1,
                                                      VendorProductGroupCodeName1 = name1,
                                                      VendorProductGroupCode2 = code2,
                                                      VendorProductGroupCodeName2 = name2,
                                                      VendorProductGroupCode3 = code3,
                                                      VendorProductGroupCodeName3 = name3,
                                                      ParentProductCustomItemNumber = string.IsNullOrEmpty(parentCustomItemNumber) ? string.Empty : parentCustomItemNumber

                                                    };
    }

    private List<Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorProductDescription> GetProductDescriptions(string itemNumber, int vendorID, int parentVendorID, ProductInfoResult productInfo, List<int> languages)
    {
      List<Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorProductDescription> result = new List<VendorAssortmentBulk.VendorProductDescription>();
      foreach (var language in languages)
      {
        result.Add(new VendorAssortmentBulk.VendorProductDescription()
        {
          CustomItemNumber = itemNumber,
          DefaultVendorID = parentVendorID,
          LanguageID = language,
          LongContentDescription = productInfo.Material,
          ShortContentDescription = productInfo.ShortDescription,
          ShortSummaryDescription = string.Empty,
          LongSummaryDescription = string.Empty,
          ModelName = string.Empty,
          ProductName = productInfo.ShortDescription,
          VendorID = vendorID
        });
      }
      return result;
    }

    private const string ENABLED_STATUS = "ENA";
    private Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportPrice GetVendorPrice(int vendorID, int parentVendorID, string itemNumber, string price, string taxRate, string specialPrice, string status = "")
    {
      return new VendorAssortmentBulk.VendorImportPrice()
      {
        VendorID = vendorID,
        DefaultVendorID = parentVendorID,
        CustomItemNumber = itemNumber,
        Price = price,
        CostPrice = price,
        TaxRate = taxRate,
        MinimumQuantity = 0,
        CommercialStatus = string.IsNullOrEmpty(status) ? ENABLED_STATUS : status,
        SpecialPrice = specialPrice
      };
    }

    private Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportStock GetVendorStock(int vendorID, int parentVendorID, string itemNumber, int qty, CCStockType stockType, string stockStatus = "")
    {
      return new Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportStock()
      {
        VendorID = vendorID,
        DefaultVendorID = parentVendorID,
        CustomItemNumber = itemNumber,
        QuantityOnHand = Math.Max(qty, 0), //Handle negative quantities
        StockType = stockType.ToString(),
        StockStatus = string.IsNullOrEmpty(stockStatus) ? (qty == 0 ? STOCK_DISABLED_STATUS : STOCK_ENABLED_STATUS) : stockStatus
      };
    }

    private List<Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportAttributeValue> GetAttributesOnConfigurableLevel(ProductInfoResult productInfo, string itemNumber, List<ProductAttributeMetaData> ConcentratorAttributes, int vendorID, int defaultVendorID)
    {
      return new List<VendorAssortmentBulk.VendorImportAttributeValue>( //size, color, targetgroup, productgroup, subproductgroup
                                                       (from a in CCAttributeHelper.Attributes
                                                        where a.UsedOnConfigurableLevel
                                                        let val = a.GetAttributeValue == null ? string.Empty : a.GetAttributeValue(productInfo)
                                                        select new Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportAttributeValue()
                                                        {
                                                          AttributeCode = a.Code,
                                                          Value = val,
                                                          AttributeID = ConcentratorAttributes.FirstOrDefault(c => c.AttributeCode == a.Code).AttributeID,
                                                          CustomItemNumber = itemNumber,
                                                          DefaultVendorID = defaultVendorID,
                                                          LanguageID = null,
                                                          VendorID = vendorID
                                                        }));
    }

    private string DetermineGenderByVendorItemNumber(string vendorItemNumber)
    {
      foreach (var keyValuePair in _genderTranslations)
      {
        if (vendorItemNumber.StartsWith(keyValuePair.Key))
        {
          return keyValuePair.Value;
        }
      }
      return string.Empty;
    }
  }
}