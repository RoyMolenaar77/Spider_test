using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Xml;
using Concentrator.Objects;
using Concentrator.Objects.ConcentratorService;
using System.Xml.Linq;
using System.Transactions;
using System.Diagnostics;
using System.Data.Linq;
using Concentrator.Objects.Ftp;
using Concentrator.Objects.Enumerations;
using Concentrator.Objects.Models.Vendors;
using System.IO;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Models.Brands;
using Concentrator.Objects.Models.Attributes;
using Concentrator.Objects.Utility;
using Concentrator.Objects.Vendors.Bulk;
using Concentrator.Objects.Vendors;
using System.Globalization;

namespace Concentrator.Plugins.AlphaInternational
{
  public class AlphaWebContentImport : VendorBase
  {
    private const string _name = "Alpha International Content Import Plugin";
    private const int UnMappedID = -1;

    protected override Configuration Config
    {
      get { return GetConfiguration(); }
    }

    protected override int VendorID
    {
      get { return int.Parse(Config.AppSettings.Settings["VendorID"].Value); }
    }

    protected override int DefaultVendorID
    {
      get { return int.Parse(Config.AppSettings.Settings["DefaultVendorID"].Value); }
    }

    private Dictionary<string, int> Languages = new Dictionary<string, int>
                                                  {
                                                    { "UK", 1 },
                                                    { "NL", 2 }
                                                  };
    public override string Name
    {
      get { return _name; }
    }

    protected override void Process()
    {
      var config = GetConfiguration();

      FtpManager downloader = new FtpManager(
        config.AppSettings.Settings["AlphaFtpUrl"].Value,
        config.AppSettings.Settings["AlphaPath"].Value,
        config.AppSettings.Settings["AlphaUserName"].Value,
        config.AppSettings.Settings["AlphaPassword"].Value,
       false, true, log);

      log.AuditInfo("Starting import process");

      var timer = Stopwatch.StartNew();
      Dictionary<string, ProductIdentifier> relations = new Dictionary<string, ProductIdentifier>();

      using (var unit = GetUnitOfWork())
      {
        var inactiveVendorAssortment = unit.Scope.Repository<VendorAssortment>().GetAll(x => x.VendorID == VendorID).ToList();

        bool processedSuceeded = true;

        foreach (var file in downloader)
        {
          if (!Running)
            break;

          log.InfoFormat("Processing file: {0}", file.FileName);
          var innerTime = Stopwatch.StartNew();
          XDocument doc = null;
          using (file)
          {
            try
            {
              using (var reader = XmlReader.Create(file.Data))
              {
                reader.MoveToContent();
                doc = XDocument.Load(reader);
              }
            }
            catch (Exception ex)
            {
              log.AuditError(String.Format("Failed to load xml for file: {0}", file.FileName), ex);
              downloader.MarkAsError(file.FileName);
              continue;
            }
          }

          if (ProcessXml(doc, relations)) // Delete file if it is succesfully processed
          {
            log.InfoFormat("Succesfully processed file: {0}", file.FileName);
            downloader.Delete(file.FileName);
          }
          else
          {
            log.InfoFormat("Failed to process file: {0}", file.FileName);
            downloader.MarkAsError(file.FileName);
            processedSuceeded = false;
          }
          log.InfoFormat("File: {0} took: {1}", file.FileName, innerTime.Elapsed);
        }

        if (processedSuceeded)
        {
          inactiveVendorAssortment.ForEach(x => x.IsActive = false);
        }
        unit.Save();

        SyncRelatedProducts(relations);

        timer.Stop();

        log.AuditSuccess(string.Format("Total import process finished in: {0}", timer.Elapsed), "Alha International Import");
      }
    }

