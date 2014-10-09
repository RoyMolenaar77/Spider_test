using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Transactions;
using Concentrator.Objects;
using Concentrator.Objects.Extensions;
using Concentrator.Objects.ConcentratorService;
using Concentrator.Objects.Ftp;
using System.Configuration;
using Concentrator.Objects.ZipUtil;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using Concentrator.Objects.Enumerations;
using Concentrator.Objects.Models.Brands;
using Concentrator.Objects.Models.Attributes;
using Concentrator.Objects.Utility;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Vendors.Bulk;
using Concentrator.Objects.Vendors;
using System.Globalization;

namespace Concentrator.Plugins.VSN
{
  public abstract class VSNProductImportBase : VendorBase
  {

    private string[] AttributeMapping = new[] { "CountryName", "UsrReleaseDate", "RatingAge", "ProductionYear", "RunningTime", "MediaType", "SubGenreName", "GenreName", "Artist", "MediaDescription", "SubProductGroup" };
    private string[] FilterAttributes = new[] { "RatingAge", "SubGenreName", "GenreName", "SubProductGroup" };
    public const string BrandVendorCode = "VSN";  

    protected void SetupBrandAndAttributes(IUnitOfWork unit, out List<ProductAttributeMetaData> attributes, out BrandVendor brandVendor)
    {
      int generalAttributegroupID = GetGeneralAttributegroupID(unit);

      #region Basic Attributes
      var attributeRepo = unit.Scope.Repository<ProductAttributeMetaData>();
      var attributeNameRepo = unit.Scope.Repository<ProductAttributeName>();
      var brandVendorRepo = unit.Scope.Repository<BrandVendor>();

      var attributesTmp = (from a in attributeRepo.GetAll()
                           where AttributeMapping.Contains(a.AttributeCode) &&
                                 a.VendorID == VendorID
                           select a).ToList();

      var attributesToAdd = from a in AttributeMapping
                            where !attributesTmp.Any(at => at.AttributeCode == a)
                            select a;

      foreach (var toAdd in attributesToAdd)
      {
        var newAttribute = new ProductAttributeMetaData
                             {
                               AttributeCode = toAdd,
                               IsVisible = true,
                               VendorID = VendorID,
                               ProductAttributeGroupID = generalAttributegroupID,
                               Index = 0,
                               NeedsUpdate = true,
                               IsSearchable = FilterAttributes.Contains(toAdd) ? true : false,
                               Sign = String.Empty
                             };

        attributeRepo.Add(newAttribute);
        attributesTmp.Add(newAttribute);

        var attNameEng = new ProductAttributeName
                           {
                             ProductAttributeMetaData = newAttribute,
                             LanguageID = (int)LanguageTypes.English,
                             Name = toAdd
                           };
        attributeNameRepo.Add(attNameEng);

        var attNameDut = new ProductAttributeName
                           {
                             ProductAttributeMetaData = newAttribute,
                             LanguageID = (int)LanguageTypes.Netherlands,
                             Name = toAdd
                           };
        attributeNameRepo.Add(attNameDut);
      }
      unit.Save();

      #endregion Basic Attributes

      #region Brand
      brandVendor = unit.Scope.Repository<BrandVendor>().GetSingle(bv => bv.VendorBrandCode == BrandVendorCode);
      if (brandVendor == null)
      {
        var brand = new Brand
                      {
                        Name = "VSN"
                      };
        unit.Scope.Repository<Brand>().Add(brand);

        brandVendor = new BrandVendor
                        {
                          VendorBrandCode = BrandVendorCode,
                          VendorID = VendorID,
                          Brand = brand
                        };
        brandVendorRepo.Add(brandVendor);

        unit.Save();
      }

      #endregion Brand

      attributes = attributesTmp;
    }

