using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using Concentrator.Objects;
using Concentrator.Objects.ConcentratorService;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects.Models.Attributes;
using Concentrator.Objects.Models.Localization;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.Vendors.Bulk;
using Concentrator.Plugins.PFA.Repos;
using Concentrator.Plugins.PFA.Helpers;
using Concentrator.Plugins.PFA.Models;
using Concentrator.Plugins.PFA.Configuration;
using Concentrator.Objects.Models.Configuration;
using Concentrator.Objects.Models.Contents;
using System.Configuration;
using Concentrator.Plugins.PFA.Objects.Helper;

namespace Concentrator.Plugins.PFA
{
  public class ATPFAImport : BaseCCATImport
  {
    private string vendorBrandCode = "AT";
    private string vendorBrandName = "America Today";

    private readonly Monitoring.Monitoring _monitoring = new Monitoring.Monitoring();

    private List<int> _vendorIDs; //= new List<int> { 2, 23, 24 };

    private List<int> _connectorIDs = new List<int> { 6, 9, 10 };

    public override string Name
    {
      get { return "PFA import AT"; }
    }

    #region "Attribute mapping"
    private List<AttributeVendorMetaData> AttributeMapping = new List<AttributeVendorMetaData>()
    {
      new AttributeVendorMetaData(){
      Code = "Season",
      VendorID = 48, 
      Configurable = false
      },

      new AttributeVendorMetaData(){
      Code = "UserMoment",
      VendorID = 48, 
      Configurable = false
      },
      new AttributeVendorMetaData(){
      Code = "Targetgroup",
      VendorID = 48, 
      Configurable = false
      },
      new AttributeVendorMetaData(){
      Code = "Productgroup",
      VendorID = 48, 
      Configurable = false
      },
      new AttributeVendorMetaData(){
      Code = "Subproductgroup",
      VendorID = 48, 
      Configurable = false
      },
      new AttributeVendorMetaData(){
      Code = "InputCode",
      VendorID = 48, 
      Configurable = false
      },
      new AttributeVendorMetaData(){
      Code = "MaterialDescription",
      VendorID = 48, 
      Configurable = false
      },
       new AttributeVendorMetaData(){
      Code = "WehkampProduct",
      VendorID = 48, 
      Configurable = false
      },
      new AttributeVendorMetaData(){
      Code = "Module",
      VendorID = 48, 
      Configurable = false
      },
      new AttributeVendorMetaData(){
      Code = "ShopWeek",
      VendorID = 48, 
      Configurable = false
      },
      new AttributeVendorMetaData(){
      Code = "Style",
      VendorID = 48, 
      Configurable = false
      },
      new AttributeVendorMetaData(){
      Code = "Size",
      VendorID = 48, 
      Configurable = true,
      Searchable = true
      },
      new AttributeVendorMetaData(){
      Code = "Color",
      VendorID = 48, 
      Configurable = true,
      Searchable = true
      }
    };
    #endregion

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

      //#endregion Basic Attributes

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

    private void ConfigureProductAttributes(int vendorID, IUnitOfWork unit)
    {

      //var products = unit.Scope.Repository<Product>().GetAll(c => c.SourceVendorID == vendorID && c.IsConfigurable);
      var products = unit.Scope.Repository<Product>().GetAll(c => c.VendorAssortments.Any(d => d.VendorID == vendorID) && c.IsConfigurable);
      var confAttributes = unit.Scope.Repository<ProductAttributeMetaData>().GetAll(c => c.IsConfigurable);

      foreach (var prod in products)
      {
        foreach (var at in confAttributes)
        {
          var confi = prod.ProductAttributeMetaDatas.FirstOrDefault(c => c.AttributeID == at.AttributeID);
          if (confi == null) prod.ProductAttributeMetaDatas.Add(at);
        }
      }
      unit.Save();

    }