    #region Example XML
    /*
   <Products version="1.0"issuedate=20100323">
   <Product id="0011B006AA">
    <Content lang="nl">
      <Description>Printer Inktjet PIXMA iP6220D Kleur</Description>
      <Brand id="CAN">Canon</Brand>
      <UNSPSC id="43212104">Inkjetprinters</UNSPSC>
      <Specifications>
        <Specification id="000027">
          <Description>printerfunctionaliteit</Description>
          <Value>Printer</Value>
        </Specification>
        <Specification id="000029">
          <Description>type printer</Description>
          <Value>Inktjet</Value>
        </Specification>
        <Specification id="000026">
          <Description>printernaam</Description>
          <Value>PIXMA iP6220D</Value>
        </Specification>
        <Specification id="000028">
          <Description>type output</Description>
          <Value>Kleur</Value>
        </Specification>
      </Specifications>
    </Content>
    <Content lang="uk">
    <Description>Printer Inktjet PIXMA iP6220D Colour</Description>
    <Brand id="CAN">Canon</Brand>
    <UNSPSC id="43212104">Inkjet printers</UNSPSC>
      <Specifications>
        <Specification id="000027">
          <Description>printer functionality</Description>
          <Value>Printer</Value>
        </Specification>
        <Specification id="000029">
          <Description>printer type</Description>
          <Value>Inktjet</Value>
        </Specification>
        <Specification id="000026">
          <Description>printer name</Description>
          <Value>PIXMA iP6220D</Value>
        </Specification>
        <Specification id="000028">
          <Description>Output type</Description>
          <Value>Colour</Value>
        </Specification>
      </Specifications>
    </Content>
    <Identifiers>
      <OEM>0011B006AA</OEM>
      <CustomerID>0011B006AA</CustomerID>
      <EANProduct>4960999258423</EANProduct>
      <EANMasterCarton></EANMasterCarton>
      <OnetrailPDI></OnetrailPDI>
    </Identifiers>
    <Purchase>
      <Stock>0</Stock>
      <MOQ>1</MOQ>
      <Status>O</Status>
      <ReplacedBy></ReplacedBy>
    </Purchase>
    <Images>
      <Product>http://images.alpha-international.eu/products/originals/0011B006AA.JPG</Product>
      <Brand>http://images.alpha-international.eu/brands/originals/CAN.GIF</Brand>
    </Images>
    <Logistics>
      <ProductsPerOuterCarton></ProductsPerOuterCarton>
      <ProductsPerLayer></ProductsPerLayer>
      <ProductsPerPallet></ProductsPerPallet>
      <OuterCartonPerPallet></OuterCartonPerPallet>
      <Weights unit="gr">
        <ProductWeight>5600</ProductWeight>
        <OuterCartonWeight></OuterCartonWeight>
      </Weights>
      <Measures unit="cm">
        <ProductLength>50.5</ProductLength>
        <ProductWidth>37.5</ProductWidth>
        <ProductHeight>24.5</ProductHeight>
        <OuterCartonLength></OuterCartonLength>
        <OuterCartonWidth></OuterCartonWidth>
        <OuterCartonHeight></OuterCartonHeight>
      </Measures>
    </Logistics>
    <Compatibility>
      <Product>CAN22165</Product>
      <Product>CAN22167</Product>
      <Product>CAN22166</Product>
      <Product>CAN22168</Product>
    </Compatibility>
    <CrossSell>
    </CrossSell>
  </Product> 
  <Product ... >
     ....
  </Product>
  </Products>
     */
    #endregion