    protected void ProcessProductsTable(DataTable table, int vendorBrandID, IEnumerable<ProductAttributeMetaData> productAttributes, ProductGroupSyncer syncer, ProductStatusVendorMapper mapper, Dictionary<int, VendorAssortment> inactiveAss)
    {

      var products = (from r in table.Rows.Cast<DataRow>()
                      where !String.IsNullOrEmpty(r.Field<string>("EANNumber"))
                      group r by r.Field<string>("EANNumber")
                        into grp
                        let r = grp.First()
                        select new
                                 {
                                   ProductID = r.Field<string>("ProductCode"),
                                   EAN = r.Field<string>("EANNumber"),
                                   EAN_Alt = r.Field<string>("EANNumberAlternative"),
                                   Description = r.Field<string>("ProductName"),
                                   TagLine = r.Field<string>("TagLine"),
                                   Title = r.Field<string>("Title"),
                                   ProductionYear = r.Field<int?>("ProductionYear"),
                                   RunningTime = r.Field<int?>("RunningTime"),
                                   CountryName = r.Field<string>("CountryName"),
                                   UsrReleaseDate = r.Field<string>("UsrReleaseDate"),
                                   ProductStatusID = r.Field<long?>("ProductStatusID"),
                                   ProductStatus = r.Field<string>("ProductStatus").Trim(),
                                   SalesPrice = r.Field<decimal?>("SalesPrice"),
                                   RetailPrice = ((r.Field<decimal?>("RetailPrice") ?? 0) / (decimal)1.19),
                                   ListRetailPrice = r.Field<decimal?>("ListRetailPrice"),
                                   ListSalesPrice = r.Field<decimal?>("ListSalesPrice"),
                                   InStock = r.Field<int?>("InStock"),
                                   GenreID = r.Field<long?>("GenreID"),
                                   GenreName = r.Field<string>("GenreName"),
                                   ProductGroupCode = r.Field<string>("ProductGroupCode"),
                                   ProductGroup = r.Field<string>("ProductGroup"),
                                   SubProductGroupCode = r.Field<string>("SubProductGroupCode"),
                                   SubProductGroup = r.Field<string>("SubProductGroup"),
                                   SubGenreID = r.Field<long?>("SubGenreID"),
                                   SubGenreName = r.Field<string>("SubGenreName"),
                                   RatingAge = r.Field<string>("RatingAge"),
                                   MediaType = r.Field<string>("MediaType"),
                                   MediaTypeID = r.Field<long?>("MediaTypeID"),
                                   Artist = r.Field<string>("Artist"),
                                   MediaDescription = r.Field<string>("MediaDescription")
                                 }).ToList();

      using (IUnitOfWork unit = GetUnitOfWork())
      {        
        //DataLoadOptions options = new DataLoadOptions();
        //options.LoadWith<ProductGroupVendor>(x => x.VendorProductGroupAssortments);
        //options.LoadWith<VendorAssortment>(x => x.VendorPrice);
        //options.LoadWith<Product>(x => x.ProductBarcodes);
        //ctx.LoadOptions = options;

        List<Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorAssortmentItem> assortmentList = new List<Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorAssortmentItem>();

        RelatedProductTypes relatedProductTypes = new RelatedProductTypes(unit.Scope.Repository<RelatedProductType>());
        var relatedProductType = relatedProductTypes.SyncRelatedProductTypes("CompatibleProducts");

        var productGroupVendorRepo = unit.Scope.Repository<ProductGroupVendor>().Include(c => c.VendorAssortments);
        var assortmentRepo = unit.Scope.Repository<VendorAssortment>().Include(c => c.VendorPrices);
        var stockRepo = unit.Scope.Repository<VendorStock>();
        var productRepo = unit.Scope.Repository<Product>().Include(c => c.ProductBarcodes);
        var priceRepo = unit.Scope.Repository<VendorPrice>();


        var productGroupVendorRecords = productGroupVendorRepo.GetAll(c => c.VendorID == VendorID).ToList();

        int step = 1000;
        int todo = products.Count();
        int done = 0;

        var languages = new[] { (int)LanguageTypes.English, (int)LanguageTypes.Netherlands };

        var vendorAssortment = assortmentRepo.GetAll(x => x.VendorID == VendorID).ToDictionary(x => x.ProductID, y => y);
        var attributeList = productAttributes.ToDictionary(x => x.AttributeCode, y => y.AttributeID);

        var vendorStock = stockRepo.GetAll(x => x.VendorID == VendorID).ToDictionary(x => x.ProductID, y => y);

        var prodType = products.First().GetType();

        var currentProductGroupVendors = productGroupVendorRepo.GetAll(v => v.VendorID == VendorID).ToList();

        log.InfoFormat(todo + " products to be processed");

        while (done < todo)
        {
          log.DebugFormat("{0} products to be processed", todo - done);

          var toProcess = products.OrderByDescending(x => x.UsrReleaseDate).Skip(done).Take(step);

          foreach (var product in toProcess)
          {
            try
            {
              if (product.ProductGroupCode != "GAMES" && product.ProductGroupCode != "HARDWARE")
                continue;

              //#if DEBUG
              //          if (product.EAN != "8714025503775")
              //            continue;
              //#endif
              //log.DebugFormat("{0} products to be processed in batch", toProcess.Count());

              var assortment = new Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorAssortmentItem
              {
                #region BrandVendor
                BrandVendors = new List<VendorAssortmentBulk.VendorImportBrand>()
                        {
                            new VendorAssortmentBulk.VendorImportBrand()
                            {
                                VendorID = VendorID,
                                VendorBrandCode = BrandVendorCode,
                                ParentBrandCode = null,
                                Name = product.GenreName
                            }
                        },
                #endregion

                #region GeneralProductInfo
                VendorProduct = new VendorAssortmentBulk.VendorProduct
                {
                  VendorItemNumber = product.EAN,
                  CustomItemNumber = product.ProductID,
                  ShortDescription = product.Description.Length > 150 ? product.Description.Substring(0, 150) : product.Description,
                  LongDescription = product.Description,
                  LineType = "S",
                  LedgerClass = null,
                  ProductDesk = null,
                  ExtendedCatalog = null,
                  VendorID = VendorID,
                  DefaultVendorID = DefaultVendorID,
                  VendorBrandCode = BrandVendorCode,
                  Barcode = product.EAN,
                  VendorProductGroupCode1 = product.ProductGroupCode,
                  VendorProductGroupCodeName1 = product.ProductGroup,
                  VendorProductGroupCode2 = product.SubProductGroupCode,
                  VendorProductGroupCodeName2 = product.SubProductGroup,
                  VendorProductGroupCode3 = product.GenreName
                  //VendorProductGroupCodeName3 = product.SubProductGroup
                },
                #endregion

                #region RelatedProducts
                RelatedProducts = new List<VendorAssortmentBulk.VendorImportRelatedProduct>()
                        {
                            new Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportRelatedProduct
                            {
                                VendorID = VendorID,
                                DefaultVendorID = DefaultVendorID,
                                CustomItemNumber = product.ProductID,
                                RelatedProductType = relatedProductType.Type,
                                RelatedCustomItemNumber = product.EAN
                            }
                        },
                #endregion

                #region Attributes
                VendorImportAttributeValues = (from attr in AttributeMapping
                                               let prop = product.Equals(attr)//d.Field<object>(attr)
                                               let attributeID = attributeList.ContainsKey(attr) ? attributeList[attr] : -1
                                               let value = prop.ToString()
                                               where !string.IsNullOrEmpty(value)
                                               select new Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportAttributeValue
                                               {
                                                 VendorID = VendorID,
                                                 DefaultVendorID = DefaultVendorID,
                                                 CustomItemNumber = product.ProductID,
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
                                DefaultVendorID = DefaultVendorID,
                                CustomItemNumber = product.ProductID,
                                Price = decimal.Round(product.RetailPrice, 4).ToString("0.00", CultureInfo.InvariantCulture),
                                CostPrice = decimal.Round(product.SalesPrice.Value, 4).ToString("0.00", CultureInfo.InvariantCulture),
                                TaxRate = "19", //TODO: Calculate this!
                                MinimumQuantity = 1,
                                CommercialStatus = product.ProductStatus
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
                                CustomItemNumber = product.ProductID,
                                QuantityOnHand = 0,
                                StockType = "Assortment",
                                StockStatus = product.ProductStatus
                            }
                        },
                #endregion
              };

              assortmentList.Add(assortment);
            }
            catch (Exception ex)
            {
              log.WarnFormat("Batch update failed with message : {0}", ex.Message);
            }
          }
          done += toProcess.Count();
        }

        #region delete unused
        foreach (var vProdVendor in currentProductGroupVendors)
        {
          if (vProdVendor.ProductGroupID == -1)
          {
            //productGroupVendorRepo.Delete(vProdVendor);
          }
        }
        #endregion        

        // Creates a new instance of VendorAssortmentBulk(Passes in the AssortmentList defined above, vendorID and DefaultVendorID)
        using (var vendorAssortmentBulk = new VendorAssortmentBulk(assortmentList, VendorID, VendorID))
        {
          vendorAssortmentBulk.Init(unit.Context);
          vendorAssortmentBulk.Sync(unit.Context);
        }


        log.AuditInfo("Products processing finished. Processed " + done + " products");
      }
    }
  }
}


