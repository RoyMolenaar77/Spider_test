using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Concentrator.Objects;
using Concentrator.Web.ServiceClient.RED;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects.Models.Brands;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Models.Attributes;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.ConcentratorService;
using Concentrator.Objects.Utility;
using Concentrator.Objects.Models.Localization;
using Concentrator.Objects.Vendors;
using Concentrator.Objects.Vendors.Bulk;

namespace Concentrator.Plugins.Arvato
{
  class ProductImport : VendorBase
  {
    //private const int unmappedID = -1;
    private int languageID = 2;
    private string[] AttributeMapping = new[] { "Category", "UPC_Master_Carton", "Warranty", "Pkg_Gram", "Unit_Gram", "Color", "MasterCartonQty", "BC_euro_cost", "MIP_euro_sell", "Bullet1", "Bullet2", "Bullet3", "Bullet4", "Bullet5" };

    public override string Name
    {
      get { return "Arvato Product Import Plugin"; }
    }

    protected override int VendorID
    {
      get { return Int32.Parse(Config.AppSettings.Settings["VendorID"].Value); }
    }

    protected override int DefaultVendorID
    {
      get { return Int32.Parse(Config.AppSettings.Settings["VendorID"].Value); }
    }

    protected override System.Configuration.Configuration Config
    {
      get { return GetConfiguration(); }
    }



    private static List<string> NotAttributes = new List<string> { "Description", "Category" };
    protected override void SyncProducts()
    {

      using (var unit = GetUnitOfWork())
      {
        List<ProductAttributeMetaData> outList = new List<ProductAttributeMetaData>();
        SetupAttributes(unit, AttributeMapping, out outList, VendorID);

        var config = GetConfiguration();

        CookieContainer cc = new CookieContainer();
        using (Concentrator.Web.ServiceClient.RED.RetailerSoapClient client = new Concentrator.Web.ServiceClient.RED.RetailerSoapClient())
        {

          Concentrator.Web.ServiceClient.RED.AuthenticationHeader authHeader = new Concentrator.Web.ServiceClient.RED.AuthenticationHeader();
          authHeader.Username = Config.AppSettings.Settings["ArvatoUserName"].Value;//"MycomDevSoap@Mycom.com";
          authHeader.Password = Config.AppSettings.Settings["ArvatoPassword"].Value;//"Myc0mD3v!#S0ap";
          var feedUrl = config.AppSettings.Settings["FeedUrl"].Value;

          var header = authHeader.ToString();

          string catalogUrl = client.GetCatalogUrlRequest(authHeader, "", "");

          XDocument catalogXml = new XDocument();

          // Create a request using a URL that can receive a post. 
          WebRequest request = WebRequest.Create(catalogUrl);
          // Set the Method property of the request to POST.
          request.Method = "POST";
          // Create POST data and convert it to a byte array.
          string postData = string.Format("username={0}&password={1}", authHeader.Username, authHeader.Password);
          byte[] byteArray = Encoding.UTF8.GetBytes(postData);
          // Set the ContentType property of the WebRequest.
          request.ContentType = "application/x-www-form-urlencoded";
          // Set the ContentLength property of the WebRequest.
          request.ContentLength = byteArray.Length;
          // Get the request stream.
          using (Stream dataStream = request.GetRequestStream())
          {
            // Write the data to the request stream.
            dataStream.Write(byteArray, 0, byteArray.Length);
            // Close the Stream object.
            //dataStream.Close();
            // Get the response.
            WebResponse response = request.GetResponse();
            // Display the status.
            Console.WriteLine(((HttpWebResponse)response).StatusDescription);
            // Get the stream containing content returned by the server.
            using (Stream returnStream = response.GetResponseStream())
            {
              // Open the stream using a StreamReader for easy access.
              using (StreamReader reader = new StreamReader(returnStream))
              {
                // Read the content.
                string responseFromServer = reader.ReadToEnd();
                // Load content in Xdocument
                catalogXml = XDocument.Parse(responseFromServer);
              }
            }
          }

          var ReferenceID = catalogXml.Element("Catalog").Attribute("Reference_ID").Value;

          var translationXml = (config.AppSettings.Settings["ArvatoProductImportPath"].Value) + "CatalogTranslationFeed.xml";

          log.InfoFormat("Processing file: {0}", "CatalogFeed.xml");

          XDocument cont = null;
          try
          {
            cont = XDocument.Load(translationXml);
          }
          catch (Exception ex)
          {
            log.Error(String.Format("No new files to process or file not available"), ex);
          }

          bool success = ParseDocument(unit, catalogXml, cont);

          if (success)
          {
            ProductExport.doResponse(client, authHeader, unit, log, ReferenceID, feedUrl);
            log.AuditInfo("File successfully processed");
          }
        }
        {
          log.AuditInfo("No new files to process");
        }
      }
    }

