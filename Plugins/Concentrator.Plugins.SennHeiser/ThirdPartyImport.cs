using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.ConcentratorService;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.Models.Brands;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Models.Attributes;
using Concentrator.Objects.Models.Media;
using Concentrator.Objects.Utility;
using System.Transactions;
using System.IO;
using LinqToExcel;
using Concentrator.Objects.Enumerations;

namespace Concentrator.Plugins.SennHeiser
{
  class ThirdPartyImport : ConcentratorPlugin
  {

    public override string Name
    {
      get { return "Sennheiser 3rd party product Import Plugin"; }
    }
    private Dictionary<int, string> vendors = new Dictionary<int, string>();

    private const int unmappedID = -1;
    private const int languageID = 1;

    protected override void Process()
    {

      var config = GetConfiguration();

      vendors.Add(int.Parse(config.AppSettings.Settings["VendorID_NL"].Value), "NL");
      vendors.Add(int.Parse(config.AppSettings.Settings["VendorID_BE"].Value), "BE");

      try
      {
        using (var unit = GetUnitOfWork())
        {
          var path = config.AppSettings.Settings["SennheiserBasePath"].Value;
          var tpPath = Path.Combine(path, config.AppSettings.Settings["SennheiserThirdPartiesPath"].Value);
          var files = Directory.GetFiles(path);

          foreach (var file in files)
          {
            if (!file.Contains("\\Import_")) continue;

            var filePath = Path.Combine(path, file);
            var excel = new ExcelQueryFactory(filePath);

            excel.AddMapping<TrdPartyProductModel>(x => x.ShortDescription, "Short Description");
            excel.AddMapping<TrdPartyProductModel>(x => x.LongDescription, "Long Description");
            excel.AddMapping<TrdPartyProductModel>(x => x.NLincl, "NL incl");
            excel.AddMapping<TrdPartyProductModel>(x => x.BEincl, "BE incl#");
            excel.AddMapping<TrdPartyProductModel>(x => x.VATExclNL, "VAT Excl NL");
            excel.AddMapping<TrdPartyProductModel>(x => x.VATExclBE, "VAT Excl BE");
            excel.AddMapping<TrdPartyProductModel>(x => x.ProductSheet, "Product Sheet");
            excel.AddMapping<TrdPartyProductModel>(x => x.Factsheet, "Fact sheet");
            excel.AddMapping<TrdPartyProductModel>(x => x.Instructionforuse, "Instruction for use");

            excel.AddMapping<Attributes>(x => x.AttArtnr, "Article number");
            excel.AddMapping<Attributes>(x => x.Feature, "Feature");
            excel.AddMapping<Attributes>(x => x.Value, "Value");

            var sheetNames = excel.GetWorksheetNames().ToList();

            var data = (from p in excel.Worksheet<TrdPartyProductModel>(sheetNames[0])
                        select p).ToList();

            var atts = (from a in excel.Worksheet<Attributes>(sheetNames[1])
                        select a).ToList();

            var columsn = excel.GetColumnNames("Blad1");

            DoProcess(data, atts, unit);
            unit.Save();
            FileInfo inf = new FileInfo(filePath);

            File.Move(filePath, Path.Combine(tpPath, inf.Name));
          }
          using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Suppress, TimeSpan.FromMinutes(2)))
          {

            ts.Complete();
          }
        }
      }
      catch (Exception ex)
      {
        log.AuditError("Error import products", ex);
        throw new NotImplementedException();
      }

    }


    private void DoProcess(List<TrdPartyProductModel> data, List<Attributes> atts, Objects.DataAccess.UnitOfWork.IUnitOfWork unit)
    {
      var config = GetConfiguration();

      var vendorID_NL = vendors.First(x => x.Value == "NL").Key;
      var vendorID_BE = vendors.First(x => x.Value == "BE").Key;

      var repoBrandVendor = unit.Scope.Repository<BrandVendor>();

      var prodRepo = unit.Scope.Repository<Product>().Include(c => c.ProductMedias, c => c.ProductBarcodes, c => c.ProductDescriptions);
      var productAttributeGroups = unit.Scope.Repository<ProductAttributeGroupMetaData>().GetAll(g => g.VendorID == vendorID_NL || g.VendorID == vendorID_BE).ToList();
      var repoAssortment = unit.Scope.Repository<VendorAssortment>().Include(c => c.VendorPrices);
      var stockRepo = unit.Scope.Repository<VendorStock>();
      var repoAttributeGroup = unit.Scope.Repository<ProductAttributeGroupMetaData>();
      var repoAttributeGroupName = unit.Scope.Repository<ProductAttributeGroupName>();
      var repoAttributeValue = unit.Scope.Repository<ProductAttributeValue>();
      var repotAttributeName = unit.Scope.Repository<ProductAttributeName>();
      var repoAttribute = unit.Scope.Repository<ProductAttributeMetaData>().Include(c => c.ProductAttributeNames, c => c.ProductAttributeValues);
      var repoVendor = unit.Scope.Repository<ProductGroupVendor>();
      var prodDescriptionRepo = unit.Scope.Repository<ProductDescription>();
      var priceRepo = unit.Scope.Repository<VendorPrice>();
      var productGroupLanguageRepo = unit.Scope.Repository<ProductGroupLanguage>();
      var productGroupMappingRepo = unit.Scope.Repository<ProductGroupMapping>();

      //var products = prodRepo.GetAll(x => x.SourceVendorID == VendorID).ToList();
      var vendorassortments = repoAssortment.GetAll(x => x.VendorID == vendorID_NL || x.VendorID == vendorID_BE).ToList();
      var productAttributes = repoAttribute.GetAll(g => g.VendorID == vendorID_NL || g.VendorID == vendorID_BE).ToList();

      var productGroupVendorRecords = repoVendor.GetAll(pc => pc.VendorID == vendorID_NL || pc.VendorID == vendorID_BE).ToList();
      var brands = repoBrandVendor.GetAll(bv => bv.VendorID == vendorID_NL || bv.VendorID == vendorID_BE).ToList();
      var brandEnts = unit.Scope.Repository<Brand>().GetAll();
      var products = prodRepo.GetAll(x => x.SourceVendorID == vendorID_NL || x.SourceVendorID == vendorID_BE).ToList();
      var vendorStock = stockRepo.GetAll(x => x.VendorID == vendorID_NL || x.VendorID == vendorID_BE).ToList();
      var productDescriptions = prodDescriptionRepo.GetAll().ToList();

      var productGroupLanguages = productGroupLanguageRepo.GetAll().ToList();
      var prodAttributeGroups = repoAttributeGroup.GetAll(g => g.VendorID == vendorID_NL || g.VendorID == vendorID_BE).ToList();
      var productGroupMapping = productGroupMappingRepo.GetAll(x => x.ConnectorID == 2).ToList();

      int counter = 0;
      int total = data.Count();
      int totalNumberOfProductsToProcess = total;
      log.InfoFormat("Start import {0} products", total);
      bool firstVendor = true;

      foreach (var VendorID in vendors.Keys)
      {

        ProductStatusVendorMapper mapper = new ProductStatusVendorMapper(unit.Scope.Repository<VendorProductStatus>(), VendorID);
        var vendor = unit.Scope.Repository<Vendor>().GetSingle(x => x.VendorID == VendorID);

        foreach (var product in data)
        {
          if (string.IsNullOrEmpty(product.Artnr)) continue;
          try
          {
            if (counter == 100)
            {
              counter = 0;
              log.InfoFormat("Still need to process {0} of {1}; {2} done for {3};", totalNumberOfProductsToProcess, total, total - totalNumberOfProductsToProcess, VendorID);
            }

            totalNumberOfProductsToProcess--;
            counter++;

            var brand = brandEnts.FirstOrDefault(c => c.Name.ToLower() == product.BrandName.Trim().ToLower());
            int brandID = 0;

            if (brand == null)
            {
              var vbrand = brands.FirstOrDefault(vb => vb.VendorBrandCode.Trim() == product.BrandName.Trim());

              if (vbrand == null)
              {
                vbrand = new BrandVendor
                {
                  VendorID = VendorID,
                  VendorBrandCode = product.BrandName.Trim(),
                  BrandID = unmappedID,
                  Name = product.BrandName.Trim()
                };

                brands.Add(vbrand);

                repoBrandVendor.Add(vbrand);
              }
              brandID = vbrand.BrandID;
            }
            else { brandID = brand.BrandID; }

            var item = products.FirstOrDefault(p => p.VendorItemNumber.Trim() == product.Artnr.Trim() && p.BrandID == brandID);
            if (item == null)
            {
              item = new Product
              {
                VendorItemNumber = product.Artnr.Trim(),
                BrandID = brandID,
                SourceVendorID = VendorID
              };
              prodRepo.Add(item);
              products.Add(item);
              unit.Save();
            }

            if (firstVendor)
            {
              var productDescription = productDescriptions.FirstOrDefault(pd => pd.Product.VendorItemNumber == item.VendorItemNumber && pd.LanguageID == 1 && (pd.VendorID == vendor.VendorID || (vendor.ParentVendorID.HasValue && pd.VendorID == vendor.VendorID)));

              if (productDescription == null)
              {
                //create ProductDescription
                productDescription = new ProductDescription
                {
                  Product = item,
                  LanguageID = languageID,
                  VendorID = VendorID,
                };

                prodDescriptionRepo.Add(productDescription);
                productDescriptions.Add(productDescription);
              }

              if (string.IsNullOrEmpty(productDescription.ProductName))
              {
                productDescription.ProductName = product.Product;
                productDescription.ModelName = product.Product;
              }

              if (!string.IsNullOrEmpty(product.ShortDescription))
                productDescription.ShortContentDescription = product.ShortDescription.Cap(1000);

              if (string.IsNullOrEmpty(productDescription.LongContentDescription))
                productDescription.LongContentDescription = product.LongDescription;

              if (string.IsNullOrEmpty(productDescription.LongSummaryDescription))
                productDescription.LongSummaryDescription = product.LongDescription;
            }

            var assortment = vendorassortments.FirstOrDefault(x => x.Product.VendorItemNumber == item.VendorItemNumber.Trim() && x.VendorID == VendorID);
            if (assortment == null)
            {
              assortment = new VendorAssortment
              {
                VendorID = VendorID,
                Product = item,
                ShortDescription = product.Try(c => c.ShortDescription.Cap(1000), string.Empty),//.ShortDescription // .Length > 150 ? product.ShortDescription.Substring(0, 150) : product.ShortDescription,
                CustomItemNumber = product.Artnr.Trim(),
                IsActive = true,
                LongDescription = product.LongDescription
              };
              vendorassortments.Add(assortment);
              repoAssortment.Add(assortment);
            }

            var productGroups = new string[4];
            productGroups[0] = product.BrandName;
            productGroups[1] = product.Chapter;
            productGroups[2] = product.Category;
            productGroups[3] = product.Subcategory;

            var pgvs = assortment.ProductGroupVendors != null ? assortment.ProductGroupVendors.ToList() : new List<ProductGroupVendor>();
            ProductGroupMapping parentpgm = null;

            for (int i = 0; i < productGroups.Length; i++)
            {
              var groupCode = productGroups[i];

              if (string.IsNullOrEmpty(groupCode)) continue;

              var productGroup = productGroupLanguages.FirstOrDefault(x => x.Name == groupCode.Trim());
              if (productGroup == null)
              {
                var pg = new Concentrator.Objects.Models.Products.ProductGroup()
                {
                  Score = 0
                };

                unit.Scope.Repository<Concentrator.Objects.Models.Products.ProductGroup>().Add(pg);

                productGroup = new ProductGroupLanguage()
                {
                  ProductGroup = pg,
                  LanguageID = 1,
                  Name = groupCode.Trim()
                };

                productGroupLanguages.Add(productGroup);
                productGroupLanguageRepo.Add(productGroup);
                unit.Save();
              }

              //var productGroupVendor = productGroupVendorRecords.Where(pg => pg.GetType().GetProperty(string.Format("VendorProductGroupCode{0}", i + 1)).GetValue(pg, null) != null
              //        && pg.GetType().GetProperty(string.Format("VendorProductGroupCode{0}", i + 1)).GetValue(pg, null).ToString() == groupCode.Trim()).FirstOrDefault();

              var productGroupVendor = productGroupVendorRecords.FirstOrDefault(x => x.ProductGroupID == productGroup.ProductGroupID);

              if (productGroupVendor == null)
              {
                productGroupVendor = new ProductGroupVendor
                {
                  ProductGroupID = productGroup.ProductGroupID,
                  VendorID = VendorID,
                  VendorName = groupCode.Trim(),
                };

                productGroupVendor.GetType().GetProperty(string.Format("VendorProductGroupCode{0}", i + 1)).SetValue(productGroupVendor, groupCode.Trim().Cap(50), null);

                repoVendor.Add(productGroupVendor);
                productGroupVendorRecords.Add(productGroupVendor);
              }

              if (productGroupVendor.VendorAssortments == null) productGroupVendor.VendorAssortments = new List<VendorAssortment>();

              var assortmentRelation = productGroupVendor.VendorAssortments.Count == 0 ? null : productGroupVendor.VendorAssortments.Where(c => c.Product != null && c.Product.VendorItemNumber == item.VendorItemNumber.Trim() && c.VendorID == VendorID).FirstOrDefault();
              if (assortmentRelation == null)
              {
                productGroupVendor.VendorAssortments.Add(assortment);
              }
              else
              {
                pgvs.Remove(productGroupVendor);
              }

              if (productGroupVendor.ProductGroupID > 0)
              {

                ProductGroupMapping pgm = null;

                if (parentpgm != null)
                  pgm = productGroupMapping.FirstOrDefault(x => x.ProductGroupID == productGroupVendor.ProductGroupID && x.ParentProductGroupMappingID == parentpgm.ProductGroupMappingID);
                else
                  pgm = productGroupMapping.FirstOrDefault(x => x.ProductGroupID == productGroupVendor.ProductGroupID);

                if (pgm == null)
                {
                  pgm = new ProductGroupMapping()
                  {
                    ConnectorID = 2,
                    ProductGroupID = productGroupVendor.ProductGroupID,
                    FlattenHierarchy = false,
                    FilterByParentGroup = parentpgm != null ? true : false,
                    Depth = i,
                    Score = 0
                  };

                  if (parentpgm != null)
                    pgm.ParentProductGroupMappingID = parentpgm.ProductGroupMappingID;

                  unit.Scope.Repository<ProductGroupMapping>().Add(pgm);
                  unit.Save();
                  productGroupMapping.Add(pgm);
                }
                parentpgm = pgm;
              }
            }

            pgvs.ForEach(pg =>
            {
              productGroupVendorRecords.Remove(pg);
            });

            if (firstVendor)
            {
              if (!string.IsNullOrEmpty(product.PriceGroup))
              {
                atts.Add(new Attributes()
                {
                  AttArtnr = product.Artnr.Trim(),
                  Feature = "PriceGroup",
                  Value = product.PriceGroup
                });
              }

              if (!string.IsNullOrEmpty(product.Features))
              {
                atts.Add(new Attributes()
               {
                 AttArtnr = product.Artnr.Trim(),
                 Feature = "Features",
                 Value = product.Features
               });
              }

              if (!string.IsNullOrEmpty(product.NEW))
              {
                atts.Add(new Attributes()
               {
                 AttArtnr = product.Artnr.Trim(),
                 Feature = "New",
                 Value = product.NEW
               });
              }

              var attributes = atts.Where(x => x.AttArtnr != null && (x.AttArtnr.Trim() == product.Artnr.Trim())).ToList();

              if (attributes != null && attributes.Count > 0)
              {
                foreach (var att in attributes)
                {

                  string AttributeCode = "General",
                    AttributeGroupCode = att.Feature,
                     AttributeValue = att.Value;

                  if (!string.IsNullOrEmpty(AttributeGroupCode) && !string.IsNullOrEmpty(AttributeValue))
                  {
                    var productAttributeMetadata = productAttributes.FirstOrDefault(c => c.ProductAttributeNames.Any(l => l.Name == AttributeGroupCode) && c.VendorID == VendorID);
                    if (productAttributeMetadata == null)
                    {
                      productAttributeMetadata = new ProductAttributeMetaData
                      {
                        AttributeCode = att.Feature,
                        Index = 0,
                        IsVisible = true,
                        NeedsUpdate = true,
                        VendorID = VendorID,
                        IsSearchable = false
                      };
                      productAttributes.Add(productAttributeMetadata);
                      repoAttribute.Add(productAttributeMetadata);
                    }

                    var attributeGroup = productAttributeMetadata.ProductAttributeGroupMetaData;
                    if (attributeGroup == null)
                    {
                      attributeGroup = new ProductAttributeGroupMetaData
                      {
                        Index = 0,
                        GroupCode = "TechnicalData",
                        VendorID = VendorID
                      };
                      repoAttributeGroup.Add(attributeGroup);
                      productAttributeMetadata.ProductAttributeGroupMetaData = attributeGroup;
                    }
                    productAttributeMetadata.ProductAttributeGroupID = attributeGroup.ProductAttributeGroupID;

                    var attributeGroupName = attributeGroup.ProductAttributeGroupNames.FirstOrDefault(c => c.LanguageID == 1);
                    if (attributeGroupName == null)
                    {
                      attributeGroupName = new ProductAttributeGroupName
                      {
                        Name = AttributeCode,
                        LanguageID = languageID,
                        ProductAttributeGroupMetaData = attributeGroup
                      };
                      repoAttributeGroupName.Add(attributeGroupName);
                      attributeGroup.ProductAttributeGroupNames.Add(attributeGroupName);
                    }



                    var attributeName = productAttributeMetadata.ProductAttributeNames.FirstOrDefault(c => c.LanguageID == 1);
                    if (attributeName == null)
                    {
                      attributeName = new ProductAttributeName
                      {
                        ProductAttributeMetaData = productAttributeMetadata,
                        LanguageID = 1,
                        Name = AttributeGroupCode
                      };
                      repotAttributeName.Add(attributeName);
                      productAttributeMetadata.ProductAttributeNames.Add(attributeName);
                    }

                    var attributeValue = productAttributeMetadata.ProductAttributeValues.FirstOrDefault(c => c.Value == AttributeValue && c.ProductID == item.ProductID);
                    if (attributeValue == null && !string.IsNullOrEmpty(AttributeValue))
                    {
                      attributeValue = new ProductAttributeValue
                      {
                        Value = AttributeValue,
                        ProductAttributeMetaData = productAttributeMetadata,
                        LanguageID = 1,
                        Product = item

                      };
                      productAttributeMetadata.ProductAttributeValues.Add(attributeValue);
                      repoAttributeValue.Add(attributeValue);
                    }
                  }
                }
              }
            }

            var stock = vendorStock.FirstOrDefault(c => c.Product.VendorItemNumber == item.VendorItemNumber.Trim() && c.VendorID == VendorID);

            if (stock == null)
            {
              stock = new VendorStock
                               {
                                 Product = item,
                                 QuantityOnHand = 0,
                                 VendorID = VendorID,
                                 VendorStockTypeID = 1
                               };
              vendorStock.Add(stock);
              stockRepo.Add(stock);
            }



            if (assortment.VendorPrices == null) assortment.VendorPrices = new List<VendorPrice>();


            var vendorPrice = assortment.VendorPrices.FirstOrDefault();
            //create vendorPrice with vendorAssortmentID
            if (vendorPrice == null)
            {
              vendorPrice = new VendorPrice
              {
                VendorAssortment = assortment,
                CommercialStatus = "S",
                MinimumQuantity = 0
              };
              priceRepo.Add(vendorPrice);
              assortment.VendorPrices.Add(vendorPrice);
            }

            decimal taxRate = 19;
            decimal price = 0;
            decimal costPrice = 0;

            switch (vendors[VendorID])
            {
              case "NL":
                taxRate = 19;
                Decimal.TryParse(product.VATExclNL, out costPrice);
                Decimal.TryParse(product.NLincl, out price);
                break;
              case "BE":
                taxRate = 21;
                Decimal.TryParse(product.VATExclBE, out costPrice);
                Decimal.TryParse(product.BEincl, out price);
                break;
            }

            vendorPrice.Price = price;
            vendorPrice.CostPrice = costPrice;
            vendorPrice.TaxRate = taxRate;

            if (product.Image != null)
            {
              if (item.ProductMedias == null) item.ProductMedias = new List<ProductMedia>();
              if (!item.ProductMedias.Any(pi => pi.VendorID == VendorID && pi.MediaUrl == product.Image))
              {
                unit.Scope.Repository<ProductMedia>().Add(new ProductMedia
                {
                  VendorID = VendorID,
                  MediaUrl = product.Image,
                  TypeID = 1, // image
                  Product = item,
                  Sequence = 0
                });
              }
            }


            if (product.EAN != null || product.Barcode != null)
            {
              if (item.ProductBarcodes == null) item.ProductBarcodes = new List<ProductBarcode>();
              if (!item.ProductBarcodes.Any(pb => pb.Barcode.Trim() == product.EAN.Trim()))
              {
                unit.Scope.Repository<ProductBarcode>().Add(new ProductBarcode
                {
                  Product = item,
                  Barcode = product.EAN != null ? product.EAN : product.Barcode != null ? product.Barcode : "",
                  BarcodeType = (int)BarcodeTypes.Default,
                  VendorID = VendorID
                });
              }
            }


          }
          catch (Exception ex)
          {
            log.AuditError("Error import products for Sennheiser 3rdParty", ex);
          }
        }
        firstVendor = false;
      }
    }
  }


  public class TrdPartyProductModel
  {
    public string Artnr { get; set; }
    public string Product { get; set; }
    public string BrandName { get; set; }
    public string Chapter { get; set; }
    public string Category { get; set; }
    public string Subcategory { get; set; }
    public string NEW { get; set; }
    public string PriceGroup { get; set; }
    public string ShortDescription { get; set; }
    public string LongDescription { get; set; }
    public string Features { get; set; }
    public string NLincl { get; set; }
    public string BEincl { get; set; }
    public string VATExclNL { get; set; }
    public string VATExclBE { get; set; }
    public string Warranty { get; set; }
    public string ProductSheet { get; set; }
    public string Factsheet { get; set; }
    public string Instructionforuse { get; set; }
    public string EAN { get; set; }
    public string Barcode { get; set; }
    public string Image { get; set; }
  }

  public class Attributes
  {
    public string AttArtnr { get; set; }
    public string Feature { get; set; }
    public string Value { get; set; }
  }

  public class ProductGroup
  {
    public string Chapter { get; set; }
    public string Category { get; set; }
    public string SubCategory { get; set; }
  }
}
