using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using Concentrator.Objects;
using Concentrator.Objects.ConcentratorService;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects.Ftp;
using Concentrator.Objects.Models.Attributes;
using Concentrator.Objects.Models.Localization;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Vendors.Bulk;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Plugins.Sapph.Repositories;
using Concentrator.Web.CustomerSpecific.Sapph.Repositories;
using System.Globalization;
using System.Configuration;

namespace Concentrator.Plugins.Sapph
{

  public class AttributeVendorMetaData
  {
    public string Code { get; set; }

    public bool Configurable { get; set; }

    public bool Searchable { get; set; }

    public int VendorID { get; set; }
  }

  public class ProductImport : ConcentratorPlugin
  {
    #region Variables
    private static Int32[] _vendorIDs;
    public static Int32 DefaultVendorID;
    public static Int32 ConcentratorVendorID = 48;

    public string ReturnVendorItemNumber;
    public string ShipmentVendorItemNumber;

    const Boolean IsPartialAssortment = true;
    /// <summary>
    /// If set per vendor, the vendor price will be set from the Advice price instead of the Labelling price
    /// </summary>
    private const string USE_ADVICE_PRICE_FROM_XML_SETTING_KEY = "UseAdvicePrice";



    private Dictionary<String, List<String>> _configuredAttributes;
    #endregion

    #region AttributeMapping
    private List<AttributeVendorMetaData> _attributeMapping;

    private void InitAttributeMapping()
    {
      _attributeMapping = new List<AttributeVendorMetaData>()
    {
      new AttributeVendorMetaData()
      {
        Code = "Season",
        VendorID = ConcentratorVendorID,
        Configurable = false
      },
      new AttributeVendorMetaData()
      {
        Code = "Material",
        VendorID = ConcentratorVendorID,
        Configurable = false
      },    
      new AttributeVendorMetaData()
      {
        Code = "Brasize",
        Configurable = true,
        Searchable = true,
        VendorID = DefaultVendorID
      },
      new AttributeVendorMetaData()
      {
        Code = "Size",
        VendorID = ConcentratorVendorID,
        Configurable = true,
        Searchable = false
      },
      new AttributeVendorMetaData()
      {
        Code = "TopSize",
        VendorID = ConcentratorVendorID,
        Configurable = false,
        Searchable = true
      },
      new AttributeVendorMetaData()
      {
        Code = "BottomSize",
        VendorID = ConcentratorVendorID,
        Configurable = false,
        Searchable = true
      },
      new AttributeVendorMetaData()
      {
        Code = "Cupsize",
        VendorID = DefaultVendorID,
        Configurable = true,
        Searchable = true
      },     
      new AttributeVendorMetaData()
      {
        Code = "Color",
        VendorID = ConcentratorVendorID,
        Configurable = true,
        Searchable = true
      },     
      new AttributeVendorMetaData()
      {
        Code = "Type",
       VendorID = DefaultVendorID,
        Configurable = true,
        Searchable = false
      },
			new AttributeVendorMetaData()
      {
        Code = "IsBra",
       VendorID = DefaultVendorID,
        Configurable = true,
        Searchable = false
      },
      	new AttributeVendorMetaData()
      {
        Code = "Model",
        Configurable = true,
        VendorID = DefaultVendorID,
        Searchable = false
      },
      	new AttributeVendorMetaData()
      {
        Code = "Group",
        Configurable = true,
        VendorID = DefaultVendorID,
        Searchable = true
      }
    };

    }
    #endregion

    #region Attributes
    protected void SetupAttributes(IUnitOfWork unit, List<AttributeVendorMetaData> attributeMapping, int vendorID, out List<ProductAttributeMetaData> attributes)
    {
      Int32 generalAttributegroupID = GetGeneralAttributegroupID(unit, vendorID);
      List<Language> languages = unit.Scope.Repository<Language>().GetAll().ToList();

      var attributeRepo = unit.Scope.Repository<ProductAttributeMetaData>();
      var attributeNameRepo = unit.Scope.Repository<ProductAttributeName>();

      List<ProductAttributeMetaData> res = new List<ProductAttributeMetaData>();

      attributeMapping.ForEach(attr =>
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

        languages.ForEach(lang =>
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
        });
      });