    private bool ProcessXml(XDocument doc, Dictionary<string, ProductIdentifier> relations)
    {
      try
      {
        var products = from pr in doc.Element("Products").Elements("Product")
                       group pr by new { pr.Element("Identifiers").Element("OEM").Value, ID = pr.Attribute("id").Value } into pro
                       let p = pro.First()
                       let identifiers = p.Element("Identifiers")
                       let purchase = p.Element("Purchase")
                       let oemCode = identifiers.Element("OEM").Value
                       let images = p.Element("Images")
                       let compat = p.Element("Compatibility")
                       select new
                                {
                                  VenderItemNumber = p.Attribute("id").Value,
                                  Content = from c in p.Elements("Content")
                                            let brand = c.Element("Brand")
                                            let cat = c.Element("UNSPSC")
                                            select new
                                                     {
                                                       Language = c.Attribute("lang").Value,
                                                       Description = c.Element("Description").Value,
                                                       Brand = new
                                                                 {
                                                                   ID = brand.Attribute("id").Value,
                                                                   Name = brand.Value
                                                                 },
                                                       Category = new
                                                                    {
                                                                      ID = cat.Attribute("id").Value,
                                                                      Name = cat.Value
                                                                    }
                                                       ,
                                                       Specs = c.Element("Specifications") == null ? null :
                                                       from s in c.Element("Specifications").Elements("Specification")
                                                       select new
                                                                {
                                                                  ID = s.Attribute("id").Value,
                                                                  Desc = s.Element("Description").Value,
                                                                  s.Element("Value").Value
                                                                }
                                                     },
                                  Identifiers = new
                                                  {
                                                    OEM = ((oemCode == "ONBEKEND" || string.IsNullOrEmpty(oemCode)) ? p.Attribute("id").Value : oemCode),
                                                    CustomerID = identifiers.Element("CustomerID").Value,
                                                    EAN = identifiers.Element("EANProduct").Value
                                                  },
                                  InStock = purchase.Element("Stock") != null ? (int)purchase.Element("Stock") : 0,
                                  MinQuantity = purchase.Element("MOQ") != null ? (int)purchase.Element("MOQ") : 1,
                                  Status = purchase.Element("Status").Value,
                                  Price = purchase.Element("Price") != null ? (decimal?)purchase.Element("Price") : null,
                                  ProductImages = from i in images.Elements("Product")
                                                  select i.Value,

                                  CompatibleProducts = new ProductIdentifier { productID = p.Attribute("id").Value, compatibleProducts = (compat != null ? (from cp in compat.Elements("Product") select cp.Value).ToList() : new List<string>()) }
                                };


        using (var unit = GetUnitOfWork())
        {
          int counter = 0;
          int total = products.Count();
          int totalNumberOfProductsToProcess = total;
          log.InfoFormat("Start import {0} products", total);

          var productAttributes = unit.Scope.Repository<ProductAttributeMetaData>().GetAll(c => c.VendorID == VendorID).ToList();
          var attributeList = productAttributes.ToDictionary(x => x.AttributeCode, y => y.AttributeID);

          List<Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorAssortmentItem> assortmentList = new List<Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorAssortmentItem>();

          RelatedProductTypes relatedProductTypes = new RelatedProductTypes(unit.Scope.Repository<RelatedProductType>());
          var relatedProductType = relatedProductTypes.SyncRelatedProductTypes("CompatibleProducts");

          var AttributeMapping = (from p in products
                                  where p.Content != null && p.Content.Count() > 0
                                  from a in p.Content.Where(x => x.Specs != null).SelectMany(x => x.Specs)
                                  select a.Desc).Distinct().ToArray();

          List<ProductAttributeMetaData> attributes;
          SetupAttributes(unit, AttributeMapping, out attributes, null);

          foreach (var product in products)
          {
            if (counter == 100)
            {
              counter = 0;
              log.InfoFormat("Still need to process {0} of {1}; {2} done;", totalNumberOfProductsToProcess, total, total - totalNumberOfProductsToProcess);
            }

            totalNumberOfProductsToProcess--;
            counter++;

            //use english content for all non-language specifics
            var enContent = product.Content.FirstOrDefault(c => c.Language.ToUpper() == "UK");

            if (enContent == null)
              enContent = product.Content.FirstOrDefault(c => c.Language.ToUpper() == "NL");
            
            var assortment = new Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorAssortmentItem
            {
              #region BrandVendor
              BrandVendors = new List<VendorAssortmentBulk.VendorImportBrand>()
                        {
                            new VendorAssortmentBulk.VendorImportBrand()
                            {
                                VendorID = VendorID,
                                VendorBrandCode = enContent.Brand.ID, 
                                ParentBrandCode = null,
                                Name = enContent.Brand.Name
                            }
                        },
              #endregion

              #region GeneralProductInfo
              VendorProduct = new VendorAssortmentBulk.VendorProduct
              {
                VendorItemNumber = product.Identifiers.OEM,
                CustomItemNumber = product.VenderItemNumber,
                ShortDescription = enContent.Description.Length > 150 ? enContent.Description.Substring(0, 150) : enContent.Description,
                LongDescription = enContent.Description,
                LineType = null,
                LedgerClass = null,
                ProductDesk = null,
                ExtendedCatalog = null,
                VendorID = VendorID,
                DefaultVendorID = DefaultVendorID,
                VendorBrandCode = enContent.Brand.ID,
                Barcode = product.Identifiers.EAN,
                VendorProductGroupCode1 = enContent.Category.ID,
                VendorProductGroupCodeName1 = enContent.Category.Name
              },
              #endregion

              #region RelatedProducts
              RelatedProducts = new List<VendorAssortmentBulk.VendorImportRelatedProduct>()
                        {
                            new Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportRelatedProduct
                            {
                                VendorID = VendorID,
                                DefaultVendorID = DefaultVendorID,
                                CustomItemNumber = product.VenderItemNumber,
                                RelatedProductType = relatedProductType.Type,
                                RelatedCustomItemNumber = product.VenderItemNumber
                            }
                        },
              #endregion

              #region Attributes
              VendorImportAttributeValues = (from att in product.Content.Where(x => x.Specs != null).SelectMany(x => x.Specs)
                                             let attributeID = attributeList.ContainsKey(att.Desc) ? attributeList[att.Desc] : -1
                                             where !string.IsNullOrEmpty(att.Value)
                                             select new Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportAttributeValue
                                             {
                                               VendorID = VendorID,
                                               DefaultVendorID = DefaultVendorID,
                                               CustomItemNumber = product.VenderItemNumber,
                                               AttributeID = attributeID,
                                               Value = att.Value,
                                               LanguageID = 1.ToString(),
                                               AttributeCode = att.Desc,
                                             }).ToList(),
              #endregion

              #region Prices
              VendorImportPrices = new List<Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportPrice>()
                        {
                            new Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportPrice()
                            {
                                VendorID = VendorID,
                                DefaultVendorID = DefaultVendorID,
                                CustomItemNumber = product.VenderItemNumber,                                
                                Price = product.Price.HasValue ? product.Price.Value.ToString("0.00", CultureInfo.InvariantCulture) : null,
                                CostPrice = null,
                                TaxRate = "19",
                                MinimumQuantity = product.MinQuantity,
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
                                DefaultVendorID = DefaultVendorID,
                                CustomItemNumber = product.VenderItemNumber,
                                QuantityOnHand = 0,
                                StockType = "Assortment",
                                StockStatus = product.Status
                            }
                        },
              #endregion
            };

            assortmentList.Add(assortment);
          }

          using (var vendorAssortmentBulk = new VendorAssortmentBulk(assortmentList, VendorID, VendorID))
          {
            vendorAssortmentBulk.Init(unit.Context);
            vendorAssortmentBulk.Sync(unit.Context);
          }

          return true;
        }
      }
      catch (Exception ex)
      {
        log.AuditFatal(String.Format("Failed to process file"), ex);
        return false;
      }
    }

