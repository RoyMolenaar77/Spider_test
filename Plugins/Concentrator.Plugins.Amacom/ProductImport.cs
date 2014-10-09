using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Vendors;
using Concentrator.Objects.Ftp;
using System.Data;
using Concentrator.Objects.Vendors.Bulk;
using System.Configuration;
using System.Globalization;
using System.IO;
using Concentrator.Objects.ConcentratorService;

namespace Concentrator.Plugins.Amacom
{
  class ProductImport : VendorBase
  {

    private const int unmappedID = -1;

    public override string Name
    {
      get { return "Amacom Product Import"; }
    }

    protected override int VendorID
    {
      get { return Int32.Parse(Config.AppSettings.Settings["VendorID"].Value); }
    }

    protected override int DefaultVendorID
    {
      get { return Int32.Parse(Config.AppSettings.Settings["VendorID"].Value); }
    }

    protected override Configuration Config
    {
      get { return GetConfiguration(); }
    }

    protected override void SyncProducts()
    {
      
      DataTable content = null;

      try
      {
        FtpManager productDownloader = new FtpManager(
          Config.AppSettings.Settings["AmacomUrl"].Value,
          Config.AppSettings.Settings["ProductPath"].Value,
          Config.AppSettings.Settings["Username"].Value,
          Config.AppSettings.Settings["Password"].Value,
         false, true, log);

        var clientID = Config.AppSettings.Settings["ClientID"].Value;

        var filename = clientID + ".csv";

        using (var file = productDownloader.OpenFile(filename))
        {
          content = CreateDataTable(file.Data, false, ';');
        }

        if (content != null)
        {

          log.AuditInfo(string.Format("Start: {0}", DateTime.Now));
          Parseproducts(content);

          log.AuditInfo(string.Format("Done: {0}", DateTime.Now));

        }

      }
      catch (Exception ex)
      {
        log.AuditFatal("Unable to retreive file from ftp");
      }
    }

    private void Parseproducts(DataTable content)
    {
      try
      {
        using (var unit = GetUnitOfWork())
        {
          // VendorID's retrieved from appsettings.config
          var config = GetConfiguration();
         
          var itemProducts = (from line in content.AsEnumerable()
                              select new
                              {
                                CustomItemNr = line[1].ToString().Trim(),
                                ProductName = line[2].ToString().Trim(),
                                SalesPrice = line[12].ToString().Trim(),
                                EAN = line[4].ToString().Trim(),
                                Brand = line[5].ToString().Trim(),
                                ProductGroup = line[6].ToString().Trim(),
                                VendorItemNr = line[7].ToString().Trim(),
                                Stock = line[9].ToString().Trim(),
                                StockIncl = line[10].ToString().Trim(),
                                Date = line[14].ToString().Trim()
                              }).ToList();

          List<Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorAssortmentItem> assortmentList = new List<Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorAssortmentItem>();


          int counter = 0;
          int total = itemProducts.Count();
          int totalNumberOfProductsToProcess = total;
          log.InfoFormat("Start processing {0} products", total);


          // Loops through all the rowss
          foreach (var product in itemProducts)
          {
            DateTime RequestDate;

            DateTime.TryParseExact(product.Date, "yyyyMMdd", null, DateTimeStyles.None, out RequestDate);

            if (RequestDate != null && RequestDate > DateTime.Now) continue;

            var assortment = new Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorAssortmentItem
            {
              #region BrandVendor
              BrandVendors = new List<VendorAssortmentBulk.VendorImportBrand>()
                        {
                            new VendorAssortmentBulk.VendorImportBrand()
                            {
                                VendorID = VendorID,
                                VendorBrandCode = product.Brand.Trim(),
                                ParentBrandCode = null,
                                Name = product.Brand.Trim() 
                            }
                        },
              #endregion

              #region GeneralProductInfo
              VendorProduct = new VendorAssortmentBulk.VendorProduct
              {
                VendorItemNumber = product.VendorItemNr.Trim(), //EAN
                CustomItemNumber = product.CustomItemNr.Trim(), //EAN
                ShortDescription = string.Empty,
                LongDescription = string.Empty,
                LineType = null,
                LedgerClass = null,
                ProductDesk = null,
                ExtendedCatalog = null,
                VendorID = VendorID,
                DefaultVendorID = DefaultVendorID,
                VendorBrandCode = product.Brand.Trim(), //UITGEVER_ID
                Barcode = string.IsNullOrEmpty(product.EAN) ? null : product.EAN.Trim(), //EAN
                VendorProductGroupCode1 = product.Try(x => x.ProductGroup, string.Empty), //REEKS_NR
                VendorProductGroupCodeName1 = product.Try(x => x.ProductGroup, string.Empty), //REEKS_NM

              },
              #endregion

              #region RelatedProducts
              RelatedProducts = new List<VendorAssortmentBulk.VendorImportRelatedProduct>()
                        {
                            new Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportRelatedProduct
                            {
                                VendorID = VendorID,
                                DefaultVendorID = DefaultVendorID,
                                CustomItemNumber = product.CustomItemNr.Trim(), //EAN
                                RelatedProductType = string.Empty,
                                RelatedCustomItemNumber = string.Empty
                            }
                        },
              #endregion

              #region Attributes
              VendorImportAttributeValues = new List<VendorAssortmentBulk.VendorImportAttributeValue>(),
              #endregion

              #region Prices
              VendorImportPrices = new List<Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportPrice>()
                        {
                            new Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportPrice()
                            {
                                VendorID = VendorID,
                                DefaultVendorID = DefaultVendorID,
                                CustomItemNumber = product.CustomItemNr.Trim(), //EAN
                                Price = (Decimal.Parse(product.SalesPrice) / 100).ToString("0.00", CultureInfo.InvariantCulture) ,
                                CostPrice = null, //NETTOPRIJS
                                TaxRate = "19", //TODO: Calculate this!
                                MinimumQuantity = 0,
                                CommercialStatus = product.Stock != null || product.Stock != "" ? Int32.Parse(product.Stock) > 0 ? "InStock" : "OutOfStock" : "OutOfStock"
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
                                CustomItemNumber = product.CustomItemNr.Trim(), //EAN
                                QuantityOnHand = product.Stock != null || product.Stock != "" ? Int32.Parse(product.Stock) : 0,
                                StockType = "Assortment",
                                StockStatus =  product.Stock != null || product.Stock != "" ? Int32.Parse(product.Stock) > 0 ? "InStock" : "OutOfStock" : "OutOfStock"
                            }
                        },
              #endregion
            };

            // assortment will be added to the list defined outside of this loop
            assortmentList.Add(assortment);
          }

          // Creates a new instance of VendorAssortmentBulk(Passes in the AssortmentList defined above, vendorID and DefaultVendorID)
          using (var vendorAssortmentBulk = new VendorAssortmentBulk(assortmentList, VendorID, DefaultVendorID))
          {

            vendorAssortmentBulk.Init(unit.Context);
            vendorAssortmentBulk.Sync(unit.Context);
          }
        }
      }
      catch (Exception ex)
      {

        log.AuditFatal(string.Format("Something went wrong: {0}", ex));
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


  }
}

