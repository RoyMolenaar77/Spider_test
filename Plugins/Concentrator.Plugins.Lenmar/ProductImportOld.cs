using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Concentrator.Objects;
using Concentrator.Objects.ConcentratorService;
using System.Transactions;
using Concentrator.Objects.Ftp;
using System.Xml;
using Concentrator.Objects.Enumerations;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Models.Brands;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.Models.Attributes;
using Concentrator.Objects.Utility;
using System.Configuration;
using Concentrator.Objects.Vendors;

namespace Concentrator.Plugins.Lenmar
{
  public class ProductImportOld : VendorBase
  {
    //private const int VendorID = 40;
    private const int unmappedID = -1;
    private const int languageID = 1;
    private string[] AttributeMapping = new[] { "Category", "UPC_Master_Carton", "Warranty", "Pkg_Gram", "Unit_Gram", "Color", "MasterCartonQty", "BC_euro_cost", "MIP_euro_sell", "Bullet1", "Bullet2", "Bullet3", "Bullet4", "Bullet5" };

    public override string Name
    {
      get { return "Lenmar Product Import Plugin"; }
    }

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

    private static List<string> NotAttributes = new List<string> { "Description", "Category" };

    protected override void SyncProducts()
    {
      var config = GetConfiguration();

      try
      {
        FtpManager productDownloader = new FtpManager(
          config.AppSettings.Settings["LenmarFtpUrl"].Value,
          config.AppSettings.Settings["LenmarProductPath"].Value,
          config.AppSettings.Settings["LenmarUserName"].Value,
          config.AppSettings.Settings["LenmarPassword"].Value,
         true, true, log);//new FtpDownloader("test/");

        FtpManager contentDownloader = new FtpManager(
         config.AppSettings.Settings["LenmarFtpUrl"].Value,
         config.AppSettings.Settings["LenmarContentPath"].Value,
         config.AppSettings.Settings["LenmarUserName"].Value,
         config.AppSettings.Settings["LenmarPassword"].Value,
        true, true, log);//new FtpDownloader("test/");

        var productList = productDownloader.ToList();
        XDocument[] products = new XDocument[productList.Count()];
        List<string> productFiles = new List<string>();
        XDocument content = new XDocument();
        List<string> contentFiles = new List<string>();

        for (int i = 0; i < productList.Count(); i++)
        {
          if (!Running)
            break;

          log.InfoFormat("Processing file: {0}", productList[i].FileName);
          using (var file = productDownloader.OpenFile(productList[i].FileName))
          {
            productFiles.Add(file.FileName);
            try
            {
              using (var reader = XmlReader.Create(file.Data))
              {
                reader.MoveToContent();
                products[i] = XDocument.Load(reader);
              }
            }
            catch (Exception ex)
            {

              log.AuditFatal("Failed to download product file");
              continue;
            }

          }

          //using (productList[i])
          //{
          //  productFiles.Add(productList[i].FileName);
          //  try
          //  {
          //    using (var reader = XmlReader.Create(productList[i].Data))
          //    {
          //      reader.MoveToContent();
          //      products[i] = XDocument.Load(reader);
          //    }
          //  }
          //  catch (Exception ex)
          //  {
          //    //log.Error(String.Format("Failed to load xml for file: {0}", productList[i].FileName), ex);
          //    //productDownloader.MarkAsError(productList[i].FileName);
          //    continue;
          //  }
          //}
        }

        foreach (var file in contentDownloader)
        {
          if (!Running)
            break;

          log.InfoFormat("Processing file: {0}", file.FileName);
          using (file)
          {
            contentFiles.Add(file.FileName);
            try
            {
              using (var reader = XmlReader.Create(file.Data))
              {
                reader.MoveToContent();
                content = XDocument.Load(reader);
              }
            }
            catch (Exception ex)
            {
              log.Error(String.Format("Failed to load xml for file: {0}", file.FileName), ex);
              contentDownloader.MarkAsError(file.FileName);
              continue;
            }
          }
        }
        //#if DEBUG
        //      XDocument content = XDocument.Load(@"C:\Personal folder\BAScontent.xml");
        //      XDocument products1 = XDocument.Load(@"C:\Personal folder\Products1.xml");
        //      XDocument products2 = XDocument.Load(@"C:\Personal folder\Products2.xml");
        //      XDocument[] products = new XDocument[2];
        //      products[0] = products1;
        //      products[1] = products2;
        //#else
        //      XDocument doc = XDocument.Load(@"D:\Projects\Documentation\Concentrator\Lenmar\Bas-Group.xml");
        //#endif


        if (true)
        {
          try
          {
            using (var unit = GetUnitOfWork())
            {

              ParseDocuments(unit, products, content);
              using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Suppress, TimeSpan.FromMinutes(2)))
              {

                ts.Complete();
              }
            }
          }
          catch (Exception ex)
          {
            log.Error("Error import products", ex);
          }
        }
        else
        {
          log.DebugFormat("No new files to process");
        }

