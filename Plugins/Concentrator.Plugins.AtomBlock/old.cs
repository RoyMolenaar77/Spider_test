

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Concentrator.Objects.ConcentratorService;
//using Concentrator.Objects.Models.Vendors;
//using Concentrator.Objects.Models.Brands;
//using Concentrator.Objects.Models.Products;
//using Concentrator.Objects.Models.Attributes;
//using Concentrator.Objects.Models.Media;
//using AuditLog4Net.Adapter;
//using System.Security.Cryptography;
//using Concentrator.Plugins.AtomBlock.AtomBlockRetailService;
//using Concentrator.Objects.DataAccess.UnitOfWork;
//using Concentrator.Objects.Utility;
//using Concentrator.Objects.Enumerations;

//namespace Concentrator.Plugins.AtomBlock
//{
//  public class ProductIsdgmport : ConcentratorPlugin
//  {
//    public override string Name
//    {
//      get { return "AtomBlock Product Import"; }
//    }


//    private string[] AttributeMapping = new[] { "DownloadSize", "ReleaseDate", "Genre", "DrmType" };
//    // private string[] FilterAttributes = new[] { "RatingAge", "SubGenreName", "GenreName", "SubProductGroup" };
//    private const int unmappedID = -1;

//    protected override void Process()
//    {
//      var config = GetConfiguration();


//      var VendorID = Int32.Parse(config.AppSettings.Settings["VendorID"].Value);
//      var Username = config.AppSettings.Settings["Username"].Value;
//      var Secret = config.AppSettings.Settings["Secret"].Value;

//      RetailServices10SoapClient client = new RetailServices10SoapClient();

//      RetailAccount account = new RetailAccount();

//      var inputString = Username + Secret;

//      MD5CryptoServiceProvider MD5 = new MD5CryptoServiceProvider();
//      byte[] byteValue = System.Text.Encoding.UTF8.GetBytes(inputString);
//      byte[] byteHash = MD5.ComputeHash(byteValue);
//      MD5.Clear();

//      account.Client = Username;
//      account.SecureHash = Convert.ToBase64String(byteHash);

//      var response = client.Ping(account);

//      if (response != null)
//      {
//        var languages = client.GetLanguages(account);

//        var productNameList = client.GetProducts(account, new RangeRequest());

//        //services are active
//        //get products list
//        var productsList = new Dictionary<ShopProduct, ShopLanguage>();
//        foreach (var lang in languages.Items)
//        {
//          foreach (var item in productNameList.Items)
//          {
//            var prod = client.GetProduct(account, item.ProductIdentifier, lang);
//            productsList.Add(prod, lang);
//          }
//        }

//        ProcessProducts(log, productsList);
//      }

//    }

//    private void ProcessProducts(IAuditLogAdapter log, Dictionary<ShopProduct, ShopLanguage> prods)
//    {
//      var itemProducts = (from a in prods.Keys
//                          select new
//                          {
//                            ProductName = a.Title,
//                            AllowedForSale = a.AllowedForSale,
//                            Currency = a.Pricing.Currency,
//                            Price = a.Pricing.Advise,
//                            CostPrice = a.Pricing.Purchase,
//                            IsActive = a.IsActive,
//                            VendorItemNumber = a.ArticleNumber,
//                            ProductID = a.Identifier,
//                            ProductGroupcode1 = a.Genre,
//                            ProductGroupCodes = a.Pegi,
//                            Barcode = a.EAN,
//                            Language = prods[a].Name,
//                            BrandName = a.Publisher,
//                            Description = a.Description,
//                            Docs = a.Documents,
//                            ReleaseDate = a.ReleaseDate,
//                            Attributes = new AtomBlockProductAttribute
//                            {
//                              DownloadSize = a.DownloadSize,
//                              ReleaseDate = a.ReleaseDate,
//                              Genre = a.Genre.Name,
//                              Pegi = a.Pegi,
//                              SystemRequirements = a.SystemRequirements,
//                              DrmType = a.DrmType.Name
//                            },
//                            ShortDescription = a.Punchline
//                          }).ToList();

