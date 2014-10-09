//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Concentrator.Objects.ConcentratorService;
//using Concentrator.Objects;
//using System.Data.Linq;
//using System.Xml.Linq;
//using System.Xml.Serialization;
//using Concentrator.Plugins.BrightPoint.BrightPointService;
//using System.IO;
//using System.Xml;
//using System.Transactions;
//using Concentrator.Objects.Enumerations;
//using Concentrator.Objects.Utility;
//using Concentrator.Objects.Models.Vendors;
//using Concentrator.Objects.Models.Products;
//using Concentrator.Objects.Models.Brands;
//using Concentrator.Objects.Models.Attributes;
//using System.Diagnostics;

//namespace Concentrator.Plugins.BrightPoint
//{
//    public class ProductImport : ConcentratorPlugin
//    {
//        public override string Name
//        {
//            get { return "BrightPoint product import"; }
//        }

//        private const int UnMappedID = -1;

//        public enum BrightPointStockStatus
//        {
//            Preorder = -10,
//            Active = 10,
//            ActiveNotForSale = 20,
//            Outgoing = 30,
//            OutgoingNotForSale = 35
//        }

//        //public enum BrightPointLanguages
//        //{
//        //    Danish = "DAN",
//        //    English = "ENG",
//        //    Estonian = "EST"
//        //}

//        protected override void Process()
//        {
//            var config = GetConfiguration();

//            var CustNo = config.AppSettings.Settings["BrightPointCustomerNo"].Value;
//            var Pass = config.AppSettings.Settings["BrightPointPassword"].Value;
//            var Instance = config.AppSettings.Settings["BrightPointInstance"].Value;
//            var Site = config.AppSettings.Settings["BrightPointSite"].Value;
//            var WorkingDirectory = config.AppSettings.Settings["WorkingDirectory"].Value;
//            int VendorID = Int32.Parse(config.AppSettings.Settings["vendorID"].Value);

//            if (!Directory.Exists(WorkingDirectory))
//                Directory.CreateDirectory(WorkingDirectory);

//            BrightPointService.AuthHeaderUser authHeader = new BrightPointService.AuthHeaderUser();

//            authHeader.sCustomerNo = CustNo;
//            authHeader.sInstance = Instance;
//            authHeader.sPassword = Pass;
//            authHeader.sSite = Site;

//            #region Get Data

//            BrightPointService.Category[] brandsList = null;
//            BrightPointService.Category[] categories_LOB = null;
//            BrightPointService.Category[] categories_Types = null;
//            //BrightPointService.Language[] languages = null;
//            //BrightPointService.InventoryInStock[] inventory;
//            //BrightPointService.SalesPart_ProjectStockItems[] pstock;
//            BrightPointService.SalesPart[] catalog = null;

//            using (BrightPointService.PartcatalogSoapClient client = new BrightPointService.PartcatalogSoapClient("Part catalogSoap"))
//            {
//                try
//                {
//                    #region salesPart
//                    var salesFile = Path.Combine(WorkingDirectory, "SalesPart.xml");


//                    if (!File.Exists(salesFile) || new FileInfo(salesFile).LastWriteTime.AddHours(1) < DateTime.Now)
//                    {
//                        catalog = client.getSalesPartCatalog(authHeader);
//                        XmlSerializer seri = new XmlSerializer(typeof(SalesPart[]));
//                        using (TextWriter writer = new StreamWriter(salesFile))
//                        {
//                            seri.Serialize(writer, catalog);
//                        }
//                    }

//                    //catalog from file
//                    XmlSerializer ser = new XmlSerializer(typeof(SalesPart[]));
//                    using (StreamReader reader = new StreamReader(salesFile))
//                    {
//                        catalog = (SalesPart[])ser.Deserialize(reader);
//                    }
//                    #endregion

//                    #region Brandlist
//                    var brandFile = Path.Combine(WorkingDirectory, "Brand.xml");

//                    if (!File.Exists(brandFile) || new FileInfo(brandFile).LastWriteTime.AddHours(1) < DateTime.Now)
//                    {