    private bool ParseDocument(IUnitOfWork unit, XDocument xml, XDocument catalog)
    {
      ProductStatusVendorMapper mapper = new ProductStatusVendorMapper(unit.Scope.Repository<VendorProductStatus>(), VendorID);

      #region Xml Data

      XNamespace xName = "http://logictec.com/schemas/internaldocuments";

      var catalogTranslations = (from cat in catalog.Element("Catalog_Translations").Elements()
                                 where cat.Name == "Translation"
                                 from translation in cat.Elements()
                                 where translation.Name == "Categorization"
                                 from catalogTranslation in translation.Elements()
                                 where catalogTranslation.Name == "Category"
                                 select new
                                 {
                                   ID = catalogTranslation.Element("Category_ID").Value,
                                   VendorProductGroupCode1 = catalogTranslation.Element("Category_Name").Value,
                                   SubCategories = (from Sub_Category in catalogTranslation.Elements()
                                                    where Sub_Category.Name == "Sub_Category"
                                                    from category in Sub_Category.Elements("Category")
                                                    select new
                                                    {
                                                      ID = category.Element("Category_ID").Value,
                                                      VendorProductGroupCode2 = category.Element("Category_Name").Value
                                                    }).ToList()
                                 }

        ).ToList();

      var itemProducts = (from Publisher in xml.Element("Catalog").Elements("Publisher")
                          from product in Publisher.Elements("Product")
                          let techinfo = product.Element("Technical_Info")
                          select new
                                   {
                                     Product_Type = product.Element("Product_Type").Value,
                                     VendorBrandCode = Publisher.Element("Publisher_ID").Value,
                                     VendorName = Publisher.Element("Company_Info").Element("Company_Name").Value,
                                     SupplierSKU = product.Element("Manufacturer_Part_Number").Value,
                                     CustomItemNr = product.Element("Product_Item_Number").Value,
                                     ProductName =
                            product.Element("Product_Offers").Element("Product_Marketing").Element("Product_Name").Value,
                                     Price =
                            product.Element("Product_Offers").Element("Offer").Element("Price").Element("MSRP").Value,
                                     Status = product.Element("Product_Offers").Element("Offer").Element("Status").Value,
                                     CostPrice =
                            product.Element("Product_Offers").Element("Offer").Element("Cost").Element("Wholesale").
                            Value,
                                     QuantityOnHand = 1,
                                     Offer_ID =
                            product.Element("Product_Offers").Element("Offer").Element("Offer_ID").Value,
                                     Zone_ReferenceID =
                            Publisher.Parent.Element("Shipment_Tables").Element("Shipment_Table").Element(
                            "Shipment_Zone").Attribute("Reference_ID").Value,
                                     Shipment_Rate_Table_Reference_ID =
                            Publisher.Parent.Element("Shipment_Tables").Element("Shipment_Table").Element(
                            "Shipment_Zone").Element("Shipment_Rate_Tables").Element("Shipment_Rate_Table").Attribute(
                            "Reference_ID").Value,

                                     VendorProductGroupCodes =
                            (from categorycode in product.Element("Categorization").Elements("Category_ID")
                             select new
                                      {
                                        VendorProductGroupCode = (from code in catalogTranslations
                                                                  where code.ID == categorycode.Value
                                                                  select code.VendorProductGroupCode1).FirstOrDefault() !=
                                                                 null
                                                                   ? (from code in catalogTranslations
                                                                      where code.ID == categorycode.Value
                                                                      select code.VendorProductGroupCode1).
                                                                       FirstOrDefault()
                                                                   : (from code in catalogTranslations
                                                                      from subcode in code.SubCategories
                                                                      where subcode.ID == categorycode.Value
                                                                      select subcode.VendorProductGroupCode2).
                                                                       FirstOrDefault()
                                        //catalogTranslations.Find(m => m.ID == categorycode.Value) != null ?
                                        //  catalogTranslations.Find(m => m.VendorProductGroupCode1 == categorycode.Value).VendorProductGroupCode1 :
                                        //  catalogTranslations.Select(x => x.SubCategories.Where(o => o.ID == categorycode.Value).Select(y => y.VendorProductGroupCode2).FirstOrDefault()).FirstOrDefault()
                                      }).ToList(),
                                     ShortContentDescription =
                            (from disc in
                               product.Element("Product_Offers").Element("Product_Marketing").Element(
                               "Product_Descriptions").Elements()
                             where disc.Name == "Char_Desc_45"
                             select disc).FirstOrDefault().Value,

                                     LongContentDescription = (from disc in product.Element("Technical_Info").Elements()
                                                               where disc.Name == "Specification_List"
                                                               from disc2 in disc.Elements()
                                                               where disc2.Name == "Specification_Formatted"
                                                               from disc3 in disc2.Elements()
                                                               where disc3.Name == "Formatted_Block"
                                                               select disc3).FirstOrDefault().Value,
                                     LanguageCode = (from disc in product.Element("Technical_Info").Elements()
                                                     where disc.Name == "Specification_List"
                                                     select disc.Attribute("LanguageCode")).FirstOrDefault().Value,
                                     TechnicalInfo = new
                                                       {
                                                         Specifications = (from specs in techinfo.Elements()
                                                                           where specs.Name == "Specification_List"
                                                                           from specs2 in specs.Elements()
                                                                           where specs2.Name == "Specification_Section"
                                                                           select new
                                                                                    {
                                                                                      GroupCode =
                                                                             specs2.Attribute("Code").Value,
                                                                                      Name =
                                                                             specs2.Attribute("Heading").Value,
                                                                                      AttributeCodes =
                                                                             (from element in specs2.Elements()
                                                                              select new
                                                                                       {
                                                                                         AttributeCode =
                                                                                element.Attribute("Code").Value,
                                                                                         AttributeValue = element.Value
                                                                                       })
                                                                                    }),

                                                         OS_Support = (from specs in techinfo.Elements()
                                                                       where specs.Name == "OS_Support"
                                                                       from specs1 in specs.Elements()
                                                                       where specs1.Name == "OS"
                                                                       select new
                                                                                {
                                                                                  GroupCode = "General",
                                                                                  Name = specs1.Name.ToString(),
                                                                                  AttributeCodes =
                                                                         (from specs2 in specs1.Elements()
                                                                          select new
                                                                                   {
                                                                                     AttributeCode =
                                                                            specs2.Name.ToString(),
                                                                                     AttributeValue = specs2.Value
                                                                                   })
                                                                                }
                                                                      ),

                                                         Language_Support = (from specs in techinfo.Elements()
                                                                             where specs.Name == "Language_Support"
                                                                             select new
                                                                                      {
                                                                                        GroupCode = "General",
                                                                                        Name = specs.Name.ToString(),
                                                                                        AttributeCodes =
                                                                               (from specs2 in specs.Elements()
                                                                                select new
                                                                                         {
                                                                                           AttributeCode =
                                                                                  specs2.Name.ToString(),
                                                                                           AttributeValue = specs2.Value
                                                                                         })
                                                                                      }
                                                                            )
                                                       },
                                     ProductReferences = (from elem in product.Elements()
                                                          where elem.Name == "Product_References"
                                                          from relProd in elem.Elements("Product_Reference")
                                                          select new
                                                                   {
                                                                     RelatedProductID = relProd.Element("Product_Item_Number").Value
                                                                   }),

                                     Images =
                            (from graphic in
                               product.Element("Product_Offers").Element("Product_Marketing").Element("Binary_Info").
                               Elements()
                             where graphic.Name == "Graphic"
                             from imageURL in graphic.Elements()
                             where imageURL.Name == "Graphic_URL"
                             select new
                                      {
                                        ImageURL = imageURL.Value
                                      }
                            ),
                                     Fulfillment_Info = new
                                                    {
                                                      Max_Quantity_Per_Shipment_Charge = (from fullfilmentInfo in product.Element("Fulfillment_Info").Elements()
                                                                                          where fullfilmentInfo.Name == "Max_Quantity_Per_Shipment_Charge"
                                                                                          select new
                                                                                          {
                                                                                            Name = "Max_Quantity_Per_Shipment_Charge",
                                                                                            Value = fullfilmentInfo.Value
                                                                                          }),
                                                      Fulfillment_Method = (from fullfilmentInfo in product.Element("Fulfillment_Info").Elements()
                                                                            where fullfilmentInfo.Name == "Fulfillment_Method"
                                                                            select new
                                                                            {
                                                                              Name = "Fulfillment_Method",
                                                                              Value = fullfilmentInfo.Value,
                                                                            }),
                                                      Fulfillment_Restriction = (from fullfilmentInfo in product.Element("Fulfillment_Info").Elements()
                                                                                 where fullfilmentInfo.Name == "Fulfillment_Restriction"
                                                                                 select new
                                                                                 {
                                                                                   Name = "Fulfillment_Restriction",
                                                                                   Value = fullfilmentInfo.Attribute("Type").Value,
                                                                                   Counties = (from country in fullfilmentInfo.Elements()
                                                                                               where country.Name == "Country_Code"
                                                                                               select new
                                                                                               {
                                                                                                 Country = country.Value
                                                                                               })
                                                                                 })
                                                    }
                                   }
                                                    ).ToList();

      #endregion

      var _mediaAssortment = unit.Scope.Repository<ProductMedia>();
      var _assortmentRepo = unit.Scope.Repository<VendorAssortment>();

      int counter = 0;
      int total = itemProducts.Count();
      int totalNumberOfProductsToProcess = total;
      log.InfoFormat("Start import {0} products", total);

      List<Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorAssortmentItem> assortmentList = new List<Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorAssortmentItem>();

      RelatedProductTypes relatedProductTypes = new RelatedProductTypes(unit.Scope.Repository<RelatedProductType>());
      var relatedProductType = relatedProductTypes.SyncRelatedProductTypes("CompatibleProducts");

      //Used for VendorImportAttributeValues
      var productAttributes = unit.Scope.Repository<ProductAttributeMetaData>().GetAll(c => c.VendorID == VendorID).ToList();
      var attributeList = productAttributes.ToDictionary(x => x.AttributeCode, y => y.AttributeID);

      foreach (var product in itemProducts)
      {
        if (counter == 50)
        {
          counter = 0;
          log.InfoFormat("Still need to process {0} of {1}; {2} done;", totalNumberOfProductsToProcess, total, total - totalNumberOfProductsToProcess);
        }
        totalNumberOfProductsToProcess--;
        counter++;

        var lang = unit.Scope.Repository<Language>().GetSingle(x => x.DisplayCode.ToLower() == product.LanguageCode.ToLower());
        if (lang != null)
        {
          languageID = lang.LanguageID;
        }
        else
        {
          continue;
        }

        try
        {
          var assortment = new Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorAssortmentItem
          {
            #region BrandVendor
            BrandVendors = new List<VendorAssortmentBulk.VendorImportBrand>()
                        {
                            new VendorAssortmentBulk.VendorImportBrand()
                            {
                                VendorID = VendorID,
                                VendorBrandCode = product.VendorBrandCode,
                                ParentBrandCode = null,
                                Name = product.VendorName,
                                Logo = null
                            }
                        },
            #endregion

            #region GeneralProductInfo
            VendorProduct = new VendorAssortmentBulk.VendorProduct
            {
              VendorItemNumber = product.SupplierSKU,
              CustomItemNumber = product.CustomItemNr,
              ShortDescription = product.ProductName.Length > 150 ? product.ProductName.Substring(0, 150) : product.ProductName,
              LongDescription = product.LongContentDescription,
              LineType = product.Product_Type.ToString() == "Primary" ? "DL" : "S",
              LedgerClass = null,
              ProductDesk = null,
              ExtendedCatalog = null,
              VendorID = VendorID,
              DefaultVendorID = VendorID,
              VendorBrandCode = product.VendorBrandCode,
              Barcode = product.SupplierSKU,
              VendorProductGroupCode1 = product.VendorProductGroupCodes.FirstOrDefault().VendorProductGroupCode,
              VendorProductGroupCodeName1 = product.VendorProductGroupCodes.FirstOrDefault().VendorProductGroupCode
            },
            #endregion

            #region RelatedProducts
            RelatedProducts = new List<VendorAssortmentBulk.VendorImportRelatedProduct>()
                        {
                            new Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportRelatedProduct
                            {
                                VendorID = VendorID,
                                DefaultVendorID = VendorID,
                                CustomItemNumber = product.CustomItemNr,
                                RelatedProductType = relatedProductType.Type,
                                RelatedCustomItemNumber = product.CustomItemNr
                            }
                        },
            #endregion

            #region Attributes
            VendorImportAttributeValues = (from attr in AttributeMapping
                                           let prop = product.Equals(attr)
                                           let attributeID = attributeList.ContainsKey(attr) ? attributeList[attr] : -1
                                           let value = prop.ToString()
                                           where !string.IsNullOrEmpty(value)
                                           select new Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportAttributeValue
                                           {
                                             VendorID = VendorID,
                                             DefaultVendorID = VendorID,
                                             CustomItemNumber = product.CustomItemNr,
                                             AttributeID = attributeID,
                                             Value = value,
                                             LanguageID = "1",
                                             AttributeCode = attr,
                                           }).ToList(),
            #endregion

            #region Prices
            VendorImportPrices = new List<Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportPrice>()
                        {
                            new Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportPrice()
                            {
                                VendorID = VendorID,
                                DefaultVendorID = VendorID,
                                CustomItemNumber = product.CustomItemNr,
                                Price = product.Price,
                                CostPrice = product.CostPrice,
                                TaxRate = "19",
                                MinimumQuantity = 0,
                                CommercialStatus = product.Status
                            }
                        },
            #endregion

            #region Stock
            VendorImportStocks = new List<Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportStock>()
                        {
                            new Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportStock()
                            {
                                VendorID = VendorID,
                                DefaultVendorID = VendorID,
                                CustomItemNumber = product.CustomItemNr,
                                QuantityOnHand = 1,
                                StockType = "Assortment",
                                StockStatus = product.Status
                            }
                        },
            #endregion

            VendorProductDescriptions = new List<VendorAssortmentBulk.VendorProductDescription>()
            {
               new VendorAssortmentBulk.VendorProductDescription(){
                VendorID = VendorID,
                LanguageID =languageID,
                DefaultVendorID = VendorID,
                CustomItemNumber = product.CustomItemNr.Trim(), //EAN
                LongContentDescription = product.LongContentDescription,
                ShortContentDescription = product.ShortContentDescription != string.Empty ? product.ShortContentDescription.Length > 1000
                                     ? product.ShortContentDescription.Substring(0, 1000)
                                      : product.ShortContentDescription : product.ProductName,
                ProductName = product.ProductName
                            }
            }
          };

          assortmentList.Add(assortment);

          //import images
          var productAssortment = _assortmentRepo.GetSingle(va => va.CustomItemNumber == product.CustomItemNr);

          if (productAssortment == null)
          {
            log.WarnFormat("Cannot process image for product with Arvato number: {0} because it doesn't exist in Concentrator database", product.CustomItemNr);
          }
          else
          {
            foreach (var image in product.Images)
            {
              var productImage = _mediaAssortment.GetSingle(pi => pi.ProductID == productAssortment.ProductID && pi.VendorID == VendorID);

              if (productImage == null)
              {
                productImage = new ProductMedia
                                 {
                                   ProductID = productAssortment.ProductID,
                                   VendorID = VendorID,
                                   MediaUrl = image.ImageURL,
                                   TypeID = 1
                                 };
                _mediaAssortment.Add(productImage);
              }
            }
          }
        }
        catch (Exception ex)
        {
          log.AuditError(string.Format("product: {0} error: {1}", product.SupplierSKU, ex.InnerException + "-" + ex.StackTrace));
        }
      }

      using (var vendorAssortmentBulk = new VendorAssortmentBulk(assortmentList, VendorID, VendorID))
      {
        vendorAssortmentBulk.Init(unit.Context);
        vendorAssortmentBulk.Sync(unit.Context);
      }

      if (totalNumberOfProductsToProcess == 0)
      {
        log.AuditInfo("Catalog succesfully imported");
      }
      else
      {
        log.AuditError("Catalog import failed");
        return false;
      }

      return true;
    }


  }
}