//      using (var unit = GetUnitOfWork())
//      {
//        var config = GetConfiguration();

//        var VendorID = int.Parse(config.AppSettings.Settings["VendorID"].Value);

//        var vendor = unit.Scope.Repository<Vendor>().GetSingle(x => x.VendorID == VendorID);

//        var _vendorRepo = unit.Scope.Repository<Vendor>();
//        var _brandRepo = unit.Scope.Repository<Brand>();
//        var _brandVendorRepo = unit.Scope.Repository<BrandVendor>();
//        var _productRepo = unit.Scope.Repository<Product>();
//        var _assortmentRepo = unit.Scope.Repository<VendorAssortment>();
//        var _productGroupVendorRepo = unit.Scope.Repository<ProductGroupVendor>();
//        var _prodDescriptionRepo = unit.Scope.Repository<ProductDescription>();
//        var _attrGroupRepo = unit.Scope.Repository<ProductAttributeGroupMetaData>();
//        var _attrGroupName = unit.Scope.Repository<ProductAttributeGroupName>();
//        var _attrRepo = unit.Scope.Repository<ProductAttributeMetaData>();
//        var _attrNameRepo = unit.Scope.Repository<ProductAttributeName>();
//        var _attrValueRepo = unit.Scope.Repository<ProductAttributeValue>();
//        var _mediaRepo = unit.Scope.Repository<ProductMedia>();
//        var _mediaTypeRepo = unit.Scope.Repository<MediaType>();
//        var _priceRepo = unit.Scope.Repository<VendorPrice>();
//        var _stockRepo = unit.Scope.Repository<VendorStock>();
//        var _barcodeRepo = unit.Scope.Repository<ProductBarcode>();
//        var _relatedProductRepo = unit.Scope.Repository<RelatedProduct>();

//        if (vendor != null)
//        {
//          ProductStatusVendorMapper mapper = new ProductStatusVendorMapper(unit.Scope.Repository<VendorProductStatus>(), vendor.VendorID);

//          var brandVendors = _brandVendorRepo.GetAll(b => (b.VendorID == vendor.VendorID || (vendor.ParentVendorID.HasValue && b.VendorID == vendor.ParentVendorID))).ToList();

//          var currentVendorProductGroups = _productGroupVendorRepo.GetAll(v => v.VendorID == vendor.VendorID).ToList();

//          var productGroupVendors = _productGroupVendorRepo.GetAll(g => (g.VendorID == vendor.VendorID || (vendor.ParentVendorID.HasValue && g.VendorID == vendor.VendorID))).ToList();

//          var products = _productRepo.GetAll(x => x.SourceVendorID == VendorID).ToList();
//          var vendorStock = _stockRepo.GetAll(x => x.VendorID == vendor.VendorID).ToList();

//          var vendorassortments = _assortmentRepo.GetAll(x => x.VendorID == vendor.VendorID).ToList();
//          //var medias = _mediaRepo.GetAll().ToList();
//          var productDescriptions = _prodDescriptionRepo.GetAll(x => x.VendorID == VendorID).ToList();
//          var productAttributes = _attrRepo.GetAll(g => g.VendorID == VendorID).ToList();
//          var productAttributeGroups = _attrGroupRepo.GetAll(g => g.VendorID == VendorID).ToList();

//          var vendorID = vendor.VendorID;

//          var itemcount = itemProducts.Where(x => x.VendorItemNumber != "");

//          int counter = 0;
//          int total = itemProducts.Count();
//          int totalNumberOfProductsToProcess = total;
//          log.InfoFormat("Start import {0} products", total);


//          foreach (var product in itemProducts)
//          {
//            var languageID = 0;

//            switch (product.Language)
//            {
//              case "nl-NL":
//                languageID = 2;
//                break;
//              case "en-GB":
//                languageID = 1;
//                break;
//              default:
//                continue;

//            }