//                        brandsList = client.getCategory_Brand(authHeader);
//                        XmlSerializer serBrand = new XmlSerializer(typeof(Category[]));
//                        using (TextWriter writer = new StreamWriter(brandFile))
//                        {
//                            serBrand.Serialize(writer, brandsList);
//                        }
//                    }
//                    //brandslist from file
//                    XmlSerializer ser2 = new XmlSerializer(typeof(Category[]));
//                    using (StreamReader reader = new StreamReader(brandFile))
//                    {
//                        brandsList = (Category[])ser2.Deserialize(reader);
//                    }
//                    #endregion

//                    #region Catalog
//                    var categoryFile = Path.Combine(WorkingDirectory, "Category.xml");

//                    if (!File.Exists(categoryFile) || new FileInfo(categoryFile).LastWriteTime.AddHours(1) < DateTime.Now)
//                    {
//                        categories_LOB = client.getCategory_LineOfBusiness(authHeader, "NED");
//                        XmlSerializer serCat = new XmlSerializer(typeof(Category[]));
//                        using (TextWriter writer = new StreamWriter(categoryFile))
//                        {
//                            serCat.Serialize(writer, categories_LOB);
//                        }
//                    }
//                    //LOB list from file
//                    XmlSerializer ser3 = new XmlSerializer(typeof(Category[]));
//                    using (StreamReader reader = new StreamReader(categoryFile))
//                    {
//                        categories_LOB = (Category[])ser3.Deserialize(reader);
//                    }
//                    #endregion

//                    #region Catalog_types
//                    var categoryFileType = Path.Combine(WorkingDirectory, "Category_types.xml");

//                    if (!File.Exists(categoryFileType) || new FileInfo(categoryFileType).LastWriteTime.AddHours(1) < DateTime.Now)
//                    {
//                        //Types list from file
//                        categories_Types = client.getCategory_Types(authHeader, "NED");
//                        XmlSerializer serCatType = new XmlSerializer(typeof(Category[]));
//                        using (TextWriter writer = new StreamWriter(categoryFileType))
//                        {
//                            serCatType.Serialize(writer, categories_Types);
//                        }
//                    }

//                    XmlSerializer ser4 = new XmlSerializer(typeof(Category[]));
//                    using (StreamReader reader = new StreamReader(categoryFileType))
//                    {
//                        categories_Types = (Category[])ser4.Deserialize(reader);
//                    }
//                    #endregion

//                    //    XmlSerializer ser = new XmlSerializer(typeof(SalesPart[]));
//                    //    //using (TextWriter writer = new StreamWriter(@"C:\test.xml"))
//                    //    //{
//                    //    //    ser.Serialize(writer, a);
//                    //    //}
//                    //    using (StreamReader reader = new StreamReader(@"C:\test.xml"))
//                    //    {
//                    //        catalog = (SalesPart[])ser.Deserialize(reader);
//                    //    }
//                }
//                catch (Exception ex)
//                {
//                    log.Error("Error Brightpoint", ex);
//                }


//                //var brands = (from b in brandsList
//                //              select new
//                //              {
//                //                  BrandCode = b.Code,
//                //                  BrandName = b.Value,
//                //              });
//            #endregion


//                //var productData = (from p in catalog
//                //                   let b = brandsList
//                //                   select new
//                //                   {
//                //                       CatalogGoup = p.CatalogGroup,
//                //                       BrandID = getBrandID(b, p)
//                //                   }).ToList();


//                // var products = catalog.ToList();


//                //ConcentratorDataContext ctx = new ConcentratorDataContext();

//                using (var unit = GetUnitOfWork())
//                {
//                    var repoProductGroupVendors = unit.Scope.Repository<ProductGroupVendor>();
//                    var repoBrandVendors = unit.Scope.Repository<BrandVendor>();
//                    var repoAttributeMetadata = unit.Scope.Repository<ProductAttributeMetaData>();
//                    var repoAttributeGroupMetadata = unit.Scope.Repository<ProductAttributeGroupMetaData>();
//                    var repoProduct = unit.Scope.Repository<Product>();
//                    var repoAssortment = unit.Scope.Repository<VendorAssortment>();
//                    var repoStock = unit.Scope.Repository<VendorStock>();
//                    var repoPrices = unit.Scope.Repository<VendorPrice>();
//                    var repoMedia = unit.Scope.Repository<ProductMedia>();
//                    var repoDescription = unit.Scope.Repository<ProductDescription>();
//                    var repoBarcode = unit.Scope.Repository<ProductBarcode>();
//                    var repoAttrName = unit.Scope.Repository<ProductAttributeName>();
//                    var repoAttrGroupName = unit.Scope.Repository<ProductAttributeGroupName>();
//                    var repoAttrValue = unit.Scope.Repository<ProductAttributeValue>();