//#region relatedProducts
//      //related products
//      var vendorAssortments = _assortmentRepo.GetAll(c => c.VendorID == VendorID);

//      RelatedProductTypes relatedProductTypes = new RelatedProductTypes(unit.Scope.Repository<RelatedProductType>());
//      var relatedProductType = relatedProductTypes.SyncRelatedProductTypes("RelatedProduct");

//      var relatedProducts = unit.Scope.Repository<RelatedProduct>().GetAll(c => c.VendorID == VendorID).ToList();

//      foreach (var product in itemProducts)
//      {
//        foreach (var relatedProd in product.ProductReferences)
//        {
//          var prod =
//               vendorAssortments.Where(p => p.CustomItemNumber == product.CustomItemNr).FirstOrDefault();

//          if (prod != null)
//          {
//            var prodID = prod.ProductID;

//            if (prodID != null)
//            {
//              var relatedProductRef =
//                vendorAssortments.Where(p => p.CustomItemNumber == relatedProd.RelatedProductID).FirstOrDefault();

//              if (relatedProductRef != null)
//              {
//                var relatedProductID = relatedProductRef.ProductID;

//                if (relatedProductID != null)
//                {
//                  var relatedProduct = (from relProd in relatedProducts
//                                        where relProd.RelatedProductID == relatedProductID
//                                        select relProd).FirstOrDefault();

