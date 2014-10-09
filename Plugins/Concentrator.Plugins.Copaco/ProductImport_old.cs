using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Service;
using Concentrator.Objects.ConcentratorService;
using Concentrator.Objects.Ftp;
using System.Xml.Linq;
using System.Xml;
using Concentrator.Objects.DataAccess.UnitOfWork;
using System.Transactions;
using System.Data;
using System.IO;
using Concentrator.Objects.Models.Brands;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.Models.Attributes;
using Concentrator.Objects.Utility;
using Concentrator.Objects.Enumerations;
using System.Text.RegularExpressions;
using System.Net;
using System.Threading;


namespace Concentrator.Plugins.Copaco
{
  public class ProductImport_old : ConcentratorPlugin
  {
    int VendorID = 0;
    int UnMappedID = -1;

    public override string Name
    {
      get { return "Copaco Product Import Plugin"; }
    }

    protected override void Process()
    {
      var config = GetConfiguration();
      XDocument products = new XDocument();
      DataTable content = new DataTable();
      DataTable statuses = new DataTable();
      DataTable taxTable = new DataTable();

      VendorID = Int32.Parse(config.AppSettings.Settings["VendorID"].Value);

      try
      {
        FtpManager productDownloader = new FtpManager(
          config.AppSettings.Settings["CopacoProductFtpUrl"].Value,
          config.AppSettings.Settings["CopacoProductPath"].Value,
          config.AppSettings.Settings["CopacoUserName"].Value,
          config.AppSettings.Settings["CopacoPassword"].Value,
         false, true, log);

        FtpManager contentDownloader = new FtpManager(
         config.AppSettings.Settings["CopacoFtpUrl"].Value,
         config.AppSettings.Settings["CopacoContentPath"].Value,
         config.AppSettings.Settings["CopacoContentUserName"].Value,
         config.AppSettings.Settings["CopacoContentPassword"].Value,
        false, true, log);


        if (!Running)
        {
          return;
        }

        using (var file = productDownloader.OpenFile("Copaco_prijslijst_73546.xml"))
        {

          try
          {
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.XmlResolver = null;
            settings.ProhibitDtd = false;

            using (var reader = XmlReader.Create(file.Data, settings))
            {
              reader.MoveToContent();
              products = XDocument.Load(reader);
            }
          }
          catch (Exception ex)
          {

            log.AuditFatal("Failed to download product file");
            return;
          }

        }

        try
        {
          SimpleFtp ftp = new SimpleFtp(
           config.AppSettings.Settings["CopacoFtpUrl"].Value,
           config.AppSettings.Settings["CopacoContentUserName"].Value,
           config.AppSettings.Settings["CopacoContentPassword"].Value, log, true);

          using (Stream contentFileStream = ftp.GetFtpFileStream(config.AppSettings.Settings["CopacoContentPath"].Value, "COPACO_PRODUCTHIERARCHIE.CSV"))
          {
            content = CreateDataTable(contentFileStream, false, ',');
            contentFileStream.Close();
          }

          using (Stream contentFileStream = ftp.GetFtpFileStream(config.AppSettings.Settings["CopacoContentPath"].Value, "COPACO_ATP_KWALIFICATIES.csv"))
          {
            statuses = CreateDataTable(contentFileStream, false, ',');
            contentFileStream.Close();
          }

          using (Stream contentFileStream = ftp.GetFtpFileStream(config.AppSettings.Settings["CopacoContentPath"].Value, "Country_tax_NL.csv"))
          {
            taxTable = CreateDataTable(contentFileStream, false, ',');
            contentFileStream.Close();
          }

        }
        catch (Exception ex)
        {
          log.AuditFatal("Failed to download catalog file", ex);
          return;
        }
      }
      catch (Exception ex)
      {
        log.Error("Error get copaco files from ftp", ex);
      }

      try
      {
        using (var unit = GetUnitOfWork())
        {
          if (products != null && content != null && statuses != null)
          {
            ParseDocuments(unit, products, content, statuses, taxTable);
          }
          using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Suppress, TimeSpan.FromMinutes(2)))
          {
            unit.Save();
            ts.Complete();
          }
        }
      }
      catch (Exception ex)
      {
        log.Error("Error import products", ex);
      }
    }





    private DataTable CreateDataTable(Stream File, bool includeHeader, char delimiter)
    {
      try
      {
        DataTable table = new DataTable();
        string csv;
        using (System.IO.StreamReader csv_file = new StreamReader(File))
        {
          csv = csv_file.ReadToEnd();
        }

        using (StringReader reader = new StringReader(csv))
        {

          int rows = 0;
          string line;
          while ((line = reader.ReadLine()) != null)
          {
            line = line.Replace(@"\""", "");
            string[] vals = line.Split(delimiter);
            for (int i = 0; i < vals.Length; i++)
            {
              if (table.Columns.Count < vals.Length)
              {
                table.Columns.Add(new DataColumn());
              }
              vals[i] = vals[i].Trim('"');
            }
            if (rows != 0 && !includeHeader)
            {
              table.Rows.Add(vals);
            }
            rows++;
          }
        }

        return table;
      }
      catch (Exception ex)
      {
        return null;
      }
    }

    private void ParseDocuments(IUnitOfWork unit, XDocument prods, DataTable cont, DataTable statuses, DataTable taxTable)
    {

      #region Xml Data

      var taxList = (from tax in taxTable.AsEnumerable()
                     select new
                     {
                       ID = tax[1].ToString(),
                       Tax = tax[2].ToString()

                     }).ToList();

      var vendorStatuses = (from status in statuses.AsEnumerable()
                            select new
                            {
                              Code = status[0].ToString(),
                              Name = status[1].ToString()
                            });

      var itemContent = (from content in cont.AsEnumerable()
                         select new
                        {
                          Code = content[0].ToString(),
                          Name = content[2].ToString(),
                          Level = content[1].ToString()
                        }).ToList();

      var itemProducts = (from itemproduct in prods.Element("XMLOUT_pricelist_01").Elements("item")
                          let content = itemContent
                          select new
                          {
                            VendorName = itemproduct.Element("vendor").Value,
                            CustomItemNr = itemproduct.Element("item_id").Value,
                            VendorItemNr = itemproduct.Element("vendor_id").Value,
                            Longdescription = itemproduct.Element("long_desc").Value,
                            Price = itemproduct.Element("price").Value,
                            Tax = taxList.FirstOrDefault(x => x.ID == itemproduct.Element("item_id").Value),
                            Status = vendorStatuses.FirstOrDefault(x => x.Code == itemproduct.Element("status").Value),
                            QuantityOnHand = itemproduct.Element("stock").Value,
                            VendorProductGroupCode1 = content.FirstOrDefault(x => x.Code.StartsWith(itemproduct.Element("item_group").Value.Substring(0, 2)) && Int32.Parse(x.Level) == 1),
                            VendorProductGroupCode2 = content.FirstOrDefault(x => x.Code.StartsWith(itemproduct.Element("item_group").Value.Substring(0, 2)) && Int32.Parse(x.Level) == 2),
                            VendorProductGroupCode3 = content.FirstOrDefault(x => x.Code.StartsWith(itemproduct.Element("item_group").Value.Substring(0, 2)) && Int32.Parse(x.Level) == 3),
                            VendorProductGroupCode4 = content.FirstOrDefault(x => x.Code.StartsWith(itemproduct.Element("item_group").Value.Substring(0, 2)) && Int32.Parse(x.Level) == 4),
                            VendorProductGroupCode5 = content.FirstOrDefault(x => x.Code.StartsWith(itemproduct.Element("item_group").Value.Substring(0, 2)) && Int32.Parse(x.Level) == 5),
                            Barcode = itemproduct.Element("EAN_code").Value,
                            ShortContentDescription = ""
                          }).ToList();

      #endregion

      var repoBrandVendor = unit.Scope.Repository<BrandVendor>();

      var prodRepo = unit.Scope.Repository<Product>().Include(c => c.ProductMedias, c => c.ProductBarcodes, c => c.ProductDescriptions);
      var productAttributeGroups = unit.Scope.Repository<ProductAttributeGroupMetaData>().GetAll(g => g.VendorID == VendorID).ToList();
      var repoAssortment = unit.Scope.Repository<VendorAssortment>().Include(c => c.VendorPrices);
      var stockRepo = unit.Scope.Repository<VendorStock>();
      var repoAttributeGroup = unit.Scope.Repository<ProductAttributeGroupMetaData>();
      var repoAttributeGroupName = unit.Scope.Repository<ProductAttributeGroupName>();
      var repoAttribute = unit.Scope.Repository<ProductAttributeMetaData>().Include(c => c.ProductAttributeNames, c => c.ProductAttributeValues);
      var repoVendor = unit.Scope.Repository<ProductGroupVendor>();
      var prodDescriptionRepo = unit.Scope.Repository<ProductDescription>();

      var products = prodRepo.GetAll(x => x.SourceVendorID == VendorID).ToList();
      var vendorassortments = repoAssortment.GetAll(x => x.VendorID == VendorID).ToList();
      var productAttributes = repoAttribute.GetAll(g => g.VendorID == VendorID).ToList();
      var currentProductGroupVendors = repoVendor.GetAll(v => v.Vendor.VendorID == VendorID).ToList();
      var productGroupVendorRecords = repoVendor.GetAll(pc => pc.VendorID == VendorID).ToList();
      var brands = repoBrandVendor.GetAll(bv => bv.VendorID == VendorID).ToList();
      
      var vendorStock = stockRepo.GetAll(x => x.VendorID == VendorID).ToList();
      var prodDescriptions = prodDescriptionRepo.GetAll().ToList();
    
      var prodAttributeGroups = repoAttributeGroup.GetAll(g => g.VendorID == VendorID).ToList();

      ProductStatusVendorMapper mapper = new ProductStatusVendorMapper(unit.Scope.Repository<VendorProductStatus>(), VendorID);

      int counter = 0;
      int total = itemProducts.Count();
      int totalNumberOfProductsToProcess = total;
      log.InfoFormat("Start import {0} products", total);

      foreach (var product in itemProducts)
      {
        try
        {
          if (counter == 100)
          {
            counter = 0;
            log.InfoFormat("Still need to process {0} of {1}; {2} done;", totalNumberOfProductsToProcess, total, total - totalNumberOfProductsToProcess);
          }

          totalNumberOfProductsToProcess--;
          counter++;

          #region Brand

          var vbrand = brands.FirstOrDefault(vb => vb.VendorBrandCode == product.VendorProductGroupCode1.Code);

          if (vbrand == null)
          {
            vbrand = new BrandVendor
            {
              VendorID = VendorID,
              VendorBrandCode = product.VendorProductGroupCode1.Code,
              BrandID = UnMappedID
            };

            brands.Add(vbrand);

            repoBrandVendor.Add(vbrand);
          }

          vbrand.Name = product.VendorName;

          #endregion Brand

          #region Product

          var item = products.FirstOrDefault(p => p.VendorItemNumber == product.VendorItemNr && p.BrandID == vbrand.BrandID);
          if (item == null)
          {
            item = new Product
            {
              VendorItemNumber = product.VendorItemNr,
              BrandID = vbrand.BrandID,
              SourceVendorID = VendorID
            };
            prodRepo.Add(item);
            products.Add(item);
            unit.Save();
          }
          else
          {
            if (item.BrandID < 0)
              item.BrandID = vbrand.BrandID;
          }

          #endregion Product

          #region Vendor assortment


          //if (item.VendorAssortments == null) item.VendorAssortments = new List<VendorAssortment>();

          var assortment = vendorassortments.FirstOrDefault(x => x.ProductID == item.ProductID);
          if (assortment == null)
          {
            assortment = new VendorAssortment
            {
              VendorID = VendorID,
              Product = item
            };
            vendorassortments.Add(assortment);
            repoAssortment.Add(assortment);

          }

          assortment.ShortDescription = product.Longdescription.Length > 150 ? product.Longdescription.Substring(0, 150) : product.Longdescription;
          assortment.CustomItemNumber = product.CustomItemNr;
          assortment.IsActive = true;
          assortment.LongDescription = product.Longdescription;


          #region Product group
          try
          {
            var brndCode = product.Try(x => x.VendorProductGroupCode1, null);

            if (brndCode != null)
            {
              var productGroupVendor = productGroupVendorRecords.Where(pg => pg.GetType().GetProperty("BrandCode").GetValue(pg, null) != null
                    && pg.GetType().GetProperty("BrandCode").GetValue(pg, null).ToString() == brndCode.Code.Trim()).FirstOrDefault();

              if (productGroupVendor == null)
              {
                productGroupVendor = new ProductGroupVendor
                {
                  ProductGroupID = UnMappedID,
                  VendorID = VendorID,
                  VendorName = brndCode.Name.Trim()
                };

                productGroupVendor.GetType().GetProperty("BrandCode").SetValue(productGroupVendor, brndCode.Code.Trim(), null);

                repoVendor.Add(productGroupVendor);
                productGroupVendorRecords.Add(productGroupVendor);
              }

              #region sync
              if (currentProductGroupVendors.Contains(productGroupVendor))
              {
                currentProductGroupVendors.Remove(productGroupVendor);
              }
              #endregion

            }
            for (int i = 2; i <= 4; i++)
            {

              var category = i == 2 ? product.Try(x => x.VendorProductGroupCode2, null) : i == 3 ? product.Try(x => x.VendorProductGroupCode3, null) : i == 4 ? product.Try(x => x.VendorProductGroupCode4, null) : null;

              if (category != null)
              {
                var productGroupVendor = productGroupVendorRecords.Where(pg => pg.GetType().GetProperty("VendorProductGroupCode" + i).GetValue(pg, null) != null
                  && pg.GetType().GetProperty("VendorProductGroupCode" + i).GetValue(pg, null).ToString() == category.Code.Trim()).FirstOrDefault();

                if (productGroupVendor == null)
                {
                  productGroupVendor = new ProductGroupVendor
                  {
                    ProductGroupID = UnMappedID,
                    VendorID = VendorID,
                    VendorName = category.Name.Trim()
                  };

                  productGroupVendor.GetType().GetProperty("VendorProductGroupCode" + i).SetValue(productGroupVendor, category.Code.Trim(), null);

                  repoVendor.Add(productGroupVendor);
                  productGroupVendorRecords.Add(productGroupVendor);

                }

                #region sync
                if (currentProductGroupVendors.Contains(productGroupVendor))
                {
                  currentProductGroupVendors.Remove(productGroupVendor);
                }
                #endregion
              }
            }
          }
          catch (Exception ex)
          {
            log.Error("Failed productgroups", ex);
          }
          #endregion

          #region Vendor Product Group Assortment

          string brandCode = product.VendorProductGroupCode1.Code;
          /// string groupCode1 = product.VendorProductGroupCode1.Code;
          string groupCode1 = product.VendorProductGroupCode2.Code;
          string groupCode2 = product.VendorProductGroupCode3.Code;
          string groupCode3 = product.VendorProductGroupCode4.Code;

          var records = (from l in productGroupVendorRecords
                         where
                           ((brandCode != null && l.BrandCode.Trim() == brandCode) || l.BrandCode == null)
                           &&
                           ((groupCode1 != null && l.VendorProductGroupCode1 != null &&
                             l.VendorProductGroupCode1.Trim() == groupCode1) || l.VendorProductGroupCode1 == null)
                           &&
                           ((groupCode2 != null && l.VendorProductGroupCode2 != null &&
                             l.VendorProductGroupCode2.Trim() == groupCode2) || l.VendorProductGroupCode2 == null)
                           &&
                           ((groupCode3 != null && l.VendorProductGroupCode3 != null &&
                             l.VendorProductGroupCode3.Trim() == groupCode3) || l.VendorProductGroupCode3 == null)
                         select l).ToList();


          List<int> existingProductGroupVendors = new List<int>();

          foreach (ProductGroupVendor prodGroupVendor in records)
          {
            existingProductGroupVendors.Add(prodGroupVendor.ProductGroupVendorID);

            if (prodGroupVendor.VendorAssortments == null)
            {
              prodGroupVendor.VendorAssortments = new List<VendorAssortment>();
            }
            if (prodGroupVendor.VendorAssortments.Any(x => x.VendorAssortmentID == assortment.VendorAssortmentID))
            { // only add new rows
              continue;
            }
            prodGroupVendor.VendorAssortments.Add(assortment);
          }

          var unusedPGV = new List<ProductGroupVendor>();
          assortment.ProductGroupVendors.ForEach((pgv, id) =>
          {
            if (!existingProductGroupVendors.Contains(pgv.ProductGroupVendorID))
            {
              unusedPGV.Add(pgv);
            }
          });

          unusedPGV.ForEach((pg) => { assortment.ProductGroupVendors.Remove(pg); });

          #endregion

          #endregion Vendor assortment

          #region Stock

          var stock = vendorStock.FirstOrDefault(c => c.ProductID == assortment.ProductID && c.VendorID == assortment.VendorID);

          if (stock == null)
          {
            stock = new VendorStock
            {
              VendorID = VendorID,
              Product = item,
              VendorStockTypeID = 1
            };
            vendorStock.Add(stock);
            stockRepo.Add(stock);
          }
          stock.QuantityOnHand = Int32.Parse(product.QuantityOnHand);
          stock.StockStatus = product.Status == null ? null : product.Status.Name;
          if (product.Status != null) stock.ConcentratorStatusID = mapper.SyncVendorStatus(product.Status.Name, -1);

          #endregion Stock

          #region Price

          if (assortment.VendorPrices == null) assortment.VendorPrices = new List<VendorPrice>();
          var price = assortment.VendorPrices.FirstOrDefault();
          if (price == null)
          {
            price = new VendorPrice
            {
              VendorAssortment = assortment,
              MinimumQuantity = 0
            };

            unit.Scope.Repository<VendorPrice>().Add(price);
          }
          price.CommercialStatus = product.Status == null ? null : product.Status.Name;
          price.BasePrice = Decimal.Parse(product.Price);
          if (product.Tax != null) price.TaxRate = Decimal.Parse(product.Tax.Tax) / Decimal.Parse(product.Price);
          price.ConcentratorStatusID = stock.ConcentratorStatusID;

          #endregion Price

          #region Barcode

          if (!String.IsNullOrEmpty(product.Barcode))
          {
            if (item.ProductBarcodes == null) item.ProductBarcodes = new List<ProductBarcode>();
            if (!item.ProductBarcodes.Any(pb => pb.Barcode.Trim() == product.Barcode.Trim()))
            {
              unit.Scope.Repository<ProductBarcode>().Add(new ProductBarcode
               {
                 Product = item,
                 Barcode = product.Barcode.Trim(),
                 BarcodeType = (int)BarcodeTypes.Default,
                 VendorID = VendorID
               });
            }
          }

          #endregion Barcode
          unit.Save();
        }
        catch (Exception ex)
        {
          log.Error("Error import products for Copaco", ex);
        }
      }
      #region delete unused productgroupvendor records
      foreach (var vProdGroup in currentProductGroupVendors)
      {
        if (vProdGroup.ProductGroupID == -1)
        {
          unit.Scope.Repository<ProductGroupVendor>().Delete(vProdGroup);
        }
      }
      #endregion
    }

    private object createProductGroup(EnumerableRowCollection<DataRow> cont, string p)
    {
      var productGroup = (from content in cont
                          where content[0].ToString().StartsWith(p)
                          select new
                          {
                            Code = content[0].ToString(),
                            Name = content[2].ToString()
                          }).ToList();

      return productGroup;
    }

  }
}