//                    ProductStatusVendorMapper mapper = new ProductStatusVendorMapper(unit.Scope.Repository<VendorProductStatus>(), VendorID);

//                    var currentProductGroupVendors = repoProductGroupVendors.GetAll(v => v.Vendor.VendorID == VendorID).ToList();

//                    var brands = repoBrandVendors.GetAll(bv => bv.VendorID == VendorID).ToList();

//                    var productGroupVendorRecords = repoProductGroupVendors.GetAll(pc => pc.VendorID == VendorID).ToList();

//                    var productAttributes = repoAttributeMetadata.GetAll(g => g.VendorID == VendorID).ToList();
//                    var productAttributeGroups = repoAttributeGroupMetadata.GetAll(g => g.VendorID == VendorID).ToList();

//                    int counter = 0;
//                    int total = catalog.Count();
//                    int totalNumberOfProductsToProcess = total;
//                    log.InfoFormat("Start import {0} products", total);

                   

//                    foreach (var product in catalog)
//                    { Stopwatch stopWatch = new Stopwatch();
//                        //foreach (var lang in languages)
//                        //{
//                        var currLanguage = 2;

//                        //TODO: set language code

//                        BrightPointService.SalesPart_Attributes[] attributes = null;

//                        //get product attributes
//                        try
//                        {
//                            stopWatch.Start();
//                            attributes = client.getSalesPartCatalog_Attributes(authHeader, product.SalesPartID, "NED");
//                            while (stopWatch.Elapsed.Minutes < 1)
//                            {

//                            }

//                            stopWatch.Stop();
//                        }
//                        catch (Exception ex)
//                        {
//                            log.WarnFormat("Error while retreiving product attributes");
//                        }

//                        if (counter == 100)
//                        {
//                            counter = 0;
//                            log.InfoFormat("Still need to process {0} of {1}; {2} done;", totalNumberOfProductsToProcess, total, total - totalNumberOfProductsToProcess);
//                        }
//                        totalNumberOfProductsToProcess--;
//                        counter++;

//                        #region Brand

//                        var br = product.CatalogGroup.Substring(0, 2);
//                        var lob = product.CatalogGroup.Substring(2, 2);
//                        var type = product.CatalogGroup.Substring(4, 2);

//                        var vbrand = brands.FirstOrDefault(vb => vb.VendorBrandCode == brandsList.First(x => x.Code == br).Code);

//                        if (vbrand == null)
//                        {
//                            vbrand = new BrandVendor
//                            {
//                                VendorID = VendorID,
//                                VendorBrandCode = brandsList.FirstOrDefault(x => x.Code == br) != null ? brandsList.FirstOrDefault(x => x.Code == br).Code : br,
//                                BrandID = UnMappedID
//                            };

//                            brands.Add(vbrand);

//                            repoBrandVendors.Add(vbrand);
//                        }

//                        vbrand.Name = brandsList.First(x => x.Code == br).Value;

//                        #endregion Brand

//                        #region Product

//                        var
//                          item = repoProduct.GetSingle(p => p.VendorItemNumber == product.InventoryManufacturerPartNo && p.BrandID == vbrand.BrandID);
//                        if (item == null)
//                        {
//                            item = new Product
//                            {
//                                VendorItemNumber = product.InventoryManufacturerPartNo.ToString(),
//                                BrandID = vbrand.BrandID,
//                                SourceVendorID = VendorID,
//                                VendorAssortments = new List<VendorAssortment>(),
//                                ProductDescriptions = new List<ProductDescription>(),
//                                ProductBarcodes = new List<ProductBarcode>()
//                            };
//                            repoProduct.Add(item);
//                            //unit.Save();
//                        }
//                        else
//                        {
//                            item.BrandID = vbrand.BrandID;
//                        }

//                        #endregion Product

//                        #region Vendor assortment

