//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Concentrator.Objects.ConcentratorService;
//using Concentrator.Objects.Ftp;
//using System.Data;
//using System.IO;
//using Concentrator.Objects.Models.Brands;
//using Concentrator.Objects.Models.Products;
//using Concentrator.Objects.Models.Attributes;
//using Concentrator.Objects.Models.Vendors;
//using Concentrator.Objects.Utility;
//using Concentrator.Objects.Enumerations;
//using System.Transactions;
//using System.Globalization;

//namespace Concentrator.Plugins.Amacom
//{
//  public class ProductImport : ConcentratorPlugin
//  {

//    public override string Name
//    {
//      get { return "Amacom product import"; }
//    }

//    private const int unmappedID = -1;

//    protected override void Process()
//    {
//      var config = GetConfiguration();

//      DataTable content = null;

//      try
//      {
//        FtpManager productDownloader = new FtpManager(
//          config.AppSettings.Settings["AmacomUrl"].Value,
//          config.AppSettings.Settings["ProductPath"].Value,
//          config.AppSettings.Settings["Username"].Value,
//          config.AppSettings.Settings["Password"].Value,
//         false, true, log);

//        var clientID = config.AppSettings.Settings["ClientID"].Value;

//        var filename = clientID + ".csv";

//        using (var file = productDownloader.OpenFile(filename))
//        {
//          content = CreateDataTable(file.Data, false, ';');
//        }

//        if (content != null)
//        {
//          using (var unit = GetUnitOfWork())
//          {
//            log.AuditInfo(string.Format("Start: {0}", DateTime.Now));
//            Parseproducts(content, unit);
//            using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Suppress, TimeSpan.FromMinutes(2)))
//            {
//              unit.Save();
//              ts.Complete();
//            }
//            log.AuditInfo(string.Format("Done: {0}", DateTime.Now));
//          }
//        }

//      }
//      catch (Exception ex)
//      {
//        log.AuditFatal("Unable to retreive file from ftp");
//      }
//    }

//    private void Parseproducts(DataTable content, Objects.DataAccess.UnitOfWork.IUnitOfWork unit)
//    {
//      var itemProducts = (from line in content.AsEnumerable()
//                          select new
//                          {
//                            CustomItemNr = line[1].ToString().Trim(),
//                            ProductName = line[2].ToString().Trim(),
//                            SalesPrice = line[12].ToString().Trim(),
//                            EAN = line[4].ToString().Trim(),
//                            Brand = line[5].ToString().Trim(),
//                            ProductGroup = line[6].ToString().Trim(),
//                            VendorItemNr = line[7].ToString().Trim(),
//                            Stock = line[9].ToString().Trim(),
//                            StockIncl = line[10].ToString().Trim(),
//                            Date = line[14].ToString().Trim()
//                          }).ToList();

//      var config = GetConfiguration();

//      var VendorID = Int32.Parse(config.AppSettings.Settings["VendorID"].Value);

//      var repoBrandVendor = unit.Scope.Repository<BrandVendor>();

//      var prodRepo = unit.Scope.Repository<Product>().Include(c => c.ProductMedias, c => c.ProductBarcodes, c => c.ProductDescriptions);
//      var productAttributeGroups = unit.Scope.Repository<ProductAttributeGroupMetaData>().GetAll(g => g.VendorID == VendorID).ToList();
//      var repoAssortment = unit.Scope.Repository<VendorAssortment>().Include(c => c.VendorPrices);
//      var stockRepo = unit.Scope.Repository<VendorStock>();
//      var repoAttributeGroup = unit.Scope.Repository<ProductAttributeGroupMetaData>();
//      var repoAttributeGroupName = unit.Scope.Repository<ProductAttributeGroupName>();
//      var repoAttribute = unit.Scope.Repository<ProductAttributeMetaData>().Include(c => c.ProductAttributeNames, c => c.ProductAttributeValues);
//      var repoVendor = unit.Scope.Repository<ProductGroupVendor>();
//      var prodDescriptionRepo = unit.Scope.Repository<ProductDescription>();

//      //var products = prodRepo.GetAll(x => x.SourceVendorID == VendorID).ToList();
//      var vendorassortments = repoAssortment.GetAll(x => x.VendorID == VendorID).ToList();
//      var productAttributes = repoAttribute.GetAll(g => g.VendorID == VendorID).ToList();
//      var currentProductGroupVendors = repoVendor.GetAll(v => v.Vendor.VendorID == VendorID).ToList();
//      var productGroupVendorRecords = repoVendor.GetAll(pc => pc.VendorID == VendorID).ToList();
//      var brands = repoBrandVendor.GetAll(bv => bv.VendorID == VendorID).ToList();