//                  if (relatedProduct == null)
//                  {
//                    relatedProduct = new RelatedProduct
//                                       {
//                                         ProductID = prodID,
//                                         RelatedProductID = relatedProductID,
//                                         VendorID = VendorID,
//                                         RelatedProductType = relatedProductType
//                                       };
//                    unit.Scope.Repository<RelatedProduct>().Add(relatedProduct);
//                    relatedProducts.Add(relatedProduct);
//                  }
//                }
//              }
//            }
//          }
//        }
//      }
//      unit.Save();
//      #endregion


//var _brandVendorRepo = unit.Scope.Repository<BrandVendor>();
//var _productRepo = unit.Scope.Repository<Product>();      
//var productGroupVendorRepo = unit.Scope.Repository<ProductGroupVendor>();
//var _prodDescriptionRepo = unit.Scope.Repository<ProductDescription>();
//var _attrGroupRepo = unit.Scope.Repository<ProductAttributeGroupMetaData>();
//var _attrGroupName = unit.Scope.Repository<ProductAttributeGroupName>();
//var _attrRepo = unit.Scope.Repository<ProductAttributeMetaData>();
//var _attrNameRepo = unit.Scope.Repository<ProductAttributeName>();
//var _attrValueRepo = unit.Scope.Repository<ProductAttributeValue>();      
//var brands = _brandVendorRepo.GetAllAsQueryable(c => c.VendorID == VendorID).ToList();

//var productGroups = unit.Scope.Repository<ProductGroupVendor>().GetAllAsQueryable(c => c.VendorID == VendorID).ToList();
//var productAttributes = unit.Scope.Repository<ProductAttributeMetaData>().GetAllAsQueryable(c => c.VendorID == VendorID).ToList();
//var productAttributeGroups = unit.Scope.Repository<ProductAttributeGroupMetaData>().GetAll().ToList();
//#region BrandVendor

//Product prod = null;
////check if brandvendor exists in db
//var brandVendor = brands.FirstOrDefault(vb => vb.VendorBrandCode == product.VendorBrandCode);

//if (brandVendor == null) //if brandvendor does not exist
//{
////create new brandVendor
//brandVendor = new BrandVendor
//{
//BrandID = unmappedID,
//VendorID = VendorID,
//VendorBrandCode = product.VendorBrandCode
//};
//_brandVendorRepo.Add(brandVendor);

//brands.Add(brandVendor);
//}
//brandVendor.Name = product.VendorName;

////use BrandID to create product and retreive ProductID

//var BrandID = brandVendor.BrandID;

//prod = _productRepo.GetSingle(p => p.VendorItemNumber == product.SupplierSKU && (p.BrandID == BrandID || p.BrandID < 0));

////if product does not exist (usually true)
//if (prod == null)
//{
//prod = new Product
//{
//VendorItemNumber = product.SupplierSKU,
//SourceVendorID = VendorID,
//VendorAssortments = new List<VendorAssortment>()
//};
//_productRepo.Add(prod);
//}
//prod.BrandID = BrandID;



//#endregion

//#region VendorAssortMent

//var vendorAssortment = prod.VendorAssortments.FirstOrDefault(va => va.VendorID == VendorID);

////if vendorAssortMent does not exist
//if (vendorAssortment == null)
//{
////create vendorAssortMent with productID
//vendorAssortment = new VendorAssortment
//{
//Product = prod,
//CustomItemNumber = product.CustomItemNr,
//VendorID = VendorID,
//VendorPrices = new List<VendorPrice>(),
//ProductGroupVendors = new List<ProductGroupVendor>()
////LongDescription = ""
//};

//_assortmentRepo.Add(vendorAssortment);

//}
//vendorAssortment.ShortDescription =
//product.ShortDescription.Length > 150
//? product.ShortDescription.Substring(0, 150)
//: product.ShortDescription;
//vendorAssortment.ActivationKey = product.Offer_ID;
//vendorAssortment.ShipmentRateTableReferenceID = product.Shipment_Rate_Table_Reference_ID.ToString();
//vendorAssortment.ZoneReferenceID = product.Zone_ReferenceID.ToString();
//vendorAssortment.IsActive = true;
//vendorAssortment.LineType = product.Product_Type.ToString() == "Primary" ? "DL" : "S";

//unit.Save();

//#endregion

//#region VendorPrice

//var priceRepo = unit.Scope.Repository<VendorPrice>();

//var vendorPrice = vendorAssortment.VendorPrices.FirstOrDefault();
////create vendorPrice with vendorAssortmentID
//if (vendorPrice == null)
//{
//vendorPrice = new VendorPrice
//{
//VendorAssortment = vendorAssortment,
//TaxRate = (Decimal)19.00,
//CommercialStatus = product.Status,
//MinimumQuantity = 0
//};
//priceRepo.Add(vendorPrice);
//vendorAssortment.VendorPrices.Add(vendorPrice);
//}
//vendorPrice.BasePrice = Decimal.Parse(product.Price) / 100;

//vendorPrice.CommercialStatus = product.Status;

//vendorPrice.BaseCostPrice = Decimal.Parse(product.CostPrice) / 100;

//vendorPrice.ConcentratorStatusID = mapper.SyncVendorStatus(vendorPrice.CommercialStatus, -1);


//#endregion

//#region VendorStock
//var stockRepo = unit.Scope.Repository<VendorStock>();
//var vendorStock = unit.Scope.Repository<VendorStock>().GetSingle(c => c.ProductID == vendorAssortment.ProductID && c.VendorID == vendorAssortment.VendorID);

////create vendorStock with productID
//if (vendorStock == null)
//{

//vendorStock = new VendorStock
//{
//Product = prod,
//VendorID = VendorID,
//VendorStockTypeID = 1,

//};

//stockRepo.Add(vendorStock);
//}
//vendorStock.QuantityOnHand = 1;
//vendorStock.StockStatus = product.Status;
//vendorStock.VendorStatus = product.Status;
//vendorStock.ConcentratorStatusID = mapper.SyncVendorStatus(vendorStock.StockStatus, -1);

//vendorStock.ConcentratorStatusID = mapper.SyncVendorStatus(vendorStock.StockStatus, -1);

//#endregion

//#region ProductGroupVendor
//for (int i = 0; i < product.VendorProductGroupCodes.Count; i++)
//{
//if (i > 4)
//break;

//int vendorGroupCodeCounter = i + 1;
//string vendorProductGroupCode = product.VendorProductGroupCodes[i].VendorProductGroupCode.Cap(50);

//var productGroupVendor = productGroups.Where(pg => pg.GetType().GetProperty("VendorProductGroupCode" + vendorGroupCodeCounter).GetValue(pg, null) != null
//      && pg.GetType().GetProperty("VendorProductGroupCode" + vendorGroupCodeCounter).GetValue(pg, null).ToString() == vendorProductGroupCode).FirstOrDefault();