    private void SyncRelatedProducts(Dictionary<string, ProductIdentifier> relations)
    {
      try
      {
        using (var unit = GetUnitOfWork())
        {
          var repoProduct = unit.Scope.Repository<Product>();
          var repoAssortment = unit.Scope.Repository<VendorAssortment>();
          var relProductRepository = unit.Scope.Repository<RelatedProduct>().Include(c => c.RelatedProductType);

          RelatedProductTypes relatedProductTypes = new RelatedProductTypes(unit.Scope.Repository<RelatedProductType>());
          var relatedProductType = relatedProductTypes.SyncRelatedProductTypes("CompatibleProducts");

          foreach (var compatibility in relations)
          {
            var cp = compatibility.Value;
            var sourceProduct = repoProduct.GetSingle(c => c.VendorItemNumber == compatibility.Key);
            if (sourceProduct != null)
            {
              int sourceProdID = sourceProduct.ProductID;

              foreach (string pi in compatibility.Value.compatibleProducts)
              {
                #region Related Products
                var relatedProductID = repoAssortment.GetSingle(va => va.CustomItemNumber == pi && va.VendorID == VendorID).Try<VendorAssortment, int?>(c => c.ProductID, null);


                if (relatedProductID.HasValue)
                {
                  RelatedProduct relProd = relProductRepository.GetSingle(rp => rp.ProductID == sourceProdID &&
                                              rp.RelatedProductID == relatedProductID.Value &&
                                              rp.VendorID == VendorID &&
                                              rp.RelatedProductType.RelatedProductTypeID == relatedProductType.RelatedProductTypeID);

                  if (relProd == null)
                  {
                    relProd = new RelatedProduct
                                {
                                  ProductID = sourceProdID,
                                  RelatedProductID = relatedProductID.Value,
                                  VendorID = VendorID,
                                  RelatedProductType = relatedProductType
                                };
                    relProductRepository.Add(relProd);
                  }
                }
                #endregion
              }
            }
          }
          using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Suppress, TimeSpan.FromMinutes(3)))
          {
            unit.Save();
            ts.Complete();
          }
        }
      }
      catch (Exception e)
      {
        log.AuditError("Import of related products failed", e);
      }


    }
    public struct ProductIdentifier
    {
      public string productID;
      public List<string> compatibleProducts;

    }

    protected override void SyncProducts()
    {
      Process();
    }
  }
}

