using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Vendors;
using Concentrator.Plugins.AtomBlock.AtomBlockRetailService;
using System.Security.Cryptography;
using Concentrator.Objects.Vendors.Bulk;
using System.Globalization;
using Concentrator.Objects.ConcentratorService;
using Concentrator.Objects.Models.Attributes;
using Concentrator.Objects.DataAccess.UnitOfWork;

namespace Concentrator.Plugins.AtomBlock
{
  public class ProductImport : VendorBase
  {
    protected override int VendorID
    {
      get { return Int32.Parse(Config.AppSettings.Settings["VendorID"].Value); }
    }

    protected override int DefaultVendorID
    {
      get { return Int32.Parse(Config.AppSettings.Settings["DefaultVendorID"].Value); }
    }

    protected override System.Configuration.Configuration Config
    {
      get { return GetConfiguration(); }
    }


    private List<string> AttributeMapping = new List<string> { 
      "DownloadSize", 
      "ReleaseDate", 
      "Genre", 
      "DrmType", 
      "ContainsViolenceField", 
      "ContainsGamblingField", 
      "ContainsDrugsField", 
      "ContainsDiscriminationField", 
      "ContainsProFoundLanguageField",
      "ContainsSexField", 
      "ContainsFearField"
    };

    public override string Name
    {
      get { return "AtomBlock product import"; }
    }
    List<ProductAttributeMetaData> attributes;

    public Dictionary<string, int> attributeList = new Dictionary<string, int>();

    protected override void SyncProducts()
    {
      var Username = Config.AppSettings.Settings["Username"].Value;
      var Secret = Config.AppSettings.Settings["Secret"].Value;

      RetailServices10SoapClient client = new RetailServices10SoapClient();

      RetailAccount account = new RetailAccount();

      var inputString = Username + Secret;

      MD5CryptoServiceProvider MD5 = new MD5CryptoServiceProvider();
      byte[] byteValue = System.Text.Encoding.UTF8.GetBytes(inputString);
      byte[] byteHash = MD5.ComputeHash(byteValue);
      MD5.Clear();

      account.Client = Username;
      account.SecureHash = Convert.ToBase64String(byteHash);

      var response = client.Ping(account);

      if (response != null)
      {
        var languages = client.GetLanguages(account);

        var productNameList = client.GetProducts(account, new RangeRequest());

        //services are active
        //get products list
        var productsList = new Dictionary<ShopProduct, ShopLanguage>();
        foreach (var lang in languages.Items)
        {
          foreach (var item in productNameList.Items)
          {
            var prod = client.GetProduct(account, item.ProductIdentifier, lang);
            productsList.Add(prod, lang);
          }
        }

        ProcessData(log, productsList);
      }

    }