    protected override void Process()
    {
      _monitoring.Notify(Name, 0);
      var cultureInfoAmerican = new CultureInfo("en-US");

      _vendorIDs = GetConfiguration().AppSettings.Settings["ATPFAImportVendorIDs"].Value.Split(',').Select(int.Parse).ToList();

      string connectionString = string.Empty;
      var dsnNameSetting = GetConfiguration().AppSettings.Settings["ATPFADSN"];
      dsnNameSetting.ThrowIfNull("AT DSN Must be specified");

      connectionString = string.Format("DSN={0};PWD=progress", dsnNameSetting.Value);

      List<ProductAttributeMetaData> attributes = new List<ProductAttributeMetaData>();
      using (var unit = GetUnitOfWork())
      {
        try
        {
          SetupAttributes(unit, AttributeMapping, out attributes);
        }
        catch (Exception e)
        {
          log.Debug(e.InnerException);
        }

        AtPFARepository repository = new AtPFARepository(connectionString, log);
        var assortmentHelper = new AssortmentHelper();

        var validProducts = repository.GetValidItemColors();
        var colorLookup = repository.GetColorLookup();
        var sizeCodeLookup = repository.GetSizeCodeLookup();

        foreach (var vendorID in _vendorIDs)
        {
          try
          {
            _monitoring.Notify(Name, vendorID);
            List<Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorAssortmentItem> items = new List<Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorAssortmentItem>();
            List<Concentrator.Objects.Vendors.Bulk.VendorBarcodeBulk.VendorImportBarcode> vendorBarcodes = new List<VendorBarcodeBulk.VendorImportBarcode>();

            var vendor = unit.Scope.Repository<Vendor>().GetSingle(c => c.VendorID == vendorID);
            var countryCode = VendorHelper.GetCountryCode(vendorID);

            log.Info("Running for " + vendor.Name);
            var connectorID = vendor.ContentProducts.Select(x => x.ConnectorID).FirstOrDefault();
            var shouldNotCheckForSolden = vendor.VendorSettings.GetValueByKey<bool>("NonSoldenVendor", false);
            var vendorDoesNotImportDescription = vendor.VendorSettings.GetValueByKey<bool>("VendorDoesNotImportDescriptions", false);

            bool isSolden;
            if (shouldNotCheckForSolden) isSolden = true;
            else
            {
              if (connectorID == null || !GetSoldenPeriod(connectorID, out isSolden)) throw new Exception("Solden period values are corrupt.");
            }
            var parentVendorID = vendor.ParentVendorID.HasValue ? vendor.ParentVendorID.Value : vendor.VendorID;

            var currencyCode = vendor.VendorSettings.FirstOrDefault(c => c.SettingKey == "PFACurrencyCode").Try(c => c.Value, string.Empty);

            currencyCode.ThrowIfNullOrEmpty(new InvalidOperationException("Missing PFA currency code"));

            log.Info("Found products in total " + validProducts.Count);

            int counter = 0;

            foreach (var validItem in validProducts)
            {
              var itemNumber = validItem.Key;
              var productInfo = repository.GetGeneralProductInformation(itemNumber, countryCode);

              counter++;

              if (productInfo == null)
              {
                log.Debug("No info found for " + itemNumber);
                continue;
              }

              if (counter % 100 == 0) log.Info("Processed " + counter);

              foreach (var color in validItem.Value)
              {
                var customItemNumber = string.Format("{0} {1}", itemNumber, color).Trim();
                var productSpecifications = repository.GetProductLooseSpecifications(itemNumber, color);

                if (productInfo == null)
                {
                  log.Debug("Product info for " + itemNumber + " is null. ");
                  continue;
                }

                var rate = assortmentHelper.GetCurrentBTWRate(productInfo.TaxRatePercentage, productInfo.TaxRateDates, 21, productInfo.TaxCode);

                DateTime? weekShop = repository.GetShopWeek(itemNumber);


                var itemToAdd = new Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorAssortmentItem
                {
                  #region BrandVendor
                  BrandVendors = new List<VendorAssortmentBulk.VendorImportBrand>()
                          {
                            new VendorAssortmentBulk.VendorImportBrand(){
                              VendorID = vendorID,
                              VendorBrandCode = "AT",
                              ParentBrandCode = null,
                              Name = "America Today",
                            }
                          },

                  #endregion

                  #region GeneralProductInfo
                  VendorProduct = new VendorAssortmentBulk.VendorProduct
                  {
                    VendorItemNumber = customItemNumber,
                    CustomItemNumber = customItemNumber,
                    ShortDescription = productInfo.ProductName,
                    LongDescription = productInfo.ShortDescription + " " + productInfo.Material,
                    LineType = null,
                    LedgerClass = null,
                    ProductDesk = null,
                    ExtendedCatalog = null,
                    VendorID = vendorID,
                    DefaultVendorID = parentVendorID,
                    VendorBrandCode = "AT",
                    Barcode = string.Empty,
                    IsConfigurable = 1,
                    VendorProductGroupCode1 = productInfo.GroupCode1,
                    VendorProductGroupCodeName1 = productInfo.GroupName1,
                    VendorProductGroupCode2 = productInfo.GroupCode2,
                    VendorProductGroupCodeName2 = productInfo.GroupName2,
                    VendorProductGroupCode3 = productInfo.GroupCode3,
                    VendorProductGroupCodeName3 = productInfo.GroupName3
                  },
                  #endregion

                  VendorProductDescriptions = vendorDoesNotImportDescription ? new List<VendorAssortmentBulk.VendorProductDescription>() : new List<VendorAssortmentBulk.VendorProductDescription>(){
                    new Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorProductDescription{
                    CustomItemNumber = customItemNumber.Trim(),
                    DefaultVendorID = parentVendorID,
                    LanguageID = 1,
                    LongContentDescription = productInfo.ShortDescription + " " + productInfo.Material,
                    ShortContentDescription = productInfo.ProductName,
                    ShortSummaryDescription = string.Empty,
                    LongSummaryDescription = string.Empty,
                    ModelName = string.Empty,
                    ProductName = productInfo.ProductName ?? string.Empty,
                    VendorID = vendorID                    
                    }
                  },
                  #region Attribures
                  VendorImportAttributeValues = new List<VendorAssortmentBulk.VendorImportAttributeValue>() //size, color, targetgroup, productgroup, subproductgroup
                                 {
                                  new Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportAttributeValue(){
                                    AttributeCode = "Targetgroup",
                                    Value = productInfo.GroupCode1,
                                    AttributeID = attributes.FirstOrDefault(c => c.AttributeCode == "Targetgroup").AttributeID,
                                    CustomItemNumber = customItemNumber,
                                    DefaultVendorID = parentVendorID,
                                    LanguageID = null,
                                    VendorID = vendorID
                                    
                                  },
                                  new Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportAttributeValue(){
                                    AttributeCode = "MaterialDescription",
                                    Value = productInfo.Material,
                                    AttributeID = attributes.FirstOrDefault(c => c.AttributeCode == "MaterialDescription").AttributeID,
                                    CustomItemNumber = customItemNumber,
                                    DefaultVendorID = parentVendorID,
                                    LanguageID = null,
                                    VendorID = vendorID
                                    
                                  },
                                  new Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportAttributeValue(){
                                    AttributeCode = "WehkampProduct",
                                    Value = productSpecifications.Any(c => c.Specification == "WEH").ToString(),
                                    AttributeID = attributes.FirstOrDefault(c => c.AttributeCode == "WehkampProduct").AttributeID,
                                    CustomItemNumber = customItemNumber,
                                    DefaultVendorID = parentVendorID,
                                    LanguageID = null,
                                    VendorID = vendorID
                                    
                                  },
                                  new Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportAttributeValue(){
                                    AttributeCode = "Productgroup",
                                    Value = productInfo.GroupCode2,
                                    AttributeID = attributes.FirstOrDefault(c => c.AttributeCode == "Productgroup").AttributeID,
                                    CustomItemNumber = customItemNumber,
                                    DefaultVendorID = parentVendorID,
                                    LanguageID = null,
                                    VendorID = vendorID
                                    
                                  },
                                  new Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportAttributeValue(){
                                    AttributeCode = "Subproductgroup",
                                    Value = productInfo.GroupCode3,
                                    AttributeID = attributes.FirstOrDefault(c => c.AttributeCode == "Subproductgroup").AttributeID,
                                    CustomItemNumber = customItemNumber,
                                    DefaultVendorID = parentVendorID,
                                    LanguageID = null,
                                    VendorID = vendorID
                                    
                                  },
                                  new Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportAttributeValue(){
                                    AttributeCode = "Season",
                                    Value = productInfo.SeasonCode,
                                    AttributeID = attributes.FirstOrDefault(c => c.AttributeCode == "Season").AttributeID,
                                    CustomItemNumber = customItemNumber,
                                    DefaultVendorID = parentVendorID,
                                    LanguageID = null,
                                    VendorID = vendorID
                                    
                                  }                                  
                                  
                                 },

                  RelatedProducts = new List<VendorAssortmentBulk.VendorImportRelatedProduct>(),

                  #endregion
                  #region Prices
                  VendorImportPrices = new List<Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportPrice>()
                          {
                            new Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportPrice(){
                              VendorID = vendorID,
                              DefaultVendorID = parentVendorID,
                              CustomItemNumber = customItemNumber,
                              Price = "0",
                              CostPrice = "0",
                              TaxRate = rate.ToString(cultureInfoAmerican),
                              MinimumQuantity = 0,
                              CommercialStatus = "ENA"
                            }
                          },

                  #endregion
                  #region Stock
                  VendorImportStocks = new List<Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportStock>()
                };

                  #endregion

                if (weekShop.HasValue)
                {
                  itemToAdd.VendorImportAttributeValues.Add(new Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportAttributeValue()
                  {
                    AttributeCode = "ShopWeek",
                    Value = weekShop.Value.ToString("yyyy-MM-dd"),
                    AttributeID = attributes.FirstOrDefault(c => c.AttributeCode == "ShopWeek").AttributeID,
                    CustomItemNumber = customItemNumber,
                    DefaultVendorID = vendorID,
                    LanguageID = null,
                    VendorID = concentratorVendorID
                  });
                }

                var skus = repository.GetValidSkus(itemNumber, color);
                var specs = repository.GetSkuSpecifications(itemNumber);


                var priceRules = repository.GetProductPriceRules(itemNumber, currencyCode);

                var firstForTaxRef = priceRules.FirstOrDefault();

                if (firstForTaxRef != null)
                {
                  //var taxOverride = repository.GetProductTaxOverride(itemNumber, firstForTaxRef.country_code);

                  if (!string.IsNullOrEmpty(productInfo.override_tax_code))
                    rate = assortmentHelper.GetCurrentBTWRate(productInfo.override_tax_rates, productInfo.override_tax_dates, 21, productInfo.override_tax_code);
                }

                bool noSkus = false;
                int skuCount = 0;
                foreach (var sku in skus)
                {
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

                  var skuNew = string.Format("{0} {1} {2}", itemNumber, color, sku.SizeCode);
                  skuCount++;

                  Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorAssortmentItem product = new VendorAssortmentBulk.VendorAssortmentItem();


                  if (sku.SizeCode.ToLower() == "no size" || (color.ToLower() == "000" && sku.SizeCode.ToLower() == "no size") || (color.ToLower() == "000" && sku.SizeCode.ToLower() == "bag"))
                  {
                    noSkus = true;
                    skuNew = customItemNumber;
                  }

                  var item = items.Where(c => c.VendorProduct.CustomItemNumber == itemNumber);
                  if (item == null)
                  {
                    log.Debug("Skipping product " + string.Format("{0} {1} {2}", itemNumber, color, sku.SizeCode));
                    continue;
                  } //short circuit in case that product is missing                    


                  var price = assortmentHelper.GetPrice(itemNumber, color, sku.SizeCode, priceRules);
                  var discount = assortmentHelper.GetDiscount(itemNumber, color, sku.SizeCode, priceRules);

                  if (!isSolden)
                  {
                    if (discount.HasValue)
                    {
                      price = discount.Value;
                      discount = null;
                    }
                  }

                  Concentrator.Objects.Vendors.Bulk.VendorBarcodeBulk.VendorImportBarcode bc = new VendorBarcodeBulk.VendorImportBarcode()
                  {
                    CustomItemNumber = skuNew,
                    Barcode = sizeCodePfa,
                    Type = 4,
                    VendorID = vendorID
                  };

                  vendorBarcodes.Add(bc);

                  //create product
                  product = new Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorAssortmentItem()
                  {
                    VendorProduct = new VendorAssortmentBulk.VendorProduct()
                    {
                      VendorItemNumber = skuNew,
                      CustomItemNumber = skuNew,
                      VendorID = vendorID,
                      DefaultVendorID = parentVendorID,
                      ShortDescription = productInfo.ProductName,
                      LongDescription = productInfo.ShortDescription + " " + productInfo.Material,
                      Barcode = CompleteBarcode(sku.Barcode),
                      IsConfigurable = 0,
                      VendorBrandCode = "AT",
                      LineType = null,
                      LedgerClass = null,
                      ProductDesk = null,
                      ExtendedCatalog = null,
                      VendorProductGroupCode1 = productInfo.GroupCode1,
                      VendorProductGroupCodeName1 = productInfo.GroupName1,
                      VendorProductGroupCode2 = productInfo.GroupCode2,
                      VendorProductGroupCodeName2 = productInfo.GroupName2,
                      VendorProductGroupCode3 = productInfo.GroupCode3,
                      VendorProductGroupCodeName3 = productInfo.GroupName3,
                      ParentProductCustomItemNumber = customItemNumber
                    },
                    VendorProductDescriptions = vendorDoesNotImportDescription ? new List<VendorAssortmentBulk.VendorProductDescription>() : new List<VendorAssortmentBulk.VendorProductDescription>(){
                    new Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorProductDescription{
                    CustomItemNumber = skuNew,
                    DefaultVendorID = parentVendorID,
                    LanguageID = 1,
                    LongContentDescription = productInfo.ShortDescription + " " + productInfo.Material,
                    ShortContentDescription = productInfo.ProductName,
                    ShortSummaryDescription = string.Empty,
                    LongSummaryDescription = string.Empty,
                    ModelName = productInfo.ShortDescription,
                    ProductName = productInfo.ProductName ?? string.Empty,
                    VendorID = vendorID										
                    }
                  },
                    BrandVendors = new List<VendorAssortmentBulk.VendorImportBrand>(){
                    new VendorAssortmentBulk.VendorImportBrand(){
                              VendorID = 2,
                              VendorBrandCode = "AT",
                              ParentBrandCode = null,
                              Name = "America Today",
                            } 
                },
                    VendorImportAttributeValues = (from aM in attributes
                                                   let nameA = aM.AttributeCode
                                                   where nameA.ToLower() == "color" || nameA.ToLower() == "size"
                                                   select new Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportAttributeValue
                                                   {
                                                     AttributeCode = aM.AttributeCode,
                                                     AttributeID = aM.AttributeID,
                                                     CustomItemNumber = skuNew,
                                                     DefaultVendorID = parentVendorID,
                                                     LanguageID = null,
                                                     VendorID = vendorID,
                                                     Value = (nameA.ToLower() == "Color".ToLower() ? color : sku.SizeCode)
                                                   }).ToList(),

                    RelatedProducts = new List<VendorAssortmentBulk.VendorImportRelatedProduct>(),
                    VendorImportPrices = new List<Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportPrice>()
                          {
                            new Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportPrice(){
                              VendorID = vendorID,
                              DefaultVendorID = parentVendorID,
                              CustomItemNumber = skuNew,
                              Price = price.ToString(cultureInfoAmerican),
                              CostPrice = price.ToString(cultureInfoAmerican),    
                              SpecialPrice = discount.HasValue ? discount.Value.ToString(cultureInfoAmerican) :null,
                              TaxRate = rate.ToString(cultureInfoAmerican),
                              MinimumQuantity = 0,                              
                              CommercialStatus = price == 0 ? "NoPrice" : "ENA"
                            }
                          },



                    VendorImportStocks = new List<Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportStock>()

                  };


                  if (weekShop.HasValue)
                  {
                    var daysInRange = (DateTime.Now - weekShop.Value).Days;
                    if (daysInRange >= 0 && daysInRange <= 28)
                    {
                      if (itemToAdd != null)
                      {
                        itemToAdd.VendorProduct.VendorProductGroupCode4 = "New arrivals";
                        itemToAdd.VendorProduct.VendorProductGroupCodeName4 = "New arrivals";
                      }

                    }
                  }

                  //simple products shouldnt be added to the sale/just arrived. Not neccessary.
                  if (discount.HasValue && discount.Value != 0 && discount.Value < price)
                  {
                    if (string.IsNullOrEmpty(itemToAdd.VendorProduct.VendorProductGroupCode4))
                    {
                      if (itemToAdd.VendorProduct.CustomItemNumber == itemNumber + " " + color)
                      {
                        itemToAdd.VendorProduct.VendorProductGroupCode4 = "SALE";
                        itemToAdd.VendorProduct.VendorProductGroupCodeName4 = "SALE";
                      }
                    }
                    else
                    {

                      if (itemToAdd.VendorProduct.CustomItemNumber == itemNumber + " " + color)
                      {
                        itemToAdd.VendorProduct.VendorProductGroupCode5 = "SALE";
                        itemToAdd.VendorProduct.VendorProductGroupCodeName5 = "SALE";
                      }
                    }
                  }

                  #region SKU specs

                  //get the right attributes
                  var attributeColorRows = specs.Where(c => c.ColorCode == color).ToList();

                  var mentality = GetAttributeValue(attributeColorRows, "Mentality");
                  var input = GetAttributeValue(attributeColorRows, "Input");
                  var Style = GetAttributeValue(attributeColorRows, "Style");
                  var Usermoment = GetAttributeValue(attributeColorRows, "Usermoment");
                  var Module = GetAttributeValue(attributeColorRows, "Module");

                  AddAttributeValue(itemToAdd, "Mentality", mentality, attributes, skuNew, vendorID);
                  AddAttributeValue(itemToAdd, "Style", Style, attributes, skuNew, vendorID);
                  AddAttributeValue(itemToAdd, "Usermoment", Usermoment, attributes, skuNew, vendorID);
                  AddAttributeValue(itemToAdd, "Module", Module, attributes, skuNew, vendorID);
                  AddAttributeValue(itemToAdd, "InputCode", input, attributes, itemNumber, vendorID);
                  #endregion


                  items.Add(product);

                  //relate to main product
                  if (!noSkus)
                  {
                    if (String.Format("{0} {1}", itemNumber, color) == customItemNumber)
                    {
                      //
                      if (itemToAdd.RelatedProducts.Where(c => c.CustomItemNumber == customItemNumber && c.RelatedCustomItemNumber == skuNew).FirstOrDefault() == null)
                      {

                        itemToAdd.RelatedProducts.Add(new VendorAssortmentBulk.VendorImportRelatedProduct()
                        {
                          CustomItemNumber = customItemNumber,
                          DefaultVendorID = parentVendorID,
                          IsConfigured = 1,
                          RelatedCustomItemNumber = skuNew,
                          RelatedProductType = "Configured product",
                          VendorID = vendorID
                        });
                      }
                    }
                  }

                  if (noSkus)
                  {
                    items.Add(product);
                  }
                  else
                  {
                    items.Add(product);
                    items.Add(itemToAdd);
                  }
                }


              }
            }

            items.AddRange(GetCustomProducts(repository, vendor, parentVendorID, currencyCode, assortmentHelper, cultureInfoAmerican, vendorBarcodes, sizeCodeLookup, countryCode));

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

            log.Info(string.Format("Finished vendor bulks for {0}", vendorID));

          }
          catch (Exception e)
          {
            log.AuditCritical("Import failed for vendor " + vendorID, e);
            _monitoring.Notify(Name, -vendorID);
          }
        }
        ConfigureProductAttributes(_vendorIDs, unit);
        AddValueLabels(unit, colorLookup);
      }
      _monitoring.Notify(Name, 1);
    }