//            try
//            {
//              if (counter == 50)
//              {
//                counter = 0;
//                log.InfoFormat("Still need to process {0} of {1}; {2} done;", totalNumberOfProductsToProcess, total, total - totalNumberOfProductsToProcess);
//              }
//              totalNumberOfProductsToProcess--;
//              counter++;

//              #region Brand
//              var vbrand = brandVendors.FirstOrDefault(vb => vb.VendorBrandCode == product.BrandName);

//              if (vbrand == null)
//              {
//                vbrand = new BrandVendor
//                {
//                  VendorID = VendorID,
//                  VendorBrandCode = product.BrandName,
//                  BrandID = unmappedID
//                };

//                brandVendors.Add(vbrand);

//                _brandVendorRepo.Add(vbrand);
//              }

//              vbrand.Name = product.BrandName;
//              #endregion

//              #region Product

//              var item = _productRepo.GetSingle(p => p.VendorItemNumber == product.ProductID && p.BrandID == vbrand.BrandID && p.SourceVendorID == VendorID);

//              if (item == null)
//              {
//                item = new Product
//                         {
//                           VendorItemNumber = product.ProductID,
//                           BrandID = vbrand.BrandID,
//                           SourceVendorID = VendorID
//                         };

//                _productRepo.Add(item);
//                unit.Save();
//              }

//              #endregion Product

//              #region Vendor assortment

//              VendorAssortment assortment = vendorassortments.FirstOrDefault(x => x.ProductID == item.ProductID);

//              if (assortment == null)
//              {
//                assortment = new VendorAssortment
//                               {
//                                 VendorID = VendorID,
//                                 Product = item
//                               };
//                _assortmentRepo.Add(assortment);
//                vendorassortments.Add(assortment);
//              }

//              if (product.ShortDescription != null && product.Description != null)
//              {
//                assortment.ShortDescription = product.Description.Length > 150
//                                      ? product.Description.Substring(0, 150)
//                                      : product.Description;
//              }
//              else
//              {
//                assortment.ShortDescription = product.ProductName.Length > 150 ? product.ProductName.Substring(0, 150) : product.ProductName;
//              }
//              assortment.CustomItemNumber = product.ProductID;
//              assortment.IsActive = true;

//              #endregion

//              #region Product Description

//              if (item.ProductDescriptions == null) item.ProductDescriptions = new List<ProductDescription>();

//              var productDescription =
//                item.ProductDescriptions.FirstOrDefault(pd => pd.LanguageID == languageID && pd.VendorID == vendorID);

//              if (productDescription == null)
//              {
//                //create ProductDescription
//                productDescription = new ProductDescription
//                {
//                  Product = item,
//                  LanguageID = languageID,
//                  VendorID = vendorID,

//                };
//                productDescription.ProductName = product.ProductName;
//                _prodDescriptionRepo.Add(productDescription);
//              }
//              productDescription.LongContentDescription = product.Description;
//              #endregion

//              #region Product group
//              #region Genre

//              var group1 =
//               productGroupVendors.FirstOrDefault(x => x.VendorProductGroupCode1 == product.ProductGroupcode1.Name);
//              if (group1 == null)
//              {
//                group1 = new ProductGroupVendor()
//                          {
//                            VendorID = VendorID,
//                            VendorProductGroupCode1 = product.ProductGroupcode1.Name,
//                            VendorName = product.ProductGroupcode1.Name,
//                            ProductGroupID = -1
//                          };
//                _productGroupVendorRepo.Add(group1);
//                productGroupVendors.Add(group1);

//              }

//              #region sync
//              if (currentVendorProductGroups.Contains(group1))
//              {
//                currentVendorProductGroups.Remove(group1);
//              }
//              #endregion


//              #endregion Product group

//              #region Vendor Product Group Assortment


//              string brandCode = null;
//              string groupCode1 = product.ProductGroupcode1.Name;


//              var records = (from l in productGroupVendors
//                             where
//                               ((brandCode != null && l.BrandCode.Trim() == brandCode) || l.BrandCode == null)
//                               &&
//                               ((groupCode1 != null && l.VendorProductGroupCode1 != null &&
//                                 l.VendorProductGroupCode1.Trim() == groupCode1) || l.VendorProductGroupCode1 == null)
//                             select l).ToList();