    public void ProcessData(AuditLog4Net.Adapter.IAuditLogAdapter log, Dictionary<ShopProduct, ShopLanguage> prods)
    {
      using (var unit = GetUnitOfWork())
      {

        var itemProducts = (from a in prods.Keys
                            select new AtomBlockProduct
                            {
                              ProductName = a.Title,
                              AllowedForSale = a.AllowedForSale,
                              Currency = a.Pricing.Currency,
                              Price = a.Pricing.Advise,
                              CostPrice = a.Pricing.Purchase,
                              IsActive = a.IsActive,
                              VendorItemNumber = a.ArticleNumber,
                              ProductID = a.Identifier,
                              ProductGroupcode1 = a.Genre,
                              ProductGroupCodes = a.Pegi,
                              Barcode = a.EAN,
                              Language = prods[a].Name,
                              BrandName = a.Publisher,
                              Description = a.Description ?? string.Empty,
                              Docs = a.Documents,
                              ReleaseDate = a.ReleaseDate,
                              Attributes = new AtomBlockProductAttribute
                              {
                                DownloadSize = a.DownloadSize,
                                ReleaseDate = a.ReleaseDate,
                                Genre = a.Genre.Name,
                                Pegi = a.Pegi,
                                SystemRequirements = a.SystemRequirements,
                                DrmType = a.DrmType.Name
                              },
                              ShortDescription = a.Punchline
                            }).ToList();


        SetupAttributes(unit, AttributeMapping.ToArray(), out attributes, VendorID);

        //Used for VendorImportAttributeValues
        var productAttributes = unit.Scope.Repository<ProductAttributeMetaData>().GetAll(c => c.VendorID == VendorID).ToList();
        attributeList = productAttributes.ToDictionary(x => x.AttributeCode, y => y.AttributeID);

        List<Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorAssortmentItem> assortmentList = new List<Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorAssortmentItem>();

        int counter = 0;
        int total = itemProducts.Count();
        int totalNumberOfProductsToProcess = total;
        log.InfoFormat("Start processing {0} products", total);


        foreach (var product in itemProducts)
        {
          var languageID = 0;

          switch (product.Language)
          {
            case "nl-NL":
              languageID = 2;
              break;
            case "en-GB":
              languageID = 1;
              break;
            default:
              continue;

          }

          if (counter == 50)
          {
            counter = 0;
            log.InfoFormat("Still need to process {0} of {1}; {2} done;", totalNumberOfProductsToProcess, total, total - totalNumberOfProductsToProcess);
          }
          totalNumberOfProductsToProcess--;
          counter++;

          var assortment = new Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorAssortmentItem
          {
            #region BrandVendor
            BrandVendors = new List<VendorAssortmentBulk.VendorImportBrand>()
                        {
                            new VendorAssortmentBulk.VendorImportBrand()
                            {
                                VendorID = VendorID,
                                VendorBrandCode = product.BrandName.Trim(),
                                ParentBrandCode = null,
                                Name = product.BrandName.Trim() 
                            }
                        },
            #endregion

            #region GeneralProductInfo
            VendorProduct = new VendorAssortmentBulk.VendorProduct
            {
              VendorItemNumber = product.ProductID.Trim(), //EAN
              CustomItemNumber = product.ProductID.Trim(), //EAN
              ShortDescription = product.Description != null ? product.Description.Length > 150
                                      ? product.Description.Substring(0, 150)
                                      : product.Description : string.Empty,
              LongDescription = product.Description != null ? product.Description.Length > 1000
                                      ? product.Description.Substring(0, 1000)
                                      : product.Description : string.Empty,
              LineType = null,
              LedgerClass = null,
              ProductDesk = null,
              ExtendedCatalog = null,
              VendorID = VendorID,
              DefaultVendorID = DefaultVendorID,
              VendorBrandCode = product.BrandName.Trim(), //UITGEVER_ID
              Barcode = product.Try(x => x.Barcode, string.Empty),//EAN
              VendorProductGroupCode1 = product.Try(x => x.ProductGroupcode1.Name, string.Empty),
              VendorProductGroupCodeName1 = product.Try(x => x.ProductGroupcode1.Name, string.Empty),

            },
            #endregion

            #region RelatedProducts
            RelatedProducts = new List<VendorAssortmentBulk.VendorImportRelatedProduct>()
                        {
                          //new Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportRelatedProduct
                          //{
                          //    VendorID = VendorID,
                          //    DefaultVendorID = DefaultVendorID,
                          //    CustomItemNumber = product.ProductID.Trim(), //EAN
                          //    RelatedProductType = string.Empty,
                          //    RelatedCustomItemNumber = string.Empty
                          //}
                        },
            #endregion

            #region Attributes

            VendorImportAttributeValues = GetProductAttributeValues(product, languageID, VendorID, unit),

            #endregion

            #region Prices
            VendorImportPrices = new List<Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportPrice>()
                        {
                            new Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportPrice()
                            {
                                VendorID = VendorID,
                                DefaultVendorID = DefaultVendorID,
                                CustomItemNumber = product.ProductID.Trim(), //EAN
                                Price = ((Decimal)product.Price / 100).ToString("0.00", CultureInfo.InvariantCulture) ,
                                CostPrice =  decimal.Round((product.CostPrice / 100), 4).ToString("0.00", CultureInfo.InvariantCulture), //NETTOPRIJS
                                TaxRate = "19", //TODO: Calculate this!
                                MinimumQuantity = 0,
                                CommercialStatus = product.AllowedForSale ? "AllowedForSale" : "NotAllowedForSale"
                            }
                        },
            #endregion

            #region Stock
            VendorImportStocks = new List<Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportStock>()
                        {
                            new Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportStock()
                            {
                                VendorID = VendorID,
                                DefaultVendorID = DefaultVendorID,
                                CustomItemNumber = product.ProductID.Trim(), //EAN
                                QuantityOnHand = 1,
                                StockType = "Assortment",
                                StockStatus =  product.ReleaseDate.HasValue ? product.ReleaseDate > DateTime.Now ? "Pre Release" : product.AllowedForSale ? "InStock" : "OutOfStock" : product.AllowedForSale ? "InStock" : "OutOfStock"
                            }
                        },
            #endregion

            #region Descriptions
            VendorProductDescriptions = new List<VendorAssortmentBulk.VendorProductDescription>()
            {
               new VendorAssortmentBulk.VendorProductDescription(){
                VendorID = VendorID,
                LanguageID =languageID,
                DefaultVendorID = VendorID,
                CustomItemNumber = product.ProductID.Trim(), //EAN
                LongContentDescription = product.Description,
                ShortContentDescription = product.Description != string.Empty ? product.Description.Length > 1000
                                     ? product.Description.Substring(0, 1000)
                                      : product.Description : product.ShortDescription,
                ProductName = product.ProductName
                            }
            }
            #endregion
          };

          // assortment will be added to the list defined outside of this loop
          assortmentList.Add(assortment);
        }

        // Creates a new instance of VendorAssortmentBulk(Passes in the AssortmentList defined above, vendorID and DefaultVendorID)
        using (var vendorAssortmentBulk = new VendorAssortmentBulk(assortmentList, VendorID, DefaultVendorID))
        {

          vendorAssortmentBulk.Init(unit.Context);
          vendorAssortmentBulk.Sync(unit.Context);
        }
      }
    }