//#region delete unused productgroupvendor records
//foreach (var vProdGroup in currentProductGroupVendors)
//{
//  if (vProdGroup.ProductGroupID == -1)
//  {
//    //unit.Scope.Repository<ProductGroupVendor>().Delete(vProdGroup);
//  }
//}
//#endregion

//#region Brand

//var vbrand = brands.FirstOrDefault(vb => vb.VendorBrandCode == enContent.Brand.ID);

//if (vbrand == null)
//{
//  vbrand = new BrandVendor
//  {
//    VendorID = VendorID,
//    VendorBrandCode = enContent.Brand.ID,
//    BrandID = UnMappedID
//  };

//  brands.Add(vbrand);

//  repoBrandVendor.Add(vbrand);
//}

//vbrand.Name = enContent.Brand.Name;

//#endregion Brand

//#region Product

//var item = prodRepo.GetSingle(p => p.VendorItemNumber == product.Identifiers.OEM && p.BrandID == vbrand.BrandID);
//if (item == null)
//{
//  item = new Product
//  {
//    VendorItemNumber = product.Identifiers.OEM,
//    BrandID = vbrand.BrandID,
//    SourceVendorID = VendorID
//  };
//  prodRepo.Add(item);
//  unit.Save();
//}
//else
//{
//  item.BrandID = vbrand.BrandID;
//}

//#endregion Product

//#region Vendor assortment


//if (item.VendorAssortments == null) item.VendorAssortments = new List<VendorAssortment>();

//var assortment = item.VendorAssortments.FirstOrDefault(va => va.VendorID == VendorID);
//if (assortment == null)
//{
//  assortment = new VendorAssortment
//  {
//    VendorID = VendorID,
//    Product = item
//  };
//  repoAssortment.Add(assortment);
//}

//assortment.ShortDescription = enContent.Description.Length > 150 ? enContent.Description.Substring(0, 150) : enContent.Description;
//assortment.CustomItemNumber = product.VenderItemNumber;
//assortment.LineType = "S";
//assortment.IsActive = true;

//#region Product group

//var vGroup = productGroupVendorRecords.FirstOrDefault(pg => pg.VendorProductGroupCode1 == enContent.Category.ID);

//if (vGroup == null)
//{
//  vGroup = new ProductGroupVendor
//  {
//    VendorProductGroupCode1 = enContent.Category.ID.Trim(),
//    VendorID = VendorID,
//    ProductGroupID = UnMappedID
//  };

//  productGroupVendorRecords.Add(vGroup);
//  unit.Scope.Repository<ProductGroupVendor>().Add(vGroup);
//}
//vGroup.VendorName = enContent.Category.Name;
//#region Sync

//if (currentProductGroupVendors.Contains(vGroup))
//{
//  currentProductGroupVendors.Remove(vGroup);
//}
//#endregion
//#endregion Product group

//#region Vendor Product Group Assortment

//string brandCode = null;
//string groupCode1 = enContent.Category.ID;
//string groupCode2 = null;
//string groupCode3 = null;

//var records = (from l in productGroupVendorRecords
//               where
//                 ((brandCode != null && l.BrandCode.Trim() == brandCode) || l.BrandCode == null)
//                 &&
//                 ((groupCode1 != null && l.VendorProductGroupCode1 != null &&
//                   l.VendorProductGroupCode1.Trim() == groupCode1) || l.VendorProductGroupCode1 == null)
//                 &&
//                 ((groupCode2 != null && l.VendorProductGroupCode2 != null &&
//                   l.VendorProductGroupCode2.Trim() == groupCode2) || l.VendorProductGroupCode2 == null)
//                 &&
//                 ((groupCode3 != null && l.VendorProductGroupCode3 != null &&
//                   l.VendorProductGroupCode3.Trim() == groupCode3) || l.VendorProductGroupCode3 == null)