//              List<int> existingProductGroupVendors = new List<int>();

//              foreach (ProductGroupVendor prodGroupVendor in records)
//              {
//                existingProductGroupVendors.Add(prodGroupVendor.ProductGroupVendorID);

//                if (prodGroupVendor.VendorAssortments == null) prodGroupVendor.VendorAssortments = new List<VendorAssortment>();

//                if (prodGroupVendor.VendorAssortments.Any(x => x.VendorAssortmentID == assortment.VendorAssortmentID))
//                { // only add new rows
//                  continue;
//                }

//                prodGroupVendor.VendorAssortments.Add(assortment);
//                //ctx.VendorProductGroupAssortments.InsertOnSubmit(vpga);
//              }
//              #endregion

//              try
//              {
//                foreach (var prop in product.ProductGroupCodes.GetType().GetProperties())
//                {
//                  //var propval = prop.GetType().GetProperty(prop.Name).GetValue(prop, null);
//                  if (prop.Name != "Age" && prop.Name != "ExtensionData")
//                  {

//                    var val = (bool)prop.GetValue(product.ProductGroupCodes, null);

//                    //TODO check productgroup language
//                    var group =
//                      productGroupVendors.FirstOrDefault(x => x.VendorProductGroupCode2 == prop.Name);
//                    if (group == null)
//                    {
//                      group = new ProductGroupVendor()
//                                {
//                                  VendorID = VendorID,
//                                  VendorProductGroupCode2 = prop.Name,
//                                  VendorName = prop.Name,
//                                  ProductGroupID = -1
//                                };
//                      _productGroupVendorRepo.Add(group);
//                      productGroupVendors.Add(group);
//                      unit.Save();
//                    }
//                    #region sync
//                    if (currentVendorProductGroups.Contains(group))
//                    {
//                      currentVendorProductGroups.Remove(group);
//                    }
//                    #endregion

//              #endregion Product group

//                    #region Vendor Product Group Assortment

//                    brandCode = null;
//                    string groupCode2 = prop.Name;


//                    records = (from l in productGroupVendors
//                               where
//                                 ((brandCode != null && l.BrandCode.Trim() == brandCode) || l.BrandCode == null)
//                                 &&
//                                 ((groupCode1 != null && l.VendorProductGroupCode1 != null &&
//                                   l.VendorProductGroupCode1.Trim() == groupCode1) || l.VendorProductGroupCode1 == null)
//                                 &&
//                                 ((groupCode2 != null && l.VendorProductGroupCode2 != null &&
//                                   l.VendorProductGroupCode2.Trim() == groupCode2) || l.VendorProductGroupCode2 == null)

//                               select l).ToList();


//                    existingProductGroupVendors = new List<int>();

//                    foreach (ProductGroupVendor prodGroupVendor in records)
//                    {
//                      existingProductGroupVendors.Add(prodGroupVendor.ProductGroupVendorID);

//                      if (prodGroupVendor.VendorAssortments == null) prodGroupVendor.VendorAssortments = new List<VendorAssortment>();

//                      if (prodGroupVendor.VendorAssortments.Any(x => x.VendorAssortmentID == assortment.VendorAssortmentID))
//                      { // only add new rows
//                        continue;
//                      }

//                      prodGroupVendor.VendorAssortments.Add(assortment);
//                      //ctx.VendorProductGroupAssortments.InsertOnSubmit(vpga);
//                    }
//                  }
//                }
//              }
//              catch (Exception ex)
//              {

//              }
//                    #endregion

//              #region Stock

//              var stock = vendorStock.FirstOrDefault(c => c.VendorID == assortment.VendorID && c.ProductID == assortment.ProductID);
//              if (stock == null)
//              {
//                stock = new VendorStock
//                {
//                  QuantityOnHand = 1,
//                  VendorID = VendorID,
//                  Product = item,
//                  VendorStockTypeID = 1
//                };
//                vendorStock.Add(stock);
//                _stockRepo.Add(stock);
//              }