        foreach (string file in productFiles)
        {
          productDownloader.MarkAsComplete(file);
        }

        //foreach (string file in contentFiles)
        //{
        //  productDownloader.MarkAsComplete(file);
        //}
      }
      catch (Exception ex)
      {
        log.Error("Error get lenmar files from ftp", ex);
      }
    }

    private void ParseDocuments(IUnitOfWork unit, XDocument[] products, XDocument cont)
    {
      #region Xml Data

      //products = new XDocument[1];
      //products[0] = XDocument.Load(@"C:\Lenmar\test.xml");

      var itemContent = (from content in cont.Root.Elements("item")
                         let attributes = content.Elements().Where(x => AttributeMapping.Contains(x.Name.LocalName))
                         where content.Element("LenmarSKU") != null && content.Element("Description") != null && content.Element("Manufacturer") != null
                         select new
                                    {
                                      LenmarSKU = content.Element("LenmarSKU").Value,
                                      ShortContentDescription = content.Element("Description").Value,
                                      GroupCode = content.Element("Manufacturer").Value,
                                      CostPrice = content.Element("BC_euro_cost") != null ? content.Element("BC_euro_cost").Value : "0",
                                      dynamic = attributes
                                    }).ToList();
      XNamespace xName = "http://logictec.com/schemas/internaldocuments";


      var itemProducts = (from d in products
                          from itemproduct in d.Elements(xName + "Envelope").Elements("Messages").Elements(xName + "Price")
                          let c = itemContent.Where(x => x.LenmarSKU == itemproduct.Element("SupplierSku").Value).FirstOrDefault()
                          select new
                                     {
                                       VendorBrandCode = itemproduct.Element("MfgName").Value,
                                       VendorName = itemproduct.Element("MfgName").Value,
                                       SupplierSKU = itemproduct.Element("SupplierSku").Value,
                                       CustomItemNr = itemproduct.Element("MfgSKU").Value,
                                       ShortDescription = itemproduct.Element("ProductName").Value,
                                       Price = itemproduct.Element("MSRP").Value,
                                       Status = itemproduct.Element("Active").Value,
                                       CostPrice = c != null ? c.CostPrice : itemproduct.Element("Price").Value,
                                       QuantityOnHand = itemproduct.Element("Inventory").Value,
                                       VendorProductGroupCode1 = itemproduct.Element("Category1").Value,
                                       VendorProductGroupCode2 = itemproduct.Element("Category2").Value,
                                       VendorProductGroupCode3 = itemproduct.Element("Category3").Value,
                                       VendorProductGroupCode4 = itemproduct.Element("Category4").Value,
                                       VendorProductGroupCode5 = itemproduct.Element("Category5").Value,
                                       Barcode = itemproduct.Element("UPCCode").Value,
                                       ShortContentDescription = c != null ? c.ShortContentDescription : string.Empty
                                     }).ToList();

      #endregion

      var _brandVendorRepo = unit.Scope.Repository<BrandVendor>();
      var _productRepo = unit.Scope.Repository<Product>();
      var _assortmentRepo = unit.Scope.Repository<VendorAssortment>();
      var productGroupVendorRepo = unit.Scope.Repository<ProductGroupVendor>();
      var _prodDescriptionRepo = unit.Scope.Repository<ProductDescription>();
      var _attrGroupRepo = unit.Scope.Repository<ProductAttributeGroupMetaData>();
      var _attrGroupName = unit.Scope.Repository<ProductAttributeGroupName>();
      var _attrRepo = unit.Scope.Repository<ProductAttributeMetaData>();
      var _attrNameRepo = unit.Scope.Repository<ProductAttributeName>();
      var _attrValueRepo = unit.Scope.Repository<ProductAttributeValue>();
      var _mediaRepo = unit.Scope.Repository<ProductMedia>();
      var _priceRepo = unit.Scope.Repository<VendorPrice>();
      var _stockRepo = unit.Scope.Repository<VendorStock>();
      var _barcodeRepo = unit.Scope.Repository<ProductBarcode>();

      var brands = _brandVendorRepo.GetAll(bv => bv.VendorID == VendorID).ToList();
      var productGroups = productGroupVendorRepo.GetAll(g => g.VendorID == VendorID).ToList();
      var productAttributes = _attrRepo.GetAll(g => g.VendorID == VendorID).ToList();
      var productAttributeGroups = _attrGroupRepo.GetAll().ToList();
      var currentProductGroupVendors = productGroupVendorRepo.GetAll(g => g.VendorID == VendorID).ToList();

      ProductStatusVendorMapper mapper = new ProductStatusVendorMapper(unit.Scope.Repository<VendorProductStatus>(), VendorID);

      var unusedVendorAssortmentItems = _assortmentRepo.GetAll(x => x.VendorID == VendorID && x.IsActive == true).ToList();

      int counter = 0;
      int total = itemProducts.Count();
      int totalNumberOfProductsToProcess = total;
      log.InfoFormat("Start import {0} products", total);

      foreach (var product in itemProducts)
      {
        if (counter == 50)
        {
          counter = 0;
          log.InfoFormat("Still need to process {0} of {1}; {2} done;", totalNumberOfProductsToProcess, total, total - totalNumberOfProductsToProcess);
        }
        totalNumberOfProductsToProcess--;
        counter++;

        try
        {
          #region BrandVendor

          Product prod = null;
          //check if brandvendor exists in db
          var brandVendor = brands.FirstOrDefault(vb => vb.VendorBrandCode == product.VendorBrandCode);

          if (brandVendor == null) //if brandvendor does not exist
          {
            //create new brandVendor
            brandVendor = new BrandVendor
                            {
                              BrandID = unmappedID,
                              VendorID = VendorID,
                              VendorBrandCode = product.VendorBrandCode,
                              Name = product.VendorBrandCode
                            };
            _brandVendorRepo.Add(brandVendor);
            brands.Add(brandVendor);
          }

          //use BrandID to create product and retreive ProductID

          var BrandID = brandVendor.BrandID;

          var prods = _productRepo.GetAll(p => p.VendorItemNumber == product.SupplierSKU && (p.BrandID == BrandID || p.BrandID < 0)).ToList();
          prod = prods.OrderByDescending(x => x.BrandID).FirstOrDefault();

          if (prods.Count() > 1)
          {
            try
            {
              _stockRepo.Delete(prods.Where(x => x.BrandID < 0).SelectMany(x => x.VendorStocks));
              unit.Save();
              _productRepo.Delete(prods.Where(x => x.BrandID < 0));
              unit.Save();
            }
            catch (Exception ex)
            {

            }
          }

          //if product does not exist (usually true)
          if (prod == null)
          {
            prod = new Product
                     {
                       VendorItemNumber = product.SupplierSKU,
                       SourceVendorID = VendorID
                     };
            _productRepo.Add(prod);
          }
          prod.BrandID = BrandID;
          unit.Save();

          #endregion

          #region VendorAssortMent

          var productID = prod.ProductID;

          if (prod.VendorAssortments == null) prod.VendorAssortments = new List<VendorAssortment>();
          var vendorAssortment = prod.VendorAssortments.FirstOrDefault(va => va.VendorID == VendorID);

          //if vendorAssortMent does not exist
          if (vendorAssortment == null)
          {
            //create vendorAssortMent with productID
            vendorAssortment = new VendorAssortment
                                 {
                                   Product = prod,
                                   CustomItemNumber = product.CustomItemNr,
                                   VendorID = VendorID
                                 };
            _assortmentRepo.Add(vendorAssortment);
          }
          vendorAssortment.IsActive = true;
          vendorAssortment.ShortDescription =
                                     product.ShortDescription.Length > 150
                                       ? product.ShortDescription.Substring(0, 150)
                                       : product.ShortDescription;
          vendorAssortment.LongDescription = "";

          unusedVendorAssortmentItems.Remove(vendorAssortment);

          #endregion

          #region VendorPrice

          if (vendorAssortment.VendorPrices == null) vendorAssortment.VendorPrices = new List<VendorPrice>();
          var vendorPrice = vendorAssortment.VendorPrices.FirstOrDefault();
          //create vendorPrice with vendorAssortmentID
          if (vendorPrice == null)
          {
            vendorPrice = new VendorPrice
                            {
                              VendorAssortment = vendorAssortment
                            };
            _priceRepo.Add(vendorPrice);
          }

          vendorPrice.Price = Decimal.Parse(product.Price) / 119;
          vendorPrice.TaxRate = (Decimal)19.00;
          vendorPrice.CommercialStatus = product.Status;
          vendorPrice.MinimumQuantity = 0;
          vendorPrice.CostPrice = Decimal.Parse(product.CostPrice) / 100;
          vendorPrice.ConcentratorStatusID = mapper.SyncVendorStatus(product.Status, -1);

          #endregion

          #region VendorStock

          //var vendorStock = vendorAssortment.VendorStock.FirstOrDefault();
          var vendorStock = _stockRepo.GetSingle(c => c.ProductID == vendorAssortment.ProductID && c.VendorID == vendorAssortment.VendorID);
          //create vendorStock with productID
          if (vendorStock == null)
          {

            vendorStock = new VendorStock
                            {
                              Product = prod
                            };
            _stockRepo.Add(vendorStock);
          }

          vendorStock.StockStatus = product.Status;
          vendorStock.QuantityOnHand = int.Parse(product.QuantityOnHand);
          vendorStock.VendorID = VendorID;
          vendorStock.VendorStockTypeID = 1;
          vendorStock.VendorStatus = product.Status;
          vendorStock.ConcentratorStatusID = mapper.SyncVendorStatus(product.Status, -1);

          #endregion

          #region ProductGroupVendor

          //var vendorProductGroupAssortments = (from c in context.VendorProductGroupAssortments
          //                                     where
          //                                       c.VendorAssortment == vendorAssortment
          //                                     select c).ToList();

          //create vendorGroup five times, each time with a different VendorProductGroupCode on a different level if not exist
          string vendorProductGroupCode1 = string.IsNullOrEmpty(product.VendorProductGroupCode1) ? "Battery" : product.VendorProductGroupCode1;

          var productGroupVendor1 =
            productGroups.FirstOrDefault(pg => pg.VendorProductGroupCode1 == vendorProductGroupCode1);

          if (productGroupVendor1 == null)
          {
            productGroupVendor1 = new ProductGroupVendor
                                    {
                                      ProductGroupID = unmappedID,
                                      VendorID = VendorID,
                                      VendorName = product.VendorName,
                                      VendorProductGroupCode1 = vendorProductGroupCode1,

                                      VendorAssortments = new List<VendorAssortment>()
                                    };
            productGroupVendorRepo.Add(productGroupVendor1);
            productGroups.Add(productGroupVendor1);
          }


          #region sync

          if (currentProductGroupVendors.Contains(productGroupVendor1))
          {
            currentProductGroupVendors.Remove(productGroupVendor1);
          }
          #endregion



          if (!productGroupVendor1.VendorAssortments.Any(c => c == vendorAssortment))
          {
            productGroupVendor1.VendorAssortments.Add(vendorAssortment);
          }

          if (!string.IsNullOrEmpty(product.VendorProductGroupCode2))
          {
            var productGroupVendor2 =
              productGroups.FirstOrDefault(pg => pg.VendorProductGroupCode2 == product.VendorProductGroupCode2);

            if (productGroupVendor2 == null)
            {
              productGroupVendor2 = new ProductGroupVendor
                                      {
                                        ProductGroupID = unmappedID,
                                        VendorID = VendorID,
                                        VendorName = product.VendorName,
                                        VendorProductGroupCode2 = product.VendorProductGroupCode2,
                                        VendorAssortments = new List<VendorAssortment>()
                                      };

              productGroupVendorRepo.Add(productGroupVendor2);
              productGroups.Add(productGroupVendor2);
            }


            #region sync

            if (currentProductGroupVendors.Contains(productGroupVendor2))
            {
              currentProductGroupVendors.Remove(productGroupVendor2);
            }
            #endregion

            var vendorProductGroupAssortment2 =
          productGroupVendor2.VendorAssortments.FirstOrDefault(c => c.VendorAssortmentID == vendorAssortment.VendorAssortmentID);

            if (vendorProductGroupAssortment2 == null)
            {
              productGroupVendor2.VendorAssortments.Add(vendorAssortment);
            }
          }

          if (!string.IsNullOrEmpty(product.VendorProductGroupCode3))
          {
            var productGroupVendor3 =
              productGroups.FirstOrDefault(pg => pg.VendorProductGroupCode3 == product.VendorProductGroupCode3);

            if (productGroupVendor3 == null)
            {
              productGroupVendor3 = new ProductGroupVendor
                                      {
                                        ProductGroupID = unmappedID,
                                        VendorID = VendorID,
                                        VendorName = product.VendorName,
                                        VendorProductGroupCode3 = product.VendorProductGroupCode3,
                                        VendorAssortments = new List<VendorAssortment>()
                                      };

              productGroupVendorRepo.Add(productGroupVendor3);
              productGroups.Add(productGroupVendor3);
            }


            #region sync

            if (currentProductGroupVendors.Contains(productGroupVendor3))
            {
              currentProductGroupVendors.Remove(productGroupVendor3);
            }
            #endregion

            var vendorProductGroupAssortment3 =
  productGroupVendor3.VendorAssortments.FirstOrDefault(c => c.VendorAssortmentID == vendorAssortment.VendorAssortmentID);

            if (vendorProductGroupAssortment3 == null)
            {
              productGroupVendor3.VendorAssortments.Add(vendorAssortment);
            }
          }

          if (!string.IsNullOrEmpty(product.VendorProductGroupCode4))
          {
            var productGroupVendor4 =
              productGroups.FirstOrDefault(pg => pg.VendorProductGroupCode4 == product.VendorProductGroupCode4);

            if (productGroupVendor4 == null)
            {
              productGroupVendor4 = new ProductGroupVendor
                                      {
                                        ProductGroupID = unmappedID,
                                        VendorID = VendorID,
                                        VendorName = product.VendorName,
                                        VendorProductGroupCode4 = product.VendorProductGroupCode4,
                                        VendorAssortments = new List<VendorAssortment>()
                                      };

              productGroupVendorRepo.Add(productGroupVendor4);
              productGroups.Add(productGroupVendor4);
            }


            #region sync

            if (currentProductGroupVendors.Contains(productGroupVendor4))
            {
              currentProductGroupVendors.Remove(productGroupVendor4);
            }
            #endregion

            var vendorProductGroupAssortment4 =
          productGroupVendor4.VendorAssortments.FirstOrDefault(c => c.VendorAssortmentID == vendorAssortment.VendorAssortmentID);

            if (vendorProductGroupAssortment4 == null)
            {
              productGroupVendor4.VendorAssortments.Add(vendorAssortment);
            }


          }

          if (!string.IsNullOrEmpty(product.VendorProductGroupCode5))
          {
            var productGroupVendor5 =
              productGroups.FirstOrDefault(pg => pg.VendorProductGroupCode5 == product.VendorProductGroupCode5);

            if (productGroupVendor5 == null)
            {
              productGroupVendor5 = new ProductGroupVendor
                                      {
                                        ProductGroupID = unmappedID,
                                        VendorID = VendorID,
                                        VendorName = product.VendorName,
                                        VendorProductGroupCode5 = product.VendorProductGroupCode5,
                                        VendorAssortments = new List<VendorAssortment>()
                                      };

              productGroupVendorRepo.Add(productGroupVendor5);
              productGroups.Add(productGroupVendor5);
            }


            #region sync

            if (currentProductGroupVendors.Contains(productGroupVendor5))
            {
              currentProductGroupVendors.Remove(productGroupVendor5);
            }

            #endregion
            var vendorProductGroupAssortment5 =
            productGroupVendor5.VendorAssortments.FirstOrDefault(c => c.VendorAssortmentID == vendorAssortment.VendorAssortmentID);

            if (vendorProductGroupAssortment5 == null)
            {
              productGroupVendor5.VendorAssortments.Add(vendorAssortment);
            }
          }

          if (!string.IsNullOrEmpty(product.VendorBrandCode))
          {
            var brandProductGroupvendor =
              productGroups.FirstOrDefault(pg => pg.BrandCode == product.VendorBrandCode && pg.VendorProductGroupCode1 == null && pg.VendorProductGroupCode2 == null && pg.VendorProductGroupCode3 == null && pg.VendorProductGroupCode4 == null && pg.VendorProductGroupCode5 == null);

            if (brandProductGroupvendor == null)
            {
              brandProductGroupvendor = new ProductGroupVendor
              {
                ProductGroupID = unmappedID,
                VendorID = VendorID,
                VendorName = product.VendorName,
                BrandCode = product.VendorBrandCode
              };

              productGroupVendorRepo.Add(brandProductGroupvendor);
              productGroups.Add(brandProductGroupvendor);
            }


            #region sync

            if (currentProductGroupVendors.Contains(brandProductGroupvendor))
            {
              currentProductGroupVendors.Remove(brandProductGroupvendor);
            }

            #endregion
            var brandProductGroupAssortment =
            brandProductGroupvendor.VendorAssortments.FirstOrDefault(c => c.VendorAssortmentID == vendorAssortment.VendorAssortmentID);

            if (brandProductGroupAssortment == null)
            {
              brandProductGroupvendor.VendorAssortments.Add(vendorAssortment);
            }
          }

          #endregion

          #region ProductBarcode
          if (prod.ProductBarcodes == null) prod.ProductBarcodes = new List<ProductBarcode>();
          if (!prod.ProductBarcodes.Any(pb => pb.Barcode.Trim() == product.Barcode))
          {
            //create ProductBarcode if not exists
            _barcodeRepo.Add(new ProductBarcode
                                                     {
                                                       Product = prod,
                                                       Barcode = product.Barcode,
                                                       VendorID = VendorID,
                                                       BarcodeType = (int)BarcodeTypes.Default
                                                     });
          }

          #endregion

          #region ProductDescription
          if (prod.ProductDescriptions == null) prod.ProductDescriptions = new List<ProductDescription>();
          var productDescription =
            prod.ProductDescriptions.FirstOrDefault(pd => pd.LanguageID == languageID && pd.VendorID == VendorID);

          if (productDescription == null)
          {
            //create ProductDescription
            productDescription = new ProductDescription
                                   {
                                     Product = prod,
                                     LanguageID = languageID,
                                     VendorID = VendorID
                                   };

            _prodDescriptionRepo.Add(productDescription);
          }
          productDescription.ShortContentDescription = product.ShortContentDescription;

          #endregion

          foreach (var content in itemContent.Where(x => x.LenmarSKU == product.SupplierSKU))
          {
            #region ProductAttributeGroupMetaData

            var productAttributeGroupMetaData =
              productAttributeGroups.FirstOrDefault(c => c.GroupCode == content.GroupCode);
            //create ProductAttributeGroupMetaData if not exists
            if (productAttributeGroupMetaData == null)
            {
              productAttributeGroupMetaData = new ProductAttributeGroupMetaData
                                                {
                                                  Index = 0,
                                                  GroupCode = content.GroupCode,
                                                  VendorID = VendorID
                                                };
              _attrGroupRepo.Add(productAttributeGroupMetaData);
              productAttributeGroups.Add(productAttributeGroupMetaData);
            }
            #endregion

            #region ProductAttributeGroupName
            if (productAttributeGroupMetaData.ProductAttributeGroupNames == null) productAttributeGroupMetaData.ProductAttributeGroupNames = new List<ProductAttributeGroupName>();
            var productAttributeGroupName =
              productAttributeGroupMetaData.ProductAttributeGroupNames.FirstOrDefault(c => c.LanguageID == languageID);
            //create ProductAttributeGroupName if not exists
            if (productAttributeGroupName == null)
            {
              productAttributeGroupName = new ProductAttributeGroupName
                                            {
                                              Name = "General",
                                              ProductAttributeGroupMetaData = productAttributeGroupMetaData,
                                              LanguageID = languageID
                                            };
              _attrGroupName.Add(productAttributeGroupName);
            }

            #endregion

            #region ProductAttributeMetaData

            //create ProductAttributeMetaData as many times that there are entrys in content.dynamic
            foreach (var element in content.dynamic)
            {
              var productAttributeMetaData =
                productAttributes.FirstOrDefault(c => c.AttributeCode == element.Name.ToString());
              //create ProductAttributeMetaData if not exists
              if (productAttributeMetaData == null)
              {
                productAttributeMetaData = new ProductAttributeMetaData
                                             {
                                               ProductAttributeGroupMetaData = productAttributeGroupMetaData,
                                               AttributeCode = element.Name.ToString(),
                                               Index = 0,
                                               IsVisible = true,
                                               NeedsUpdate = true,
                                               VendorID = VendorID,
                                               IsSearchable = false
                                             };
                _attrRepo.Add(productAttributeMetaData);
                productAttributes.Add(productAttributeMetaData);
              }

            #endregion

              #region ProductAttributeName
              if (productAttributeMetaData.ProductAttributeNames == null) productAttributeMetaData.ProductAttributeNames = new List<ProductAttributeName>();
              var productAttributeName =
                productAttributeMetaData.ProductAttributeNames.FirstOrDefault(c => c.LanguageID == languageID);
              //create ProductAttributeName with generated productAttributeMetaData.AttributeID if not exists
              if (productAttributeName == null)
              {
                productAttributeName = new ProductAttributeName
                                         {
                                           ProductAttributeMetaData = productAttributeMetaData,
                                           LanguageID = languageID,
                                           Name = element.Name.ToString()
                                         };
                _attrNameRepo.Add(productAttributeName);
              }

              #endregion

              #region ProductAttributeValue

              if (productAttributeMetaData.ProductAttributeValues == null) productAttributeMetaData.ProductAttributeValues = new List<ProductAttributeValue>();
              var productAttributeValue =
                productAttributeMetaData.ProductAttributeValues.FirstOrDefault(c => c.ProductID == prod.ProductID);
              //create ProductAttributeValue with generated productAttributeMetaData.AttributeID
              if (productAttributeValue == null)
              {
                productAttributeValue = new ProductAttributeValue
                                          {
                                            ProductAttributeMetaData = productAttributeMetaData,
                                            Product = prod,
                                            Value = element.Value,
                                            LanguageID = languageID
                                          };
                _attrValueRepo.Add(productAttributeValue);
              }

              #endregion

            }
          }
          unit.Save();
        }
        catch (Exception ex)
        {
          log.ErrorFormat("product: {0} error: {1}", product.SupplierSKU, ex.StackTrace);
        }
      }
      #region delete unused vendorProductGroups
      foreach (var vProdVendor in currentProductGroupVendors)
      {
        if (vProdVendor.ProductGroupID == -1)
        {
          productGroupVendorRepo.Add(vProdVendor);
        }
      }
      unit.Save();

      unusedVendorAssortmentItems.ForEach(x => x.IsActive = false);
      unit.Save();

      #endregion
    }
  }
}