//if (productGroupVendor == null)
//{
//productGroupVendor = new ProductGroupVendor
//{
//ProductGroupID = unmappedID,
//VendorID = VendorID,
//VendorName = product.VendorName,
////VendorProductGroupCode1 = category.Trim()
//};

//productGroupVendor.GetType().GetProperty("VendorProductGroupCode" + vendorGroupCodeCounter).SetValue(productGroupVendor, vendorProductGroupCode, null);

//unit.Scope.Repository<ProductGroupVendor>().Add(productGroupVendor);
//productGroups.Add(productGroupVendor);
//vendorAssortment.ProductGroupVendors.Add(productGroupVendor);
//}
//}
//#endregion

//#region ProductGroupVendor Brand
//var productGroupVendorBrand =
//productGroups.FirstOrDefault(pg => pg.BrandCode == product.VendorName && pg.VendorProductGroupCode1 == null && pg.VendorProductGroupCode2 == null && pg.VendorProductGroupCode3 == null
//&& pg.VendorProductGroupCode4 == null
//&& pg.VendorProductGroupCode5 == null);

//if (productGroupVendorBrand == null)
//{
//productGroupVendorBrand = new ProductGroupVendor
//{
//ProductGroupID = unmappedID,
//VendorID = VendorID,
//VendorName = product.VendorName,
//BrandCode = product.VendorName

//};

//productGroupVendorRepo.Add(productGroupVendorBrand);
//productGroups.Add(productGroupVendorBrand);
//vendorAssortment.ProductGroupVendors.Add(productGroupVendorBrand);
//}

//#endregion

//#region ProductDescription

//var productDescription =
//prod.ProductDescriptions.FirstOrDefault(pd => pd.LanguageID == languageID && pd.VendorID == VendorID);

//if (productDescription == null)
//{
////create ProductDescription
//productDescription = new ProductDescription
//                    {
//                      Product = prod,
//                      LanguageID = languageID,
//                      VendorID = VendorID,

//                    };

//_prodDescriptionRepo.Add(productDescription);
//}
//productDescription.ShortContentDescription = product.ShortContentDescription;

//#endregion


//var GeneralGroupCode = "General";

//#region ProductAttributeGroupMetaData

//ProductAttributeGroupName generalProductAttributeGroupName = null;
//var generalProductAttributeGroupMetaData =
//productAttributeGroups.FirstOrDefault(c => c.GroupCode == GeneralGroupCode);
////create ProductAttributeGroupMetaData if not exists
//if (generalProductAttributeGroupMetaData == null)
//{
//generalProductAttributeGroupMetaData = new ProductAttributeGroupMetaData
//                              {
//                                Index = 0,
//                                GroupCode = GeneralGroupCode,
//                                VendorID = VendorID
//                              };

//_attrGroupRepo.Add(generalProductAttributeGroupMetaData);
//productAttributeGroups.Add(generalProductAttributeGroupMetaData);

//generalProductAttributeGroupName = new ProductAttributeGroupName
//{
//Name = "General",
//ProductAttributeGroupMetaData = generalProductAttributeGroupMetaData,
//LanguageID = languageID
//};
//_attrGroupName.Add(generalProductAttributeGroupName);
//generalProductAttributeGroupMetaData.ProductAttributeGroupNames.Add(generalProductAttributeGroupName);
//}

//#endregion

//#region ProductAttributeGroupName

//if (generalProductAttributeGroupMetaData.ProductAttributeGroupNames.Count() > 0)
//{
//generalProductAttributeGroupName = generalProductAttributeGroupMetaData.ProductAttributeGroupNames.FirstOrDefault(c => c.LanguageID == languageID);
//}

////create ProductAttributeGroupName if not exists
//if (generalProductAttributeGroupName == null)
//{
//generalProductAttributeGroupName = new ProductAttributeGroupName
//                          {
//                            Name = "General",
//                            ProductAttributeGroupMetaData = generalProductAttributeGroupMetaData,
//                            LanguageID = languageID
//                          };
//_attrGroupName.Add(generalProductAttributeGroupName);
//}

//#endregion

//#region ProductAttributeMetaData

////create ProductAttributeMetaData as many times that there are entrys in content.AttributeCodes

//var generalProductAttributeMetaData =
//productAttributes.FirstOrDefault(c => c.AttributeCode == "Product_Type");
////create ProductAttributeMetaData if not exists
//if (generalProductAttributeMetaData == null)
//{
//generalProductAttributeMetaData = new ProductAttributeMetaData
//                          {
//                            ProductAttributeGroupMetaData = generalProductAttributeGroupMetaData,
//                            AttributeCode = "Product_Type",
//                            Index = 0,
//                            IsVisible = true,
//                            NeedsUpdate = true,
//                            VendorID = VendorID,
//                            IsSearchable = false,
//                            ProductAttributeNames = new List<ProductAttributeName>(),
//                            ProductAttributeValues = new List<ProductAttributeValue>()
//                          };
//_attrRepo.Add(generalProductAttributeMetaData);
//productAttributes.Add(generalProductAttributeMetaData);
//}

//#endregion

//#region ProductAttributeName

//var generalProductAttributeName =
//generalProductAttributeMetaData.ProductAttributeNames.FirstOrDefault(c => c.LanguageID == languageID);
////create ProductAttributeName with generated productAttributeMetaData.AttributeID if not exists
//if (generalProductAttributeName == null)
//{
//generalProductAttributeName = new ProductAttributeName
//                      {
//                        ProductAttributeMetaData = generalProductAttributeMetaData,
//                        LanguageID = languageID,
//                        Name = "Product_Type"
//                      };
//_attrNameRepo.Add(generalProductAttributeName);
//}

//#endregion

//#region ProductAttributeValue

//var generalProductAttributeValue =
//generalProductAttributeMetaData.ProductAttributeValues.FirstOrDefault(c => c.ProductID == prod.ProductID);
////create ProductAttributeValue with generated productAttributeMetaData.AttributeID
//if (generalProductAttributeValue == null)
//{
//generalProductAttributeValue = new ProductAttributeValue
//                      {
//                        ProductAttributeMetaData = generalProductAttributeMetaData,
//                        Product = prod,
//                        LanguageID = languageID
//                      };
//_attrValueRepo.Add(generalProductAttributeValue);
//}

//generalProductAttributeValue.Value = product.Product_Type.ToString();
//#endregion


////first for the TechnicalData specs
//foreach (var content in product.TechnicalInfo.Specifications)
//{
//var GroupCode = content.GroupCode;

//#region ProductAttributeGroupMetaData

//var productAttributeGroupMetaData =
//productAttributeGroups.FirstOrDefault(c => c.GroupCode == content.GroupCode);
////create ProductAttributeGroupMetaData if not exists
//if (productAttributeGroupMetaData == null)
//{
//productAttributeGroupMetaData = new ProductAttributeGroupMetaData
//{
//Index = 0,
//GroupCode = content.GroupCode,
//VendorID = VendorID,
//ProductAttributeGroupNames = new List<ProductAttributeGroupName>()
//};
//_attrGroupRepo.Add(productAttributeGroupMetaData);
//productAttributeGroups.Add(productAttributeGroupMetaData);
//}
//#endregion

//#region ProductAttributeGroupName