//              var status = product.ReleaseDate.HasValue ? product.ReleaseDate > DateTime.Now ? "Pre Release" : product.AllowedForSale ? "InStock" : "OutOfStock" : product.AllowedForSale ? "InStock" : "OutOfStock";

//              stock.StockStatus = status;
//              stock.VendorStatus = status;
//              stock.ConcentratorStatusID = mapper.SyncVendorStatus(status, -1);

//              #endregion Stock

//              #region Price

//              if (assortment.VendorPrices == null) assortment.VendorPrices = new List<VendorPrice>();
//              var price = assortment.VendorPrices.FirstOrDefault();
//              if (price == null)
//              {
//                price = new VendorPrice
//                {
//                  VendorAssortment = assortment,
//                  MinimumQuantity = 0
//                };

//                _priceRepo.Add(price);
//              }

//              price.CommercialStatus = product.AllowedForSale ? "AllowedForSale" : "NotAllowedForSale";
//              price.ConcentratorStatusID = stock.ConcentratorStatusID;
//              price.BasePrice = (decimal)product.Price / 100;
//              price.BaseCostPrice = decimal.Round((product.CostPrice / 100), 4);


//              #endregion Price

//              #region Barcode

//              if (!String.IsNullOrEmpty(product.Barcode))
//              {
//                if (item.ProductBarcodes == null) item.ProductBarcodes = new List<ProductBarcode>();
//                if (!item.ProductBarcodes.Any(pb => pb.Barcode.Trim() == product.Barcode))
//                {
//                  unit.Scope.Repository<ProductBarcode>().Add(new ProductBarcode
//                                                        {
//                                                          Product = item,
//                                                          Barcode = product.Barcode,
//                                                          VendorID = VendorID,
//                                                          BarcodeType = (int)BarcodeTypes.Default
//                                                        });
//                }
//              }

//              #endregion Barcode

//              #region Attributes

//              #region General

//              #region ProductAttributeGroupMetaData


//              //TODO CHECK LANGUAGE 

//              var value = "";


//              var productAttributeGroupMetaData = productAttributeGroups.FirstOrDefault();
//              //create ProductAttributeGroupMetaData if not exists
//              if (productAttributeGroupMetaData == null)
//              {
//                productAttributeGroupMetaData = new ProductAttributeGroupMetaData
//                {
//                  Index = 0,
//                  VendorID = vendorID
//                };
//                _attrGroupRepo.Add(productAttributeGroupMetaData);
//                productAttributeGroups.Add(productAttributeGroupMetaData);
//              }
//              #endregion

//              #region ProductAttributeGroupName
//              if (productAttributeGroupMetaData.ProductAttributeGroupNames == null) productAttributeGroupMetaData.ProductAttributeGroupNames = new List<ProductAttributeGroupName>();
//              var productAttributeGroupName =
//                productAttributeGroupMetaData.ProductAttributeGroupNames.FirstOrDefault(c => c.LanguageID == languageID);
//              //create ProductAttributeGroupName if not exists
//              if (productAttributeGroupName == null)
//              {
//                productAttributeGroupName = new ProductAttributeGroupName
//                {
//                  Name = "General",
//                  ProductAttributeGroupMetaData = productAttributeGroupMetaData,
//                  LanguageID = languageID
//                };
//                _attrGroupName.Add(productAttributeGroupName);
//              }

//              #endregion

//              #region ProductAttributeMetaData
//              foreach (var att in AttributeMapping)
//              {


//                switch (att)
//                {
//                  case "DownloadSize":
//                    value = product.Attributes.DownloadSize.ToString();
//                    break;
//                  case "ReleaseDate":
//                    value = product.Attributes.ReleaseDate.ToString();
//                    break;
//                  case "Genre":
//                    value = product.Attributes.Genre;
//                    break;
//                  case "DrmType":
//                    value = product.Attributes.DrmType;
//                    break;
//                }