    public string GetAttributeValue(List<SkuSpecification> rows, string key)
    {
      var Row = (from r in rows
                 where r.KmkDescription == key
                 select r).FirstOrDefault();

      string value = null;

      if (Row != null) value = Row.KmkCode;

      return value;
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
          var translation = work.Scope.Repository<ProductAttributeValueLabel>().GetSingle(c => c.Value == value && c.AttributeID == attributeColors.AttributeID && c.ConnectorID == 6 && c.LanguageID == lang.LanguageID);

          if (translation == null)
          {
            translation = new ProductAttributeValueLabel
            {
              AttributeID = attributeColors.AttributeID,
              ConnectorID = 6,
              Label = label.Value,
              Value = value,
              LanguageID = lang.LanguageID
            };

            work.Scope.Repository<ProductAttributeValueLabel>().Add(translation);
          }
        }
      }
      work.Save();
    }

    public void AddAttributeValue(Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorAssortmentItem item, string key, string value, List<ProductAttributeMetaData> items, string customItemNumber, int vendorID)
    {
      if (!string.IsNullOrEmpty(value))
      {
        item.VendorImportAttributeValues.Add(new VendorAssortmentBulk.VendorImportAttributeValue()
        {
          AttributeCode = key,
          AttributeID = items.First(c => c.AttributeCode.ToLower() == key.ToLower()).AttributeID,
          CustomItemNumber = customItemNumber,
          DefaultVendorID = vendorID,
          LanguageID = null,
          Value = value,
          VendorID = vendorID
        });
      }
    }

    public List<Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorAssortmentItem> GetCustomProducts(AtPFARepository repository, Vendor vendor, int defaultVendorID, string currencyCode, AssortmentHelper helper, CultureInfo info,
      List<Concentrator.Objects.Vendors.Bulk.VendorBarcodeBulk.VendorImportBarcode> barcodeList, List<SizeCodeResult> sizeCodeLookup, string countryCode)
    {
      int vendorID = vendor.VendorID;
      //var customProducts = repository.GetCustomProducts();
      List<Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorAssortmentItem> items = new List<VendorAssortmentBulk.VendorAssortmentItem>();

      var ReturnCostsProduct = vendor.VendorSettings.GetValueByKey<string>("ReturnCostsProduct", string.Empty);
      var ShipmentCostsProduct = vendor.VendorSettings.GetValueByKey<string>("ShipmentCostsProduct", string.Empty);
      var KialaReturnCostsProduct = vendor.VendorSettings.GetValueByKey<string>("KialaReturnCostsProduct", string.Empty);
      var KialaShipmentCostsProduct = vendor.VendorSettings.GetValueByKey<string>("KialaShipmentCostsProduct", string.Empty);

      List<string> customProductIDs = new List<string> { 
				ReturnCostsProduct,
				ShipmentCostsProduct,
        KialaReturnCostsProduct,
        KialaShipmentCostsProduct
			};

      foreach (var customProductId in customProductIDs.Where(c => !string.IsNullOrEmpty(c)))
      {
        var productInfo = repository.GetGeneralProductInformation(customProductId, countryCode);
        var prices = repository.GetProductPriceRules(customProductId, currencyCode);
        var price = helper.GetPrice(customProductId, string.Empty, string.Empty, prices);
        var specialPrice = helper.GetDiscount(customProductId, string.Empty, string.Empty, prices);
        var rate = helper.GetCurrentBTWRate(productInfo.TaxRatePercentage, productInfo.TaxRateDates, 21, productInfo.TaxCode);
        var sku = repository.GetValidSkus(customProductId, "000").FirstOrDefault();

        if (sku == null)
          throw new InvalidOperationException("Custom product should have skus");

        //Parse barcode

        string sizeCodePfa = string.Empty;

        foreach (var lookupT in new List<string>() { productInfo.MtbCode1, productInfo.MtbCode2, productInfo.MtbCode3, productInfo.MtbCode4 })
        {
          var lookup = sizeCodeLookup.FirstOrDefault(c => c.SizeCode.ToLower() == sku.SizeCode.ToLower() && c.MtbCode.ToLower() == lookupT.ToLower());
          if (lookup != null)
          {
            sizeCodePfa = lookup.PfaCode;
            break;

          }
        }

        barcodeList.Add(new VendorBarcodeBulk.VendorImportBarcode()
        {
          Barcode = sizeCodePfa,
          CustomItemNumber = customProductId,
          Type = 4,
          VendorID = vendorID
        });

        var vendorAssortmentItem = new Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorAssortmentItem()
        {

          VendorImportAttributeValues = new List<VendorAssortmentBulk.VendorImportAttributeValue>(),
          RelatedProducts = new List<VendorAssortmentBulk.VendorImportRelatedProduct>(),
          ProductGroupVendors = new List<ProductGroupVendor>(),
          VendorProduct = new VendorAssortmentBulk.VendorProduct()
          {
            IsConfigurable = 0,
            CustomItemNumber = customProductId,
            ShortDescription = productInfo.ProductName,
            LongDescription = productInfo.ShortDescription,
            VendorItemNumber = customProductId,
            VendorID = vendorID,
            VendorBrandCode = vendorBrandCode,
            Barcode = CompleteBarcode(sku.Barcode),
            DefaultVendorID = defaultVendorID,
            VendorProductGroupCode1 = productInfo.GroupCode1,
            VendorProductGroupCodeName1 = productInfo.GroupName1,
            VendorProductGroupCode2 = productInfo.GroupCode2,
            VendorProductGroupCodeName2 = productInfo.GroupName2,
            VendorProductGroupCode3 = productInfo.GroupCode3,
            VendorProductGroupCodeName3 = productInfo.GroupName3

          },
          BrandVendors = new List<VendorAssortmentBulk.VendorImportBrand>(){
					new Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportBrand(){
						Name = vendorBrandName,
						VendorBrandCode = vendorBrandCode,
						VendorID = vendorID,
						ParentBrandCode = null
					}
					},
          VendorImportPrices = new List<VendorAssortmentBulk.VendorImportPrice>() { 
						new Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportPrice(){
									Price = price.ToString(info),
									SpecialPrice = specialPrice.HasValue ?  specialPrice.Value.ToString(info) : null,
									CostPrice = price.ToString(info), 
									CustomItemNumber = customProductId,
									CommercialStatus = "ENA",
									MinimumQuantity = 0,
									DefaultVendorID = defaultVendorID,
									VendorID =vendorID,
									TaxRate = rate.ToString(info)
							}
					},
          VendorImportStocks = new List<Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportStock>(),
          //{
          //  new Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportStock(){
          //    VendorID = vendorID,
          //    DefaultVendorID = defaultVendorID,
          //    CustomItemNumber = customProductId,                              
          //    QuantityOnHand = 1,                              
          //    StockType = "Webshop",                              
          //    StockStatus = "ENA"
          //  }
          //},
          VendorProductDescriptions = new List<VendorAssortmentBulk.VendorProductDescription>() { 
						new Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorProductDescription(){
							LanguageID = 1,
							DefaultVendorID = defaultVendorID,
							CustomItemNumber = customProductId,
							ProductName = productInfo.ProductName ?? string.Empty,
							ShortContentDescription = productInfo.ShortDescription,
							VendorID = vendorID,
							ModelName = string.Empty,
							ShortSummaryDescription = string.Empty,
							LongSummaryDescription = string.Empty,
							LongContentDescription = string.Empty
						}
					}
        };

        items.Add(vendorAssortmentItem);
      }

      return items;
    }

    private string CompleteBarcode(string s)
    {
      if (s.Length != 12) return s;

      var c = 0;
      for (var i = 11; i >= 0; i--)
        c += Int32.Parse(s[i].ToString()) * (i % 2 == 0 ? 1 : 3);

      return s + ((10 - (c % 10)) % 10);


    }
  }
}
