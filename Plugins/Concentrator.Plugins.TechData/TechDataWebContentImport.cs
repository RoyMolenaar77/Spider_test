using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Concentrator.Objects.CSV;
using Concentrator.Objects.Enumerations;
using Concentrator.Objects.Ftp;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.Utility;
using Concentrator.Objects.Vendors;
using Concentrator.Objects.Vendors.Bulk;
using Concentrator.Objects.ZipUtil;
using Concentrator.Objects.Models.Attributes;
using Concentrator.Objects.ConcentratorService;
using System.Configuration;

namespace Concentrator.Plugins.TechData
{
  public class TechDataWebContentImport : VendorBase
  {
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

    public CultureInfo info
    {
      get
      {
        if (_info == null)
        {
          _info = new CultureInfo("nl-NL");
        }
        return _info;
      }
    }

    public override string Name
    {
      get { return "Tech Data Web Content Import"; }
    }

    public const int unmappedID = -1;
    private CultureInfo _info;

    protected override void SyncProducts()
    {
      try
      {
        log.AuditInfo("Start Tech Data import");
        var stopWatch = Stopwatch.StartNew();

        FtpManager downloader = new FtpManager(GetConfiguration().AppSettings.Settings["TechDataFtpSite"].Value, string.Empty, GetConfiguration().AppSettings.Settings["TechDataFtpUserName"].Value,
                                                       GetConfiguration().AppSettings.Settings["TechDataFtpPass"].Value,
                                                     false, false, log);

        using (var file = downloader.OpenFile("prices.zip"))
        {
          using (ZipProcessor zipUtil = new ZipProcessor(file.Data))
          {
            var zipEligible = (from z in zipUtil where z.FileName == "00136699.txt" select z);

            foreach (var zippedFile in zipEligible)
            {
              using (CsvParser parser = new CsvParser(zippedFile.Data, ColumnDefinitions, true))
              {
                ProcessCsv(parser);
              }
            }
          }
        }

        log.AuditSuccess(string.Format("Total import process finished in: {0}", stopWatch.Elapsed), "Tech Data Import");
      }
      catch (Exception e)
      {
        log.AuditFatal("Tech data import failed", e, "Tech Data Import");
      }
    }