//      var vendorStock = stockRepo.GetAll(x => x.VendorID == VendorID).ToList();
//      //var prodDescriptions = prodDescriptionRepo.GetAll().ToList();

//      var prodAttributeGroups = repoAttributeGroup.GetAll(g => g.VendorID == VendorID).ToList();

//      ProductStatusVendorMapper mapper = new ProductStatusVendorMapper(unit.Scope.Repository<VendorProductStatus>(), VendorID);


//      int counter = 0;
//      int total = itemProducts.Count();
//      int totalNumberOfProductsToProcess = total;
//      log.InfoFormat("Start import {0} products", total);



//      foreach (var product in itemProducts)
//      {
//        try
//        {
//          if (counter == 100)
//          {
//            counter = 0;
//            log.InfoFormat("Still need to process {0} of {1}; {2} done;", totalNumberOfProductsToProcess, total, total - totalNumberOfProductsToProcess);
//          }

//          totalNumberOfProductsToProcess--;
//          counter++;

//          DateTime RequestDate;

//          DateTime.TryParseExact(product.Date, "yyyyMMdd", null, DateTimeStyles.None, out RequestDate);

//          if (RequestDate != null && RequestDate > DateTime.Now) continue;

//          #region Brand

//          var vbrand = brands.FirstOrDefault(vb => vb.VendorBrandCode.Trim() == product.Brand);

//          if (vbrand == null)
//          {
//            vbrand = new BrandVendor
//            {
//              VendorID = VendorID,
//              VendorBrandCode = product.Brand,
//              BrandID = unmappedID
//            };

//            brands.Add(vbrand);

//            repoBrandVendor.Add(vbrand);
//          }

//          vbrand.Name = product.Brand;

//          #endregion Brand

//          #region Product

//          var item = prodRepo.GetSingle(p => p.VendorItemNumber.Trim() == product.VendorItemNr && p.BrandID == vbrand.BrandID);
//          if (item == null)
//          {
//            item = new Product
//            {
//              VendorItemNumber = product.VendorItemNr,
//              BrandID = vbrand.BrandID,
//              SourceVendorID = VendorID
//            };
//            prodRepo.Add(item);
//          // products.Add(item);
//            unit.Save();
//          }
//          //else
//          //{
//          //  if (item.BrandID < 0)
//          //    item.BrandID = vbrand.BrandID;
//          //}

//          #endregion Product

//          #region Vendor assortment


//          //if (item.VendorAssortments == null) item.VendorAssortments = new List<VendorAssortment>();

//          var assortment = vendorassortments.FirstOrDefault(x => x.ProductID== item.ProductID);
//          if (assortment == null)
//          {
//            assortment = new VendorAssortment
//            {
//              VendorID = VendorID,
//              Product = item
//            };
//            vendorassortments.Add(assortment);
//            repoAssortment.Add(assortment);

//          }

//          //assortment.ShortDescription = product.Longdescription.Length > 150 ? product.Longdescription.Substring(0, 150) : product.Longdescription;
//          assortment.CustomItemNumber = product.CustomItemNr;
//          assortment.IsActive = true;
//          //assortment.LongDescription = product.Longdescription;


//          #region Product group
//          try
//          {
//            var groupCode = product.Try(x => x.ProductGroup, null);

//            if (groupCode != null)
//            {
//              var productGroupVendor = productGroupVendorRecords.Where(pg => pg.GetType().GetProperty("VendorProductGroupCode1").GetValue(pg, null) != null
//                    && pg.GetType().GetProperty("VendorProductGroupCode1").GetValue(pg, null).ToString() == groupCode.Trim()).FirstOrDefault();

//              if (productGroupVendor == null)
//              {
//                productGroupVendor = new ProductGroupVendor
//                {
//                  ProductGroupID = unmappedID,
//                  VendorID = VendorID,
//                  VendorName = groupCode.Trim()
//                };

//                productGroupVendor.GetType().GetProperty("VendorProductGroupCode1").SetValue(productGroupVendor, groupCode.Trim(), null);

//                repoVendor.Add(productGroupVendor);
//                productGroupVendorRecords.Add(productGroupVendor);
//              }

//              #region sync
//              if (currentProductGroupVendors.Contains(productGroupVendor))
//              {
//                currentProductGroupVendors.Remove(productGroupVendor);
//              }
//              #endregion
//            }
//          }
//          catch (Exception ex)
//          {
//            log.Error("Failed productgroups", ex);
//          }
//          #endregion


//          #region Vendor Product Group Assortment

//          string VendorProductGroupCode1 = product.ProductGroup;


//          var records = (from l in productGroupVendorRecords
//                         where ((VendorProductGroupCode1 != null && l.VendorProductGroupCode1 != null &&
//                             l.VendorProductGroupCode1.Trim() == VendorProductGroupCode1) || l.VendorProductGroupCode1 == null)
//                         select l).ToList();