//#region Product

//var item = productRepo.GetSingle(p => p.VendorItemNumber == product.EAN && p.BrandID == vendorBrandID);

//if (item == null)
//{
//  item = new Product
//            {
//              VendorItemNumber = product.EAN,
//              BrandID = vendorBrandID,
//              SourceVendorID = VendorID
//            };

//  productRepo.Add(item);
//  unit.Save();
//}

//#endregion Product

//#region Vendor assortment

//VendorAssortment assortment = null;
//vendorAssortment.TryGetValue(item.ProductID, out assortment);
//if (assortment == null)
//{
//  assortment = new VendorAssortment
//                  {
//                    VendorID = VendorID,
//                    Product = item
//                  };
//  assortmentRepo.Add(assortment);
//  vendorAssortment.Add(item.ProductID, assortment);
//}
//else
//{
//  if (inactiveAss != null)
//    inactiveAss.Remove(assortment.VendorAssortmentID);
//}

//assortment.ShortDescription = product.Description.Length > 150
//                                ? product.Description.Substring(0, 150)
//                                : product.Description;
//assortment.CustomItemNumber = product.ProductID;
//assortment.LineType = "S";
//assortment.IsActive = true;

//#endregion

//#region Product group


//var group =
//  productGroupVendorRecords.FirstOrDefault(x => x.VendorProductGroupCode1 == product.ProductGroupCode);
//if (group == null)
//{
//  group = new ProductGroupVendor()
//            {
//              VendorID = VendorID,
//              VendorProductGroupCode1 = product.ProductGroupCode,
//              VendorName = product.ProductGroup,
//              ProductGroupID = -1
//            };
//  productGroupVendorRepo.Add(group);
//  productGroupVendorRecords.Add(group);
//  unit.Save();
//}
//#region sync
//if (currentProductGroupVendors.Contains(group))
//{
//  currentProductGroupVendors.Remove(group);
//}
//#endregion