//                        var assortment = item.VendorAssortments.FirstOrDefault(va => va.VendorID == VendorID);
//                        if (assortment == null)
//                        {
//                            assortment = new VendorAssortment
//                            {
//                                VendorID = VendorID,
//                                Product = item,
//                                VendorPrices = new List<VendorPrice>(),
//                                ProductGroupVendors = new List<ProductGroupVendor>()
//                            };
//                            item.VendorAssortments.Add(assortment);
//                            repoAssortment.Add(assortment);
//                        }

//                        assortment.ShortDescription = product.Description.Length > 150 ? product.Description.Substring(0, 150) : product.Description;
//                        assortment.CustomItemNumber = product.SalesPartID.ToString();

//                        assortment.IsActive = true;

//                        #region Product group

//                        var vGroup = productGroupVendorRecords.FirstOrDefault(pg => pg.Try(x => x.VendorProductGroupCode1, null) == lob.Trim());

//                        if (vGroup == null)
//                        {
//                            vGroup = new ProductGroupVendor
//                            {
//                                VendorProductGroupCode1 = lob.Trim(),
//                                VendorID = VendorID,
//                                ProductGroupID = UnMappedID
//                            };

//                            productGroupVendorRecords.Add(vGroup);
//                            repoProductGroupVendors.Add(vGroup);
//                        }
//                        vGroup.VendorName = categories_LOB.First(x => x.Code == lob).Value;

//                        var vGroup2 = productGroupVendorRecords.FirstOrDefault(pg => pg.Try(x => x.VendorProductGroupCode2, null) == type.Trim());

//                        if (vGroup2 == null)
//                        {
//                            vGroup2 = new ProductGroupVendor
//                            {
//                                VendorProductGroupCode2 = type.Trim(),
//                                VendorID = VendorID,
//                                ProductGroupID = UnMappedID
//                            };

//                            productGroupVendorRecords.Add(vGroup2);
//                            repoProductGroupVendors.Add(vGroup2);
//                        }
//                        vGroup2.VendorName = categories_Types.First(x => x.Code == type).Value;


//                        #region Sync

//                        if (currentProductGroupVendors.Contains(vGroup))
//                        {
//                            currentProductGroupVendors.Remove(vGroup);
//                        }
//                        #endregion

//                        #endregion Product group

//                        #region Vendor Product Group Assortment

//                        string brandCode = null;
//                        string groupCode1 = lob;
//                        string groupCode2 = type;
//                        string groupCode3 = null;

//                        var records = (from l in productGroupVendorRecords
//                                       where
//                                         ((brandCode != null && l.BrandCode.Trim() == brandCode) || l.BrandCode == null)
//                                         &&
//                                         ((groupCode1 != null && l.VendorProductGroupCode1 != null &&
//                                           l.VendorProductGroupCode1.Trim() == groupCode1) || l.VendorProductGroupCode1 == null)
//                                         &&
//                                         ((groupCode2 != null && l.VendorProductGroupCode2 != null &&
//                                           l.VendorProductGroupCode2.Trim() == groupCode2) || l.VendorProductGroupCode2 == null)
//                                         &&
//                                         ((groupCode3 != null && l.VendorProductGroupCode3 != null &&
//                                           l.VendorProductGroupCode3.Trim() == groupCode3) || l.VendorProductGroupCode3 == null)

//                                       select l).ToList();


//                        List<int> existingProductGroupVendors = new List<int>();

//                        foreach (ProductGroupVendor prodGroupVendor in records)
//                        {
//                            existingProductGroupVendors.Add(prodGroupVendor.ProductGroupVendorID);

//                            if (prodGroupVendor.VendorAssortments == null)
//                            {
//                                prodGroupVendor.VendorAssortments = new List<VendorAssortment>();
//                            }
//                            if (prodGroupVendor.VendorAssortments.Any(x => x.VendorAssortmentID == assortment.VendorAssortmentID))
//                            { // only add new rows
//                                continue;
//                            }
//                            prodGroupVendor.VendorAssortments.Add(assortment);
//                        }

//                        var unusedPGV = new List<ProductGroupVendor>();
//                        assortment.ProductGroupVendors.ForEach((pgv, id) =>
//                        {
//                            if (!existingProductGroupVendors.Contains(pgv.ProductGroupVendorID))
//                            {
//                                unusedPGV.Add(pgv);
//                            }
//                        });

