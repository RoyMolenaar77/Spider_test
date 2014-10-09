using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Vendors;
using Concentrator.Objects.Vendors.Bulk;
using Concentrator.Objects.Ftp;
using System.Xml;
using System.Xml.Linq;
using System.Data;
using System.IO;
using System.Globalization;
using System.Configuration;

namespace Concentrator.Plugins.Copaco
{
  class ProductImport : VendorBase
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

    public override string Name
    {
      get { return "Copaco product import"; }
    }
    protected override void SyncProducts()
    {
      var config = GetConfiguration();
      XDocument products = new XDocument();
      DataTable content = new DataTable();
      DataTable statuses = new DataTable();
      DataTable taxTable = new DataTable();

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

        log.AuditInfo("Getting data..");

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
          log.AuditInfo("Finished getting data");
        }
        catch (Exception ex)
        {
          log.AuditFatal("Failed to download catalog file", ex);
          return;
        }
      }
      catch (Exception ex)
      {
        log.AuditError("Error get copaco files from ftp", ex);
      }

      int count = 0;
      try
      {
        using (var unit = GetUnitOfWork())
        {
          if (products != null && content != null && statuses != null)
          {
            #region Parse Data

            log.AuditInfo("Parse data");

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

            var itemContent = (from cont in content.AsEnumerable()
                               select new
                              {
                                Code = cont[0].ToString(),
                                Name = cont[2].ToString(),
                                Level = cont[1].ToString()
                              }).ToList();

            var itemProducts = (from itemproduct in products.Element("XMLOUT_pricelist_01").Elements("item")
                                let cont = itemContent
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
                                  VendorProductGroupCode1 = cont.FirstOrDefault(x => x.Code.StartsWith(itemproduct.Element("item_group").Value.Substring(0, 2)) && Int32.Parse(x.Level) == 1),
                                  VendorProductGroupCode2 = cont.FirstOrDefault(x => x.Code.StartsWith(itemproduct.Element("item_group").Value.Substring(0, 2)) && Int32.Parse(x.Level) == 2),
                                  VendorProductGroupCode3 = cont.FirstOrDefault(x => x.Code.StartsWith(itemproduct.Element("item_group").Value.Substring(0, 2)) && Int32.Parse(x.Level) == 3),
                                  VendorProductGroupCode4 = cont.FirstOrDefault(x => x.Code.StartsWith(itemproduct.Element("item_group").Value.Substring(0, 2)) && Int32.Parse(x.Level) == 4),
                                  VendorProductGroupCode5 = cont.FirstOrDefault(x => x.Code.StartsWith(itemproduct.Element("item_group").Value.Substring(0, 2)) && Int32.Parse(x.Level) == 5),
                                  Barcode = itemproduct.Element("EAN_code").Value,
                                  ShortContentDescription = ""
                                }).ToList();

            log.AuditInfo("Finished parsing data");
            #endregion


            List<Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorAssortmentItem> assortmentList = new List<Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorAssortmentItem>();


            // Loops through all the rowss
            foreach (var product in itemProducts)
            {
              var assortment = new Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorAssortmentItem
              {
                #region BrandVendor
                BrandVendors = new List<VendorAssortmentBulk.VendorImportBrand>()
                           {
                            new VendorAssortmentBulk.VendorImportBrand()
                            {
                                VendorID = DefaultVendorID,
                                VendorBrandCode = product.VendorProductGroupCode1 != null ? product.VendorProductGroupCode1.Code.Trim(): "Unknown" , //UITGEVER_ID
                                ParentBrandCode = null,
                                Name = product.VendorName != null ? product.VendorName.Trim() : "Unknown"//UITGEVER_NM
                            }
                        },
                #endregion

                #region GeneralProductInfo
                VendorProduct = new VendorAssortmentBulk.VendorProduct
                {
                  VendorItemNumber = product.VendorItemNr != null ? product.VendorItemNr.Trim() : "Unknown", //EAN
                  CustomItemNumber = product.CustomItemNr != null ? product.CustomItemNr.Trim() : "Unknown", //EAN
                  ShortDescription = product.Longdescription != null ? product.Longdescription.Length > 150 ? product.Longdescription.Substring(0, 150) : product.Longdescription : string.Empty, //Subtitel
                  LongDescription = product.Longdescription != null ? product.Longdescription.Trim() : string.Empty,
                  LineType = null,
                  LedgerClass = null,
                  ProductDesk = null,
                  ExtendedCatalog = null,
                  VendorID = VendorID,
                  DefaultVendorID = DefaultVendorID,
                  VendorBrandCode = product.VendorProductGroupCode1 != null ? product.VendorProductGroupCode1.Code.Trim() : "Unknown",
                  Barcode = String.IsNullOrEmpty(product.Barcode) ? product.Barcode.Trim() : string.Empty, //EAN
                  VendorProductGroupCode1 = product.VendorProductGroupCode1 != null ? product.VendorProductGroupCode1.Code : string.Empty,
                  VendorProductGroupCodeName1 = product.VendorProductGroupCode1 != null ? product.VendorProductGroupCode1.Name : string.Empty,
                  VendorProductGroupCode2 = product.VendorProductGroupCode2 != null ? product.VendorProductGroupCode2.Code : string.Empty,
                  VendorProductGroupCodeName2 = product.VendorProductGroupCode2 != null ? product.VendorProductGroupCode2.Name : string.Empty,
                  VendorProductGroupCode3 = product.VendorProductGroupCode3 != null ? product.VendorProductGroupCode3.Code : string.Empty,
                  VendorProductGroupCodeName3 = product.VendorProductGroupCode3 != null ? product.VendorProductGroupCode3.Name : string.Empty,
                  VendorProductGroupCode4 = product.VendorProductGroupCode4 != null ? product.VendorProductGroupCode4.Code : string.Empty,
                  VendorProductGroupCodeName4 = product.VendorProductGroupCode4 != null ? product.VendorProductGroupCode4.Name : string.Empty,
                  VendorProductGroupCode5 = product.VendorProductGroupCode5 != null ? product.VendorProductGroupCode5.Code : string.Empty,
                  VendorProductGroupCodeName5 = product.VendorProductGroupCode5 != null ? product.VendorProductGroupCode5.Name : string.Empty
                },
                #endregion

                #region RelatedProducts not needed
                RelatedProducts = new List<VendorAssortmentBulk.VendorImportRelatedProduct>()
        {

        },
                #endregion

                #region Attributes not needed
                VendorImportAttributeValues = new List<VendorAssortmentBulk.VendorImportAttributeValue>()
                {

                },
                #endregion

                #region Prices
                VendorImportPrices = new List<Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportPrice>()
                        {
                            new Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportPrice()
                            {
                                VendorID = VendorID,
                                DefaultVendorID = DefaultVendorID,
                                CustomItemNumber = product.CustomItemNr.Trim(), //EAN
                                Price =  product.Price != null && product.Price != "" ? Decimal.Parse(product.Price).ToString("0.00", CultureInfo.InvariantCulture) : null, //ADVIESPRIJS
                                CostPrice = null, //NETTOPRIJS
                                TaxRate = product.Tax != null  ?  (Decimal.Parse(product.Tax.Tax) / Decimal.Parse(product.Price)).ToString("0.00", CultureInfo.InvariantCulture): null, //TODO: Calculate this!
                                MinimumQuantity = 0,
                                CommercialStatus = product.Status == null ? Int32.Parse(product.QuantityOnHand) > 0 ? "InStock" : "OutOfStock" : product.Status.Name //STADIUM_LEVENSCYCLUS_KD
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
                                QuantityOnHand = product.QuantityOnHand != null ? Int32.Parse(product.QuantityOnHand) : 0,
                                StockType = "Assortment",
                                StockStatus = product.Status == null ? Int32.Parse(product.QuantityOnHand) > 0 ? "InStock" : "OutOfStock" : product.Status.Name//STADIUM_LEVENSCYCLUS_KD
                            }
                        },
                #endregion
              };

              // assortment will be added to the list defined outside of this loop
              assortmentList.Add(assortment);
              count++;
            }
            using (var vendorAssortmentBulk = new VendorAssortmentBulk(assortmentList, VendorID, DefaultVendorID))
            {

              vendorAssortmentBulk.Init(unit.Context);
              vendorAssortmentBulk.Sync(unit.Context);
            }
          }




        }
      }
      catch (Exception ex)
      {
        log.AuditError("Error import products", ex);
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