//               select l).ToList();


//List<int> existingProductGroupVendors = new List<int>();

//foreach (ProductGroupVendor prodGroupVendor in records)
//{
//  existingProductGroupVendors.Add(prodGroupVendor.ProductGroupVendorID);

//  if (prodGroupVendor.VendorAssortments == null)
//  {
//    prodGroupVendor.VendorAssortments = new List<VendorAssortment>();
//  }
//  if (prodGroupVendor.VendorAssortments.Any(x => x.VendorAssortmentID == assortment.VendorAssortmentID))
//  {
//    // only add new rows
//    continue;
//  }

//  prodGroupVendor.VendorAssortments.Add(assortment);
//}



//#endregion


//#endregion Vendor assortment

//#region Stock

//var stock = stockRepo.GetSingle(c => c.ProductID == assortment.ProductID && c.VendorID == assortment.VendorID);

//if (stock == null)
//{
//  stock = new VendorStock
//  {
//    VendorID = VendorID,
//    Product = item,
//    VendorStockTypeID = 1
//  };
//  stockRepo.Add(stock);
//}
//stock.QuantityOnHand = product.InStock;
//stock.StockStatus = String.IsNullOrEmpty(product.Status) ? null : product.Status;
//stock.ConcentratorStatusID = mapper.SyncVendorStatus(product.Status, -1);

//#endregion Stock

//#region Price

//if (assortment.VendorPrices == null) assortment.VendorPrices = new List<VendorPrice>();
//var price = assortment.VendorPrices.FirstOrDefault();
//if (price == null)
//{
//  price = new VendorPrice
//  {
//    VendorAssortment = assortment,
//    MinimumQuantity = product.MinQuantity
//  };

//  unit.Scope.Repository<VendorPrice>().Add(price);
//}
//price.CommercialStatus = String.IsNullOrEmpty(product.Status) ? null : product.Status;
//price.Price = product.Price;

//price.CommercialStatus = product.Status;
//price.ConcentratorStatusID = stock.ConcentratorStatusID;

//#endregion Price

//#region Barcode

//if (!String.IsNullOrEmpty(product.Identifiers.EAN))
//{
//  if (item.ProductBarcodes == null) item.ProductBarcodes = new List<ProductBarcode>();
//  if (!item.ProductBarcodes.Any(pb => pb.Barcode.Trim() == product.Identifiers.EAN.Trim()))
//  {
//    unit.Scope.Repository<ProductBarcode>().Add(new ProductBarcode
//     {
//       Product = item,
//       Barcode = product.Identifiers.EAN,
//       BarcodeType = (int)BarcodeTypes.Default,
//       VendorID = VendorID
//     });
//  }
//}

//#endregion Barcode

//#region Images
//if (item.ProductMedias == null) item.ProductMedias = new List<ProductMedia>();
//var maxSeq = (item.ProductMedias.Max(p => (int?)p.Sequence) ?? 0) + 1;


//product.ProductImages.ForEach((imgUrl, idx) =>
//{
//  if (!item.ProductMedias.Any(pi => pi.VendorID == VendorID && pi.MediaUrl == imgUrl))
//  {
//    unit.Scope.Repository<ProductMedia>().Add(new ProductMedia
//      {
//        VendorID = VendorID,
//        MediaUrl = imgUrl,
//        TypeID = 1,
//        Product = item,
//        Sequence = 0
//      });
//  }
//});
//#endregion

//#region Related Products

//if (product.CompatibleProducts.compatibleProducts.Count > 0)
//{
//  if (!relations.ContainsKey(product.Identifiers.OEM))
//  {
//    relations.Add(product.Identifiers.OEM, new ProductIdentifier() { productID = product.CompatibleProducts.productID, compatibleProducts = new List<string>() });
//  }

