using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.ConcentratorService;
using System.Xml.Linq;
using System.IO;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Models.Media;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.Models.Attributes;
using Concentrator.Objects.Models.Brands;
using System.Net;

namespace Concentrator.Plugins.SennHeiser
{
  public class AssortmentImportWeb : ConcentratorPlugin
  {
    private const int languageID = 1;
    private const int unmappedID = -1;
    public override string Name
    {
      get { return "Web assortment import "; }
    }


    protected override void Process()
    {
      var config = GetConfiguration();

      log.AuditInfo("Retrieved plugin specific information");

      var vendors = (from v in config.AppSettings.Settings["Vendors"].Value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                     select int.Parse(v)
                    ).ToList();


      var pathToWebImportDocs = Path.Combine(config.AppSettings.Settings["SennheiserBasePath"].Value, config.AppSettings.Settings["SennheiserWebDocumentsPath"].Value);
      var pathToProductImport = Path.Combine(pathToWebImportDocs, "products.xml");
      var pathCategories = Path.Combine(pathToWebImportDocs, "categories.xml");

      try
      {
        WebRequest prodReq = (WebRequest)WebRequest.Create("http://www.sennheiser.nl/nl/home_nl.nsf/vwExportData/products.xml/$File/products.xml");
        using (var prodStream = ((WebResponse)prodReq.GetResponse()).GetResponseStream())
        {
          if (File.Exists(pathToProductImport))
            File.Delete(pathToProductImport);


          FileStream fsP = new FileStream(pathToProductImport, FileMode.OpenOrCreate, FileAccess.ReadWrite);
          byte[] chunk = new byte[4096];
          int bytesRead;

          while ((bytesRead = prodStream.Read(chunk, 0, chunk.Length)) > 0)
          {
            fsP.Write(chunk, 0, bytesRead);
          }

          //read to mem stream
          fsP.Close();
        }

        var catReq = WebRequest.Create("http://www.sennheiser.nl/nl/home_nl.nsf/vwExportData/categories.xml/$File/categories.xml");
        using (var catStream = catReq.GetResponse().GetResponseStream())
        {
          if (File.Exists(pathCategories))
            File.Delete(pathCategories);

          //read to mem stream
          byte[] chunk = new byte[4096];
          var fileStream = new FileStream(pathCategories, FileMode.OpenOrCreate, FileAccess.ReadWrite);
          int bytesRead = 0;
          while ((bytesRead = catStream.Read(chunk, 0, chunk.Length)) > 0)
          {
            fileStream.Write(chunk, 0, bytesRead);
          }
          fileStream.Close();
        }

        string productsXmlText = string.Empty;

        productsXmlText = File.ReadAllText(pathToProductImport);




        productsXmlText = productsXmlText.Replace("<br>", " ");
        productsXmlText = productsXmlText.Replace("&M", "&amp;M");


        using (var unit = GetUnitOfWork())
        {
          Import(vendors, XDocument.Parse(productsXmlText), XDocument.Load(pathCategories), unit);



          log.AuditSuccess("Web assortment import has completed", Name);
        }
      }
      catch (Exception e)
      {
        log.AuditFatal("Something went wrong while importing web assortment", e, Name);
      }
    }