    private List<VendorAssortmentBulk.VendorImportAttributeValue> GetProductAttributeValues(AtomBlockProduct product, int LanguageID, int VendorID, IUnitOfWork unit)
    {

      List<VendorAssortmentBulk.VendorImportAttributeValue> attributes = new List<VendorAssortmentBulk.VendorImportAttributeValue>();

      #region General

      var value = "";

      foreach (var att in AttributeMapping)
      {
        switch (att)
        {
          case "DownloadSize":
            value = product.Attributes.DownloadSize.ToString();
            break;
          case "ReleaseDate":
            value = product.Attributes.ReleaseDate.ToString();
            break;
          case "Genre":
            value = product.Attributes.Genre;
            break;
          case "DrmType":
            value = product.Attributes.DrmType;
            break;
        }

        var GeneralAttributes = new VendorAssortmentBulk.VendorImportAttributeValue
                                                       {
                                                         VendorID = VendorID,
                                                         DefaultVendorID = VendorID,
                                                         CustomItemNumber = product.ProductID.Trim(), //EAN
                                                         AttributeID = attributeList.ContainsKey(att) ? attributeList[att] : AddNewAttibute(att, unit),
                                                         Value = string.IsNullOrEmpty(value) ? "" : value,
                                                         LanguageID = LanguageID.ToString(),
                                                         AttributeCode = att
                                                       };

        attributes.Add(GeneralAttributes);
      }

      #endregion

      #region SystemRequirements
      foreach (var element in product.Attributes.SystemRequirements)
      {
        foreach (var att in element.Items)
        {

          var SystemRequirementsAttributes = new VendorAssortmentBulk.VendorImportAttributeValue
                                                       {
                                                         VendorID = VendorID,
                                                         DefaultVendorID = VendorID,
                                                         CustomItemNumber = product.ProductID.Trim(), //EAN
                                                         AttributeID = attributeList.ContainsKey(att.Label) ? attributeList[att.Label] : AddNewAttibute(att.Label, unit),
                                                         Value = string.IsNullOrEmpty(att.Value) ? "" : att.Value,
                                                         LanguageID = LanguageID.ToString(),
                                                         AttributeCode = att.Label
                                                       };

          attributes.Add(SystemRequirementsAttributes);

        }
      }
      #endregion

      #region PEGI

      foreach (var prop in product.Attributes.Pegi.GetType().GetProperties())
      {

        if (prop.Name != "ExtensionData")
        {
          var propval = "";
          try
          {
            propval = prop.GetValue(product.Attributes.Pegi, null).ToString();
          }
          catch (Exception ex)
          {
            continue;
          }


          var PEGIAttributes = new VendorAssortmentBulk.VendorImportAttributeValue
          {
            VendorID = VendorID,
            DefaultVendorID = VendorID,
            CustomItemNumber = product.ProductID.Trim(), //EAN
            AttributeID = attributeList.ContainsKey(prop.Name) ? attributeList[prop.Name] : AddNewAttibute(prop.Name, unit),
            Value = string.IsNullOrEmpty(propval) ? "" : propval,
            LanguageID = LanguageID.ToString(),
            AttributeCode = prop.Name
          };

          attributes.Add(PEGIAttributes);
        }
      }
      #endregion

      return attributes;
    }

    private int AddNewAttibute(string attr, IUnitOfWork unit)
    {
      AttributeMapping.Add(attr);
      SetupAttributes(unit, AttributeMapping.ToArray(), out attributes, VendorID);
      return attributes.First(x => x.AttributeCode == attr).AttributeID;
    }
  }

  class AtomBlockProduct
  {
    public string ProductName;
    public bool AllowedForSale;
    public string Currency;
    public int Price;
    public int CostPrice;
    public bool IsActive;
    public string VendorItemNumber;
    public string ProductID;
    public ShopGenre ProductGroupcode1;
    public ShopPegi ProductGroupCodes;
    public string Barcode;
    public string Language;
    public string BrandName;
    public string Description;
    public ShopDocument[] Docs;
    public DateTime? ReleaseDate;
    public AtomBlockProductAttribute Attributes;
    public string ShortDescription;



  }

  class AtomBlockProductAttribute
  {
    public int DownloadSize;
    public DateTime? ReleaseDate;
    public string Genre;
    public ShopPegi Pegi;
    public ShopSystemRequirement[] SystemRequirements;
    public string DrmType;

  }
}