//var productAttributeGroupName =
//productAttributeGroupMetaData.ProductAttributeGroupNames.FirstOrDefault(c => c.LanguageID == languageID);
////create ProductAttributeGroupName if not exists
//if (productAttributeGroupName == null)
//{
//productAttributeGroupName = new ProductAttributeGroupName
//{
//Name = content.Name,
//ProductAttributeGroupMetaData = productAttributeGroupMetaData,
//LanguageID = languageID
//};
//_attrGroupName.Add(productAttributeGroupName);
//}

//#endregion

//foreach (var attribute in content.AttributeCodes)
//{
//#region ProductAttributeMetaData

////create ProductAttributeMetaData as many times that there are entrys in content.AttributeCodes

//var productAttributeMetaData =
//  productAttributes.FirstOrDefault(c => c.AttributeCode == attribute.AttributeCode.ToString());
////create ProductAttributeMetaData if not exists
//if (productAttributeMetaData == null)
//{
//productAttributeMetaData = new ProductAttributeMetaData
//                              {
//                                ProductAttributeGroupMetaData = productAttributeGroupMetaData,
//                                AttributeCode = attribute.AttributeCode.ToString(),
//                                Index = 0,
//                                IsVisible = true,
//                                NeedsUpdate = true,
//                                VendorID = VendorID,
//                                IsSearchable = false,
//                                ProductAttributeNames = new List<ProductAttributeName>(),
//                                ProductAttributeValues = new List<ProductAttributeValue>()
//                              };
//_attrRepo.Add(productAttributeMetaData);
//productAttributes.Add(productAttributeMetaData);
//}

//#endregion

//#region ProductAttributeName

//var productAttributeName =
//productAttributeMetaData.ProductAttributeNames.FirstOrDefault(c => c.LanguageID == languageID);
////create ProductAttributeName with generated productAttributeMetaData.AttributeID if not exists
//if (productAttributeName == null)
//{
//productAttributeName = new ProductAttributeName
//                          {
//                            ProductAttributeMetaData = productAttributeMetaData,
//                            LanguageID = languageID,
//                            Name = attribute.AttributeCode.ToString()
//                          };
//_attrNameRepo.Add(productAttributeName);
//}

//#endregion

//#region ProductAttributeValue

//var productAttributeValue =
//productAttributeMetaData.ProductAttributeValues.FirstOrDefault(c => c.ProductID == prod.ProductID);
////create ProductAttributeValue with generated productAttributeMetaData.AttributeID
//if (productAttributeValue == null)
//{
//productAttributeValue = new ProductAttributeValue
//                          {
//                            ProductAttributeMetaData = productAttributeMetaData,
//                            Product = prod,
//                            Value = attribute.AttributeValue,
//                            LanguageID = languageID
//                          };
//_attrValueRepo.Add(productAttributeValue);
//}

//#endregion
//}
//}

//// for the OS_Support specs
//foreach (var content in product.TechnicalInfo.OS_Support)
//{
//var GroupCode = content.GroupCode;

//#region ProductAttributeGroupMetaData

//var productAttributeGroupMetaData =
//productAttributeGroups.FirstOrDefault(c => c.GroupCode == content.GroupCode);
////create ProductAttributeGroupMetaData if not exists
//if (productAttributeGroupMetaData == null)
//{
//productAttributeGroupMetaData = new ProductAttributeGroupMetaData
//{
//Index = 0,
//GroupCode = content.GroupCode,
//VendorID = VendorID,
//ProductAttributeGroupNames = new List<ProductAttributeGroupName>()
//};
//_attrGroupRepo.Add(productAttributeGroupMetaData);
//productAttributeGroups.Add(productAttributeGroupMetaData);
//}
//#endregion

//#region ProductAttributeGroupName

//var productAttributeGroupName =
//productAttributeGroupMetaData.ProductAttributeGroupNames.FirstOrDefault(c => c.LanguageID == languageID);
////create ProductAttributeGroupName if not exists
//if (productAttributeGroupName == null)
//{
//productAttributeGroupName = new ProductAttributeGroupName
//{
//Name = content.Name,
//ProductAttributeGroupMetaData = productAttributeGroupMetaData,
//LanguageID = languageID
//};
//_attrGroupName.Add(productAttributeGroupName);
//}

//#endregion

//foreach (var attribute in content.AttributeCodes)
//{
//#region ProductAttributeMetaData

////create ProductAttributeMetaData as many times that there are entrys in content.AttributeCodes

//var productAttributeMetaData =
//  productAttributes.FirstOrDefault(c => c.AttributeCode == attribute.AttributeCode.ToString());
////create ProductAttributeMetaData if not exists
//if (productAttributeMetaData == null)
//{
//productAttributeMetaData = new ProductAttributeMetaData
//                              {
//                                ProductAttributeGroupMetaData = productAttributeGroupMetaData,
//                                AttributeCode = attribute.AttributeCode.ToString(),
//                                Index = 0,
//                                IsVisible = true,
//                                NeedsUpdate = true,
//                                VendorID = VendorID,
//                                IsSearchable = false,
//                                ProductAttributeNames = new List<ProductAttributeName>(),
//                                ProductAttributeValues = new List<ProductAttributeValue>()
//                              };
//_attrRepo.Add(productAttributeMetaData);
//productAttributes.Add(productAttributeMetaData);
//}

//#endregion

//#region ProductAttributeName

//var productAttributeName =
//productAttributeMetaData.ProductAttributeNames.FirstOrDefault(c => c.LanguageID == languageID);
////create ProductAttributeName with generated productAttributeMetaData.AttributeID if not exists
//if (productAttributeName == null)
//{
//productAttributeName = new ProductAttributeName
//                          {
//                            ProductAttributeMetaData = productAttributeMetaData,
//                            LanguageID = languageID,
//                            Name = attribute.AttributeCode.ToString()
//                          };
//_attrNameRepo.Add(productAttributeName);
//}

//#endregion

//#region ProductAttributeValue

//var productAttributeValue =
//productAttributeMetaData.ProductAttributeValues.FirstOrDefault(c => c.ProductID == prod.ProductID);
////create ProductAttributeValue with generated productAttributeMetaData.AttributeID
//if (productAttributeValue == null)
//{
//productAttributeValue = new ProductAttributeValue
//                          {
//                            ProductAttributeMetaData = productAttributeMetaData,
//                            Product = prod,
//                            Value = attribute.AttributeValue,
//                            LanguageID = languageID
//                          };
//_attrValueRepo.Add(productAttributeValue);
//}

//#endregion
//}
//}

//// for the Language_Support specs
//foreach (var content in product.TechnicalInfo.Language_Support)
//{
//var GroupCode = content.GroupCode;

//#region ProductAttributeGroupMetaData

//var productAttributeGroupMetaData =
//productAttributeGroups.FirstOrDefault(c => c.GroupCode == content.GroupCode);
////create ProductAttributeGroupMetaData if not exists
//if (productAttributeGroupMetaData == null)
//{
//productAttributeGroupMetaData = new ProductAttributeGroupMetaData
//{
//Index = 0,
//GroupCode = content.GroupCode,
//VendorID = VendorID,
//ProductAttributeGroupNames = new List<ProductAttributeGroupName>()
//};
//_attrGroupRepo.Add(productAttributeGroupMetaData);
//productAttributeGroups.Add(productAttributeGroupMetaData);
//}
//#endregion

//#region ProductAttributeGroupName