//                var productAttributeMetaData =
//                          productAttributes.FirstOrDefault(c => c.AttributeCode == att);
//                //create ProductAttributeMetaData if not exists
//                if (productAttributeMetaData == null)
//                {
//                  productAttributeMetaData = new ProductAttributeMetaData
//                  {
//                    ProductAttributeGroupMetaData = productAttributeGroupMetaData,
//                    AttributeCode = att,
//                    Index = 0,
//                    IsVisible = true,
//                    NeedsUpdate = true,
//                    VendorID = vendorID,
//                    IsSearchable = att == "Genre" ? true : false
//                  };
//                  _attrRepo.Add(productAttributeMetaData);
//                  productAttributes.Add(productAttributeMetaData);
//                }

//              #endregion

//                #region ProductAttributeName
//                if (productAttributeMetaData.ProductAttributeNames == null) productAttributeMetaData.ProductAttributeNames = new List<ProductAttributeName>();
//                var productAttributeName =
//                  productAttributeMetaData.ProductAttributeNames.FirstOrDefault(c => c.LanguageID == languageID);
//                //create ProductAttributeName with generated productAttributeMetaData.AttributeID if not exists
//                if (productAttributeName == null)
//                {
//                  productAttributeName = new ProductAttributeName
//                  {
//                    ProductAttributeMetaData = productAttributeMetaData,
//                    LanguageID = languageID,
//                    Name = att
//                  };
//                  _attrNameRepo.Add(productAttributeName);
//                }

//                #endregion

//                #region ProductAttributeValue

//                if (productAttributeMetaData.ProductAttributeValues == null) productAttributeMetaData.ProductAttributeValues = new List<ProductAttributeValue>();
//                var productAttributeValue =
//                  productAttributeMetaData.ProductAttributeValues.FirstOrDefault(c => c.ProductID == item.ProductID);
//                //create ProductAttributeValue with generated productAttributeMetaData.AttributeID
//                if (productAttributeValue == null)
//                {
//                  productAttributeValue = new ProductAttributeValue
//                  {
//                    ProductAttributeMetaData = productAttributeMetaData,
//                    Product = item,
//                    Value = value == null ? "" : value,
//                    LanguageID = languageID
//                  };
//                  _attrValueRepo.Add(productAttributeValue);
//                }
//              }
//                #endregion

//              #endregion

//              #region SystemRequirements
//              foreach (var element in product.Attributes.SystemRequirements)
//              {

//                #region ProductAttributeGroupMetaData
//                productAttributeGroupMetaData = productAttributeGroups.FirstOrDefault(x => x.GroupCode == element.Type.Name);
//                //create ProductAttributeGroupMetaData if not exists
//                if (productAttributeGroupMetaData == null)
//                {
//                  productAttributeGroupMetaData = new ProductAttributeGroupMetaData
//                  {
//                    Index = 0,
//                    VendorID = vendorID,
//                    GroupCode = element.Type.Name
//                  };
//                  _attrGroupRepo.Add(productAttributeGroupMetaData);
//                  productAttributeGroups.Add(productAttributeGroupMetaData);
//                }
//                #endregion

//                #region ProductAttributeGroupName
//                if (productAttributeGroupMetaData.ProductAttributeGroupNames == null) productAttributeGroupMetaData.ProductAttributeGroupNames = new List<ProductAttributeGroupName>();
//                productAttributeGroupName =
//                  productAttributeGroupMetaData.ProductAttributeGroupNames.FirstOrDefault(c => c.LanguageID == languageID);
//                //create ProductAttributeGroupName if not exists
//                if (productAttributeGroupName == null)
//                {
//                  productAttributeGroupName = new ProductAttributeGroupName
//                  {
//                    Name = element.Type.Name,
//                    ProductAttributeGroupMetaData = productAttributeGroupMetaData,
//                    LanguageID = languageID
//                  };
//                  _attrGroupName.Add(productAttributeGroupName);

