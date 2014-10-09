using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Data.Linq;
using System.Linq.Dynamic;
using System.Text;
using Concentrator.Objects;
using Concentrator.Objects.ConcentratorService;
using Excel;
using Concentrator.Objects.Utility;
using System.Web;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using Concentrator.Objects.Enumerations;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.Models.Brands;
using Concentrator.Objects.Models.Attributes;
using Concentrator.Objects.Models.Media;

namespace Concentrator.Plugins.SennHeiser
{
  class ProductImportWeb : ConcentratorPlugin
  {
    public override string Name
    {
      get { return "Sennheiser Web Product Import Plugin"; }
    }
    //private int vendorID_NL;
    private const int unmappedID = -1;
    private const int languageID = 1;
    private Dictionary<int, string> vendors = new Dictionary<int, string>();

    string Category = string.Empty;
    System.Configuration.Configuration config;

    protected override void Process()
    {
      config = GetConfiguration();

      vendors.Add(int.Parse(config.AppSettings.Settings["VendorID_NL"].Value), "NL");
      vendors.Add(int.Parse(config.AppSettings.Settings["VendorID_BE"].Value), "BE");

      bool success = ParseDocument();

      if (success)
      {
        log.DebugFormat("File successfully processed");
      }


    }

    private bool ParseDocument()
    {
      using (var unit = GetUnitOfWork())
      {
        var _vendorRepo = unit.Scope.Repository<Vendor>();
        var _brandRepo = unit.Scope.Repository<Brand>();
        var _brandVendorRepo = unit.Scope.Repository<BrandVendor>();
        var _productRepo = unit.Scope.Repository<Product>();
        var _assortmentRepo = unit.Scope.Repository<VendorAssortment>();
        var _productGroupVendorRepo = unit.Scope.Repository<ProductGroupVendor>();
        var _prodDescriptionRepo = unit.Scope.Repository<ProductDescription>();
        var _attrGroupRepo = unit.Scope.Repository<ProductAttributeGroupMetaData>();
        var _attrGroupName = unit.Scope.Repository<ProductAttributeGroupName>();
        var _attrRepo = unit.Scope.Repository<ProductAttributeMetaData>();
        var _attrNameRepo = unit.Scope.Repository<ProductAttributeName>();
        var _attrValueRepo = unit.Scope.Repository<ProductAttributeValue>();
        var _mediaRepo = unit.Scope.Repository<ProductMedia>();
        var _mediaTypeRepo = unit.Scope.Repository<MediaType>();
        var _priceRepo = unit.Scope.Repository<VendorPrice>();
        var _stockRepo = unit.Scope.Repository<VendorStock>();
        var _barcodeRepo = unit.Scope.Repository<ProductBarcode>();
        var _relatedProductRepo = unit.Scope.Repository<RelatedProduct>();
        var _relatedProductTypeRepo = unit.Scope.Repository<RelatedProductType>();

        bool somethingWentWrong = false;
        var currentItem = string.Empty;

        try
        {
          int defaultVendorID = vendors.First().Key;

          var vendor = _vendorRepo.GetSingle(bv => bv.VendorID == defaultVendorID);

          #region Werkbestand verwerking

          #region Xml Data

          var productXmlFilePath = config.AppSettings.Settings["SennheiserBasePath"].Value + "products.xml";
          var productXmlCatPath = config.AppSettings.Settings["SennheiserBasePath"].Value + "categories.xml";

          var xmlText = File.ReadAllText(productXmlFilePath);

          //var doc2 = HttpUtility.HtmlDecode(xmlText);

          XDocument doc = null;
          XDocument categories = null;

          try
          {
            xmlText = xmlText.Replace("<br>", " ");
            xmlText = xmlText.Replace("&M", "&amp;M");

            doc = XDocument.Parse(xmlText);
            categories = XDocument.Load(productXmlCatPath);
          }
          catch (Exception ex)
          {
            log.Error(String.Format("No new files to process or file not available"), ex);
            return false;
          }

          var Categories = (from cat in categories.Element("CATEGORYDATA").Elements("category")
                            select new
                            {
                              category = cat.Element("Category").Value,
                              path = cat.Element("Path").Value
                            }
                              );


          var importProducts = (from productData in doc.Element("PRODUCTDATA").Elements("product")
                                let td = productData.Element("TechnicalData").Elements()
                                let c =
                                  Categories.Where(
                                  x =>
                                  x.path ==
                                  "private_" + productData.Elements().Where(p => p.Name == "Path").FirstOrDefault().Value).
                                  FirstOrDefault()
                                select new SennHeiserItemImport
                                         {
                                           CustomItemNumber = productData.Attribute("name").Value,
                                           VendorItemNumber = productData.Attribute("no").Value.Trim(),
                                           ShortContentDescription =
                                  System.Web.HttpUtility.UrlDecode(productData.Element("ShortDescription").Value),
                                           LongContentDescription =
                                           System.Web.HttpUtility.UrlDecode(productData.Element("GeneralDescription").Value),
                                           Attributes = (from items in td
                                                         group items by items.Attribute("order").Value
                                                           into g
                                                           select new SennHeiserAttribute
                                                                    {
                                                                      AttributeCode =
                                                           System.Web.HttpUtility.UrlDecode(
                                                           td.Where(
                                                             x =>
                                                             x.Attribute("order").Value == g.Key &&
                                                             x.Name == "TechnicalData").FirstOrDefault().Value),
                                                                      AttributeValue =
                                                           System.Web.HttpUtility.UrlDecode(
                                                           td.Where(
                                                             x =>
                                                             x.Attribute("order").Value == g.Key &&
                                                             x.Name == "TechnicalData_Content").FirstOrDefault().Value),
                                                                      AttributeGroupCode = "TechnicalData"
                                                                    }

                                                           ).Union(
                                            (from elem in productData.Elements()
                                             where
                                               elem.Name == "KeyFeatures" || elem.Name == "Features" ||
                                               elem.Name == "DeliveryIncludes"
                                             select new SennHeiserAttribute
                                                      {
                                                        AttributeCode =
                                               elem.Name.LocalName.Length > 150
                                                 ? elem.Name.LocalName.Substring(0, 150)
                                                 : elem.Name.LocalName,
                                                        AttributeValue = System.Web.HttpUtility.UrlDecode(elem.Value),
                                                        AttributeGroupCode = elem.Name.LocalName
                                                      })).ToList(),
                                           Variants = (from data in productData.Elements()
                                                       where data.Name == "Variant"
                                                       from data1 in data.Elements()
                                                       where
                                                         data1.Name == "VariantData"
                                                       select new ProductData
                                                       {
                                                         VendorItemNumber = data1.Element("ProductNo").Value,
                                                         OrderID = int.Parse(data1.Attribute("order").Value),
                                                         CustomItemNumber = data1.Element("ProductName").Value,
                                                         Description = data1.Element("Description").Value
                                                       }).ToList(),
                                           Accessoires = (from data in productData.Elements()
                                                          where data.Name == "Accessories"
                                                          from data1 in data.Elements()
                                                          where
                                                            data1.Name == "AccessoriesData"
                                                          select new ProductData
                                                      {
                                                        VendorItemNumber = data1.Element("ProductNo").Value,
                                                        OrderID = int.Parse(data1.Attribute("order").Value),
                                                        CustomItemNumber = data1.Element("ProductName").Value,
                                                        Description = data1.Element("Description").Value,
                                                        Designation = data1.Element("Designation").Value
                                                      }).ToList(),
                                           ProductCategory = c.Try(q => q.category.Remove(0, "Private_".Length), string.Empty),
                                           ProductMedia = (from url in productData.Elements()
                                                           where url.Name == "Image"
                                                           select new ProductMedia
                                                           {
                                                             MediaUrl = url.Element("ImageFile").Value,
                                                             Description = url.Attribute("description").Value
                                                           }).ToList(),
                                           OtherMedia = (from m in productData.Elements()
                                                         where m.Name == "Download"
                                                         select new ProductMedia
                                                         {
                                                           MediaUrl = m.Element("DownloadFile").Value,
                                                           Sequence = int.Parse(m.Element("DownloadFile").Attribute("order").Value),
                                                           Description = m.Attribute("description").Value
                                                         }).ToList()
                                         }).ToList();

          #endregion

          var brandVendors = _brandVendorRepo.GetAll(b => (b.VendorID == vendor.VendorID || (vendor.ParentVendorID.HasValue && b.VendorID == vendor.ParentVendorID))).ToList();

          var currentVendorProductGroups = _productGroupVendorRepo.GetAll(v => v.VendorID == vendor.VendorID).ToList();

          var productGroupVendors = _productGroupVendorRepo.GetAll(g => (g.VendorID == vendor.VendorID || (vendor.ParentVendorID.HasValue && g.VendorID == vendor.VendorID))).ToList();
          var brands = _brandRepo.GetAll().ToList();

          var products = _productRepo.GetAll().ToList();

          var vendorassortments = _assortmentRepo.GetAll(x => x.VendorID == vendor.VendorID).ToList();
          var medias = _mediaRepo.GetAll().ToList();
          var productDescriptions = _prodDescriptionRepo.GetAll().ToList();
          var relatedProductTypeVariant = _relatedProductTypeRepo.GetSingle(x => x.Type == "Variant");
          var accessoaireProductTypeVariant = _relatedProductTypeRepo.GetSingle(x => x.Type == "Accossaire");
          var imageMediaType = _mediaTypeRepo.GetSingle(i => i.Type.ToLower() == "Image");

          foreach (var vendorID in vendors.Keys)
          {
            var stocks = _stockRepo.GetAll(c => c.VendorID == vendorID).ToList();
            int couterProduct = 0;
            int logCount = 0;
            int totalProducts = importProducts.Count();
            foreach (var product in importProducts)
            {
              try
              {
                couterProduct++;
                logCount++;
                if (logCount == 250)
                {
                  log.DebugFormat("Products Processed : {0}/{1} for Vendor {2}", couterProduct, totalProducts, vendor.Name);
                  logCount = 0;
                }

                string vendorBrandCode = "Sennheiser";

                if (!string.IsNullOrEmpty(product.MainCategory))
                  vendorBrandCode = int.Parse(product.MainCategory.Substring(1, 3)) < 8 ? "Sennheiser" : product.MainCategory.Substring(4).Replace("$'", "");

                #region BrandVendor

                Product prod = null;
                //check if brandvendor exists in db
                var brandVendor = brandVendors.FirstOrDefault(c => c.VendorBrandCode == vendorBrandCode.Trim());

                #region Brand Vendor
                if (brandVendor == null) //if brandvendor does not exist
                {
                  Brand brand = brands.FirstOrDefault(x => x.Name == vendorBrandCode.Trim());

                  if (brand == null)
                  {
                    brand = new Brand()
                   {
                     Name = vendorBrandCode
                   };
                    brands.Add(brand); //memory ref
                    _brandRepo.Add(brand);
                  }

                  //create new brandVendor
                  brandVendor = new BrandVendor
                  {
                    Brand = brand,
                    VendorID = vendor.VendorID,
                    VendorBrandCode = vendorBrandCode.Trim(),
                    Name = vendorBrandCode,
                  };
                  _brandVendorRepo.Add(brandVendor);
                  brandVendors.Add(brandVendor);
                }
                #endregion

                var BrandID = brandVendor.BrandID;

                prod = products.FirstOrDefault(x => x.VendorItemNumber == product.VendorItemNumber.ToString());

                //if product does not exist (usually true)
                if (prod == null)
                {
                  prod = new Product
                  {
                    VendorItemNumber = product.VendorItemNumber.ToString(),
                    BrandID = BrandID,
                    SourceVendorID = vendor.VendorID
                  };
                  _productRepo.Add(prod);
                  products.Add(prod);
                  //unit.Save();
                }

                #endregion

                #region VendorAssortment

                //var productID = prod.ProductID;

                var vendorAssortment = vendorassortments.FirstOrDefault(x => x.Product.VendorItemNumber == product.VendorItemNumber.ToString());

                //if vendorAssortMent does not exist
                if (vendorAssortment == null)
                {
                  //create vendorAssortMent with productID
                  vendorAssortment = new VendorAssortment
                  {
                    VendorID = vendorID
                  };

                  _assortmentRepo.Add(vendorAssortment);
                  vendorassortments.Add(vendorAssortment);
                  // context.SubmitChanges();
                }


                if (!string.IsNullOrEmpty(product.ShortDescription))
                {
                  if (string.IsNullOrEmpty(vendorAssortment.ShortDescription))
                  {
                    vendorAssortment.ShortDescription = product.ShortDescription.Cap(150);
                  }
                }


                vendorAssortment.LongDescription = vendorAssortment.LongDescription.IfNullOrEmpty(product.LongDescription);

                vendorAssortment.LongDescription = product.LongDescription;


                vendorAssortment.IsActive = true;
                vendorAssortment.CustomItemNumber = product.VendorItemNumber;
                vendorAssortment.Product = prod;
                #endregion

                #region VendorPrice
                if (vendorAssortment.VendorPrices == null) vendorAssortment.VendorPrices = new List<VendorPrice>();

                var vendorPrice = vendorAssortment.VendorPrices.FirstOrDefault();

                //create vendorPrice with vendorAssortmentID
                if (vendorPrice == null && product.Price_BE.HasValue && product.Price_NL.HasValue)
                {
                  decimal price = (decimal)product.GetType().GetProperty("Price_" + vendors[vendorID]).GetValue(product, null);
                  decimal costPrice = 0;
                  decimal taxRate = 0;

                  if (decimal.TryParse(product.CostPrice, out costPrice))
                  {
                    if (costPrice > 0)
                      taxRate = ((price / costPrice) * 100) - 100;

                    switch (vendors[vendorID])
                    {
                      case "NL":
                        taxRate = taxRate >= 0 ? 19 : 0;
                        break;
                      case "BE":
                        taxRate = taxRate >= 0 ? 21 : 0;
                        break;
                    }
                  }
                  else
                  {
                    if (!string.IsNullOrEmpty(product.CostPrice))
                    {
                      string reg = @"(\d*)%";
                      Regex expr = new Regex(reg);
                      Match match = expr.Match(product.CostPrice);
                      taxRate = decimal.Parse(match.Value.Replace("%", string.Empty));
                    }

                    if (taxRate < 1)
                    {
                      switch (vendors[vendorID])
                      {
                        case "NL":
                          taxRate = 19;
                          break;
                        case "BE":
                          taxRate = 21;
                          break;
                      }
                    }
                  }

                  vendorPrice = new VendorPrice
                  {
                    VendorAssortment = vendorAssortment,
                    BasePrice = price,
                    BaseCostPrice = costPrice,
                    TaxRate = taxRate,
                    CommercialStatus = "S",
                    MinimumQuantity = 0
                  };
                  _priceRepo.Add(vendorPrice);
                  vendorAssortment.VendorPrices.Add(vendorPrice);
                }
                #endregion

                #region VendorStock
                var vendorStock = stocks.FirstOrDefault(c => c.VendorID == vendorAssortment.VendorID && c.Product.VendorItemNumber == prod.VendorItemNumber);

                //create vendorPrice with vendorAssortmentID
                if (vendorStock == null)
                {

                  vendorStock = new VendorStock
                  {
                    Product = prod,
                    QuantityOnHand = 0,
                    VendorID = vendor.VendorID,
                    VendorStockTypeID = 1
                  };
                  _stockRepo.Add(vendorStock);
                }
                #endregion

                #region ProductGroupVendor
                try
                {
                  ////create vendorGroup five times, each time with a different VendorProductGroupCode on a different level if not exist
                  int productGroupCount = 1;

                  if (!string.IsNullOrEmpty(product.MainCategory))
                  {
                    string category = product.ProductCategory;
                    product.ProductCategory = string.Format("{0}_{1}", product.MainCategory.Substring(4).Replace("$'", ""), category);
                  }

                  foreach (var cat in product.ProductCategory.Split('_'))
                  {
                    string category = cat;

                    category = cat.Cap(50);


                    var productGroupVendor = productGroupVendors.Where(pg => pg.GetType().GetProperty("VendorProductGroupCode" + productGroupCount).GetValue(pg, null) != null
                      && pg.GetType().GetProperty("VendorProductGroupCode" + productGroupCount).GetValue(pg, null).ToString() == category.Trim()).FirstOrDefault();

                    if (productGroupVendor == null)
                    {
                      productGroupVendor = new ProductGroupVendor
                      {
                        ProductGroupID = unmappedID,
                        VendorID = vendorID,
                        VendorName = vendorBrandCode,
                      };

                      productGroupVendor.GetType().GetProperty("VendorProductGroupCode" + productGroupCount).SetValue(productGroupVendor, category.Trim(), null);

                      _productGroupVendorRepo.Add(productGroupVendor);
                      productGroupVendors.Add(productGroupVendor);
                    }

                    #region sync
                    if (currentVendorProductGroups.Contains(productGroupVendor))
                    {
                      currentVendorProductGroups.Remove(productGroupVendor);
                    }
                    #endregion


                    if (productGroupVendor.VendorAssortments == null) productGroupVendor.VendorAssortments = new List<VendorAssortment>();

                    var vendorProductGroupAssortment1 =
                      productGroupVendor.VendorAssortments.FirstOrDefault(c => c.VendorAssortmentID == vendorAssortment.VendorAssortmentID);

                    if (vendorProductGroupAssortment1 == null)
                    {
                      productGroupVendor.VendorAssortments.Add(vendorAssortment);
                    }

                    productGroupCount++;
                  }
                }
                catch (Exception ex)
                {
                  log.Error("Failed productgroups", ex);
                }
                #endregion

                #region ProductDescription

                var productDescription = productDescriptions.FirstOrDefault(pd => pd.LanguageID == languageID && (pd.VendorID == vendor.VendorID || (vendor.ParentVendorID.HasValue && pd.VendorID == vendor.VendorID)));

                if (productDescription == null)
                {
                  //create ProductDescription
                  productDescription = new ProductDescription
                  {
                    Product = prod,
                    LanguageID = languageID,
                    VendorID = vendorID,
                  };

                  _prodDescriptionRepo.Add(productDescription);
                  productDescriptions.Add(productDescription);
                }

                if (string.IsNullOrEmpty(productDescription.ProductName))
                {
                  productDescription.ProductName = product.CustomItemNumber.ToString();
                  productDescription.ModelName = product.CustomItemNumber.ToString();
                }

                if (!string.IsNullOrEmpty(product.ShortDescription))
                  productDescription.ShortContentDescription = product.ShortDescription.Cap(1000);

                if (string.IsNullOrEmpty(productDescription.LongContentDescription))
                  productDescription.LongContentDescription = product.LongContentDescription;

                if (string.IsNullOrEmpty(productDescription.LongSummaryDescription))
                  productDescription.LongSummaryDescription = product.ShortContentDescription;

                //unit.Save();
                #endregion

                #region ProductImage
                if (product.ProductMedia != null)
                {
                  int imageSequence = 0;

                  var imageType = imageMediaType;


                  if (imageType == null)
                  {
                    imageType = new MediaType()
                    {
                      Type = "Image"
                    };

                    _mediaTypeRepo.Add(imageType);
                  }

                  foreach (var image in product.ProductMedia)
                  {
                    if (!string.IsNullOrEmpty(image.MediaUrl))
                    {
                      var productImage = medias.FirstOrDefault(pd => (pd.VendorID == vendor.VendorID || (vendor.ParentVendorID.HasValue && pd.VendorID == vendor.VendorID)) &&
                        pd.MediaUrl == image.MediaUrl);

                      if (productImage == null)
                      {
                        //create ProductDescription
                        productImage = new ProductMedia
                        {
                          Product = prod,
                          MediaUrl = image.MediaUrl,
                          VendorID = vendorID
                        };

                        _mediaRepo.Add(productImage);
                        medias.Add(productImage);
                      }

                      productImage.Sequence = imageSequence;
                      productImage.MediaType = imageType;
                      productImage.Description = image.Description;
                      imageSequence++;
                    }
                  }
                }
                #endregion

                #region ProductMedia
                if (product.OtherMedia != null)
                {
                  int productMediaSequence = 0;
                  foreach (var media in product.OtherMedia)
                  {
                    string fileType = media.MediaUrl.Substring(media.MediaUrl.Length - 3).ToLower();

                    var mediaType = _mediaTypeRepo.GetSingle(c => c.Type.ToLower() == fileType);

                    if (mediaType == null)
                    {
                      mediaType = new MediaType()
                      {
                        Type = fileType
                      };

                      _mediaTypeRepo.Add(mediaType);
                    }

                    if (!string.IsNullOrEmpty(media.MediaUrl))
                    {
                      var productImage = medias.FirstOrDefault(pd => (pd.VendorID == vendor.VendorID || (vendor.ParentVendorID.HasValue && pd.VendorID == vendor.VendorID)) &&
                        pd.MediaUrl == media.MediaUrl);

                      if (productImage == null)
                      {
                        //create ProductDescription
                        productImage = new ProductMedia
                        {
                          Product = prod,
                          MediaUrl = media.MediaUrl,
                          VendorID = vendorID
                        };

                        _mediaRepo.Add(productImage);
                        medias.Add(productImage);
                      }

                      productImage.Sequence = productMediaSequence;
                      productImage.MediaType = mediaType;
                      productImage.Description = media.Description;
                      productMediaSequence++;
                    }
                  }
                }
                #endregion

                #region ProductAttribute
                product.Attributes.Add(new SennHeiserAttribute
                              {
                                AttributeCode = "Guarantee",
                                AttributeGroupCode = "GuaranteeSupplier",
                                AttributeValue = "Sennheiser"
                              });

                //  insert all other Attributes

                int keyFeatures = 0;
                int features = 0;
                int DeliveryIncludes = 0;

                foreach (var content in product.Attributes)
                {
                  if (content == null)
                    continue;

                  #region ProductAttributeGroupMetaData

                  var productAttributeGroupMetaData = _attrGroupRepo.GetSingle(c => c.GroupCode == content.AttributeGroupCode && (c.VendorID == vendor.VendorID || (vendor.ParentVendorID.HasValue && c.VendorID == vendor.VendorID)));

                  //create ProductAttributeGroupMetaData if not exists
                  if (productAttributeGroupMetaData == null)
                  {
                    productAttributeGroupMetaData = new ProductAttributeGroupMetaData
                    {
                      Index = 0,
                      GroupCode = content.AttributeGroupCode,
                      VendorID = vendorID
                    };
                    _attrGroupRepo.Add(productAttributeGroupMetaData);
                  }
                  #endregion

                  #region ProductAttributeGroupName

                  if (productAttributeGroupMetaData.ProductAttributeGroupNames == null) productAttributeGroupMetaData.ProductAttributeGroupNames = new List<ProductAttributeGroupName>();
                  var productAttributeGroupName = productAttributeGroupMetaData.ProductAttributeGroupNames.FirstOrDefault(c => c.LanguageID == languageID);
                  //create ProductAttributeGroupName if not exists
                  if (productAttributeGroupName == null)
                  {
                    productAttributeGroupName = new ProductAttributeGroupName
                    {
                      Name = content.AttributeGroupCode,
                      ProductAttributeGroupMetaData = productAttributeGroupMetaData,
                      LanguageID = languageID
                    };
                    _attrGroupName.Add(productAttributeGroupName);
                  }

                  #endregion

                  #region ProductAttributeMetaData

                  //create ProductAttributeMetaData as many times that there are entrys in technicaldata
                  string attributeCode = content.AttributeCode;
                  if (content.AttributeCode == "KeyFeatures")
                  {
                    if (keyFeatures > 0)
                      attributeCode = attributeCode + keyFeatures.ToString();

                    keyFeatures++;
                  }
                  else if (content.AttributeCode == "Features")
                  {
                    if (features > 0)
                      attributeCode = attributeCode + features.ToString();

                    features++;
                  }
                  else if (content.AttributeCode == "DeliveryIncludes")
                  {
                    if (DeliveryIncludes > 0)
                      attributeCode = attributeCode + DeliveryIncludes.ToString();

                    DeliveryIncludes++;
                  }

                  var productAttributeMetaData = _attrRepo.GetSingle(x => x.AttributeCode == attributeCode && (x.VendorID == vendor.VendorID || (vendor.ParentVendorID.HasValue && x.VendorID == vendor.VendorID)));
                  //create ProductAttributeMetaData if not exists
                  if (productAttributeMetaData == null)
                  {
                    productAttributeMetaData = new ProductAttributeMetaData
                    {
                      ProductAttributeGroupMetaData = productAttributeGroupMetaData,
                      AttributeCode = attributeCode,
                      Index = 0,
                      IsVisible = true,
                      NeedsUpdate = true,
                      VendorID = vendorID,
                      IsSearchable = false
                    };
                    _attrRepo.Add(productAttributeMetaData);
                  }

                  #endregion

                  #region ProductAttributeName
                  if (productAttributeMetaData.ProductAttributeNames == null) productAttributeMetaData.ProductAttributeNames = new List<ProductAttributeName>();
                  var productAttributeName = productAttributeMetaData.ProductAttributeNames.FirstOrDefault(c => c.LanguageID == languageID);
                  //create ProductAttributeName if not exists
                  if (productAttributeName == null)
                  {
                    productAttributeName = new ProductAttributeName
                    {
                      ProductAttributeMetaData = productAttributeMetaData,
                      LanguageID = languageID
                    };
                    _attrNameRepo.Add(productAttributeName);
                  }
                  productAttributeName.Name = productAttributeName.Name.IfNullOrEmpty(content.AttributeCode);


                  #endregion

                  #region ProductAttributeValue

                  if (productAttributeMetaData.ProductAttributeValues == null) productAttributeMetaData.ProductAttributeValues = new List<ProductAttributeValue>();

                  var productAttributeValue =
                    productAttributeMetaData.ProductAttributeValues.FirstOrDefault(c => c.ProductID == prod.ProductID);
                  //create ProductAttributeValue 
                  if (productAttributeValue == null)
                  {
                    productAttributeValue = new ProductAttributeValue
                    {
                      ProductAttributeMetaData = productAttributeMetaData,
                      Product = prod,
                      LanguageID = languageID
                    };
                    _attrValueRepo.Add(productAttributeValue);
                  }
                  productAttributeValue.Value = content.AttributeValue;
                  #endregion
                }

                //unit.Save();
                #endregion

                #region variants
                if (product.Variants != null)
                {
                  var relatedType = relatedProductTypeVariant;
                  if (relatedType == null)
                  {
                    relatedType = new RelatedProductType
                    {
                      Type = "Variant"
                    };
                    _relatedProductTypeRepo.Add(relatedType);
                  }

                  foreach (var varaint in product.Variants)
                  {
                    var relatedProduct = prod;//  _productRepo.GetSingle(va => va.VendorItemNumber == varaint.ProductNo);

                    if (relatedProduct == null)
                    {
                      relatedProduct = vendorAssortment.Try(c => c.Product, null);// _assortmentRepo.GetSingle(va => va.CustomItemNumber == varaint.ProductName).Try(c => c.Product, null);

                      if (relatedProduct == null)
                      {
                        relatedProduct = new Product
                        {
                          VendorItemNumber = varaint.VendorItemNumber,
                          BrandID = BrandID,
                          SourceVendorID = vendor.VendorID
                        };

                        var desc = new ProductDescription
                        {
                          LanguageID = languageID,
                          Product = relatedProduct,
                          ProductName = varaint.CustomItemNumber,
                          ShortContentDescription = varaint.Description,
                          ModelName = varaint.Designation,
                          VendorID = vendor.VendorID
                        };
                        _productRepo.Add(relatedProduct);
                        _prodDescriptionRepo.Add(desc);
                      }
                    }

                    if (prod.RelatedProductsSource == null) prod.RelatedProductsSource = new List<RelatedProduct>();
                    var relatedProducts = prod.RelatedProductsSource.FirstOrDefault(c => c.RProduct.VendorItemNumber == relatedProduct.VendorItemNumber && c.VendorID == vendor.VendorID);

                    if (relatedProducts == null)
                    {
                      relatedProducts = new RelatedProduct
                      {
                        SourceProduct = prod,
                        RProduct = relatedProduct,
                        VendorID = vendor.VendorID,
                        RelatedProductType = relatedType
                      };
                      _relatedProductRepo.Add(relatedProducts);
                    }
                    //unit.Save();
                  }
                }
                #endregion

                #region Accessory
                if (product.Accessoires != null)
                {
                  var relatedType = accessoaireProductTypeVariant;
                  if (relatedType == null)
                  {
                    relatedType = new RelatedProductType
                    {
                      Type = "Accossaire"
                    };
                    _relatedProductTypeRepo.Add(relatedType);
                  }

                  foreach (var acc in product.Accessoires)
                  {
                    var relatedProduct = _productRepo.GetSingle(va => va.VendorItemNumber == acc.VendorItemNumber);

                    if (relatedProduct == null)
                    {
                      relatedProduct = _assortmentRepo.GetSingle(va => va.CustomItemNumber == acc.CustomItemNumber).Try(c => c.Product, null);

                      if (relatedProduct == null)
                      {
                        relatedProduct = new Product
                        {
                          VendorItemNumber = acc.VendorItemNumber,
                          BrandID = BrandID,
                          SourceVendorID = vendor.VendorID
                        };

                        var desc = new ProductDescription
                        {
                          LanguageID = languageID,
                          Product = relatedProduct,
                          ProductName = acc.CustomItemNumber,
                          ShortContentDescription = acc.Description,
                          ModelName = acc.Designation,
                          VendorID = vendor.VendorID
                        };
                        _productRepo.Add(relatedProduct);
                        _prodDescriptionRepo.Add(desc);
                      }
                    }
                    //unit.Save();
                    if (prod.RelatedProductsSource == null) prod.RelatedProductsSource = new List<RelatedProduct>();

                    var relatedProducts = prod.RelatedProductsSource.FirstOrDefault(c => c.RProduct.VendorItemNumber == relatedProduct.VendorItemNumber && c.VendorID == vendor.VendorID);
                    if (relatedProducts == null)
                    {
                      relatedProducts = new RelatedProduct
                      {
                        SourceProduct = prod,
                        RProduct = relatedProduct,
                        VendorID = vendor.VendorID,
                        RelatedProductType = relatedType
                      };
                      _relatedProductRepo.Add(relatedProducts);
                    }
                    // unit.Save();
                  }
                }
                #endregion

              }
              catch (Exception ex)
              {
                log.AuditError(string.Format("Error import product {0}", product.VendorItemNumber), ex, "Product import");
              }
            }
            unit.Save();
          }
          #endregion
        }
        catch (Exception ex)
        {
          log.ErrorFormat("product: {0} error: {1}", currentItem, ex.StackTrace);
          somethingWentWrong = true;
        }

        if (somethingWentWrong)
        {
          return false;
        }
        return true;
      }
    }

    private string checkRow(object row, string productCategory)
    {
      DataRow rowToCheck = (DataRow)row;

      if (rowToCheck.IsNull(4))
      {
        Category = rowToCheck[0].ToString();
        return string.Empty;
      }
      return productCategory;
    }
  }

}