//var productAttributeGroupName =
//productAttributeGroupMetaData.ProductAttributeGroupNames.FirstOrDefault(c => c.LanguageID == languageID);
////create ProductAttributeGroupName if not exists
//if (productAttributeGroupName == null)
//{
//productAttributeGroupName = new ProductAttributeGroupName
//{
//Name = content.Name,
//ProductAttributeGroupMetaData = productAttributeGroupMetaData,
//LanguageID = languageID
//};
//_attrGroupName.Add(productAttributeGroupName);
//}

//#endregion

//foreach (var attribute in content.AttributeCodes)
//{
//#region ProductAttributeMetaData

////create ProductAttributeMetaData as many times that there are entrys in content.AttributeCodes

//var productAttributeMetaData =
//  productAttributes.FirstOrDefault(c => c.AttributeCode == attribute.AttributeCode.ToString());
////create ProductAttributeMetaData if not exists
//if (productAttributeMetaData == null)
//{
//productAttributeMetaData = new ProductAttributeMetaData
//{
//  ProductAttributeGroupMetaData = productAttributeGroupMetaData,
//  AttributeCode = attribute.AttributeCode.ToString(),
//  Index = 0,
//  IsVisible = true,
//  NeedsUpdate = true,
//  VendorID = VendorID,
//  IsSearchable = false,
//  ProductAttributeNames = new List<ProductAttributeName>(),
//  ProductAttributeValues = new List<ProductAttributeValue>()
//};
//_attrRepo.Add(productAttributeMetaData);
//productAttributes.Add(productAttributeMetaData);
//}

//#endregion

//#region ProductAttributeName

//var productAttributeName =
//productAttributeMetaData.ProductAttributeNames.FirstOrDefault(c => c.LanguageID == languageID);
////create ProductAttributeName with generated productAttributeMetaData.AttributeID if not exists
//if (productAttributeName == null)
//{
//productAttributeName = new ProductAttributeName
//{
//  ProductAttributeMetaData = productAttributeMetaData,
//  LanguageID = languageID,
//  Name = attribute.AttributeCode.ToString()
//};
//_attrNameRepo.Add(productAttributeName);
//}

//#endregion

//#region ProductAttributeValue

//var productAttributeValue =
//productAttributeMetaData.ProductAttributeValues.FirstOrDefault(c => c.ProductID == prod.ProductID);
////create ProductAttributeValue with generated productAttributeMetaData.AttributeID
//if (productAttributeValue == null)
//{
//productAttributeValue = new ProductAttributeValue
//{
//  ProductAttributeMetaData = productAttributeMetaData,
//  Product = prod,
//  Value = attribute.AttributeValue,
//  LanguageID = languageID
//};
//_attrValueRepo.Add(productAttributeValue);
//}

//#endregion
//}
//}

//#region productType specs

//// for the shipping specs
//foreach (var content in product.Fulfillment_Info.Max_Quantity_Per_Shipment_Charge)
//{
//#region ProductAttributeGroupMetaData

//var GroupCode = "General";

//var productAttributeGroupMetaData =
//productAttributeGroups.FirstOrDefault(c => c.GroupCode == GroupCode);
////create ProductAttributeGroupMetaData if not exists
//if (productAttributeGroupMetaData == null)
//{
//productAttributeGroupMetaData = new ProductAttributeGroupMetaData
//{
//Index = 0,
//GroupCode = GroupCode,
//VendorID = VendorID,
//ProductAttributeGroupNames = new List<ProductAttributeGroupName>()
//};
//_attrGroupRepo.Add(productAttributeGroupMetaData);
//productAttributeGroups.Add(productAttributeGroupMetaData);
//}
//#endregion

//#region ProductAttributeGroupName

//var productAttributeGroupName =
//productAttributeGroupMetaData.ProductAttributeGroupNames.FirstOrDefault(c => c.LanguageID == languageID);
////create ProductAttributeGroupName if not exists
//if (productAttributeGroupName == null)
//{
//productAttributeGroupName = new ProductAttributeGroupName
//{
//Name = "General",
//ProductAttributeGroupMetaData = productAttributeGroupMetaData,
//LanguageID = languageID
//};
//_attrGroupName.Add(productAttributeGroupName);
//}

//#endregion

//#region ProductAttributeMetaData

////create ProductAttributeMetaData as many times that there are entrys in content.AttributeCodes

//var productAttributeMetaData =
//productAttributes.FirstOrDefault(c => c.AttributeCode == content.Name);
////create ProductAttributeMetaData if not exists
//if (productAttributeMetaData == null)
//{
//productAttributeMetaData = new ProductAttributeMetaData
//{
//ProductAttributeGroupMetaData = productAttributeGroupMetaData,
//AttributeCode = content.Name,
//Index = 0,
//IsVisible = true,
//NeedsUpdate = true,
//VendorID = VendorID,
//IsSearchable = false
//};
//_attrRepo.Add(productAttributeMetaData);
//productAttributes.Add(productAttributeMetaData);
//}

//#endregion

//#region ProductAttributeName

//var productAttributeName =
//productAttributeMetaData.ProductAttributeNames.FirstOrDefault(c => c.LanguageID == languageID);
////create ProductAttributeName with generated productAttributeMetaData.AttributeID if not exists
//if (productAttributeName == null)
//{
//productAttributeName = new ProductAttributeName
//{
//ProductAttributeMetaData = productAttributeMetaData,
//LanguageID = languageID,
//Name = content.Name
//};
//_attrNameRepo.Add(productAttributeName);
//}

//#endregion

//#region ProductAttributeValue

//var productAttributeValue =
//productAttributeMetaData.ProductAttributeValues.FirstOrDefault(c => c.ProductID == prod.ProductID);
////create ProductAttributeValue with generated productAttributeMetaData.AttributeID
//if (productAttributeValue == null)
//{
//productAttributeValue = new ProductAttributeValue
//{
//ProductAttributeMetaData = productAttributeMetaData,
//Product = prod,
//Value = content.Value,
//LanguageID = languageID
//};
//_attrValueRepo.Add(productAttributeValue);
//}

//#endregion


//}

//// for the product type specs
//foreach (var content in product.Fulfillment_Info.Fulfillment_Method)
//{
//#region ProductAttributeGroupMetaData

//var GroupCode = "General";

//var productAttributeGroupMetaData =
//productAttributeGroups.FirstOrDefault(c => c.GroupCode == GroupCode);
////create ProductAttributeGroupMetaData if not exists
//if (productAttributeGroupMetaData == null)
//{
//productAttributeGroupMetaData = new ProductAttributeGroupMetaData
//{
//Index = 0,
//GroupCode = GroupCode,
//VendorID = VendorID
//};
//_attrGroupRepo.Add(productAttributeGroupMetaData);
//productAttributeGroups.Add(productAttributeGroupMetaData);
//}
//#endregion

//#region ProductAttributeGroupName

//var productAttributeGroupName =
//productAttributeGroupMetaData.ProductAttributeGroupNames.FirstOrDefault(c => c.LanguageID == languageID);
////create ProductAttributeGroupName if not exists
//if (productAttributeGroupName == null)
//{
//productAttributeGroupName = new ProductAttributeGroupName
//{
//Name = "General",
//ProductAttributeGroupMetaData = productAttributeGroupMetaData,
//LanguageID = languageID
//};
//_attrGroupName.Add(productAttributeGroupName);
//}