//                }
//                foreach (var att in element.Items)
//                {
//                  var productAttributeMetaData =
//                     productAttributes.FirstOrDefault(c => c.AttributeCode == att.Label);
//                  //create ProductAttributeMetaData if not exists
//                  if (productAttributeMetaData == null)
//                  {
//                    productAttributeMetaData = new ProductAttributeMetaData
//                    {
//                      ProductAttributeGroupMetaData = productAttributeGroupMetaData,
//                      AttributeCode = att.Label,
//                      Index = 0,
//                      IsVisible = true,
//                      NeedsUpdate = true,
//                      VendorID = vendorID,
//                      IsSearchable = false
//                    };
//                    _attrRepo.Add(productAttributeMetaData);
//                    productAttributes.Add(productAttributeMetaData);
//                  }

//                #endregion

//                  #region ProductAttributeName
//                  if (productAttributeMetaData.ProductAttributeNames == null) productAttributeMetaData.ProductAttributeNames = new List<ProductAttributeName>();
//                  var productAttributeName =
//                     productAttributeMetaData.ProductAttributeNames.FirstOrDefault(c => c.LanguageID == languageID);
//                  //create ProductAttributeName with generated productAttributeMetaData.AttributeID if not exists
//                  if (productAttributeName == null)
//                  {
//                    productAttributeName = new ProductAttributeName
//                    {
//                      ProductAttributeMetaData = productAttributeMetaData,
//                      LanguageID = languageID, //TODO: SET LANGUAGE
//                      Name = att.Label
//                    };
//                    _attrNameRepo.Add(productAttributeName);
//                  }

//                  #endregion

//                  #region ProductAttributeValue

//                  if (productAttributeMetaData.ProductAttributeValues == null) productAttributeMetaData.ProductAttributeValues = new List<ProductAttributeValue>();
//                  var productAttributeValue =
//                    productAttributeMetaData.ProductAttributeValues.FirstOrDefault(c => c.ProductID == item.ProductID);
//                  //create ProductAttributeValue with generated productAttributeMetaData.AttributeID
//                  if (productAttributeValue == null)
//                  {
//                    productAttributeValue = new ProductAttributeValue
//                    {
//                      ProductAttributeMetaData = productAttributeMetaData,
//                      Product = item,
//                      Value = att.Value == null ? "" : att.Value,
//                      LanguageID = languageID
//                    };
//                    _attrValueRepo.Add(productAttributeValue);
//                  }
//                }
//              }
//                  #endregion
//              #endregion

//              #region PEGI

//              #region ProductAttributeGroupMetaData
//              productAttributeGroupMetaData = productAttributeGroups.FirstOrDefault(x => x.GroupCode == "Pegi");
//              //create ProductAttributeGroupMetaData if not exists
//              if (productAttributeGroupMetaData == null)
//              {
//                productAttributeGroupMetaData = new ProductAttributeGroupMetaData
//                {
//                  Index = 0,
//                  VendorID = vendorID,
//                  GroupCode = "Pegi"
//                };
//                _attrGroupRepo.Add(productAttributeGroupMetaData);
//                productAttributeGroups.Add(productAttributeGroupMetaData);
//              }
//              #endregion

//              #region ProductAttributeGroupName
//              if (productAttributeGroupMetaData.ProductAttributeGroupNames == null) productAttributeGroupMetaData.ProductAttributeGroupNames = new List<ProductAttributeGroupName>();
//              productAttributeGroupName =
//                productAttributeGroupMetaData.ProductAttributeGroupNames.FirstOrDefault(c => c.LanguageID == languageID);
//              //create ProductAttributeGroupName if not exists
//              if (productAttributeGroupName == null)
//              {
//                productAttributeGroupName = new ProductAttributeGroupName
//                {
//                  Name = "Pegi",
//                  ProductAttributeGroupMetaData = productAttributeGroupMetaData,
//                  LanguageID = languageID
//                };
//                _attrGroupName.Add(productAttributeGroupName);

//              }
//              #endregion

//              #region ProductAttributeName
//              foreach (var prop in product.Attributes.Pegi.GetType().GetProperties())
//              {