//                        unusedPGV.ForEach((pg) => { assortment.ProductGroupVendors.Remove(pg); });

//                        #endregion


//                        #endregion Vendor assortment

//                        #region Stock
//                        var stock = repoStock.GetSingle(c => c.VendorID == assortment.VendorID && c.ProductID == assortment.ProductID);
//                        if (stock == null)
//                        {
//                            stock = new VendorStock
//                            {
//                                VendorID = VendorID,
//                                Product = item,
//                                VendorStockTypeID = 1
//                            };
//                            repoStock.Add(stock);
//                        }
//                        stock.QuantityOnHand = product.InventoryLevel;
//                        stock.StockStatus = Enum.GetName(typeof(BrightPointStockStatus), product.Flag);
//                        stock.ConcentratorStatusID = mapper.SyncVendorStatus(Enum.GetName(typeof(BrightPointStockStatus), product.Flag), -1);

//                        #endregion Stock

//                        #region Price

//                        var price = assortment.VendorPrices.FirstOrDefault();
//                        if (price == null)
//                        {
//                            price = new VendorPrice
//                            {
//                                VendorAssortment = assortment,
//                                MinimumQuantity = 0
//                            };

//                            repoPrices.Add(price);
//                        }
//                        price.CommercialStatus = String.IsNullOrEmpty(product.Flag.ToString()) ? null : product.Flag.ToString();
//                        price.BasePrice = product.UnitPrice;
//                        price.ConcentratorStatusID = stock.ConcentratorStatusID;

//                        #endregion Price

//                        #region Images


//                        var productMedia = repoMedia.GetSingle(x => x.VendorID == VendorID && x.MediaUrl == product.Image);

//                        if (productMedia == null)
//                        {
//                            productMedia = new ProductMedia
//                            {
//                                VendorID = VendorID,
//                                FileName = product.Image,
//                                TypeID = 1,
//                                Product = item,
//                                Sequence = 0
//                            };
//                            repoMedia.Add(productMedia);
//                        }

//                        productMedia.FileName = product.Image;

//                        #endregion

//                        #region Related Products
//                        //  if (product.CompatibleProducts.compatibleProducts.Count > 0)
//                        //  {
//                        //    if (!relations.ContainsKey(product.Identifiers.OEM))
//                        //    {
//                        //      relations.Add(product.Identifiers.OEM, new ProductIdentifier() { productID = product.CompatibleProducts.productID, compatibleProducts = new List<string>() });
//                        //    }

//                        //    relations[product.Identifiers.OEM].compatibleProducts.AddRange(
//                        //      from cp in product.CompatibleProducts.compatibleProducts
//                        //      where !relations[product.Identifiers.OEM].compatibleProducts.Contains(cp)
//                        //      select cp
//                        //      );
//                        //  }

//                        #endregion

//                        #region Descriptions

//                        var desc = item.ProductDescriptions.FirstOrDefault(pd => pd.VendorID == VendorID);
//                        if (desc == null)
//                        {
//                            desc = new ProductDescription
//                                     {
//                                         VendorID = VendorID,
//                                         LanguageID = currLanguage,
//                                         Product = item
//                                     };
//                            repoDescription.Add(desc);
//                        }

//                        desc.ShortContentDescription = product.Description;

//                        #endregion Descriptions

//                        #region ProductBarcode

//                        if (!item.ProductBarcodes.Any(pb => pb.Barcode.Trim() == product.EANCode))
//                        {
//                            //create ProductBarcode if not exists
//                            repoBarcode.Add(new ProductBarcode
//                             {
//                                 Product = item,
//                                 Barcode = product.EANCode,
//                                 VendorID = VendorID,
//                                 BarcodeType = (int)BarcodeTypes.Default
//                             });
//                        }

//                        #endregion

//                        if (attributes != null && attributes.Length != 0)
//                        {
//                            foreach (var currContent in attributes)
//                            {

//                                #region Specifications