    private void Import(List<int> Vendors, XDocument productsDoc, XDocument categoriesDoc, IUnitOfWork unit)
    {

      #region Repositories
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
      #endregion

      #region Parsing xmls

      //Parse categories
      var Categories = (from cat in categoriesDoc.Element("CATEGORYDATA").Elements("category")
                        select new
                        {
                          category = cat.Element("Category").Value,
                          path = cat.Element("Path").Value
                        }
                      );

      //Parse products
      var importProducts = (from productData in productsDoc.Element("PRODUCTDATA").Elements("product")
                            let td = productData.Element("TechnicalData").Elements()
                            let c =
                              Categories.Where(
                              x =>
                              x.path ==
                              "private_" + productData.Elements().Where(p => p.Name == "Path").FirstOrDefault().Value).
                              FirstOrDefault()
                            select new SennHeiserItemImport
                            {
                              CustomItemNumber = productData.Attribute("name").Value.Trim(),
                              VendorItemNumber = productData.Attribute("no").Value.Trim(),
                              Url = productData.Element("URL") != null ? productData.Element("URL").Value.Trim() : string.Empty,
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
                                            Description = data1.Element("Description").Value,
                                            IsVariant = true
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
                                               IsVariant = false,
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

      #region Initialize independent inmemory collections

      var brands = _brandRepo.GetAll().ToList();

      var products = _productRepo.GetAll().ToList();

      var relatedProductTypeVariant = _relatedProductTypeRepo.GetSingle(x => x.Type == "Variant");

      var relatedProductTypeAccessory = _relatedProductTypeRepo.GetSingle(x => x.Type == "Accessory");

      var mediaTypes = _mediaTypeRepo.GetAll().ToList();

      var imageMediaType = _mediaTypeRepo.GetSingle(i => i.Type.ToLower() == "Image");
      if (imageMediaType == null)
      {
        imageMediaType = new MediaType()
        {
          Type = "Image"
        };

        _mediaTypeRepo.Add(imageMediaType);
      }
      #endregion

      bool firstVendor = true;
      foreach (var vendorID in Vendors)
      {
        
        var vendor = _vendorRepo.GetSingle(c => c.VendorID == vendorID);

        log.InfoFormat("Starting processing for {0}", vendor.Name);

        #region Initialize vendor dependent inmemory collections

        var brandVendors = _brandVendorRepo.GetAll(b => (b.VendorID == vendor.VendorID || (vendor.ParentVendorID.HasValue && b.VendorID == vendor.ParentVendorID))).ToList();

        var currentVendorProductGroups = _productGroupVendorRepo.GetAll(v => v.VendorID == vendor.VendorID).ToList();

        var productGroupVendors = _productGroupVendorRepo.GetAll(g => (g.VendorID == vendor.VendorID || (vendor.ParentVendorID.HasValue && g.VendorID == vendor.VendorID))).ToList();

        var vendorassortments = _assortmentRepo.GetAll(x => x.VendorID == vendor.VendorID).ToList();

        var medias = _mediaRepo.GetAll().ToList();

        var productDescriptions = _prodDescriptionRepo.GetAll().ToList();

        var vendorStocks = _stockRepo.GetAll(c => c.VendorID == vendorID).ToList();

        #endregion

        log.InfoFormat("About to process {0} products", importProducts.Count);

          int counter = 0;
          int total = importProducts.Count();
      int totalNumberOfProductsToProcess = total;
      log.InfoFormat("Start import {0} products", total);

        foreach (var product in importProducts)
        {
          if (counter == 50)
          {
            counter = 0;
            log.InfoFormat("Still need to process {0} of {1}; {2} done;", totalNumberOfProductsToProcess, total, total - totalNumberOfProductsToProcess);
          }
          totalNumberOfProductsToProcess--;
          counter++;

          string vendorBrandCode = "Sennheiser";

          if (!string.IsNullOrEmpty(product.MainCategory))
            vendorBrandCode = int.Parse(product.MainCategory.Substring(1, 3)) < 8 ? "Sennheiser" : product.MainCategory.Substring(4).Replace("$'", "").Trim();

          int BrandID;
          #region Brands
          var brandVendor = brandVendors.FirstOrDefault(c => c.VendorBrandCode == vendorBrandCode.Trim());

          if (brandVendor == null)
          {
            Brand brand = brands.FirstOrDefault(c => c.Name == vendorBrandCode);

            brandVendor = new BrandVendor()
            {
              BrandID = brand == null ? unmappedID : brand.BrandID,
              VendorID = vendorID,
              VendorBrandCode = vendorBrandCode,
              Name = vendorBrandCode
            };

            _brandVendorRepo.Add(brandVendor);
            brandVendors.Add(brandVendor);
          }

          BrandID = brandVendor.BrandID;
          #endregion

          #region product and assortment

          var productIsExisting = false;

          var productEntity = products.FirstOrDefault(c => c.VendorItemNumber == product.VendorItemNumber);

          if (productEntity == null)
          {
            //productIsExisting = false;

            productEntity = new Product()
            {
              VendorItemNumber = product.VendorItemNumber,
              BrandID = brandVendor.BrandID,
              SourceVendorID = vendorID
            };
            _productRepo.Add(productEntity);
            products.Add(productEntity);
          }
          productEntity.BrandID = BrandID;

          bool assortmentIsExisting = true;
          var vendorAssortment = vendorassortments.FirstOrDefault(c => c.Product.VendorItemNumber == product.VendorItemNumber && c.VendorID == vendorID);

          if (vendorAssortment == null)
          {
            assortmentIsExisting = false;
            vendorAssortment = new VendorAssortment()
            {

              VendorID = vendorID,
              Product = productEntity,
              IsActive = true,
              CustomItemNumber = product.CustomItemNumber
            };
            _assortmentRepo.Add(vendorAssortment);
            vendorassortments.Add(vendorAssortment);
          }

          vendorAssortment.CustomItemNumber = product.CustomItemNumber;
          vendorAssortment.LongDescription = string.Empty;
          vendorAssortment.ShortDescription = string.Empty;
          #endregion

          #region Vendor stock --> insert empty stock

          var vendorStock = vendorStocks.FirstOrDefault(c => c.VendorID == vendorID && c.Product.VendorItemNumber == product.VendorItemNumber);

          //create vendorPrice with vendorAssortmentID
          if (vendorStock == null)
          {

            vendorStock = new VendorStock
            {
              Product = productEntity,
              QuantityOnHand = 0,
              VendorID = vendorID,
              VendorStockTypeID = 1
            };

            vendorStocks.Add(vendorStock);
            _stockRepo.Add(vendorStock);
          }

          #endregion

          if (firstVendor)
          {
            #region Product description


            var productDescription = productDescriptions.FirstOrDefault(pd => pd.Product.VendorItemNumber == product.VendorItemNumber && pd.LanguageID == 1 && (pd.VendorID == vendor.VendorID || (vendor.ParentVendorID.HasValue && pd.VendorID == vendor.VendorID)));

            if (productDescription == null)
            {
              //create ProductDescription
              productDescription = new ProductDescription
              {
                Product = productEntity,
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

            if (string.IsNullOrEmpty(productDescription.ShortContentDescription))
            {
              productDescription.ShortContentDescription = product.ShortContentDescription.Cap(1000);
            }

            if (string.IsNullOrEmpty(productDescription.ShortSummaryDescription))
            {
              productDescription.ShortSummaryDescription = product.ShortContentDescription.Cap(1000);
            }

            if (string.IsNullOrEmpty(productDescription.LongContentDescription))
              productDescription.LongContentDescription = product.LongContentDescription;

            if (string.IsNullOrEmpty(productDescription.LongSummaryDescription))
              productDescription.LongSummaryDescription = product.LongContentDescription;

            if (string.IsNullOrEmpty(productDescription.WarrantyInfo))
              productDescription.WarrantyInfo = "2 years warranty";

            #endregion

            #region Product images

            if (product.ProductMedia != null)
            {
              int imageSequence = 0;

              foreach (var image in product.ProductMedia.OrderByDescending(x => x.Description == "ProductImage").OrderByDescending(x => x.Description == "ProductImageNew"))
              {
                if (!string.IsNullOrEmpty(image.MediaUrl))
                {
                  var productImage = medias.FirstOrDefault(pd => (pd.VendorID == vendor.VendorID || (vendor.ParentVendorID.HasValue && pd.VendorID == vendor.VendorID)) &&
                    pd.MediaUrl == image.MediaUrl && pd.Product == productEntity);

                  if (productImage == null)
                  {
                    //create ProductDescription
                    productImage = new ProductMedia
                    {
                      Product = productEntity,
                      MediaUrl = image.MediaUrl,
                      VendorID = vendorID
                    };

                    _mediaRepo.Add(productImage);
                    medias.Add(productImage);
                  }

                  productImage.Sequence = imageSequence;
                  productImage.MediaType = imageMediaType;
                  productImage.Description = image.Description;
                  imageSequence++;
                }
              }


            }

            if (product.OtherMedia != null)
            {
              int productMediaSequence = 0;

              foreach (var media in product.OtherMedia.OrderByDescending(x => x.Description.Contains("Instruction")).OrderByDescending(x => x.Description.Contains("Sheet")))
              {
                string fileType = media.MediaUrl.Substring(media.MediaUrl.Length - 3).ToLower();

                var mediaType = mediaTypes.FirstOrDefault(c => c.Type.ToLower() == fileType);

                if (mediaType == null)
                {
                  mediaType = new MediaType()
                  {
                    Type = fileType
                  };

                  _mediaTypeRepo.Add(mediaType);
                  mediaTypes.Add(mediaType);
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
                      Product = productEntity,
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

            if (!productIsExisting)
            {
              #region Product attribute

              //add the mandatory gurantee supplier
              product.Attributes.Add(new SennHeiserAttribute
              {
                AttributeCode = "Guarantee",
                AttributeGroupCode = "GuaranteeSupplier",
                AttributeValue = "Sennheiser"
              });

              product.Attributes.Add(new SennHeiserAttribute
              {
                AttributeCode = "ProductUrl",
                AttributeGroupCode = "General",
                AttributeValue = product.Url
              });

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
                  productAttributeMetaData.ProductAttributeValues.FirstOrDefault(c => c.Product.VendorItemNumber == productEntity.VendorItemNumber && c.LanguageID == languageID);
                //create ProductAttributeValue 
                if (productAttributeValue == null)
                {
                  productAttributeValue = new ProductAttributeValue
                  {
                    ProductAttributeMetaData = productAttributeMetaData,
                    Product = productEntity,
                    LanguageID = languageID
                  };
                  _attrValueRepo.Add(productAttributeValue);
                }
                productAttributeValue.Value = content.AttributeValue;
                #endregion
              }

              #endregion
            }

            #region Related products

            var allRelatedItems = product.Variants;
            allRelatedItems.AddRange(product.Accessoires);

            foreach (var relatedProduct in allRelatedItems)
            {
              var relatedProductEntity = products.FirstOrDefault(c => c.VendorItemNumber == relatedProduct.VendorItemNumber);

              if (relatedProductEntity == null)
              {
                relatedProductEntity = new Product
                {
                  VendorItemNumber = relatedProduct.VendorItemNumber,
                  BrandID = BrandID,
                  SourceVendorID = vendorID
                };

                _productRepo.Add(relatedProductEntity);
                products.Add(relatedProductEntity);
                log.DebugFormat("New product inserted: {0}", relatedProduct.VendorItemNumber);
                unit.Save();
              }

              //relatedProductEntity.BrandID = BrandID;

              var assortment = vendorassortments.FirstOrDefault(c => c.Product.VendorItemNumber == relatedProduct.VendorItemNumber && c.VendorID == vendorID);

              if (assortment == null)
              {
                assortment = new VendorAssortment
                {
                  Product = relatedProductEntity,
                  CustomItemNumber = relatedProduct.CustomItemNumber,
                  VendorID = vendorID,
                  IsActive = true
                };
                vendorassortments.Add(assortment);
                _assortmentRepo.Add(assortment);
              }

              var vendorStockRelated = vendorStocks.FirstOrDefault(c => c.Product.VendorItemNumber == relatedProduct.VendorItemNumber && c.VendorID == vendorID);
              if (vendorStockRelated == null)
              {

                vendorStockRelated = new VendorStock
                {
                  Product = relatedProductEntity,
                  QuantityOnHand = 0,
                  VendorID = vendorID,
                  VendorStockTypeID = 1
                };

                vendorStocks.Add(vendorStockRelated);
                _stockRepo.Add(vendorStockRelated);
              }

              var descriptionEntity = productDescriptions.FirstOrDefault(c => c.Product.VendorItemNumber == relatedProduct.VendorItemNumber && c.LanguageID == languageID && c.VendorID == vendorID);
              if (descriptionEntity == null)
              {
                descriptionEntity = new ProductDescription()
                {
                  LanguageID = languageID,
                  Product = relatedProductEntity,
                  ProductName = relatedProduct.CustomItemNumber,
                  ShortContentDescription = relatedProduct.Description,
                  ModelName = relatedProduct.Designation,
                  VendorID = vendor.VendorID
                };

                _prodDescriptionRepo.Add(descriptionEntity);
                productDescriptions.Add(descriptionEntity);
              }

              if (productEntity.RelatedProductsSource == null) productEntity.RelatedProductsSource = new List<RelatedProduct>();
              var relatedProductActual = productEntity.RelatedProductsSource.FirstOrDefault(c => c.RProduct.VendorItemNumber == relatedProduct.VendorItemNumber && c.VendorID == vendorID);
              if (relatedProductActual == null)
              {
                relatedProductActual = new RelatedProduct()
                {
                  SourceProduct = productEntity,
                  RProduct = relatedProductEntity,
                  VendorID = vendorID,
                  RelatedProductType = relatedProduct.IsVariant ? relatedProductTypeVariant : relatedProductTypeAccessory
                };

                _relatedProductRepo.Add(relatedProductActual);
                productEntity.RelatedProductsSource.Add(relatedProductActual);
              }

            }

            #endregion
          }
          unit.Save();
        }

        //unit.Save();
        log.InfoFormat("Finished processing {0} products for vendor {1}", importProducts.Count, vendor.Name);
        firstVendor = false;
      }
    }
  }

  public class SennHeiserItemImport
  {
    public string VendorItemNumber { get; set; }
    public string CustomItemNumber { get; set; }
    public string ShortDescription { get; set; }
    public string Url { get; set; }
    public string LongDescription { get; set; }
    public string LongContentDescription { get; set; }
    public decimal? Price_BE { get; set; }
    public decimal? Price_NL { get; set; }
    public string CostPrice { get; set; }
    public string ProductCategory { get; set; }
    public string MainCategory { get; set; }
    public List<ProductMedia> ProductMedia { get; set; }
    public List<ProductMedia> OtherMedia { get; set; }
    public List<SennHeiserAttribute> Attributes { get; set; }
    public List<ProductData> Variants { get; set; }
    public List<ProductData> Accessoires { get; set; }

    public string ShortContentDescription { get; set; }
  }

  public class SennHeiserAttribute
  {
    public string AttributeCode { get; set; }
    public string AttributeValue { get; set; }
    public string AttributeGroupCode { get; set; }
  }

  public class ProductData
  {
    public string CustomItemNumber { get; set; }
    public string Designation { get; set; }
    public string VendorItemNumber { get; set; }
    public string Description { get; set; }
    public int OrderID { get; set; }
    public bool IsVariant { get; set; }
  }
}