//                if (prop.Name != "ExtensionData")
//                {
//                  var propval = "";
//                  try
//                  {
//                    propval = prop.GetValue(product.Attributes.Pegi, null).ToString();
//                  }
//                  catch (Exception ex)
//                  {
//                    continue;
//                  }
//                  var productAttributeMetaData =
//                     productAttributes.FirstOrDefault(c => c.AttributeCode == prop.Name);
//                  //create ProductAttributeMetaData if not exists
//                  if (productAttributeMetaData == null)
//                  {
//                    productAttributeMetaData = new ProductAttributeMetaData
//                    {
//                      ProductAttributeGroupMetaData = productAttributeGroupMetaData,
//                      AttributeCode = prop.Name,
//                      Index = 0,
//                      IsVisible = true,
//                      NeedsUpdate = true,
//                      VendorID = vendorID,
//                      IsSearchable = false
//                    };
//                    _attrRepo.Add(productAttributeMetaData);
//                    productAttributes.Add(productAttributeMetaData);
//                  }

//                  if (productAttributeMetaData.ProductAttributeNames == null) productAttributeMetaData.ProductAttributeNames = new List<ProductAttributeName>();
//                  var productAttributeName =
//                     productAttributeMetaData.ProductAttributeNames.FirstOrDefault(c => c.LanguageID == languageID);
//                  //create ProductAttributeName with generated productAttributeMetaData.AttributeID if not exists
//                  if (productAttributeName == null)
//                  {
//                    productAttributeName = new ProductAttributeName
//                    {
//                      ProductAttributeMetaData = productAttributeMetaData,
//                      LanguageID = languageID, //TODO: SET LANGUAGE
//                      Name = prop.Name
//                    };
//                    _attrNameRepo.Add(productAttributeName);
//                  }

//              #endregion

//                  #region ProductAttributeValue

//                  if (productAttributeMetaData.ProductAttributeValues == null) productAttributeMetaData.ProductAttributeValues = new List<ProductAttributeValue>();
//                  var productAttributeValue =
//                    productAttributeMetaData.ProductAttributeValues.FirstOrDefault(c => c.ProductID == item.ProductID);
//                  //create ProductAttributeValue with generated productAttributeMetaData.AttributeID
//                  if (productAttributeValue == null)
//                  {
//                    productAttributeValue = new ProductAttributeValue
//                    {
//                      ProductAttributeMetaData = productAttributeMetaData,
//                      Product = item,
//                      Value = propval == null ? "" : propval,
//                      LanguageID = languageID
//                    };
//                    _attrValueRepo.Add(productAttributeValue);
//                  }
//                }
//              }
//                  #endregion

//              #endregion

//              #endregion

//              #region Images

//              foreach (var doc in product.Docs)
//              {
//                if (item.ProductMedias == null) item.ProductMedias = new List<ProductMedia>();
//                if (!doc.IsImage)
//                {
//                  if (!item.ProductMedias.Any(pi => pi.VendorID == VendorID && pi.MediaUrl == doc.Location))
//                  {
//                    unit.Scope.Repository<ProductMedia>().Add(new ProductMedia
//                    {
//                      VendorID = VendorID,
//                      MediaUrl = doc.Location,
//                      TypeID = 4, //manuals docs or something
//                      Product = item,
//                      Sequence = 0
//                    });
//                  }
//                }
//                else
//                {
//                  if (!item.ProductMedias.Any(pi => pi.VendorID == VendorID && pi.MediaUrl == doc.Location))
//                  {
//                    unit.Scope.Repository<ProductMedia>().Add(new ProductMedia
//                      {
//                        VendorID = VendorID,
//                        MediaUrl = doc.Location,
//                        TypeID = 1, // image
//                        Product = item,
//                        Sequence = 0
//                      });
//                  }
//                }

//              #endregion
//              }
//            }
//            catch (Exception ex)
//            {

//            }
//          }
//        }
//        else
//        {
//          log.AuditFatal("Vendor For AtomBlock Not Available: create in database");
//        }
//        unit.Save();
//      }
//    }
//  }
//}