//group =
//  productGroupVendorRecords.FirstOrDefault(x => x.VendorProductGroupCode2 == product.SubProductGroupCode);
//if (group == null)
//{
//  group = new ProductGroupVendor()
//            {
//              VendorID = VendorID,
//              VendorProductGroupCode2 = product.SubProductGroupCode,
//              VendorName = product.SubProductGroup,
//              ProductGroupID = -1
//            };
//  productGroupVendorRepo.Add(group);
//  productGroupVendorRecords.Add(group);
//  unit.Save();
//}

//#region sync
//if (currentProductGroupVendors.Contains(group))
//{
//  currentProductGroupVendors.Remove(group);
//}
//#endregion
//group = productGroupVendorRecords.FirstOrDefault(x => x.VendorProductGroupCode3 == product.GenreName);
//if (group == null)
//{
//  group = new ProductGroupVendor()
//            {
//              VendorID = VendorID,
//              VendorProductGroupCode3 = product.GenreName,
//              VendorName = product.SubProductGroup,
//              ProductGroupID = -1
//            };
//  productGroupVendorRepo.Add(group);
//  productGroupVendorRecords.Add(group);
//  unit.Save();
//}
//#region sync
//if (currentProductGroupVendors.Contains(group))
//{
//  currentProductGroupVendors.Remove(group);
//}
//#endregion
//#endregion Product group