//          List<int> existingProductGroupVendors = new List<int>();

//          foreach (ProductGroupVendor prodGroupVendor in records)
//          {
//            existingProductGroupVendors.Add(prodGroupVendor.ProductGroupVendorID);

//            if (prodGroupVendor.VendorAssortments == null)
//            {
//              prodGroupVendor.VendorAssortments = new List<VendorAssortment>();
//            }
//            if (prodGroupVendor.VendorAssortments.Any(x => x.VendorAssortmentID == assortment.VendorAssortmentID))
//            { // only add new rows
//              continue;
//            }
//            prodGroupVendor.VendorAssortments.Add(assortment);
//          }

//          var unusedPGV = new List<ProductGroupVendor>();
//          assortment.ProductGroupVendors.ForEach((pgv, id) =>
//          {
//            if (!existingProductGroupVendors.Contains(pgv.ProductGroupVendorID))
//            {
//              unusedPGV.Add(pgv);
//            }
//          });

//          unusedPGV.ForEach((pg) => { assortment.ProductGroupVendors.Remove(pg); });

//          #endregion

//          #endregion Vendor assortment

//          #region Stock

//          var stock = vendorStock.FirstOrDefault(c => c.ProductID == assortment.ProductID && c.VendorID == assortment.VendorID);

//          if (stock == null)
//          {
//            stock = new VendorStock
//            {
//              VendorID = VendorID,
//              Product = item,
//              VendorStockTypeID = 1
//            };
//            vendorStock.Add(stock);
//            stockRepo.Add(stock);
//          }
//          stock.QuantityOnHand = Int32.Parse(product.Stock);
//          stock.StockStatus = stock.QuantityOnHand > 0 ? "InStock" : "OutOfStock";
//          stock.ConcentratorStatusID = mapper.SyncVendorStatus(stock.StockStatus, -1);

//          #endregion Stock

//          #region Price

//          if (assortment.VendorPrices == null) assortment.VendorPrices = new List<VendorPrice>();
//          var price = assortment.VendorPrices.FirstOrDefault();
//          if (price == null)
//          {
//            price = new VendorPrice
//            {
//              VendorAssortment = assortment,
//              MinimumQuantity = 0
//            };

//            unit.Scope.Repository<VendorPrice>().Add(price);
//          }
//          price.BasePrice = Decimal.Parse(product.SalesPrice,  CultureInfo.InvariantCulture);
//          price.ConcentratorStatusID = stock.ConcentratorStatusID;

//          #endregion Price

//          #region Barcode

//          if (!String.IsNullOrEmpty(product.EAN))
//          {
//            if (item.ProductBarcodes == null) item.ProductBarcodes = new List<ProductBarcode>();
//            if (!item.ProductBarcodes.Any(pb => pb.Barcode.Trim() == product.EAN.Trim()))
//            {
//              unit.Scope.Repository<ProductBarcode>().Add(new ProductBarcode
//               {
//                 Product = item,
//                 Barcode = product.EAN.Trim(),
//                 BarcodeType = (int)BarcodeTypes.Default,
//                 VendorID = VendorID
//               });
//            }
//          }

//          #endregion Barcode
          
//        }
//        catch (Exception ex)
//        {
//          log.Error("Error import products for Copaco", ex);
//        }
//      }
//      #region delete unused productgroupvendor records
//      foreach (var vProdGroup in currentProductGroupVendors)
//      {
//        if (vProdGroup.ProductGroupID == -1)
//        {
//          unit.Scope.Repository<ProductGroupVendor>().Delete(vProdGroup);
//        }
//      }
//      #endregion


//    }

//    private DataTable CreateDataTable(Stream File, bool includeHeader, char delimiter)
//    {
//      try
//      {
//        DataTable table = new DataTable();
//        string csv;
//        using (System.IO.StreamReader csv_file = new StreamReader(File))
//        {
//          csv = csv_file.ReadToEnd();
//        }

//        using (StringReader reader = new StringReader(csv))
//        {

//          int rows = 0;
//          string line;
//          while ((line = reader.ReadLine()) != null)
//          {
//            line = line.Replace(@"\""", "");
//            string[] vals = line.Split(delimiter);
//            for (int i = 0; i < vals.Length; i++)
//            {
//              if (table.Columns.Count < vals.Length)
//              {
//                table.Columns.Add(new DataColumn());
//              }
//              vals[i] = vals[i].Trim('"');
//            }
//            if (rows != 0 && !includeHeader)
//            {
//              table.Rows.Add(vals);
//            }
//            rows++;
//          }
//        }

//        return table;
//      }
//      catch (Exception ex)
//      {
//        return null;
//      }
//    }

//  }
//}
