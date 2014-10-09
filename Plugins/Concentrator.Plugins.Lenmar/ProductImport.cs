using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Vendors;
using System.Configuration;
using Concentrator.Objects.Ftp;
using System.Xml.Linq;
using System.Xml;
using Concentrator.Objects.Models.Attributes;
using Concentrator.Objects.Vendors.Bulk;
using System.Globalization;

namespace Concentrator.Plugins.Lenmar
{
  class ProductImport : VendorBase
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
      get { return int.Parse(Config.AppSettings.Settings["VendorID"].Value); }
    }

    private static List<string> NotAttributes = new List<string> { "Description", "Category" };

    List<ProductAttributeMetaData> attributes;

    public Dictionary<string, int> attributeList = new Dictionary<string, int>();

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
        }

        foreach (var file in contentDownloader)
        {
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
              log.AuditError(String.Format("Failed to load xml for file: {0}", file.FileName), ex);
              contentDownloader.MarkAsError(file.FileName);
              continue;
            }
          }
        }

        if (true)
        {
          try
          {
            using (var unit = GetUnitOfWork())
            {
              ParseDocuments(unit, products, content);
            }
          }
          catch (Exception ex)
          {
            log.AuditError("Error import products", ex);
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
      }
      catch (Exception ex)
      {
        log.AuditError("Error get lenmar files from ftp", ex);
      }
    }

    private void ParseDocuments(Objects.DataAccess.UnitOfWork.IUnitOfWork unit, XDocument[] products, XDocument cont)
    {
      #region Xml Data

      //products = new XDocument[1];
      //products[0] = XDocument.Load(@"C:\Lenmar\test.xml");

      log.AuditInfo("Start parsing items");

      var itemContent = (from content in cont.Root.Elements("item")
                         let atts = content.Elements().Where(x => AttributeMapping.Contains(x.Name.LocalName))
                         where content.Element("LenmarSKU") != null && content.Element("Description") != null && content.Element("Manufacturer") != null
                         select new
                         {
                           LenmarSKU = content.Element("LenmarSKU").Value,
                           ShortContentDescription = content.Element("Description").Value,
                           GroupCode = content.Element("Manufacturer").Value,
                           CostPrice = content.Element("BC_euro_cost") != null ? content.Element("BC_euro_cost").Value : "0",
                           dynamic = atts
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

      log.AuditInfo("Finished parsing items");
      #endregion


      SetupAttributes(unit, AttributeMapping.ToArray(), out attributes, VendorID);

      //Used for VendorImportAttributeValues
      var productAttributes = unit.Scope.Repository<ProductAttributeMetaData>().GetAll(c => c.VendorID == VendorID).ToList();
      attributeList = productAttributes.ToDictionary(x => x.AttributeCode, y => y.AttributeID);

      List<Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorAssortmentItem> assortmentList = new List<Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorAssortmentItem>();

      int counter = 0;
      int total = itemProducts.Count();
      int totalNumberOfProductsToProcess = total;
      log.InfoFormat("Start processing {0} products", total);


      foreach (var product in itemProducts)
      {
        if (counter == 50)
        {
          counter = 0;
          log.InfoFormat("Still need to process {0} of {1}; {2} done;", totalNumberOfProductsToProcess, total, total - totalNumberOfProductsToProcess);
        }
        totalNumberOfProductsToProcess--;
        counter++;

        var assortment = new Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorAssortmentItem
        {
          #region BrandVendor
          BrandVendors = new List<VendorAssortmentBulk.VendorImportBrand>()
                        {
                            new VendorAssortmentBulk.VendorImportBrand()
                            {
                                VendorID = VendorID,
                                VendorBrandCode = product.VendorBrandCode.Trim(),
                                ParentBrandCode = null,
                                Name = product.VendorBrandCode.Trim() 
                            }
                        },
          #endregion

          #region GeneralProductInfo
          VendorProduct = new VendorAssortmentBulk.VendorProduct
          {
            VendorItemNumber = product.SupplierSKU.Trim(), //EAN
            CustomItemNumber = product.CustomItemNr.Trim(), //EAN
            ShortDescription = product.ShortDescription.Length > 150
                                     ? product.ShortDescription.Substring(0, 150)
                                     : product.ShortDescription,
            LongDescription = "",
            LineType = null,
            LedgerClass = null,
            ProductDesk = null,
            ExtendedCatalog = null,
            VendorID = VendorID,
            DefaultVendorID = DefaultVendorID,
            VendorBrandCode = product.VendorBrandCode.Trim(), //UITGEVER_ID
            Barcode = product.Try(x => x.Barcode, string.Empty),//EAN
            VendorProductGroupCode1 = string.IsNullOrEmpty(product.VendorProductGroupCode1) ? "Battery" : product.VendorProductGroupCode1,
            VendorProductGroupCodeName1 = product.VendorName,
            VendorProductGroupCode2 = !string.IsNullOrEmpty(product.VendorProductGroupCode2) ? product.VendorProductGroupCode2 : null,
            VendorProductGroupCodeName2 = product.VendorName,
            VendorProductGroupCode3 = !string.IsNullOrEmpty(product.VendorProductGroupCode3) ? product.VendorProductGroupCode3 : null,
            VendorProductGroupCodeName3 = product.VendorName,
            VendorProductGroupCode4 = !string.IsNullOrEmpty(product.VendorProductGroupCode4) ? product.VendorProductGroupCode4 : null,
            VendorProductGroupCodeName4 = product.VendorName,
            VendorProductGroupCode5 = !string.IsNullOrEmpty(product.VendorBrandCode) ? product.VendorBrandCode : null,
            VendorProductGroupCodeName5 = product.VendorName
          },
          #endregion

          #region RelatedProducts
          RelatedProducts = new List<VendorAssortmentBulk.VendorImportRelatedProduct>() { },
          #endregion

          #region Attributes

          VendorImportAttributeValues = (from content in itemContent.Where(x => x.LenmarSKU == product.SupplierSKU)
                                         from att in content.dynamic
                                         let attributeID = attributeList.ContainsKey(att.Name.ToString()) ? attributeList[att.Name.ToString()] : -1
                                         let value = att.Value.ToString()
                                         where !string.IsNullOrEmpty(value)
                                         select new Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportAttributeValue
                                         {
                                           VendorID = VendorID,
                                           DefaultVendorID = DefaultVendorID,
                                           CustomItemNumber = product.CustomItemNr.Trim(), //EAN
                                           AttributeID = attributeID,
                                           Value = value,
                                           LanguageID = "1",
                                           AttributeCode = att.Name.ToString(),
                                         }).ToList(),

          #endregion

          #region Prices
          VendorImportPrices = new List<Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportPrice>()
                        {
                            new Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportPrice()
                            {
                                VendorID = VendorID,
                                DefaultVendorID = DefaultVendorID,
                                CustomItemNumber = product.CustomItemNr.Trim(), //EAN
                                Price = (Decimal.Parse(product.Price) / 119).ToString("0.00", CultureInfo.InvariantCulture) ,
                                CostPrice =  (Decimal.Parse(product.CostPrice) / 100).ToString("0.00", CultureInfo.InvariantCulture), //NETTOPRIJS
                                TaxRate = "19",
                                MinimumQuantity = 0,
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
                                CustomItemNumber = product.CustomItemNr.Trim(), //EAN
                                QuantityOnHand = int.Parse(product.QuantityOnHand),
                                StockType = "Assortment",
                                StockStatus = product.Status
                            }
                        },
          #endregion

          #region Descriptions
          VendorProductDescriptions = new List<VendorAssortmentBulk.VendorProductDescription>()
            {
               new VendorAssortmentBulk.VendorProductDescription(){
                VendorID = VendorID,
                LanguageID =languageID,
                DefaultVendorID = VendorID,
                CustomItemNumber = product.CustomItemNr.Trim(), //EAN
                LongContentDescription = string.Empty,
                ShortContentDescription = product.ShortContentDescription != string.Empty ? product.ShortContentDescription.Length > 1000
                                     ?product.ShortContentDescription.Substring(0, 1000)
                                      : product.ShortContentDescription : product.ShortDescription,
                ProductName = product.ShortDescription
                            }
            }
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
}