      unit.Save();

      attributes = res;
    }

    private Int32 GetGeneralAttributegroupID(IUnitOfWork unit, int vendorID)
    {
      Int32 generalAttributegroupID;

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
          VendorID = vendorID
        };

        unit.Scope.Repository<ProductAttributeGroupMetaData>().Add(attributeGroup);

        var groupNameEng = new ProductAttributeGroupName
        {
          LanguageID = (int)LanguageTypes.English,
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

        var groupNameGer = new ProductAttributeGroupName
        {
          LanguageID = (int)LanguageTypes.German,
          Name = GeneralProductGroupName,
          ProductAttributeGroupMetaData = attributeGroup
        };

        attributeGroupNameRepo.Add(groupNameGer);

        unit.Save();

        generalAttributegroupID = attributeGroup.ProductAttributeGroupID;
      }

      return generalAttributegroupID;
    }

    private void AddConfiguredAttributes(VendorAssortmentBulk.VendorAssortmentItem configurableProduct, bool hasCupsize)
    {
      var configuredAttributesNames = new List<String> { "Color", "Size" };

      if (!_configuredAttributes.Keys.Contains(configurableProduct.VendorProduct.VendorItemNumber))
        _configuredAttributes.Add(configurableProduct.VendorProduct.VendorItemNumber, configuredAttributesNames);
    }
    #endregion

    public override string Name
    {
      get { return "Sapph assortment import"; }
    }

    protected override void Process()
    {
      var defaultVendorIDSetting = GetConfiguration().AppSettings.Settings["VendorID"];
      if (defaultVendorIDSetting == null)
        throw new Exception("Setting VendorID is missing");

      DefaultVendorID = Int32.Parse(defaultVendorIDSetting.Value);

      var vendorIdsToProcess = GetConfiguration().AppSettings.Settings["VendorIDs"];
      if (vendorIdsToProcess == null)
        throw new Exception("Setting VendorIDs is missing");

      _vendorIDs = vendorIdsToProcess.Value.Split(',').Select(x => Convert.ToInt32(x)).ToArray();

      var attributes = new List<ProductAttributeMetaData>();
      using (var unit = GetUnitOfWork())
      {
        try
        {
          InitAttributeMapping();
          SetupAttributes(unit, _attributeMapping, DefaultVendorID, out attributes);
        }
        catch (Exception e)
        {
          log.Debug(e.InnerException);
        }

        foreach (var vendorID in _vendorIDs)
        {
          _configuredAttributes = new Dictionary<string, List<string>>();



          var repositoryProductTypes = new ProductTypeRepository((IServiceUnitOfWork)unit, DefaultVendorID);
          var repositoryModels = new ProductModelRepository((IServiceUnitOfWork)unit, DefaultVendorID);

          bool useAdvicePrice = false;
          var useAdvicePriceSetting = unit.Scope.Repository<VendorSetting>().GetSingle(c => c.VendorID == vendorID && c.SettingKey == USE_ADVICE_PRICE_FROM_XML_SETTING_KEY);
          if (useAdvicePriceSetting != null)
          {
            bool.TryParse(useAdvicePriceSetting.Value, out useAdvicePrice);
          }


          var ReturnCostsVendorItemNumber = unit.Scope.Repository<VendorSetting>().GetSingle(c => c.VendorID == vendorID && c.SettingKey == "ReturnCostsProduct");
          if (ReturnCostsVendorItemNumber != null)
            ReturnVendorItemNumber = ReturnCostsVendorItemNumber.Value;

          var ShipmentCostsVendorItemNumber = unit.Scope.Repository<VendorSetting>().GetSingle(c => c.VendorID == vendorID && c.SettingKey == "ShipmentCostsProduct");
          if (ShipmentCostsVendorItemNumber != null)
            ShipmentVendorItemNumber = ShipmentCostsVendorItemNumber.Value;

          foreach (var attributeMapping in _attributeMapping)
          {
            attributeMapping.VendorID = vendorID;
          }


          var xmlAssortmentRepository = new XmlAssortmentRepository(vendorID, unit, log);
          foreach (var assortment in xmlAssortmentRepository.GetAssortment(vendorID))
          {
            var assortmentList = new List<VendorAssortmentBulk.VendorAssortmentItem>();
            var stockPerProduct = (from p in unit.Scope.Repository<Product>().GetAll(c => c.SourceVendorID == vendorID)
                                   let stock = p.VendorStocks.FirstOrDefault(c => c.VendorID == vendorID)
                                   where stock != null
                                   select new
                                   {
                                     p.VendorItemNumber,
                                     stock.QuantityOnHand
                                   }).ToDictionary(c => c.VendorItemNumber, c => c.QuantityOnHand);

            var productGroup = assortment.Assortments.GroupBy(c => c.ConfigurableVendorItemNumber).ToList();

            foreach (var configurableProductGroup in productGroup)
            {
              var first = configurableProductGroup.First();
              var isConfigurableProductAWebsiteProduct = configurableProductGroup.Select(x => x.Website).Contains("WEB");

              var type = first.TypeCode;

              var typeObject = repositoryProductTypes.Get(type);
              var modelObject = repositoryModels.Get(first.ModelCode);

              #region "Configurable Product"
              var configurableProduct = new VendorAssortmentBulk.VendorAssortmentItem();

              if (first.ConfigurableVendorItemNumber != ShipmentVendorItemNumber && first.ConfigurableVendorItemNumber != ReturnVendorItemNumber)
              {
                #region BrandVendor
                configurableProduct.BrandVendors = new List<VendorAssortmentBulk.VendorImportBrand>()
                {
                  new VendorAssortmentBulk.VendorImportBrand()
                    {
                      VendorID = DefaultVendorID,
                      VendorBrandCode = first.Brand,
                      Name = first.Brand
                    }
                };
                #endregion

                #region GeneralProductInfo
                configurableProduct.VendorProduct = new VendorAssortmentBulk.VendorProduct
                  {
                    VendorID = vendorID,
                    DefaultVendorID = DefaultVendorID,
                    CustomItemNumber = configurableProductGroup.Key,
                    VendorItemNumber = configurableProductGroup.Key,
                    VendorBrandCode = first.Brand,
                    IsConfigurable = 1,
                    VendorProductGroupCode1 = first.ProductGroupCode1,
                    VendorProductGroupCodeName1 = first.ProductGroupCode1,
                    VendorProductGroupCode2 = first.ProductGroupCode2,
                    VendorProductGroupCodeName2 = first.ProductGroupCode2,
                    VendorProductGroupCode3 = first.ProductGroupCode3,
                    VendorProductGroupCodeName3 = first.ProductGroupCode3,
                    VendorProductGroupCode4 = first.Brand,
                    VendorProductGroupCodeName4 = first.Brand,
                    ShortDescription = first.ShortDescription,
                    LongDescription = first.LongDescription,
                    Barcode = String.Empty
                  };
                #endregion

                #region ProductDescription
                configurableProduct.VendorProductDescriptions = new List<VendorAssortmentBulk.VendorProductDescription>()
                {
                  new Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorProductDescription()
                    {
                      VendorID = vendorID,
                      DefaultVendorID = DefaultVendorID,
                      CustomItemNumber = first.ConfigurableVendorItemNumber,
                      LanguageID = 1,
                      ProductName = first.ShortDescription,
                      ShortContentDescription = first.ShortDescription,
                      LongContentDescription = first.LongDescription
                    }
                };
                #endregion

                #region Attributes

                configurableProduct.VendorImportAttributeValues = new List
                  <VendorAssortmentBulk.VendorImportAttributeValue>()
                {
                  new VendorAssortmentBulk.VendorImportAttributeValue()
                    {
                      AttributeCode = "Season",
                      Value = first.ProductGroupCode2,
                      AttributeID = attributes.FirstOrDefault(c => c.AttributeCode == "Season").AttributeID,
                      CustomItemNumber = first.ConfigurableVendorItemNumber,
                      DefaultVendorID = DefaultVendorID,
                      LanguageID = null,
                      VendorID = ConcentratorVendorID
                    },                
                  new Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportAttributeValue()
                    {
                      AttributeCode = "IsBra",
                      Value = typeObject == null ? "false" : typeObject.IsBra.ToString(),
                      AttributeID = attributes.FirstOrDefault(c => c.AttributeCode == "IsBra").AttributeID,
                      CustomItemNumber = first.ConfigurableVendorItemNumber,
                      DefaultVendorID = DefaultVendorID,
                      LanguageID = null,
                      VendorID = ConcentratorVendorID
                    },                 
                  new Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportAttributeValue()
                    {
                      AttributeCode = "Group",
                      Value = typeObject == null ? type : typeObject.ProductType.ToString(),
                      AttributeID = attributes.FirstOrDefault(c => c.AttributeCode == "Group").AttributeID,
                      CustomItemNumber = first.ConfigurableVendorItemNumber,
                      DefaultVendorID = DefaultVendorID,
                      LanguageID = null,
                      VendorID = ConcentratorVendorID
                    }
                };

                #region ProductType Translation Attributes

                if (typeObject != null) //add translations
                {
                  foreach (var translation in typeObject.Translations)
                  {
                    configurableProduct.VendorImportAttributeValues.Add(new VendorAssortmentBulk.VendorImportAttributeValue
                    {
                      AttributeCode = "Type",
                      Value = translation.Value,
                      AttributeID = attributes.FirstOrDefault(c => c.AttributeCode == "Type").AttributeID,
                      CustomItemNumber = first.ConfigurableVendorItemNumber,
                      DefaultVendorID = DefaultVendorID,
                      LanguageID = translation.Key.ToString(),
                      VendorID = ConcentratorVendorID
                    });
                  }
                }

                #endregion

                #region Model Translation

                if (modelObject != null)
                {
                  configurableProduct.VendorImportAttributeValues.Add(new Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportAttributeValue()
                     {
                       AttributeCode = "Model",
                       Value = modelObject.Translation,
                       AttributeID = attributes.FirstOrDefault(c => c.AttributeCode == "Model").AttributeID,
                       CustomItemNumber = first.ConfigurableVendorItemNumber,
                       DefaultVendorID = DefaultVendorID,
                       LanguageID = null,
                       VendorID = ConcentratorVendorID
                     });
                }

                #endregion

                #endregion

                #region Prices
                configurableProduct.VendorImportPrices =
                  new List<Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportPrice>()
                  {
                    new Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportPrice()
                      {
                        VendorID = vendorID,
                        DefaultVendorID = DefaultVendorID,
                        CustomItemNumber = configurableProductGroup.Key,
                        CommercialStatus = isConfigurableProductAWebsiteProduct ? "Active" : "Non-Web",
                        Price = "0"
                      }
                  };
                #endregion

                #region VendorStock
                configurableProduct.VendorImportStocks = new List<VendorAssortmentBulk.VendorImportStock>()
                {
                  new VendorAssortmentBulk.VendorImportStock()
                    {
                      VendorID = vendorID,
                      DefaultVendorID = DefaultVendorID,
                      CustomItemNumber = configurableProductGroup.Key,
                      StockType = "Webshop",
                      StockStatus = "NonStock"
                    }
                };
                #endregion

                assortmentList.Add(configurableProduct);

                configurableProduct.RelatedProducts = new List<VendorAssortmentBulk.VendorImportRelatedProduct>();

                var otherProductsFromModel =
                  assortment.Assortments.Where(c => c.ModelCode == first.ModelCode && c.TypeCode != first.ModelCode)
                            .Select(c => c.ConfigurableVendorItemNumber)
                            .Distinct()
                            .ToList();
                var otherProductsFromModelAndSameType = new List<string>();
                var otherProductsFromModelAndDifferentType = new List<string>();

                foreach (var product in otherProductsFromModel)
                {
                  //var typeRelated = product.Split(new char[] {'-'}, StringSplitOptions.RemoveEmptyEntries)[1];
                  var typeObjectRelated = repositoryProductTypes.Get(type);

                  if (typeObjectRelated != null && typeObject != null)
                  {
                    if (typeObjectRelated.IsBra == typeObject.IsBra)
                      otherProductsFromModelAndSameType.Add(product);
                    else
                      otherProductsFromModelAndDifferentType.Add(product);
                  }
                }

                foreach (var relatedFromType in otherProductsFromModelAndSameType)
                {
                  configurableProduct.RelatedProducts.Add(new VendorAssortmentBulk.VendorImportRelatedProduct()
                    {
                      CustomItemNumber = configurableProductGroup.Key,
                      DefaultVendorID = DefaultVendorID,
                      VendorID = vendorID,
                      RelatedCustomItemNumber = relatedFromType,
                      IsConfigured = 0,
                      RelatedProductType = "ModelType"
                    });
                }

                foreach (var relatedFromType in otherProductsFromModelAndDifferentType)
                {
                  configurableProduct.RelatedProducts.Add(new VendorAssortmentBulk.VendorImportRelatedProduct()
                    {
                      CustomItemNumber = configurableProductGroup.Key,
                      DefaultVendorID = DefaultVendorID,
                      VendorID = vendorID,
                      RelatedCustomItemNumber = relatedFromType,
                      IsConfigured = 0,
                      RelatedProductType = "ModelTypeCross"
                    });
                }
              }
              #endregion

              var isBra = typeObject != null && typeObject.IsBra;

              foreach (var simpleItem in configurableProductGroup)
              {
                var cupsizeProduct = false;

                #region "Simple Product"
                var simpleProduct = new VendorAssortmentBulk.VendorAssortmentItem();

                #region "Brand"
                simpleProduct.BrandVendors = new List<VendorAssortmentBulk.VendorImportBrand>()
                {
                  new VendorAssortmentBulk.VendorImportBrand()
                    {
                      VendorID = DefaultVendorID,
                      VendorBrandCode = simpleItem.Brand,
                      Name = simpleItem.Brand
                    }
                };
                #endregion

                #region "Vendor Product"
                simpleProduct.VendorProduct = new VendorAssortmentBulk.VendorProduct
                  {
                    VendorID = vendorID,
                    DefaultVendorID = DefaultVendorID,
                    CustomItemNumber = simpleItem.SimpleVendorItemNumber,
                    VendorItemNumber = simpleItem.SimpleVendorItemNumber,
                    VendorBrandCode = first.Brand,
                    IsConfigurable = 0,
                    VendorProductGroupCode1 = simpleItem.ProductGroupCode1,
                    VendorProductGroupCodeName1 = simpleItem.ProductGroupCode1,
                    VendorProductGroupCode2 = simpleItem.ProductGroupCode2,
                    VendorProductGroupCodeName2 = simpleItem.ProductGroupCode2,
                    VendorProductGroupCode3 = simpleItem.ProductGroupCode3,
                    VendorProductGroupCodeName3 = simpleItem.ProductGroupCode3,
                    VendorProductGroupCode4 = first.Brand,
                    VendorProductGroupCodeName4 = first.Brand,
                    ShortDescription = simpleItem.ShortDescription,
                    LongDescription = simpleItem.LongDescription,
                    Barcode = simpleItem.Barcode,
                    ParentProductCustomItemNumber = configurableProductGroup.Key
                  };
                #endregion

                #region "Product Description"
                simpleProduct.VendorProductDescriptions = new List<VendorAssortmentBulk.VendorProductDescription>()
                {
                  new Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorProductDescription()
                    {
                      VendorID = vendorID,
                      DefaultVendorID = DefaultVendorID,
                      CustomItemNumber = simpleItem.SimpleVendorItemNumber,
                      LanguageID = 1,
                      ProductName = simpleItem.ShortDescription,
                      ShortContentDescription = simpleItem.ShortDescription,
                      LongContentDescription = simpleItem.LongDescription
                    }
                };
                #endregion

                #region "Attribute"
                simpleProduct.VendorImportAttributeValues = new List<VendorAssortmentBulk.VendorImportAttributeValue>()
                {
                  new Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportAttributeValue()
                    {
                      AttributeCode = "Color",
                      Value = simpleItem.ColourcodeSupplier,
                      AttributeID = attributes.FirstOrDefault(c => c.AttributeCode == "Color").AttributeID,
                      CustomItemNumber = simpleItem.SimpleVendorItemNumber,
                      DefaultVendorID = DefaultVendorID,
                      LanguageID = null,
                      VendorID = ConcentratorVendorID
                    },
                     new Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportAttributeValue()
                    {
                      AttributeCode = "Group",
                      Value = typeObject == null ? type : typeObject.ProductType.ToString(),
                      AttributeID = attributes.FirstOrDefault(c => c.AttributeCode == "Group").AttributeID,
                      CustomItemNumber = simpleItem.SimpleVendorItemNumber,
                      DefaultVendorID = DefaultVendorID,
                      LanguageID = null,
                      VendorID = ConcentratorVendorID
                    }
                };

                if (!string.IsNullOrEmpty(simpleItem.SubsizeSupplier))
                {
                  simpleProduct.VendorImportAttributeValues.Add(new VendorAssortmentBulk.VendorImportAttributeValue()
                  {
                    AttributeCode = "Size",
                    Value = string.Format("{0}{1}", simpleItem.SizeSupplier, simpleItem.SubsizeSupplier),
                    AttributeID = attributes.FirstOrDefault(c => c.AttributeCode == "Size").AttributeID,
                    CustomItemNumber = simpleItem.SimpleVendorItemNumber,
                    DefaultVendorID = DefaultVendorID,
                    LanguageID = null,
                    VendorID = ConcentratorVendorID
                  });

                  cupsizeProduct = true;
                }
                else
                {
                  simpleProduct.VendorImportAttributeValues.Add(new VendorAssortmentBulk.VendorImportAttributeValue()
                  {
                    AttributeCode = "Size",
                    Value = simpleItem.SizeSupplier,
                    AttributeID = attributes.FirstOrDefault(c => c.AttributeCode == "Size").AttributeID,
                    CustomItemNumber = simpleItem.SimpleVendorItemNumber,
                    DefaultVendorID = DefaultVendorID,
                    LanguageID = null,
                    VendorID = ConcentratorVendorID
                  });
                }

                if (isBra)
                {
                  simpleProduct.VendorImportAttributeValues.Add(new VendorAssortmentBulk.VendorImportAttributeValue()
                    {
                      AttributeCode = "TopSize",
                      Value = string.Format("{0}{1}", simpleItem.SizeSupplier, simpleItem.SubsizeSupplier),
                      AttributeID = attributes.FirstOrDefault(c => c.AttributeCode == "TopSize").AttributeID,
                      CustomItemNumber = simpleItem.SimpleVendorItemNumber,
                      DefaultVendorID = DefaultVendorID, // 50
                      LanguageID = null,
                      VendorID = ConcentratorVendorID // 48
                    });
                }
                else
                {
                  simpleProduct.VendorImportAttributeValues.Add(new VendorAssortmentBulk.VendorImportAttributeValue()
                  {
                    AttributeCode = "BottomSize",
                    Value = simpleItem.SizeSupplier,
                    AttributeID = attributes.FirstOrDefault(c => c.AttributeCode == "BottomSize").AttributeID,
                    CustomItemNumber = simpleItem.SimpleVendorItemNumber,
                    DefaultVendorID = DefaultVendorID, // 50
                    LanguageID = null,
                    VendorID = ConcentratorVendorID // 48
                  });
                }


                #endregion

                #region "Price"
                simpleProduct.VendorImportPrices = new List<Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportPrice>()
                {
                  new Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportPrice()
                    {
                      VendorID = vendorID,
                      DefaultVendorID = DefaultVendorID,
                      CustomItemNumber = simpleItem.SimpleVendorItemNumber,
                      CommercialStatus = simpleItem.Website.Equals("WEB") ? "Active" : "Non-Web",
                      Price =  (useAdvicePrice  ? simpleItem.AdvicePrice :  simpleItem.LabellingPrice).ToString(CultureInfo.InvariantCulture),
                      CostPrice =(useAdvicePrice  ? simpleItem.AdvicePrice :  simpleItem.LabellingPrice).ToString(CultureInfo.InvariantCulture)
                    }
                };
                #endregion

                #region "Stock"

                if (!stockPerProduct.ContainsKey(simpleItem.SimpleVendorItemNumber))
                {
                  simpleProduct.VendorImportStocks = new List<VendorAssortmentBulk.VendorImportStock>()
                {
                  new VendorAssortmentBulk.VendorImportStock()
                    {
                      VendorID = vendorID,
                      DefaultVendorID = DefaultVendorID,
                      CustomItemNumber = simpleItem.SimpleVendorItemNumber,
                      StockType = "Webshop",
                      StockStatus = "ENA"
                    }                     
                };
                }
                else
                {
                  simpleProduct.VendorImportStocks = new List<VendorAssortmentBulk.VendorImportStock>();
                }
                #endregion

                assortmentList.Add(simpleProduct);

                if (first.ConfigurableVendorItemNumber != ShipmentVendorItemNumber && first.ConfigurableVendorItemNumber != ReturnVendorItemNumber)
                {
                  configurableProduct.RelatedProducts.Add(new VendorAssortmentBulk.VendorImportRelatedProduct()
                    {
                      CustomItemNumber = first.ConfigurableVendorItemNumber,
                      DefaultVendorID = DefaultVendorID,
                      IsConfigured = 1,
                      VendorID = vendorID,
                      RelatedCustomItemNumber = simpleItem.SimpleVendorItemNumber,
                      RelatedProductType = "Configured product"
                    });

                  AddConfiguredAttributes(configurableProduct, cupsizeProduct);
                }
                #endregion
              }
            }

            try
            {
              var bulkConfig = new VendorAssortmentBulkConfiguration { IsPartialAssortment = IsPartialAssortment };
              using (var vendorAssortmentBulk = new VendorAssortmentBulk(assortmentList, vendorID, DefaultVendorID, bulkConfig))
              {
                vendorAssortmentBulk.Init(unit.Context);
                vendorAssortmentBulk.Sync(unit.Context);
              }
            }
            catch (Exception ex)
            {
              log.AuditError(ex.Message);
            }

            foreach (var productAttributeConfiguration in _configuredAttributes)
            {
              var product = unit.Scope.Repository<Product>().GetSingle(c => c.VendorItemNumber == productAttributeConfiguration.Key && c.IsConfigurable);

              foreach (var code in productAttributeConfiguration.Value)
              {
                var ent = product.ProductAttributeMetaDatas.FirstOrDefault(c => c.AttributeCode == code);

                if (ent == null)
                {
                  var attr = unit.Scope.Repository<ProductAttributeMetaData>().GetSingle(c => c.AttributeCode == code);

                  product.ProductAttributeMetaDatas.Add(attr);
                }
              }
            }
            unit.Save();
            CopyArticleInformationToTnt(assortment.FileName);
          }
        }
      }
    }

    private void CopyArticleInformationToTnt(string fileNameToCopy)
    {
      try
      {
        Vendor vendor;

        using (var unit = GetUnitOfWork())
        {
          vendor = unit.Scope.Repository<Vendor>().GetSingle(x => x.Name.Equals("Sapph"));
        }

        var ftpAddress = vendor.VendorSettings.GetValueByKey("FtpAddress", string.Empty);
        var ftpUsername = HttpUtility.UrlEncode(vendor.VendorSettings.GetValueByKey("FtpUsername", string.Empty));
        var ftpPassword = HttpUtility.UrlEncode(vendor.VendorSettings.GetValueByKey("FtpPassword", string.Empty));
        var ftpPath = vendor.VendorSettings.GetValueByKey("Ax Ftp Dir ArticleInformation", string.Empty);
        var ftpUri = string.Format("ftp://{0}:{1}@{2}/{3}", ftpUsername, ftpPassword, ftpAddress, ftpPath);

        var tntFtpSetting = vendor.VendorSettings.GetValueByKey("TNTDestinationURI", string.Empty);

        var archiveDirectoryPath = vendor.VendorSettings.GetValueByKey("Archive Directory", string.Empty);

        var fileNameRegex = new Regex(".*\\.xml$", RegexOptions.IgnoreCase);

        var axaptaFtpManager = new FtpManager(ftpUri, null, false, usePassive: true);
        var tnTftpManager = new FtpManager(tntFtpSetting, log, usePassive: true);

        foreach (var articleInformatioFile in axaptaFtpManager.GetFiles())
        {
          var fileName = Path.GetFileName(articleInformatioFile);

          if (!fileNameRegex.IsMatch(articleInformatioFile)) continue;
          if (fileName != null && !fileName.Equals(Path.GetFileName(fileNameToCopy))) continue;

          try
          {
            log.InfoFormat("Processing file {0}", articleInformatioFile);

            using (var stream = axaptaFtpManager.Download(fileName))
            {
              tnTftpManager.Upload(stream, fileName);

              var archiveDirectory = SetArchiveDirectoryPath(archiveDirectoryPath, ftpPath);

              SaveStreamToFile(Path.Combine(archiveDirectory, fileName), stream);

              axaptaFtpManager.Delete(fileName);
            }
            return;
          }
          catch (Exception e)
          {
            log.ErrorFormat("Could not process file {0}! Error: {1}", articleInformatioFile, e.Message);
          }
        }
      }
      catch (Exception e)
      {
        log.AuditError("Sapph passthrough product information to TNT Failed", e);
      }
    }

    private static void SaveStreamToFile(string fileFullPath, Stream stream)
    {
      if (stream.Length == 0) return;
      using (var fileStream = File.Create(fileFullPath, (int)stream.Length))
      {
        var bytesInStream = new byte[stream.Length];
        stream.Read(bytesInStream, 0, bytesInStream.Length);

        fileStream.Write(bytesInStream, 0, bytesInStream.Length);
      }
    }

    private string SetArchiveDirectoryPath(string archiveDirectoryPath, string directory)
    {
      var directoryPathName = Path.Combine(
        archiveDirectoryPath,
        String.Format("{0:yyyy-MM}", DateTime.Now),
        directory,
        String.Format("{0:dd}", DateTime.Now));

      if (!Directory.Exists(directoryPathName))
      {
        Directory.CreateDirectory(directoryPathName);
      }

      return directoryPathName;
    }
  }
}