    private void ProcessCsv(CsvParser parser)
    {
      using (var unit = GetUnitOfWork())
      {
        RelatedProductTypes relatedProductTypes = new RelatedProductTypes(unit.Scope.Repository<RelatedProductType>());
        var relatedProductType = relatedProductTypes.SyncRelatedProductTypes("CompatibleProducts");

        //there are products with repeating vendoritemnumbers
        List<string> vendorItemNumbers = unit.Scope.Repository<Product>().GetAll(p => p.SourceVendorID == VendorID).Select(c => c.VendorItemNumber).ToList();

        var vendor = unit.Scope.Repository<Vendor>().GetSingle(x => x.VendorID == VendorID);
        int couterProduct = 0;

        int logCount = 0;
        var importedproducts = parser.ToList();
        int totalProducts = importedproducts.Count();

        ProductStatusVendorMapper mapper = new ProductStatusVendorMapper(unit.Scope.Repository<VendorProductStatus>(), VendorID);
        List<Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorAssortmentItem> assortmentList = new List<Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorAssortmentItem>();

        //Used for VendorImportAttributeValues
        var productAttributes = unit.Scope.Repository<ProductAttributeMetaData>().GetAll(c => c.VendorID == VendorID).ToList();
        var attributeList = productAttributes.ToDictionary(x => x.AttributeCode, y => y.AttributeID);

        foreach (Dictionary<string, string> record in importedproducts)
        {
          couterProduct++;
          logCount++;
          if (logCount == 250)
          {
            log.DebugFormat("Products Processed : {0}/{1} for Vendor {2}", couterProduct, totalProducts, vendor.Name);
            logCount = 0;
          }

          int concentratorStatusID = -1;
          DateTime backorderDate = DateTime.Now;

          if (!string.IsNullOrEmpty(record.Get(TechDataColumnDefs.BackorderDate.ToString()))
            && DateTime.TryParseExact(record.Get(TechDataColumnDefs.BackorderDate.ToString()), "dd/MM/yyyy", null, DateTimeStyles.None, out backorderDate)
            && DateTime.Compare(backorderDate, DateTime.Now) > 0)
            concentratorStatusID = mapper.SyncVendorStatus("BackOrder", -1);
          else
            concentratorStatusID = mapper.SyncVendorStatus("InStock", -1);

          string vendorItemNumber = record.Get(TechDataColumnDefs.VendorItemNumber.ToString()).Trim();

          //if already an item with this vendor item number exists --> do nothing
          if (vendorItemNumbers.Contains(vendorItemNumber.Trim()))
            continue;

          Decimal price = decimal.Parse(record.Get(TechDataColumnDefs.Price.ToString()));

          string customItemNumber = record.Get(TechDataColumnDefs.CustomItemNumber.ToString()).Trim();
          string productGroupCode = record.Get(TechDataColumnDefs.GrandChildVendorProductGroupCode.ToString());
          string parentProductGroupCode = record.Get(TechDataColumnDefs.ChildVendorProductGroupCode.ToString());
          string grandParentProductGroupCode = record.Get(TechDataColumnDefs.ParentVendorProductGroupCode.ToString());

          var assortment = new Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorAssortmentItem
          {
            #region BrandVendor
            BrandVendors = new List<VendorAssortmentBulk.VendorImportBrand>()
                        {
                            new VendorAssortmentBulk.VendorImportBrand()
                            {
                                VendorID = VendorID,
                                VendorBrandCode = record.Get(TechDataColumnDefs.Brand.ToString()).Trim().ToLower(),
                                ParentBrandCode = null,
                                Name = record.Get(TechDataColumnDefs.Brand.ToString()).Trim().ToLower(),
                            }
                        },
            #endregion

            #region GeneralProductInfo
            VendorProduct = new VendorAssortmentBulk.VendorProduct
            {
              VendorItemNumber = vendorItemNumber,
              CustomItemNumber = customItemNumber,
              ShortDescription = record.Get(TechDataColumnDefs.Description.ToString()),
              LongDescription = record.Get(TechDataColumnDefs.Description.ToString()),
              LineType = null,
              LedgerClass = null,
              ProductDesk = null,
              ExtendedCatalog = null,
              VendorID = VendorID,
              DefaultVendorID = DefaultVendorID,
              VendorBrandCode = record.Get(TechDataColumnDefs.Brand.ToString()).Trim().ToLower(),
              Barcode = record.Get(TechDataColumnDefs.EanCode.ToString()),
              VendorProductGroupCode1 = grandParentProductGroupCode,
              VendorProductGroupCodeName1 = null,
              VendorProductGroupCode2 = parentProductGroupCode,
              VendorProductGroupCodeName2 = null,
              VendorProductGroupCode3 = productGroupCode,
              VendorProductGroupCodeName3 = null
            },
            #endregion

            #region RelatedProducts
            RelatedProducts = new List<VendorAssortmentBulk.VendorImportRelatedProduct>()
                        {
                            new Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportRelatedProduct
                            {
                                VendorID = VendorID,
                                DefaultVendorID = DefaultVendorID,
                                CustomItemNumber = customItemNumber,
                                RelatedProductType = relatedProductType.Type,
                                RelatedCustomItemNumber = vendorItemNumber
                            }
                        },
            #endregion

            #region Attributes
                        VendorImportAttributeValues = new List<VendorAssortmentBulk.VendorImportAttributeValue>(),
            //VendorImportAttributeValues = (from attr in enumValList
            //                               let prop = record.Equals(attr)
            //                               let attributeID = attributeList.ContainsKey(attr) ? attributeList[attr] : 2 //TODO set as -1
            //                               let value = prop.ToString()
            //                               where !string.IsNullOrEmpty(value)
            //                               select new Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportAttributeValue
            //                               {
            //                                 VendorID = VendorID,
            //                                 DefaultVendorID = DefaultVendorID,
            //                                 CustomItemNumber = customItemNumber,
            //                                 AttributeID = attributeID,
            //                                 Value = value,
            //                                 LanguageID = "1",
            //                                 AttributeCode = attr,
            //                               }).ToList(),
            #endregion

            #region Prices
            VendorImportPrices = new List<Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportPrice>()
                        {
                            new Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportPrice()
                            {
                                VendorID = VendorID,
                                DefaultVendorID = DefaultVendorID,
                                CustomItemNumber = customItemNumber,                                
                                Price = price.ToString("0.00", CultureInfo.InvariantCulture),
                                CostPrice = price.ToString("0.00", CultureInfo.InvariantCulture),
                                TaxRate = "19",
                                MinimumQuantity = 0,
                                CommercialStatus = "SR"
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
                                CustomItemNumber = customItemNumber,
                                QuantityOnHand = 0,
                                StockType = "Assortment",
                                StockStatus = "S"
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
      }
    }

    private List<string> _columnDefinitions;

    private List<string> ColumnDefinitions
    {
      get
      {
        if (_columnDefinitions == null)
        {
          _columnDefinitions = new List<string>();
          _columnDefinitions.AddRange(Enum.GetNames(typeof(TechDataColumnDefs)));
        }
        return _columnDefinitions;
      }
    }

    public enum TechDataColumnDefs
    {
      Description,
      CustomItemNumber,
      VendorItemNumber,
      ParentVendorProductGroupCode,
      ChildVendorProductGroupCode,
      GrandChildVendorProductGroupCode,
      Brand,
      Version,
      Language,
      Media,
      Trend,
      PriceGroup,
      PriceCode,
      LPEur,
      DEEur,
      D1Eur,
      D2Eur,
      Price,
      Stock,
      BackorderDate,
      ModifDate,
      EanCode
    }
  }
}

//var productGroupVendorRepo = unit.Scope.Repository<ProductGroupVendor>();
//var brandVendorRepo = unit.Scope.Repository<BrandVendor>();
//var brandRepo = unit.Scope.Repository<Brand>();
//var productRepo = unit.Scope.Repository<Product>();
//var barcodeRepo = unit.Scope.Repository<ProductBarcode>();
//var matchRepo = unit.Scope.Repository<ProductMatch>();
//var assortmentRepo = unit.Scope.Repository<VendorAssortment>();
//var repoVendor = unit.Scope.Repository<ProductGroupVendor>();

//ProductGroupSyncer pgSyncer = new ProductGroupSyncer(vendorID, productGroupVendorRepo);
//var brands = unit.Scope.Repository<BrandVendor>().GetAll(b => b.VendorID == vendorID).ToList();
//var productGroups = productGroupVendorRepo.GetAll(p => p.VendorID == vendorID).ToList();

//var currentProductGroupVendors = productGroups.Where(v => v.VendorID == vendorID).ToList();

//var productGroupVendorRecords = repoVendor.GetAll(pc => pc.VendorID == vendorID).ToList();

//#region delete unused
//foreach (var vProdVendor in currentProductGroupVendors)
//{
//  if (vProdVendor.ProductGroupID == -1)
//  {
//    productGroupVendorRepo.Delete(vProdVendor);
//  }
//}
//unit.Save();
//#endregion

//#region Brand

//string brand = record.Get(TechDataColumnDefs.Brand.ToString()).Trim().ToLower();

//var brandVendor = brands.FirstOrDefault(c => c.VendorBrandCode.ToLower() == brand); // To verify with Tim

//if (brandVendor == null)
//{
//  #region Try Map
//  int brandID = unmappedID;
//  var existingBrand = brandRepo.GetSingle(b => b.Name == brand);
//  if (existingBrand != null)
//    brandID = existingBrand.BrandID;
//  #endregion

//  brandVendor = new BrandVendor
//                  {
//                    BrandID = brandID,
//                    VendorID = vendorID
//                  };
//  brandVendorRepo.Add(brandVendor);
//  brands.Add(brandVendor);
//}
//brandVendor.VendorBrandCode = brand;
//#endregion

//#region ProductGroup


////Tech data provides a three level hierarchy
////ParentVendorProductGroupCode --> ChildVendorProductGroupCode --> GrandChildVendorProductGroupCode
////start at bottom level and work to top levels

//string productGroupCode = record.Get(TechDataColumnDefs.GrandChildVendorProductGroupCode.ToString());
//string parentProductGroupCode = record.Get(TechDataColumnDefs.ChildVendorProductGroupCode.ToString());
//string grandParentProductGroupCode = record.Get(TechDataColumnDefs.ParentVendorProductGroupCode.ToString());



//#region Product group

//var group = productGroupVendorRepo.GetSingle(
//  x => x.VendorProductGroupCode3 == productGroupCode && x.VendorID == vendorID);
//if (group == null)
//{
//  group = new ProductGroupVendor()
//  {
//    VendorID = vendorID,
//    VendorProductGroupCode3 = productGroupCode,
//    ProductGroupID = -1,
//    BrandCode = brand
//  };
//  productGroupVendorRepo.Add(group);

//}

//#region sync
//if (currentProductGroupVendors.Contains(group))
//{
//  currentProductGroupVendors.Remove(group);
//}
//#endregion

//group = productGroupVendorRepo.GetSingle(
//x => x.VendorProductGroupCode2 == parentProductGroupCode && x.VendorID == vendorID);
//if (group == null)
//{
//  group = new ProductGroupVendor()
//  {
//    VendorID = vendorID,
//    VendorProductGroupCode2 = parentProductGroupCode,
//    ProductGroupID = -1,
//    BrandCode = brand
//  };
//  productGroupVendorRepo.Add(group);

//}

//#region sync
//if (currentProductGroupVendors.Contains(group))
//{
//  currentProductGroupVendors.Remove(group);
//}
//#endregion

//group = productGroupVendorRepo.GetSingle(
//x => x.VendorProductGroupCode1 == grandParentProductGroupCode && x.VendorID == vendorID);
//if (group == null)
//{
//  group = new ProductGroupVendor()
//  {
//    VendorID = vendorID,
//    VendorProductGroupCode1 = grandParentProductGroupCode,
//    ProductGroupID = -1,
//    BrandCode = brand
//  };
//  productGroupVendorRepo.Add(group);

//}

//#region sync
//if (currentProductGroupVendors.Contains(group))
//{
//  currentProductGroupVendors.Remove(group);
//}
//#endregion

//#endregion Product group

//#endregion

//#region Product

//var product = productRepo.GetSingle(c => c.VendorItemNumber == vendorItemNumber && !vendorItemNumbers.Contains(vendorItemNumber));

//if (product == null)
//{
//  product = new Product
//              {
//                BrandID = unmappedID,
//                VendorItemNumber = vendorItemNumber,
//                SourceVendorID = vendorID
//              };

//  var existingMatch = (from p in barcodeRepo.GetAll().ToList()
//                        where p.Barcode == record.Get(TechDataColumnDefs.EanCode.ToString())
//                        select p.ProductID).Distinct().ToList();

//  foreach (var productID in existingMatch)
//  {
//    var productMatch = matchRepo.GetSingle(x => x.ProductID == productID);

//    if (productMatch != null)
//    {
//      var match = new ProductMatch
//                    {
//                      ProductMatchID = productMatch.ProductMatchID,
//                      Product = product,
//                      isMatched = false
//                    };
//      matchRepo.Add(match);
//    }
//  }

//  productRepo.Add(product);
//  vendorItemNumbers.Add(vendorItemNumber);
//}

//#endregion

//#region VendorAssortment
//if (product.VendorAssortments == null) product.VendorAssortments = new List<VendorAssortment>();
//var assortment = product.VendorAssortments.FirstOrDefault((c => c.VendorID == vendorID));
//if (assortment == null)
//{
//  assortment = new VendorAssortment
//                  {
//                    VendorID = vendorID,
//                    Product = product
//                  };
//  assortmentRepo.Add(assortment);
//}
//assortment.CustomItemNumber = customItemNumber;
//assortment.ShortDescription = record.Get(TechDataColumnDefs.Description.ToString());
//assortment.IsActive = true;
//#endregion

//#region Vendor Product Group Assortment

//string brandCode = null;
//string groupCode1 = grandParentProductGroupCode;
//string groupCode2 = parentProductGroupCode;
//string groupCode3 = productGroupCode;

//var records = (from l in productGroupVendorRecords
//                where
//                  ((brandCode != null && l.BrandCode.Trim() == brandCode) || l.BrandCode == null)
//                  &&
//                  ((groupCode1 != null && l.VendorProductGroupCode1 != null &&
//                    l.VendorProductGroupCode1.Trim() == groupCode1) || l.VendorProductGroupCode1 == null)
//                  &&
//                  ((groupCode2 != null && l.VendorProductGroupCode2 != null &&
//                    l.VendorProductGroupCode2.Trim() == groupCode2) || l.VendorProductGroupCode2 == null)
//                  &&
//                  ((groupCode3 != null && l.VendorProductGroupCode3 != null &&
//                    l.VendorProductGroupCode3.Trim() == groupCode3) || l.VendorProductGroupCode3 == null)

//                select l).ToList();


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

////unit.Scope.Repository<ProductGroupVendor>().Delete(a => !existingProductGroupVendors.Contains(a.ProductGroupVendorID) && a.VendorID == vendorID);

//#endregion

//#region Price
//if (assortment.VendorPrices == null) assortment.VendorPrices = new List<VendorPrice>();
//var price = assortment.VendorPrices.FirstOrDefault();
//if (price == null)
//{
//  price = new VendorPrice
//            {
//              VendorAssortment = assortment
//            };
//  unit.Scope.Repository<VendorPrice>().Add(price);
//}

//var itemPrice = record.Try<Dictionary<string, string>, decimal?>(c => decimal.Parse(c.Get(TechDataColumnDefs.Price.ToString()), info), null);
//price.BaseCostPrice = itemPrice;
//price.BasePrice = itemPrice;
//price.ConcentratorStatusID = concentratorStatusID;
//#endregion

//#region Stock

//int qtyOnHand = 0;
//int.TryParse(record.Get(TechDataColumnDefs.Stock.ToString()), out qtyOnHand);

//if (product.VendorStocks == null) product.VendorStocks = new List<VendorStock>();
//var stock = product.VendorStocks.FirstOrDefault(c => c.VendorID == vendorID);
//if (stock == null)
//{
//  stock = new VendorStock
//            {
//              VendorID = vendorID,
//              Product = product,
//              VendorStockTypeID = 1
//            };
//  unit.Scope.Repository<VendorStock>().Add(stock);

//}
//stock.QuantityOnHand = qtyOnHand;
//stock.ConcentratorStatusID = concentratorStatusID;

//#endregion

//#region Barcode

//string barcode = record.Get(TechDataColumnDefs.EanCode.ToString());
//if (!string.IsNullOrEmpty(barcode))
//{
//  if (product.ProductBarcodes == null) product.ProductBarcodes = new List<ProductBarcode>();
//  if (!product.ProductBarcodes.Any(pb => pb.Barcode.Trim() == barcode))
//  {
//    barcodeRepo.Add(new ProductBarcode
//                                              {
//                                                Product = product,
//                                                Barcode = barcode,
//                                                VendorID = vendor.VendorID,
//                                                BarcodeType = (int)BarcodeTypes.Default
//                                              });
//  }
//}

//#endregion

//#region Description

//int language = (int)LanguageTypes.English;
////TODO : No language yet in csv
//if (product.ProductDescriptions == null) product.ProductDescriptions = new List<ProductDescription>();

//var description = product.ProductDescriptions.FirstOrDefault(c => c.LanguageID == language);
//if (description == null)
//{
//  description = new ProductDescription
//                  {
//                    Product = product,
//                    VendorID = vendorID,
//                    LanguageID = language
//                  };
//  unit.Scope.Repository<ProductDescription>().Add(description);
//}
//description.ShortContentDescription = record.Get(TechDataColumnDefs.Description.ToString());


//#endregion

//#region Images

//string imageUrl = record.Get(TechDataColumnDefs.Media.ToString());
//if (!string.IsNullOrEmpty(imageUrl))
//{
//  if (product.ProductMedias == null) product.ProductMedias = new List<ProductMedia>();
//  var maxSeq = (product.ProductMedias.Max(p => (int?)p.Sequence) ?? 0) + 1;

//  product.ProductMedias.ForEach((imgUrl, idx) =>
//  {
//    if (!product.ProductMedias.Any(pi => pi.VendorID == vendorID && pi.MediaUrl == imageUrl))
//    {
//      unit.Scope.Repository<ProductMedia>().Add(new ProductMedia
//      {
//        VendorID = vendorID,
//        MediaUrl = imageUrl,
//        TypeID = 1,
//        Product = product,
//        Sequence = maxSeq + idx
//      });
//    }
//  });
//}
//#endregion