//#region Vendor Product Group Assortment

//string brandCode = null;
//string groupCode1 = product.ProductGroupCode;
//string groupCode2 = product.SubProductGroupCode;
//string groupCode3 = product.GenreName;

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

//  if (prodGroupVendor.VendorAssortments == null) prodGroupVendor.VendorAssortments = new List<VendorAssortment>();

//  if (prodGroupVendor.VendorAssortments.Any(x => x.VendorAssortmentID == assortment.VendorAssortmentID))
//  { // only add new rows
//    continue;
//  }

//  prodGroupVendor.VendorAssortments.Add(assortment);
//  //ctx.VendorProductGroupAssortments.InsertOnSubmit(vpga);
//}

//#endregion

//#region Stock

//VendorStock stock = null;
//vendorStock.TryGetValue(item.ProductID, out stock);
//if (stock == null)
//{
//  stock = new VendorStock
//            {
//              VendorID = VendorID,
//              Product = item,
//              VendorStockTypeID = 1,
//              QuantityOnHand = 0
//            };
//  stockRepo.Add(stock);

//}
//stock.VendorStatus = product.ProductStatus;
//stock.ConcentratorStatusID = mapper.SyncVendorStatus(product.ProductStatus, -1);
//stock.StockStatus = product.ProductStatus;

//#endregion Stock

//#region Price
//if (assortment.VendorPrices == null) assortment.VendorPrices = new List<VendorPrice>();
//var price = assortment.VendorPrices.FirstOrDefault();
//if (price == null)
//{
//  price = new VendorPrice
//            {
//              VendorAssortment = assortment,
//              MinimumQuantity = 1
//            };

//  priceRepo.Add(price);
//}
//price.CommercialStatus = product.ProductStatus;
//price.ConcentratorStatusID = stock.ConcentratorStatusID;
//price.Price = decimal.Round(product.RetailPrice, 4);

//if (product.SalesPrice.HasValue)
//  price.CostPrice = decimal.Round(product.SalesPrice.Value, 4);


//#endregion Price

//#region Barcode

//if (!String.IsNullOrEmpty(product.EAN))
//{
//  if (item.ProductBarcodes == null) item.ProductBarcodes = new List<ProductBarcode>();
//  if (!item.ProductBarcodes.Any(pb => pb.Barcode.Trim() == product.EAN))
//  {
//    unit.Scope.Repository<ProductBarcode>().Add(new ProductBarcode
//                                          {
//                                            Product = item,
//                                            Barcode = product.EAN,
//                                            VendorID = VendorID,
//                                            BarcodeType = (int)BarcodeTypes.Default
//                                          });
//  }
//}

//#endregion Barcode

//#region Attributes

//var productAttributeValues = unit.Scope.Repository<ProductAttributeValue>().GetAll(x => x.ProductID == item.ProductID && x.ProductAttributeMetaData.VendorID == VendorID).ToList();

//foreach (var attr in AttributeMapping)
//{
//  var prop = prodType.GetProperty(attr);
//  int metaId = -1;
//  attributeList.TryGetValue(attr, out metaId);

//  foreach (var lang in languages)
//  {
//    var val = productAttributeValues.Where(pam => pam.LanguageID == lang && pam.AttributeID == metaId).FirstOrDefault();
//    if (val == null)
//    {
//      var pval = new ProductAttributeValue
//              {
//                AttributeID = metaId,
//                LanguageID = lang,
//                Product = item
//              };
//      unit.Scope.Repository<ProductAttributeValue>().Add(pval);
//      val = pval;
//    }

//    var value = prop.GetValue(product, null);

//    val.Value = value != null ? value.ToString() : String.Empty;
//  }
//}

//#endregion

//unit.Save();