//#endregion

//#region ProductAttributeMetaData

////create ProductAttributeMetaData as many times that there are entrys in content.AttributeCodes

//var productAttributeMetaData =
//productAttributes.FirstOrDefault(c => c.AttributeCode == content.Name);
////create ProductAttributeMetaData if not exists
//if (productAttributeMetaData == null)
//{
//productAttributeMetaData = new ProductAttributeMetaData
//{
//ProductAttributeGroupMetaData = productAttributeGroupMetaData,
//AttributeCode = content.Name,
//Index = 0,
//IsVisible = true,
//NeedsUpdate = true,
//VendorID = VendorID,
//IsSearchable = false,
//ProductAttributeNames = new List<ProductAttributeName>(),
//ProductAttributeValues = new List<ProductAttributeValue>()
//};
//_attrRepo.Add(productAttributeMetaData);
//productAttributes.Add(productAttributeMetaData);
//}

//#endregion

//#region ProductAttributeName

//var productAttributeName =
//productAttributeMetaData.ProductAttributeNames.FirstOrDefault(c => c.LanguageID == languageID);
////create ProductAttributeName with generated productAttributeMetaData.AttributeID if not exists
//if (productAttributeName == null)
//{
//productAttributeName = new ProductAttributeName
//{
//ProductAttributeMetaData = productAttributeMetaData,
//LanguageID = languageID,
//Name = content.Name
//};
//_attrNameRepo.Add(productAttributeName);
//}

//#endregion

//#region ProductAttributeValue

//var productAttributeValue =
//productAttributeMetaData.ProductAttributeValues.FirstOrDefault(c => c.ProductID == prod.ProductID);
////create ProductAttributeValue with generated productAttributeMetaData.AttributeID
//if (productAttributeValue == null)
//{
//productAttributeValue = new ProductAttributeValue
//{
//ProductAttributeMetaData = productAttributeMetaData,
//Product = prod,
//Value = content.Value,
//LanguageID = languageID
//};
//_attrValueRepo.Add(productAttributeValue);
//}

//#endregion


//}
//#endregion

//// for the country restrictions
//foreach (var content in product.Fulfillment_Info.Fulfillment_Restriction)
//{
//var GroupCode = "General";

//#region ProductAttributeGroupMetaData

//var productAttributeGroupMetaData =
//productAttributeGroups.FirstOrDefault(c => c.GroupCode == GroupCode);
////create ProductAttributeGroupMetaData if not exists
//if (productAttributeGroupMetaData == null)
//{
//productAttributeGroupMetaData = new ProductAttributeGroupMetaData
//{
//Index = 0,
//GroupCode = GroupCode,
//VendorID = VendorID,
//ProductAttributeGroupNames = new List<ProductAttributeGroupName>()
//};
//_attrGroupRepo.Add(productAttributeGroupMetaData);
//productAttributeGroups.Add(productAttributeGroupMetaData);
//}
//#endregion

//#region ProductAttributeGroupName

//var productAttributeGroupName =
//productAttributeGroupMetaData.ProductAttributeGroupNames.FirstOrDefault(c => c.LanguageID == languageID);
////create ProductAttributeGroupName if not exists
//if (productAttributeGroupName == null)
//{
//productAttributeGroupName = new ProductAttributeGroupName
//{
//Name = "General",
//ProductAttributeGroupMetaData = productAttributeGroupMetaData,
//LanguageID = languageID
//};
//_attrGroupName.Add(productAttributeGroupName);
//}

//#endregion

//#region ProductAttributeMetaData

////create ProductAttributeMetaData as many times that there are entrys in content.AttributeCodes

//var productAttributeMetaData =
//productAttributes.FirstOrDefault(c => c.AttributeCode == content.Name);
////create ProductAttributeMetaData if not exists
//if (productAttributeMetaData == null)
//{
//productAttributeMetaData = new ProductAttributeMetaData
//{
//ProductAttributeGroupMetaData = productAttributeGroupMetaData,
//AttributeCode = content.Name,
//Index = 0,
//IsVisible = true,
//NeedsUpdate = true,
//VendorID = VendorID,
//IsSearchable = false,
//ProductAttributeNames = new List<ProductAttributeName>(),
//ProductAttributeValues = new List<ProductAttributeValue>()
//};
//_attrRepo.Add(productAttributeMetaData);
//productAttributes.Add(productAttributeMetaData);
//}

//#endregion

//#region ProductAttributeName

//var productAttributeName =
//productAttributeMetaData.ProductAttributeNames.FirstOrDefault(c => c.LanguageID == languageID);
////create ProductAttributeName with generated productAttributeMetaData.AttributeID if not exists
//if (productAttributeName == null)
//{
//productAttributeName = new ProductAttributeName
//{
//ProductAttributeMetaData = productAttributeMetaData,
//LanguageID = languageID,
//Name = content.Name
//};
//_attrNameRepo.Add(productAttributeName);
//}

//#endregion

//#region ProductAttributeValue

//var countries = string.Empty;
//foreach (var country in content.Counties)
//countries = countries + country + ",";

//var productAttributeValue =
//productAttributeMetaData.ProductAttributeValues.FirstOrDefault(c => c.ProductID == prod.ProductID);
////create ProductAttributeValue with generated productAttributeMetaData.AttributeID
//if (productAttributeValue == null)
//{
//productAttributeValue = new ProductAttributeValue
//{
//ProductAttributeMetaData = productAttributeMetaData,
//Product = prod,
//Value = countries,
//LanguageID = languageID
//};
//_attrValueRepo.Add(productAttributeValue);
//}

//#endregion
//}

//#region relatedProducts
////related products
//var vendorAssortments = _assortmentRepo.GetAll(c => c.VendorID == VendorID);



//RelatedProductTypes relatedProductTypes = new RelatedProductTypes(unit.Scope.Repository<RelatedProductType>());
//var relatedProductType = relatedProductTypes.SyncRelatedProductTypes("RelatedProduct");

//var relatedProducts = unit.Scope.Repository<RelatedProduct>().GetAll(c => c.VendorID == VendorID).ToList();

//foreach (var product in itemProducts)
//{
//foreach (var relatedProd in product.ProductReferences)
//{
//var prod =
//vendorAssortments.Where(p => p.CustomItemNumber == product.CustomItemNr).FirstOrDefault();

//if (prod != null)
//{
//var prodID = prod.ProductID;

//if (prodID != null)
//{
//var relatedProductRef =
//vendorAssortments.Where(p => p.CustomItemNumber == relatedProd.RelatedProductID).FirstOrDefault();

//if (relatedProductRef != null)
//{
//var relatedProductID = relatedProductRef.ProductID;

//if (relatedProductID != null)
//{
//var relatedProduct = (from relProd in relatedProducts
//                  where relProd.RelatedProductID == relatedProductID
//                  select relProd).FirstOrDefault();

//if (relatedProduct == null)
//{
//relatedProduct = new RelatedProduct
//                  {
//                    ProductID = prodID,
//                    RelatedProductID = relatedProductID,
//                    VendorID = VendorID,
//                    RelatedProductType = relatedProductType
//                  };
//unit.Scope.Repository<RelatedProduct>().Add(relatedProduct);
//relatedProducts.Add(relatedProduct);
//}
//}
//}
//}
//}
//}
//}
//unit.Save();
//#endregion