//  relations[product.Identifiers.OEM].compatibleProducts.AddRange(
//    from cp in product.CompatibleProducts.compatibleProducts
//    where !relations[product.Identifiers.OEM].compatibleProducts.Contains(cp)
//    select cp
//    );
//}

//#endregion

//foreach (var currContent in product.Content)
//{
//  var currLanguage = 0;
//  Languages.TryGetValue(currContent.Language.ToUpper(), out currLanguage);

//  #region Descriptions
//  if (item.ProductDescriptions == null) item.ProductDescriptions = new List<ProductDescription>();
//  var desc = item.ProductDescriptions.FirstOrDefault(pd => pd.LanguageID == currLanguage && pd.VendorID == VendorID);
//  if (desc == null)
//  {
//    desc = new ProductDescription
//             {
//               VendorID = VendorID,
//               LanguageID = currLanguage,
//               Product = item
//             };
//    unit.Scope.Repository<ProductDescription>().Add(desc);
//  }

//  desc.ShortContentDescription = currContent.Description;


//  #endregion Descriptions

//  if (currContent.Specs != null)
//  {
//    #region Specifications

//    #region Product attribute group
//    var prAttrGroup = productAttributeGroups.FirstOrDefault(c => c.GroupCode == currContent.Category.ID);
//    if (prAttrGroup == null)
//    {
//      prAttrGroup = new ProductAttributeGroupMetaData
//                      {
//                        GroupCode = currContent.Category.ID,
//                        Index = 0,
//                        VendorID = VendorID
//                      };
//      repoAttributeGroup.Add(prAttrGroup);
//      productAttributeGroups.Add(prAttrGroup);
//    }




//    if (prAttrGroup.ProductAttributeGroupNames == null) prAttrGroup.ProductAttributeGroupNames = new List<ProductAttributeGroupName>();
//    var prAttrGroupName = prAttrGroup.ProductAttributeGroupNames.FirstOrDefault(c => c.LanguageID == currLanguage);


//    if (prAttrGroupName == null)
//    {
//      prAttrGroupName = new ProductAttributeGroupName
//                          {
//                            ProductAttributeGroupMetaData = prAttrGroup,
//                            LanguageID = currLanguage
//                          };
//      repoAttributeGroupName.Add(prAttrGroupName);
//    }

//    prAttrGroupName.Name = currContent.Category.Name;
//    #endregion

//    unit.Save();

//    #region Product Attributes


//    foreach (var attr in currContent.Specs)
//    {
//      var productAttrMD = productAttributes.FirstOrDefault(c => c.AttributeCode == attr.ID && c.VendorID == VendorID);

//      if (productAttrMD == null)
//      {
//        productAttrMD = new ProductAttributeMetaData()
//                          {
//                            VendorID = VendorID,
//                            IsVisible = true,
//                            AttributeCode = attr.ID,
//                            ProductAttributeGroupMetaData = prAttrGroup
//                          };
//        repoAttribute.Add(productAttrMD);
//        productAttributes.Add(productAttrMD);
//      }

//      if (productAttrMD.ProductAttributeNames == null) productAttrMD.ProductAttributeNames = new List<ProductAttributeName>();
//      var productAttrName = productAttrMD.ProductAttributeNames.FirstOrDefault(c => c.LanguageID == currLanguage);

//      if (productAttrName == null)
//      {
//        productAttrName = new ProductAttributeName()
//                            {
//                              LanguageID = currLanguage,
//                              ProductAttributeMetaData = productAttrMD
//                            };
//        unit.Scope.Repository<ProductAttributeName>().Add(productAttrName);
//      }
//      productAttrName.Name = attr.Desc;
//      if (productAttrMD.ProductAttributeValues == null) productAttrMD.ProductAttributeValues = new List<ProductAttributeValue>();
//      var attrValue = productAttrMD.ProductAttributeValues.FirstOrDefault(
//        c => c.ProductID == item.ProductID);
//      if (attrValue == null)
//      {
//        attrValue = new ProductAttributeValue
//                      {
//                        Product = item,
//                        ProductAttributeMetaData = productAttrMD,
//                        LanguageID = currLanguage
//                      };
//        unit.Scope.Repository<ProductAttributeValue>().Add(attrValue);
//      }
//      attrValue.Value = attr.Value;
//    }
//    #endregion
//    #endregion
//  }

//}