//                                #region Product attribute group
//                                var prAttrGroup = productAttributeGroups.FirstOrDefault(c => IfNull(c.GroupCode) == currContent.GroupName);
//                                if (prAttrGroup == null)
//                                {
//                                    prAttrGroup = new ProductAttributeGroupMetaData
//                                                    {
//                                                        GroupCode = currContent.GroupName,
//                                                        Index = 0,
//                                                        VendorID = VendorID,
//                                                        ProductAttributeGroupNames = new List<ProductAttributeGroupName>()
//                                                    };
//                                    repoAttributeGroupMetadata.Add(prAttrGroup);
//                                    productAttributeGroups.Add(prAttrGroup);
//                                }

//                                var prAttrGroupName = prAttrGroup.ProductAttributeGroupNames.FirstOrDefault(c => c.LanguageID == currLanguage);


//                                if (prAttrGroupName == null)
//                                {
//                                    prAttrGroupName = new ProductAttributeGroupName
//                                                        {
//                                                            ProductAttributeGroupMetaData = prAttrGroup,
//                                                            LanguageID = currLanguage
//                                                        };
//                                    repoAttrGroupName.Add(prAttrGroupName);
//                                }

//                                prAttrGroupName.Name = "General";
//                                #endregion

//                                #region Product Attributes


//                                var productAttrMD = productAttributes.FirstOrDefault(c => c.AttributeCode == currContent.AttributeID.ToString() && c.VendorID == VendorID);

//                                if (productAttrMD == null)
//                                {
//                                    productAttrMD = new ProductAttributeMetaData()
//                                                      {
//                                                          VendorID = VendorID,
//                                                          IsVisible = true,
//                                                          AttributeCode = currContent.AttributeID.ToString(),
//                                                          ProductAttributeGroupMetaData = prAttrGroup,
//                                                          ProductAttributeValues = new List<ProductAttributeValue>(),
//                                                          ProductAttributeNames = new List<ProductAttributeName>()
//                                                      };
//                                    repoAttributeMetadata.Add(productAttrMD);
//                                    productAttributes.Add(productAttrMD);
//                                }

//                                var productAttrName = productAttrMD.ProductAttributeNames.FirstOrDefault(c => c.LanguageID == currLanguage);

//                                if (productAttrName == null)
//                                {
//                                    productAttrName = new ProductAttributeName()
//                                                        {
//                                                            LanguageID = currLanguage,
//                                                            ProductAttributeMetaData = productAttrMD
//                                                        };
//                                    repoAttrName.Add(productAttrName);
//                                }
//                                productAttrName.Name = currContent.AttributeName;

//                                var attrValue = productAttrMD.ProductAttributeValues.FirstOrDefault(
//                                  c => c.ProductID == item.ProductID);
//                                if (attrValue == null)
//                                {
//                                    attrValue = new ProductAttributeValue
//                                                  {
//                                                      Product = item,
//                                                      ProductAttributeMetaData = productAttrMD,
//                                                      LanguageID = currLanguage,
//                                                  };
//                                    repoAttrValue.Add(attrValue);
//                                }
//                                attrValue.Value = currContent.Description;

//                                #endregion
//                                #endregion
//                            }
//                        }

//                        unit.Save();
//                    }

//                    //}
//                    try
//                    {
//                        using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Suppress, TimeSpan.FromMinutes(10)))
//                        {
//                            unit.Save();
//                            log.AuditSuccess("Products processing finished. Processed " + (total - totalNumberOfProductsToProcess) + " products", "Bright Point import");
//                        }

//                        #region delete unused productgroupvendor records
//                        foreach (var vProdGroup in currentProductGroupVendors)
//                        {
//                            if (vProdGroup.ProductGroupID == -1)
//                            {
//                                repoProductGroupVendors.Delete(vProdGroup);
//                            }
//                        }
//                        #endregion
//                        unit.Save();

//                    }
//                    catch (Exception e)
//                    {
//                        log.AuditFatal("Import failed", e, "Bright Point Import");
//                    }
//                }
//            }
//        }

//        private string IfNull(string input)
//        {
//            if (input != null)
//            {
//                return input.Trim();
//            }
//            else
//            {
//                return null;
//            }
//        }

//        private object getBrandID(Category[] b, SalesPart p)
//        {
//            foreach (var id in b)
//            {
//                if (p.CatalogGroup.StartsWith(id.Code))
//                {
//                    return id;
//                }
//            }
//            return null;
//        }



//    }
//}
