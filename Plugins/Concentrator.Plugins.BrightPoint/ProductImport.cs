using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Vendors;
using System.Configuration;
using Concentrator.Objects.Models.Attributes;
using System.IO;
using System.Xml.Serialization;
using Concentrator.Plugins.BrightPoint.BrightPointService;
using Concentrator.Objects.Vendors.Bulk;
using System.Diagnostics;
using System.Globalization;
using Concentrator.Objects.ConcentratorService;

namespace Concentrator.Plugins.BrightPoint
{
  class ProductImport : VendorBase
  {

    public override string Name
    {
      get { return "Brightpoint product import"; }
    }

    protected override int VendorID
    {
      get { return Int32.Parse(Config.AppSettings.Settings["vendorID"].Value); }
    }

    protected override int DefaultVendorID
    {
      get { return Int32.Parse(Config.AppSettings.Settings["vendorID"].Value); }
    }

    protected override Configuration Config
    {
      get { return GetConfiguration(); }
    }

    List<string> AttributeMapping = new List<string>();

    List<ProductAttributeMetaData> outList = new List<ProductAttributeMetaData>();


    protected override void SyncProducts()
    {
      var CustNo = Config.AppSettings.Settings["BrightPointCustomerNo"].Value;
      var Pass = Config.AppSettings.Settings["BrightPointPassword"].Value;
      var Instance = Config.AppSettings.Settings["BrightPointInstance"].Value;
      var Site = Config.AppSettings.Settings["BrightPointSite"].Value;
      var WorkingDirectory = Config.AppSettings.Settings["WorkingDirectory"].Value;
      int VendorID = Int32.Parse(Config.AppSettings.Settings["vendorID"].Value);
      int DefaultVendorID = Int32.Parse(Config.AppSettings.Settings["vendorID"].Value);

      if (!Directory.Exists(WorkingDirectory))
        Directory.CreateDirectory(WorkingDirectory);

      BrightPointService.AuthHeaderUser authHeader = new BrightPointService.AuthHeaderUser();

      authHeader.sCustomerNo = CustNo;
      authHeader.sInstance = Instance;
      authHeader.sPassword = Pass;
      authHeader.sSite = Site;

      #region Get Data

      BrightPointService.Category[] brandsList = null;
      BrightPointService.Category[] categories_LOB = null;
      BrightPointService.Category[] categories_Types = null;
      //BrightPointService.Language[] languages = null;
      //BrightPointService.InventoryInStock[] inventory;
      //BrightPointService.SalesPart_ProjectStockItems[] pstock;
      BrightPointService.SalesPart[] catalog = null;

      using (BrightPointService.PartcatalogSoapClient client = new BrightPointService.PartcatalogSoapClient("Part catalogSoap"))
      {
        try
        {
          #region salesPart
          var salesFile = Path.Combine(WorkingDirectory, "SalesPart.xml");


          if (!File.Exists(salesFile) || new FileInfo(salesFile).LastWriteTime.AddHours(1) < DateTime.Now)
          {
            catalog = client.getSalesPartCatalog(authHeader);
            XmlSerializer seri = new XmlSerializer(typeof(SalesPart[]));
            using (TextWriter writer = new StreamWriter(salesFile))
            {
              seri.Serialize(writer, catalog);
            }
          }

          //catalog from file
          XmlSerializer ser = new XmlSerializer(typeof(SalesPart[]));
          using (StreamReader reader = new StreamReader(salesFile))
          {
            catalog = (SalesPart[])ser.Deserialize(reader);
          }
          #endregion

          #region Brandlist
          var brandFile = Path.Combine(WorkingDirectory, "Brand.xml");

          if (!File.Exists(brandFile) || new FileInfo(brandFile).LastWriteTime.AddHours(1) < DateTime.Now)
          {

            brandsList = client.getCategory_Brand(authHeader);
            XmlSerializer serBrand = new XmlSerializer(typeof(Category[]));
            using (TextWriter writer = new StreamWriter(brandFile))
            {
              serBrand.Serialize(writer, brandsList);
            }
          }
          //brandslist from file
          XmlSerializer ser2 = new XmlSerializer(typeof(Category[]));
          using (StreamReader reader = new StreamReader(brandFile))
          {
            brandsList = (Category[])ser2.Deserialize(reader);
          }
          #endregion

          #region Catalog
          var categoryFile = Path.Combine(WorkingDirectory, "Category.xml");

          if (!File.Exists(categoryFile) || new FileInfo(categoryFile).LastWriteTime.AddHours(1) < DateTime.Now)
          {
            categories_LOB = client.getCategory_LineOfBusiness(authHeader, "NED");
            XmlSerializer serCat = new XmlSerializer(typeof(Category[]));
            using (TextWriter writer = new StreamWriter(categoryFile))
            {
              serCat.Serialize(writer, categories_LOB);
            }
          }
          //LOB list from file
          XmlSerializer ser3 = new XmlSerializer(typeof(Category[]));
          using (StreamReader reader = new StreamReader(categoryFile))
          {
            categories_LOB = (Category[])ser3.Deserialize(reader);
          }
          #endregion

          #region Catalog_types
          var categoryFileType = Path.Combine(WorkingDirectory, "Category_types.xml");

          if (!File.Exists(categoryFileType) || new FileInfo(categoryFileType).LastWriteTime.AddHours(1) < DateTime.Now)
          {
            //Types list from file
            categories_Types = client.getCategory_Types(authHeader, "NED");
            XmlSerializer serCatType = new XmlSerializer(typeof(Category[]));
            using (TextWriter writer = new StreamWriter(categoryFileType))
            {
              serCatType.Serialize(writer, categories_Types);
            }
          }

          XmlSerializer ser4 = new XmlSerializer(typeof(Category[]));
          using (StreamReader reader = new StreamReader(categoryFileType))
          {
            categories_Types = (Category[])ser4.Deserialize(reader);
          }
          #endregion

          //    XmlSerializer ser = new XmlSerializer(typeof(SalesPart[]));
          //    //using (TextWriter writer = new StreamWriter(@"C:\test.xml"))
          //    //{
          //    //    ser.Serialize(writer, a);
          //    //}
          //    using (StreamReader reader = new StreamReader(@"C:\test.xml"))
          //    {
          //        catalog = (SalesPart[])ser.Deserialize(reader);
          //    }
        }
        catch (Exception ex)
        {
          log.AuditError("Error Brightpoint", ex);
        }

      #endregion
        using (var unit = GetUnitOfWork())
        {


          List<ProductAttributeMetaData> attributes;
          //SetupAttributes(unit, AttributeMapping, out attributes, null);

          //Used for VendorImportAttributeValues
          var productAttributes = unit.Scope.Repository<ProductAttributeMetaData>().GetAll(c => c.VendorID == DefaultVendorID).ToList();
          var attributeList = productAttributes.ToDictionary(x => x.AttributeCode, y => y.AttributeID);


          List<VendorAssortmentBulk.VendorAssortmentItem> assortmentList = new List<Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorAssortmentItem>();
          // Create new stopwatch
          Stopwatch stopwatch = new Stopwatch();
          // Loops through all the rowss
          foreach (var product in catalog)
          {
            BrightPointService.SalesPart_Attributes[] atts = null;

            //get product attributes
            try
            {

              if (stopwatch.Elapsed.Seconds > 0)
              {
                while (stopwatch.Elapsed.Seconds < 15)
                {
                  log.Info("Wainting for download");
                }
 stopwatch.Reset();
              }
             
              atts = client.getSalesPartCatalog_Attributes(authHeader, product.SalesPartID, "NED");
              // Begin timing
              stopwatch.Start();
            }
            catch (Exception ex)
            {
              log.AuditWarning("Error while retreiving product attributes");
            }

            if (atts == null)
            {

              atts = new SalesPart_Attributes[1];
            }
            // Line is a row of data seperated by semicolons. The values will be split and added to the column array
            var br = product.CatalogGroup.Substring(0, 2);
            var lob = product.CatalogGroup.Substring(2, 2);
            var type = product.CatalogGroup.Substring(4, 2);

            var assortment = new Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorAssortmentItem
              {
                #region BrandVendor
                BrandVendors = new List<VendorAssortmentBulk.VendorImportBrand>()
                        {
                            new VendorAssortmentBulk.VendorImportBrand()
                            {
                                VendorID = DefaultVendorID,
                                VendorBrandCode = brandsList.FirstOrDefault(x => x.Code == br) != null ? brandsList.FirstOrDefault(x => x.Code == br).Code.Trim() : br.Trim(), //UITGEVER_ID
                                ParentBrandCode = null,
                                Name = brandsList.FirstOrDefault(x => x.Code == br) != null ? brandsList.First(x => x.Code == br).Value.Trim() : br.Trim() //UITGEVER_NM
                            }
                        },
                #endregion

                #region GeneralProductInfo
                VendorProduct = new VendorAssortmentBulk.VendorProduct
                {
                  VendorItemNumber = product.InventoryManufacturerPartNo.ToString().Trim(), //EAN
                  CustomItemNumber = product.SalesPartID.ToString(), //EAN
                  ShortDescription = product.Description.Length > 150 ? product.Description.Substring(0, 150).Trim() : product.Description.Trim(), //Subtitel
                  LongDescription = product.Description.Trim(),
                  LineType = null,
                  LedgerClass = null,
                  ProductDesk = null,
                  ExtendedCatalog = null,
                  VendorID = VendorID,
                  DefaultVendorID = DefaultVendorID,
                  VendorBrandCode = brandsList.FirstOrDefault(x => x.Code == br) != null ? brandsList.FirstOrDefault(x => x.Code == br).Code.Trim() : br.Trim(), //UITGEVER_ID
                  Barcode = product.EANCode.Trim(), //EAN
                  VendorProductGroupCode1 = lob.Trim(), //REEKS_NR
                  VendorProductGroupCodeName1 = categories_LOB.First(x => x.Code == lob) != null ? categories_LOB.First(x => x.Code == lob).Value.Trim() : null, //REEKS_NM
                  VendorProductGroupCode2 = type.Trim(),//BOEKSOORT_KD
                  VendorProductGroupCodeName2 = categories_Types.First(x => x.Code == type) != null ? categories_Types.First(x => x.Code == type).Value.Trim() : null //GEEN NAAM                        
                },
                #endregion

                #region RelatedProducts
                RelatedProducts = new List<VendorAssortmentBulk.VendorImportRelatedProduct>()
                {
                  //new Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportRelatedProduct
                  //{
                  //    VendorID = VendorID,
                  //    DefaultVendorID = DefaultVendorID,
                  //    CustomItemNumber = column[0].Trim(), //EAN
                  //    RelatedProductType = column[19].Trim(), //ISBN_FYSIEK_BOEK
                  //    RelatedCustomItemNumber = column[19].Trim(), //ISBN_FYSIEK_BOEK
                  //}
                },
                #endregion

                #region Attributes
                VendorImportAttributeValues = (from attr in atts
                                               let attributeID = GetAttributeID(attr.AttributeName, unit)//d.Field<object>(attr)                                                       
                                               //let attributeID = attributeList.ContainsKey(attr.AttributeName) ? attributeList[attr] : 2//-1
                                               //let value = prop.ToString()
                                               where !string.IsNullOrEmpty(attr.AttributeName)
                                               select new Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportAttributeValue
                                               {
                                                 VendorID = VendorID,
                                                 DefaultVendorID = DefaultVendorID,
                                                 CustomItemNumber = product.SalesPartID.ToString(), //EAN
                                                 AttributeID = attributeID,
                                                 Value = attr.Value,
                                                 LanguageID = "1",
                                                 AttributeCode = attr.AttributeName,
                                               }).ToList(),
                #endregion

                #region Prices
                VendorImportPrices = new List<Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportPrice>()
                        {
                            new Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportPrice()
                            {
                                VendorID = VendorID,
                                DefaultVendorID = DefaultVendorID,
                                CustomItemNumber = product.SalesPartID.ToString().Trim(), //EAN
                                Price = product.UnitPrice.ToString("0.00", CultureInfo.InvariantCulture), //ADVIESPRIJS
                                CostPrice = product.UnitCostPrice.ToString("0.00", CultureInfo.InvariantCulture), //NETTOPRIJS
                                TaxRate = "19", //TODO: Calculate this!
                                MinimumQuantity = 0,
                                CommercialStatus = String.IsNullOrEmpty(product.Flag.ToString()) ? null : product.Flag.ToString().Trim()//STADIUM_LEVENSCYCLUS_KD
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
                                CustomItemNumber = product.SalesPartID.ToString().Trim(), //EAN
                                QuantityOnHand = 0,
                                StockType = "Assortment",
                                StockStatus =  Enum.GetName(typeof(BrightPointStockStatus), product.Flag)//STADIUM_LEVENSCYCLUS_KD
                            }
                        }
                //VendorImportImages = new List<Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportImage>()
                //{
                //}
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

          bool success = true;

        }
      }
    }

    private int GetAttributeID(string name, Objects.DataAccess.UnitOfWork.IUnitOfWork unit)
    {
      if (AttributeMapping.Contains(name))
      {
        return outList.FirstOrDefault(x => x.AttributeCode == name).AttributeID;
      }
      else
      {
        AttributeMapping.Add(name);
        SetupAttributes(unit, AttributeMapping.ToArray(), out outList, VendorID);
        return outList.FirstOrDefault(x => x.AttributeCode == name).AttributeID;
      }
    }



    private const int UnMappedID = -1;

    public enum BrightPointStockStatus
    {
      Preorder = -10,
      Active = 10,
      ActiveNotForSale = 20,
      Outgoing = 30,
      OutgoingNotForSale = 35
    }

    public enum BrightPointLanguages
    {
      //Danish = "DAN",
      //English = "ENG",
      //Estonian = "EST"
    }

  }
}
