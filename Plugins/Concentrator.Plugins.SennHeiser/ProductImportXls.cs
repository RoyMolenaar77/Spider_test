using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Data.Linq;
using System.Linq.Dynamic;
using System.Text;
using Concentrator.Objects;
using Concentrator.Objects.ConcentratorService;
using Excel;
using Concentrator.Objects.Utility;
using System.Web;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using Concentrator.Objects.Enumerations;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.Models.Brands;
using Concentrator.Objects.Models.Attributes;
using Concentrator.Objects.Models.Media;

namespace Concentrator.Plugins.SennHeiser
{
  class ProductImportXls : ConcentratorPlugin
  {
    public override string Name
    {
      get { return "Sennheiser Xls Product Import Plugin"; }
    }
    //private int vendorID_NL;
    private const int unmappedID = -1;
    private const int languageID = 1;
    private Dictionary<int, string> vendors = new Dictionary<int, string>();

    string Category = string.Empty;
    System.Configuration.Configuration config;

    protected override void Process()
    {
      config = GetConfiguration();

      vendors.Add(int.Parse(config.AppSettings.Settings["VendorID_NL"].Value), "NL");
      vendors.Add(int.Parse(config.AppSettings.Settings["VendorID_BE"].Value), "BE");

      var productXlsFilePath = config.AppSettings.Settings["SennheiserBasePath"].Value + "CSN-productlijst.xls";
      var eab_upc_ART_Codes_XlsPath = config.AppSettings.Settings["SennheiserBasePath"].Value + "Nuemann_EAC_UPC_ART_Codes.xls";
      var eab_upc_INHOUD_etc_XlsPath = config.AppSettings.Settings["SennheiserBasePath"].Value + "Sencomm_EAN_UPC_INHOUD_etc.xls";
      var eab_upc_ART_AFMETINGEN_XlsPath = config.AppSettings.Settings["SennheiserBasePath"].Value + "Sennheiser_EAN_UPC_ART_AFMETINGEN.xls";
      var alleen_Voorbeeld_XlsPath = config.AppSettings.Settings["SennheiserBasePath"].Value + "Alleen _voorbeeld.xls";
      var base_prices_XlsPath = config.AppSettings.Settings["SennheiserBasePath"].Value + "Base prices in BE001 NL001 ex JDE v20101105.xlsx";
      var werkbestand_XlsPath = config.AppSettings.Settings["SennheiserBasePath"].Value + "Pricelist201128_1.xlsx";




      DataSet[] datas = new DataSet[7];

      using (FileStream stream1 = File.Open(productXlsFilePath, FileMode.Open, FileAccess.Read))
      {
        using (FileStream stream2 = File.Open(eab_upc_ART_Codes_XlsPath, FileMode.Open, FileAccess.Read))
        {
          using (FileStream stream3 = File.Open(eab_upc_INHOUD_etc_XlsPath, FileMode.Open, FileAccess.Read))
          {
            using (FileStream stream4 = File.Open(eab_upc_ART_AFMETINGEN_XlsPath, FileMode.Open, FileAccess.Read))
            {
              using (FileStream stream5 = File.Open(alleen_Voorbeeld_XlsPath, FileMode.Open, FileAccess.Read))
              {
                using (FileStream stream6 = File.Open(base_prices_XlsPath, FileMode.Open, FileAccess.Read))
                {
                  using (FileStream stream7 = File.Open(werkbestand_XlsPath, FileMode.Open, FileAccess.Read))
                  {

                    //1. Reading from a binary Excel file ('97-2003 format; *.xls)
                    IExcelDataReader excelReader1 = ExcelReaderFactory.CreateBinaryReader(stream1);
                    IExcelDataReader excelReader2 = ExcelReaderFactory.CreateBinaryReader(stream2);
                    IExcelDataReader excelReader3 = ExcelReaderFactory.CreateBinaryReader(stream3);
                    IExcelDataReader excelReader4 = ExcelReaderFactory.CreateBinaryReader(stream4);
                    IExcelDataReader excelReader5 = ExcelReaderFactory.CreateBinaryReader(stream5);
                    //IExcelDataReader excelReader6 = ExcelReaderFactory.CreateBinaryReader(stream6);
                    //...
                    //2. Reading from a OpenXml Excel file (2007 format; *.xlsx)
                    IExcelDataReader excelReader6 = ExcelReaderFactory.CreateOpenXmlReader(stream6);

                    IExcelDataReader excelReader7 = ExcelReaderFactory.CreateBinaryReader(stream7);
                    excelReader7.IsFirstRowAsColumnNames = true;
                    //...
                    //3. DataSet - The result of each spreadsheet will be created in the result.Tables
                    DataSet result1 = excelReader1.AsDataSet();
                    DataSet result2 = excelReader2.AsDataSet();
                    DataSet result3 = excelReader3.AsDataSet();
                    DataSet result4 = excelReader4.AsDataSet();
                    DataSet result5 = excelReader5.AsDataSet();
                    DataSet result6 = excelReader6.AsDataSet();

                    ExcelToCSV xlstoCsv = new ExcelToCSV(werkbestand_XlsPath, true);
                    DataSet result7 = xlstoCsv.asDataSet();

                    //...

                    datas[0] = result1;
                    datas[1] = result2;
                    datas[2] = result3;
                    datas[3] = result4;
                    datas[4] = result5;
                    datas[5] = result6;
                    datas[6] = result7;


                    bool success = ParseDocument(datas);

                    if (success)
                    {
                      log.DebugFormat("File successfully processed");
                    }

                  }
                  {
                    log.DebugFormat("No new files to process");
                  }
                }
              }
            }
          }
        }
      }
    }

    private bool ParseDocument(DataSet[] tables)
    {
      using (var unit = GetUnitOfWork())
      {
        var _vendorRepo = unit.Scope.Repository<Vendor>();
        var _brandRepo = unit.Scope.Repository<Brand>();
        var _brandVendorRepo = unit.Scope.Repository<BrandVendor>();
        var _productRepo = unit.Scope.Repository<Product>();
        var _assortmentRepo = unit.Scope.Repository<VendorAssortment>();
        var _productGroupVendorRepo = unit.Scope.Repository<ProductGroupVendor>();
        var _prodDescriptionRepo = unit.Scope.Repository<ProductDescription>();
        var _attrGroupRepo = unit.Scope.Repository<ProductAttributeGroupMetaData>();
        var _attrGroupName = unit.Scope.Repository<ProductAttributeGroupName>();
        var _attrRepo = unit.Scope.Repository<ProductAttributeMetaData>();
        var _attrNameRepo = unit.Scope.Repository<ProductAttributeName>();
        var _attrValueRepo = unit.Scope.Repository<ProductAttributeValue>();
        var _mediaRepo = unit.Scope.Repository<ProductMedia>();
        var _mediaTypeRepo = unit.Scope.Repository<MediaType>();
        var _priceRepo = unit.Scope.Repository<VendorPrice>();
        var _stockRepo = unit.Scope.Repository<VendorStock>();
        var _barcodeRepo = unit.Scope.Repository<ProductBarcode>();
        var _relatedProductRepo = unit.Scope.Repository<RelatedProduct>();


        bool somethingWentWrong = false;
        var currentItem = string.Empty;

        try
        {
          int defaultVendorID = vendors.First().Key;

          var vendor = _vendorRepo.GetSingle(bv => bv.VendorID == defaultVendorID);

          #region Werkbestand verwerking

          foreach (DataTable table in tables[6].Tables)
          {
            foreach (DataColumn col in table.Columns)
            {
              if (col.ColumnName.Contains("BENELUX"))
              {
                col.ColumnName = "BENELUX";
                continue;
              }

              if (col.ColumnName.Contains("BELUX incl"))
              {
                col.ColumnName = "BE";
                continue;
              }

              if (col.ColumnName.Contains("NL incl"))
              {
                col.ColumnName = "NL";
                continue;
              }

              if (col.ColumnName == "F10")
              {
                col.ColumnName = "PriceChange";
                continue;
              }
            }
          }

          var itemAttributes7 = (from DataTable productData in tables[6].Tables
                                 from row in productData.AsEnumerable()
                                 let NL = !row.Table.Columns.Contains("BENELUX") ? (!row.Table.Columns.Contains("NL") ? string.Empty : row["NL"]) : row["BENELUX"]
                                 let BE = !row.Table.Columns.Contains("BENELUX") ? (!row.Table.Columns.Contains("BE") ? string.Empty : row["BE"]) : row["BENELUX"]
                                 let CostPrice = ((row.Table.Columns.Contains("VAT EXCL") && !row.IsNull("VAT EXCL")) ? row["VAT EXCL"].ToString() : "0")
                                 let priceChange = row.Table.Columns.Contains("PriceChange") && !row.IsNull("PriceChange") ? row["PriceChange"].ToString() : string.Empty
                                 let Color = row.Table.Columns.Contains("Color") && !row.IsNull("Color") ? row["Color"].ToString() : string.Empty
                                 let productCategory = checkRow(row, Category)
                                 where productCategory != string.Empty
                                 select new SennHeiserItemImport
                                 {
                                   //VendorBrandCode = vendor,
                                   //VendorName = vendor,
                                   VendorItemNumber = row[1].ToString().Trim(),
                                   CustomItemNumber = row[0].ToString().Trim(),
                                   ShortDescription = row[4].ToString().Trim(),
                                   LongDescription = row[5].ToString().Trim(),
                                   Price_BE = string.IsNullOrEmpty(BE.ToString()) ? 0 : decimal.Parse(BE.ToString()),
                                   Price_NL = string.IsNullOrEmpty(NL.ToString()) ? 0 : decimal.Parse(NL.ToString()),
                                   CostPrice = CostPrice.ToString(),
                                   ProductCategory = Category,
                                   MainCategory = productData.TableName,
                                   Attributes = new List<SennHeiserAttribute>()
                                   {
                                     new SennHeiserAttribute{
                                       AttributeCode = "New",
                                       AttributeGroupCode = "General",
                                       AttributeValue = string.IsNullOrEmpty(row[3].ToString()) ? "N" : "Y"
                                     },
                                     new SennHeiserAttribute{
                                       AttributeGroupCode = "Pricing",
                                       AttributeCode = "PriceGroup",
                                       AttributeValue = row[2].ToString()
                                     }, 
                                     !string.IsNullOrEmpty(priceChange) ? new SennHeiserAttribute{
                                       AttributeGroupCode = "Pricing",
                                       AttributeCode = "PriceChange",
                                       AttributeValue = priceChange
                                     } : null,
                                     !string.IsNullOrEmpty(Color) ? new SennHeiserAttribute{
                                       AttributeGroupCode = "General",
                                       AttributeCode = "Color",
                                       AttributeValue = Color
                                     }: null
                                   }
                                 }).ToList();

          #region Xml Data

          var productXmlFilePath = config.AppSettings.Settings["SennheiserBasePath"].Value + "products.xml";
          var productXmlCatPath = config.AppSettings.Settings["SennheiserBasePath"].Value + "categories.xml";

          var xmlText = File.ReadAllText(productXmlFilePath);

          var doc2 = HttpUtility.HtmlDecode(xmlText);

          XDocument doc = null;
          XDocument categories = null;

          try
          {
            xmlText = xmlText.Replace("<br>", " ");
            xmlText = xmlText.Replace("&M", "&amp;M");

            doc = XDocument.Parse(xmlText);
            categories = XDocument.Load(productXmlCatPath);
          }
          catch (Exception ex)
          {
            log.Error(String.Format("No new files to process or file not available"), ex);
          }

          var Categories = (from cat in categories.Element("CATEGORYDATA").Elements("category")
                            select new
                            {
                              category = cat.Element("Category").Value,
                              path = cat.Element("Path").Value
                            }
                              );


          var itemProducts = (from productData in doc.Element("PRODUCTDATA").Elements("product")
                              let td = productData.Element("TechnicalData").Elements()
                              let c =
                                Categories.Where(
                                x =>
                                x.path ==
                                "private_" + productData.Elements().Where(p => p.Name == "Path").FirstOrDefault().Value).
                                FirstOrDefault()
                              select new SennHeiserItemImport
                                       {
                                         CustomItemNumber = productData.Attribute("name").Value,
                                         VendorItemNumber = productData.Attribute("no").Value.Trim(),
                                         ShortContentDescription =
                                System.Web.HttpUtility.UrlDecode(productData.Element("ShortDescription").Value),
                                         LongContentDescription =
                                         System.Web.HttpUtility.UrlDecode(productData.Element("GeneralDescription").Value),
                                         Attributes = (from items in td
                                                       group items by items.Attribute("order").Value
                                                         into g
                                                         select new SennHeiserAttribute
                                                                  {
                                                                    AttributeCode =
                                                         System.Web.HttpUtility.UrlDecode(
                                                         td.Where(
                                                           x =>
                                                           x.Attribute("order").Value == g.Key &&
                                                           x.Name == "TechnicalData").FirstOrDefault().Value),
                                                                    AttributeValue =
                                                         System.Web.HttpUtility.UrlDecode(
                                                         td.Where(
                                                           x =>
                                                           x.Attribute("order").Value == g.Key &&
                                                           x.Name == "TechnicalData_Content").FirstOrDefault().Value),
                                                                    AttributeGroupCode = "TechnicalData"
                                                                  }

                                                         ).Union(
                                          (from elem in productData.Elements()
                                           where
                                             elem.Name == "KeyFeatures" || elem.Name == "Features" ||
                                             elem.Name == "DeliveryIncludes"
                                           select new SennHeiserAttribute
                                                    {
                                                      AttributeCode =
                                             elem.Name.LocalName.Length > 150
                                               ? elem.Name.LocalName.Substring(0, 150)
                                               : elem.Name.LocalName,
                                                      AttributeValue = System.Web.HttpUtility.UrlDecode(elem.Value),
                                                      AttributeGroupCode = elem.Name.LocalName
                                                    })).ToList(),
                                         Variants = (from data in productData.Elements()
                                                     where data.Name == "Variant"
                                                     from data1 in data.Elements()
                                                     where
                                                       data1.Name == "VariantData"
                                                     select new ProductData
                                                     {
                                                       VendorItemNumber = data1.Element("ProductNo").Value,
                                                       OrderID = int.Parse(data1.Attribute("order").Value),
                                                       CustomItemNumber = data1.Element("ProductName").Value,
                                                       Description = data1.Element("Description").Value
                                                     }).ToList(),
                                         Accessoires = (from data in productData.Elements()
                                                        where data.Name == "Accessories"
                                                        from data1 in data.Elements()
                                                        where
                                                          data1.Name == "AccessoriesData"
                                                        select new ProductData
                                                    {
                                                      VendorItemNumber = data1.Element("ProductNo").Value,
                                                      OrderID = int.Parse(data1.Attribute("order").Value),
                                                      CustomItemNumber = data1.Element("ProductName").Value,
                                                      Description = data1.Element("Description").Value,
                                                      Designation = data1.Element("Designation").Value
                                                    }).ToList(),
                                         ProductCategory = c.Try(q => q.category.Remove(0, "Private_".Length), string.Empty),
                                         ProductMedia = (from url in productData.Elements()
                                                         where url.Name == "Image"
                                                         select new ProductMedia
                                                         {
                                                           MediaUrl = url.Element("ImageFile").Value,
                                                           Description = url.Attribute("description").Value
                                                         }).ToList(),
                                         OtherMedia = (from m in productData.Elements()
                                                       where m.Name == "Download"
                                                       select new ProductMedia
                                                       {
                                                         MediaUrl = m.Element("DownloadFile").Value,
                                                         Sequence = int.Parse(m.Element("DownloadFile").Attribute("order").Value),
                                                         Description = m.Attribute("description").Value
                                                       }).ToList()
                                       }).ToList();

          #endregion

          var importProducts = itemProducts;//itemProducts.Union(itemAttributes7);

          //var importProducts = itemAttributes7;

          var brandVendors = _brandVendorRepo.GetAll(b => (b.VendorID == vendor.VendorID || (vendor.ParentVendorID.HasValue && b.VendorID == vendor.ParentVendorID))).ToList();

          var currentVendorProductGroups = _productGroupVendorRepo.GetAll(v => v.VendorID == vendor.VendorID).ToList();

          var productGroupVendors = _productGroupVendorRepo.GetAll(g => (g.VendorID == vendor.VendorID || (vendor.ParentVendorID.HasValue && g.VendorID == vendor.VendorID))).ToList();


          var products = _productRepo.GetAll().ToList();

          var vendorassortments = _assortmentRepo.GetAll(x => x.VendorID == vendor.VendorID).ToList();
          var medias = _mediaRepo.GetAll().ToList();
          var productDescriptions = _prodDescriptionRepo.GetAll().ToList();

          foreach (var vendorID in vendors.Keys)
          {

            int couterProduct = 0;
            int logCount = 0;
            int totalProducts = importProducts.Count();
            foreach (var product in importProducts)
            {
              try
              {
                couterProduct++;
                logCount++;
                if (logCount == 250)
                {
                  log.DebugFormat("Products Processed : {0}/{1} for Vendor {2}", couterProduct, totalProducts, vendor.Name);
                  logCount = 0;
                }

                string vendorBrandCode = "Sennheiser";
                if (!string.IsNullOrEmpty(product.MainCategory))
                  vendorBrandCode = int.Parse(product.MainCategory.Substring(1, 3)) < 8 ? "Sennheiser" : product.MainCategory.Substring(4).Replace("$'", "");

                #region BrandVendor

                Product prod = null;
                //check if brandvendor exists in db
                var brandVendor = (from b in brandVendors
                                   where b.VendorBrandCode == vendorBrandCode.Trim()
                                   select b).FirstOrDefault();

                if (brandVendor == null) //if brandvendor does not exist
                {
                  Brand brand = _brandRepo.GetSingle(x => x.Name == vendorBrandCode.Trim());

                  if (brand == null)
                  {
                    brand = new Brand()
                   {
                     Name = vendorBrandCode
                   };
                    _brandRepo.Add(brand);
                  }

                  //create new brandVendor
                  brandVendor = new BrandVendor
                  {
                    Brand = brand,
                    VendorID = vendor.VendorID,
                    VendorBrandCode = vendorBrandCode.Trim(),
                    Name = vendorBrandCode,
                  };
                  _brandVendorRepo.Add(brandVendor);
                  brandVendors.Add(brandVendor);
                }

                var BrandID = brandVendor.BrandID;

                prod = products.FirstOrDefault(p => p.VendorItemNumber == product.VendorItemNumber || p.VendorItemNumber == "00" + product.VendorItemNumber);

                //if product does not exist (usually true)
                if (prod == null)
                {
                  prod = new Product
                  {
                    VendorItemNumber = product.VendorItemNumber.ToString(),
                    BrandID = BrandID,
                    SourceVendorID = vendor.VendorID
                  };
                  _productRepo.Add(prod);
                  products.Add(prod);
                  //context.SubmitChanges();
                  unit.Save();
                }


                #endregion

                #region VendorAssortment

                var productID = prod.ProductID;

                var vendorAssortment = vendorassortments.FirstOrDefault(x => x.ProductID == productID);

                //if vendorAssortMent does not exist
                if (vendorAssortment == null)
                {
                  //create vendorAssortMent with productID
                  vendorAssortment = new VendorAssortment
                  {
                    Product = prod,

                    VendorID = vendorID
                  };
                  _assortmentRepo.Add(vendorAssortment);
                  vendorassortments.Add(vendorAssortment);
                  // context.SubmitChanges();
                }
                if (!string.IsNullOrEmpty(product.ShortDescription))
                  vendorAssortment.ShortDescription =
                    product.ShortDescription.Length > 150
                      ? product.ShortDescription.Substring(0, 150)
                      : product.ShortDescription;

                vendorAssortment.LongDescription = product.LongDescription;
                vendorAssortment.IsActive = true;
                vendorAssortment.CustomItemNumber = product.VendorItemNumber;
                #endregion

                #region VendorPrice
                if (vendorAssortment.VendorPrices == null) vendorAssortment.VendorPrices = new List<VendorPrice>();
                var vendorPrice = vendorAssortment.VendorPrices.FirstOrDefault();

                //create vendorPrice with vendorAssortmentID
                if (vendorPrice == null && product.Price_BE.HasValue && product.Price_NL.HasValue)
                {
                  decimal price = (decimal)product.GetType().GetProperty("Price_" + vendors[vendorID]).GetValue(product, null);
                  decimal costPrice = 0;
                  decimal taxRate = 0;

                  if (decimal.TryParse(product.CostPrice, out costPrice))
                  {
                    if (costPrice > 0)
                      taxRate = ((price / costPrice) * 100) - 100;

                    switch (vendors[vendorID])
                    {
                      case "NL":
                        taxRate = taxRate >= 0 ? 19 : 0;
                        break;
                      case "BE":
                        taxRate = taxRate >= 0 ? 21 : 0;
                        break;
                    }
                  }
                  else
                  {
                    if (!string.IsNullOrEmpty(product.CostPrice))
                    {
                      string reg = @"(\d*)%";
                      Regex expr = new Regex(reg);
                      Match match = expr.Match(product.CostPrice);
                      taxRate = decimal.Parse(match.Value.Replace("%", string.Empty));
                    }

                    if (taxRate < 1)
                    {
                      switch (vendors[vendorID])
                      {
                        case "NL":
                          taxRate = 19;
                          break;
                        case "BE":
                          taxRate = 21;
                          break;
                      }
                    }
                  }

                  vendorPrice = new VendorPrice
                  {
                    VendorAssortment = vendorAssortment,
                    BasePrice = price,
                    BaseCostPrice = costPrice,
                    TaxRate = taxRate,
                    CommercialStatus = "S",
                    MinimumQuantity = 0
                  };
                  _priceRepo.Add(vendorPrice);
                  vendorAssortment.VendorPrices.Add(vendorPrice);
                }
                #endregion

                #region VendorStock
                var vendorStock = _stockRepo.GetSingle(c => c.VendorID == vendorAssortment.VendorID && c.ProductID == vendorAssortment.ProductID);

                //create vendorPrice with vendorAssortmentID
                if (vendorStock == null)
                {

                  vendorStock = new VendorStock
                  {
                    Product = prod,
                    QuantityOnHand = 0,
                    VendorID = vendor.VendorID,
                    VendorStockTypeID = 1
                  };
                  _stockRepo.Add(vendorStock);
                  //vendorAssortment.VendorStock.Add(vendorStock);
                }
                #endregion

                #region ProductGroupVendor
                try
                {
                  ////create vendorGroup five times, each time with a different VendorProductGroupCode on a different level if not exist
                  int productGroupCount = 1;

                  if (!string.IsNullOrEmpty(product.MainCategory))
                  {
                    string category = product.ProductCategory;
                    product.ProductCategory = string.Format("{0}_{1}", product.MainCategory.Substring(4).Replace("$'", ""), category);
                  }

                  foreach (var cat in product.ProductCategory.Split('_'))
                  {
                    string category = cat;

                    if (cat.Length > 50)
                      category = cat.Remove(50);

                    //ProductGroupVendor productGroupVendor = null;

                    //foreach (var pgv in productGroupVendors)
                    //{
                    //  var name = pgv.GetType().GetProperty("VendorProductGroupCode" + productGroupCount).GetValue(pgv, null);
                    //  if (name != null && name == category.Trim())
                    //  {
                    //    productGroupVendor = pgv;
                    //    break;
                    //  }
                    //}

                    var productGroupVendor = productGroupVendors.Where(pg => pg.GetType().GetProperty("VendorProductGroupCode" + productGroupCount).GetValue(pg, null) != null
                      && pg.GetType().GetProperty("VendorProductGroupCode" + productGroupCount).GetValue(pg, null).ToString() == category.Trim()).FirstOrDefault();

                    if (productGroupVendor == null)
                    {
                      productGroupVendor = new ProductGroupVendor
                      {
                        ProductGroupID = unmappedID,
                        VendorID = vendorID,
                        VendorName = vendorBrandCode,
                        //VendorProductGroupCode1 = category.Trim()
                      };

                      productGroupVendor.GetType().GetProperty("VendorProductGroupCode" + productGroupCount).SetValue(productGroupVendor, category.Trim(), null);

                      _productGroupVendorRepo.Add(productGroupVendor);
                      productGroupVendors.Add(productGroupVendor);
                    }

                    #region sync
                    if (currentVendorProductGroups.Contains(productGroupVendor))
                    {
                      currentVendorProductGroups.Remove(productGroupVendor);
                    }
                    #endregion

                    //var vendorProductGroupAssortments = (from c in vendorAssortment.VendorProductGroupAssortments
                    //                                     where
                    //                                       c.VendorAssortment == vendorAssortment
                    //                                     select c).ToList();

                    var vendorProductGroupAssortment1 =
                      productGroupVendor.VendorAssortments.FirstOrDefault(c => c.VendorAssortmentID == vendorAssortment.VendorAssortmentID);

                    if (vendorProductGroupAssortment1 == null)
                    {
                      productGroupVendor.VendorAssortments.Add(vendorAssortment);
                    }

                    productGroupCount++;
                  }
                }
                catch (Exception ex)
                {
                  log.Error("Failed productgroups", ex);
                }
                #endregion

                #region ProductDescription

                var productDescription = productDescriptions.FirstOrDefault(pd => pd.LanguageID == languageID && (pd.VendorID == vendor.VendorID || (vendor.ParentVendorID.HasValue && pd.VendorID == vendor.VendorID)));

                if (productDescription == null)
                {
                  //create ProductDescription
                  productDescription = new ProductDescription
                  {
                    Product = prod,
                    LanguageID = languageID,
                    VendorID = vendorID,

                  };

                  _prodDescriptionRepo.Add(productDescription);
                  productDescriptions.Add(productDescription);
                }

                if (string.IsNullOrEmpty(productDescription.ProductName))
                {
                  productDescription.ProductName = product.CustomItemNumber.ToString();
                  productDescription.ModelName = product.CustomItemNumber.ToString();
                }

                if (!string.IsNullOrEmpty(product.ShortDescription))
                  productDescription.ShortContentDescription = product.ShortDescription.Length > 1000
                                                                 ? product.ShortDescription.Substring(0, 1000)
                                                                 : product.ShortDescription;

                if (string.IsNullOrEmpty(productDescription.LongContentDescription))
                  productDescription.LongContentDescription = product.LongContentDescription;

                if (string.IsNullOrEmpty(productDescription.LongSummaryDescription))
                  productDescription.LongSummaryDescription = product.ShortContentDescription;

                unit.Save();
                #endregion

                #region ProductImage
                if (product.ProductMedia != null)
                {
                  int imageSequence = 0;

                  var imageType = _mediaTypeRepo.GetSingle(i => i.Type.ToLower() == "Image");


                  if (imageType == null)
                  {
                    imageType = new MediaType()
                    {
                      Type = "Image"
                    };

                    _mediaTypeRepo.Add(imageType);
                  }

                  foreach (var image in product.ProductMedia)
                  {
                    if (!string.IsNullOrEmpty(image.MediaUrl))
                    {
                      var productImage = medias.FirstOrDefault(pd => (pd.VendorID == vendor.VendorID || (vendor.ParentVendorID.HasValue && pd.VendorID == vendor.VendorID)) &&
                        pd.MediaUrl == image.MediaUrl);

                      if (productImage == null)
                      {
                        //create ProductDescription
                        productImage = new ProductMedia
                        {
                          Product = prod,
                          MediaUrl = image.MediaUrl,
                          VendorID = vendorID
                        };

                        _mediaRepo.Add(productImage);
                        medias.Add(productImage);
                      }

                      productImage.Sequence = imageSequence;
                      productImage.MediaType = imageType;
                      productImage.Description = image.Description;
                      imageSequence++;
                    }
                  }
                }
                #endregion

                #region ProductMedia
                if (product.OtherMedia != null)
                {
                  int productMediaSequence = 0;
                  foreach (var media in product.OtherMedia)
                  {
                    string fileType = media.MediaUrl.Substring(media.MediaUrl.Length - 3).ToLower();

                    var mediaType = _mediaTypeRepo.GetSingle(c => c.Type.ToLower() == fileType);

                    if (mediaType == null)
                    {
                      mediaType = new MediaType()
                      {
                        Type = fileType
                      };

                      _mediaTypeRepo.Add(mediaType);
                    }

                    if (!string.IsNullOrEmpty(media.MediaUrl))
                    {
                      var productImage = medias.FirstOrDefault(pd => (pd.VendorID == vendor.VendorID || (vendor.ParentVendorID.HasValue && pd.VendorID == vendor.VendorID)) &&
                        pd.MediaUrl == media.MediaUrl);

                      if (productImage == null)
                      {
                        //create ProductDescription
                        productImage = new ProductMedia
                        {
                          Product = prod,
                          MediaUrl = media.MediaUrl,
                          VendorID = vendorID
                        };

                        _mediaRepo.Add(productImage);
                        medias.Add(productImage);
                      }

                      productImage.Sequence = productMediaSequence;
                      productImage.MediaType = mediaType;
                      productImage.Description = media.Description;
                      productMediaSequence++;
                    }
                  }
                }
                #endregion

                #region ProductAttribute
                product.Attributes.Add(new SennHeiserAttribute
                              {
                                AttributeCode = "Guarantee",
                                AttributeGroupCode = "GuaranteeSupplier",
                                AttributeValue = "Sennheiser"
                              });

                //  insert all other Attributes

                int keyFeatures = 0;
                int features = 0;
                int DeliveryIncludes = 0;

                foreach (var content in product.Attributes)
                {
                  if (content == null)
                    continue;

                  #region ProductAttributeGroupMetaData

                  var productAttributeGroupMetaData = _attrGroupRepo.GetSingle(c => c.GroupCode == content.AttributeGroupCode && (c.VendorID == vendor.VendorID || (vendor.ParentVendorID.HasValue && c.VendorID == vendor.VendorID)));
                  //create ProductAttributeGroupMetaData if not exists
                  if (productAttributeGroupMetaData == null)
                  {
                    productAttributeGroupMetaData = new ProductAttributeGroupMetaData
                    {
                      Index = 0,
                      GroupCode = content.AttributeGroupCode,
                      VendorID = vendorID
                    };
                    _attrGroupRepo.Add(productAttributeGroupMetaData);
                  }
                  #endregion

                  #region ProductAttributeGroupName

                  if (productAttributeGroupMetaData.ProductAttributeGroupNames == null) productAttributeGroupMetaData.ProductAttributeGroupNames = new List<ProductAttributeGroupName>();
                  var productAttributeGroupName = productAttributeGroupMetaData.ProductAttributeGroupNames.FirstOrDefault(c => c.LanguageID == languageID);
                  //create ProductAttributeGroupName if not exists
                  if (productAttributeGroupName == null)
                  {
                    productAttributeGroupName = new ProductAttributeGroupName
                    {
                      Name = content.AttributeGroupCode,
                      ProductAttributeGroupMetaData = productAttributeGroupMetaData,
                      LanguageID = languageID
                    };
                    _attrGroupName.Add(productAttributeGroupName);
                  }

                  #endregion

                  #region ProductAttributeMetaData

                  //create ProductAttributeMetaData as many times that there are entrys in technicaldata
                  string attributeCode = content.AttributeCode;
                  if (content.AttributeCode == "KeyFeatures")
                  {
                    if (keyFeatures > 0)
                      attributeCode = attributeCode + keyFeatures.ToString();

                    keyFeatures++;
                  }
                  else if (content.AttributeCode == "Features")
                  {
                    if (features > 0)
                      attributeCode = attributeCode + features.ToString();

                    features++;
                  }
                  else if (content.AttributeCode == "DeliveryIncludes")
                  {
                    if (DeliveryIncludes > 0)
                      attributeCode = attributeCode + DeliveryIncludes.ToString();

                    DeliveryIncludes++;
                  }

                  var productAttributeMetaData = _attrRepo.GetSingle(x => x.AttributeCode == attributeCode && (x.VendorID == vendor.VendorID || (vendor.ParentVendorID.HasValue && x.VendorID == vendor.VendorID)));
                  //create ProductAttributeMetaData if not exists
                  if (productAttributeMetaData == null)
                  {
                    productAttributeMetaData = new ProductAttributeMetaData
                    {
                      ProductAttributeGroupMetaData = productAttributeGroupMetaData,
                      AttributeCode = attributeCode,
                      Index = 0,
                      IsVisible = true,
                      NeedsUpdate = true,
                      VendorID = vendorID,
                      IsSearchable = false
                    };
                    _attrRepo.Add(productAttributeMetaData);
                  }

                  #endregion

                  #region ProductAttributeName
                  if (productAttributeMetaData.ProductAttributeNames == null) productAttributeMetaData.ProductAttributeNames = new List<ProductAttributeName>();
                  var productAttributeName = productAttributeMetaData.ProductAttributeNames.FirstOrDefault(c => c.LanguageID == languageID);
                  //create ProductAttributeName if not exists
                  if (productAttributeName == null)
                  {
                    productAttributeName = new ProductAttributeName
                    {
                      ProductAttributeMetaData = productAttributeMetaData,
                      LanguageID = languageID
                    };
                    _attrNameRepo.Add(productAttributeName);
                  }
                  productAttributeName.Name = content.AttributeCode;


                  #endregion

                  #region ProductAttributeValue

                  if (productAttributeMetaData.ProductAttributeValues == null) productAttributeMetaData.ProductAttributeValues = new List<ProductAttributeValue>();

                  var productAttributeValue =
                    productAttributeMetaData.ProductAttributeValues.FirstOrDefault(c => c.ProductID == prod.ProductID);
                  //create ProductAttributeValue 
                  if (productAttributeValue == null)
                  {
                    productAttributeValue = new ProductAttributeValue
                    {
                      ProductAttributeMetaData = productAttributeMetaData,
                      Product = prod,
                      LanguageID = languageID
                    };
                    _attrValueRepo.Add(productAttributeValue);
                  }
                  productAttributeValue.Value = content.AttributeValue;
                  #endregion

                  unit.Save();
                }
                #endregion

                var _relatedProductTypeRepo = unit.Scope.Repository<RelatedProductType>();

                #region variants
                if (product.Variants != null)
                {
                  var relatedType = _relatedProductTypeRepo.GetSingle(x => x.Type == "Variant");
                  if (relatedType == null)
                  {
                    relatedType = new RelatedProductType
                    {
                      Type = "Variant"
                    };
                    _relatedProductTypeRepo.Add(relatedType);
                  }

                  foreach (var varaint in product.Variants)
                  {
                    var relatedProduct = _productRepo.GetSingle(va => va.VendorItemNumber == varaint.VendorItemNumber);

                    if (relatedProduct == null)
                    {
                      relatedProduct = _assortmentRepo.GetSingle(va => va.CustomItemNumber == varaint.CustomItemNumber).Try(c => c.Product, null);

                      if (relatedProduct == null)
                      {
                        relatedProduct = new Product
                        {
                          VendorItemNumber = varaint.VendorItemNumber,
                          BrandID = BrandID,
                          SourceVendorID = vendor.VendorID
                        };

                        var desc = new ProductDescription
                        {
                          LanguageID = languageID,
                          Product = relatedProduct,
                          ProductName = varaint.CustomItemNumber,
                          ShortContentDescription = varaint.Description,
                          ModelName = varaint.Designation,
                          VendorID = vendor.VendorID
                        };
                        _productRepo.Add(relatedProduct);
                        _prodDescriptionRepo.Add(desc);
                      }
                    }
                    unit.Save();
                    var relatedProducts = _relatedProductRepo.GetSingle(rp => rp.ProductID == prod.ProductID
                                           && rp.RelatedProductID == relatedProduct.ProductID
                                           && rp.VendorID == vendor.VendorID);

                    if (relatedProducts == null)
                    {
                      relatedProducts = new RelatedProduct
                      {
                        SourceProduct = prod,
                        RelatedProductID = relatedProduct.ProductID,
                        VendorID = vendor.VendorID,
                        RelatedProductType = relatedType
                      };
                      _relatedProductRepo.Add(relatedProducts);
                    }
                    unit.Save();
                  }
                }
                #endregion

                #region Accessory
                if (product.Accessoires != null)
                {
                  var relatedType = _relatedProductTypeRepo.GetSingle(x => x.Type == "Accossaire");
                  if (relatedType == null)
                  {
                    relatedType = new RelatedProductType
                    {
                      Type = "Accossaire"
                    };
                    _relatedProductTypeRepo.Add(relatedType);
                  }

                  foreach (var acc in product.Accessoires)
                  {
                    var relatedProduct = _productRepo.GetSingle(va => va.VendorItemNumber == acc.VendorItemNumber);

                    if (relatedProduct == null)
                    {
                      relatedProduct = _assortmentRepo.GetSingle(va => va.CustomItemNumber == acc.CustomItemNumber).Try(c => c.Product, null);

                      if (relatedProduct == null)
                      {
                        relatedProduct = new Product
                        {
                          VendorItemNumber = acc.VendorItemNumber,
                          BrandID = BrandID,
                          SourceVendorID = vendor.VendorID
                        };

                        var desc = new ProductDescription
                        {
                          LanguageID = languageID,
                          Product = relatedProduct,
                          ProductName = acc.CustomItemNumber,
                          ShortContentDescription = acc.Description,
                          ModelName = acc.Designation,
                          VendorID = vendor.VendorID
                        };
                        _productRepo.Add(relatedProduct);
                        _prodDescriptionRepo.Add(desc);
                      }
                    }
                    unit.Save();

                    var relatedProducts = _relatedProductRepo.GetSingle(rp => rp.ProductID == prod.ProductID
                                           && rp.RelatedProductID == relatedProduct.ProductID
                                           && rp.VendorID == vendor.VendorID);
                    //TODO: Make sure that the related product is the correct product
                    if (relatedProducts == null)
                    {
                      relatedProducts = new RelatedProduct
                      {
                        SourceProduct = prod,
                        RelatedProductID = relatedProduct.ProductID,
                        VendorID = vendor.VendorID,
                        RelatedProductType = relatedType
                      };
                      _relatedProductRepo.Add(relatedProducts);
                    }
                    unit.Save();
                  }
                }
                #endregion

              }
              catch (Exception ex)
              {
                log.AuditError(string.Format("Error import product {0}", product.VendorItemNumber), ex, "Product import");
              }
            }
          }
          #endregion

          var productAttributeGroups = _attrGroupRepo.GetAll().ToList();

          var productAttributes = _attrRepo.GetAll(g => g.VendorID == defaultVendorID).ToList();

          #region CSN-productlijst verwerking
          try
          {
            var itemAttributes = (from productData in tables[0].Tables[0].AsEnumerable()
                                  where productData[0] != null && (from p in productDescriptions where p.ProductName == productData[1].ToString().Trim() && p.VendorID == defaultVendorID select p.ProductID).FirstOrDefault() != 0
                                  select new
                                  {
                                    VendorItemNumber = productData[0].ToString().Trim(),
                                    ProductName = productData[1].ToString().Trim(),
                                    ProductID = (from p in products where p.VendorItemNumber == productData[0].ToString().Trim() select p.ProductID).FirstOrDefault()
                                  }
                         ).ToList();

            var garuanteeSupplierAttributes = productAttributes.FirstOrDefault(c => c.AttributeCode == "Guarantee");

            foreach (var content in itemAttributes)
            {
              currentItem = "CSN ProductList: product > " + content.VendorItemNumber;

              if (content.ProductID != 0)
              {

                var proda = _attrValueRepo.GetSingle(p => p.Product.ProductID == Int32.Parse(content.ProductID.ToString()) &&
                               p.AttributeID == garuanteeSupplierAttributes.AttributeID);

                if (proda != null)
                  proda.Value = "CSN";

                unit.Save();
              }
            }
          }
          catch (Exception ex)
          {
            log.AuditError("Failied import CSN products", ex);
          }
          #endregion

          #region Nuemann_EAC_UPC_ART_Codes verwerking
          try
          {
            var itemAttributes2 = (from productData in tables[1].Tables[0].AsEnumerable()
                                   //where productData.Field<double?>(0).HasValue
                                   where !productData.IsNull(0)
                                   &&
                                   productData[0] != null && (from p in products where p.VendorItemNumber.Trim() == "00" + productData[0].ToString().Trim() select p.ProductID).FirstOrDefault() != 0 /*&&  productData[9] != null && productData[11] != null*/
                                   select new
                                   {
                                     VendorItemNumber = "00" + productData[0].ToString().Trim(),
                                     EANCode = productData[9].ToString().Trim().Replace(" ", ""),
                                     UPCCode = productData[11].ToString().Trim().Replace(" ", ""),
                                     ProductID = (from p in products where p.VendorItemNumber.Trim() == productData[0].ToString().Trim() select p.ProductID).FirstOrDefault()
                                   }).ToList();

            foreach (var content in itemAttributes2)
            {
              currentItem = "Nuemann barcodes: product > " + content.VendorItemNumber;

              var product = (from p in products
                             where p.ProductID == content.ProductID
                             select p).FirstOrDefault();
              var productBarcodes = (from p in products
                                     where p.ProductID == content.ProductID
                                     select p.ProductBarcodes).ToList();
              if (product != null)
              {
                if (product.ProductBarcodes == null) product.ProductBarcodes = new List<ProductBarcode>();

                if (!product.ProductBarcodes.Any(b => b.Barcode == content.EANCode))
                {
                  //create ProductBarcode if not exists

                  var barcode = new ProductBarcode
                                  {
                                    Product = product,
                                    Barcode = content.EANCode.ToString(),
                                    VendorID = vendor.VendorID,
                                    BarcodeType = (int)BarcodeTypes.EAN
                                  };
                  _barcodeRepo.Add(barcode);
                  // productBarcodes.Add()
                }

                if (!product.ProductBarcodes.Any(b => b.Barcode == content.UPCCode))
                {
                  //create ProductBarcode if not exists
                  _barcodeRepo.Add(new ProductBarcode
                                                           {
                                                             Product = product,
                                                             Barcode = content.UPCCode.ToString(),
                                                             VendorID = vendor.VendorID,
                                                             BarcodeType = (int)BarcodeTypes.UPC
                                                           });
                }
              }

              var brandVendor = brandVendors.FirstOrDefault(vb => vb.VendorBrandCode.Trim() == "Nuemann");

              if (brandVendor == null) //if brandvendor does not exist
              {
                Brand brand = _brandRepo.GetSingle(x => x.Name == "Nuemann");

                if (brand == null)
                {
                  brand = new Brand()
                 {
                   Name = "Nuemann"
                 };
                  _brandRepo.Add(brand);
                }

                //create new brandVendor
                brandVendor = new BrandVendor
                {
                  Brand = brand,
                  VendorID = defaultVendorID,
                  VendorBrandCode = "Nuemann",
                  Name = "Nuemann"
                };
                _brandVendorRepo.Add(brandVendor);
                brandVendors.Add(brandVendor);
              }
              product.BrandID = brandVendor.BrandID;
            }

            unit.Save();
          }
          catch (Exception ex)
          {
            log.AuditError("Failed import Neuman barcodes", ex);
          }
          #endregion

          #region Sencomm_EAN_UPC_INHOUD_etc verwerking
          try
          {
            var itemAttributes3 = (from productData in tables[2].Tables[0].AsEnumerable()
                                   //where int.TryParse(productData.Field<string>(0), out bla)
                                   where !productData.IsNull(0) &&
                                   productData[0] != null && (from p in products where p.VendorItemNumber == productData[0].ToString().Trim() select p.ProductID).FirstOrDefault() != 0 && productData[4] != null && productData[5] != null
                                   select new
                                   {
                                     VendorItemNumber = productData[0].ToString().Trim(),
                                     EANCode = productData[4].ToString().Trim().Replace(" ", ""),
                                     UPCCode = productData[5].ToString().Trim().Replace(" ", ""),
                                     ProductID = (from p in products where p.VendorItemNumber == productData[0].ToString().Trim() select p.ProductID).FirstOrDefault()
                                   }).ToList();

            foreach (var content in itemAttributes3)
            {
              currentItem = "Sencomm barcodes: product > " + content.VendorItemNumber;

              var product = (from p in products
                             where p.ProductID == content.ProductID
                             select p).FirstOrDefault();
              if (product != null)
              {
                if (product.ProductBarcodes == null) product.ProductBarcodes = new List<ProductBarcode>();
                if (!product.ProductBarcodes.Any(b => b.Barcode == content.EANCode))
                {
                  //create ProductBarcode if not exists
                  _barcodeRepo.Add(new ProductBarcode
                                                           {
                                                             Product = product,
                                                             Barcode = content.EANCode.ToString(),
                                                             VendorID = vendor.VendorID,
                                                             BarcodeType = (int)BarcodeTypes.EAN
                                                           });
                }

                if (!product.ProductBarcodes.Any(b => b.Barcode == content.UPCCode))
                {
                  //create ProductBarcode if not exists
                  _barcodeRepo.Add(new ProductBarcode
                                                           {
                                                             Product = product,
                                                             Barcode = content.UPCCode.ToString(),
                                                             VendorID = vendor.VendorID,
                                                             BarcodeType = (int)BarcodeTypes.UPC
                                                           });
                }
              }
            }

            unit.Save();
          }
          catch (Exception ex)
          {
            log.AuditError("Failed import Senncom info", ex);
          }
          #endregion

          #region Sennheiser_EAN_UPC_ART_AFMETINGEN
          try
          {
            var itemAttributes4 = (from productData in tables[3].Tables[0].AsEnumerable()
                                   //where productData.Field<double?>(0).HasValue
                                   where !productData.IsNull(0) && productData[0] != null && (from p in products where p.VendorItemNumber == productData[0].ToString().Trim() select p.ProductID).FirstOrDefault() != 0 && productData[2] != null && productData[3] != null
                                   select new
                                   {
                                     VendorItemNumber = productData[0].ToString().Trim(),
                                     EANCode = productData[2].ToString().Trim().Replace(" ", ""),
                                     UPCCode = productData[3].ToString().Trim().Replace(" ", ""),
                                     ProductID = (from p in products where p.VendorItemNumber == productData[0].ToString().Trim() select p.ProductID).FirstOrDefault(),
                                     Attributes = new Dictionary<string, string>()
                                                          {
                                                           
                                                           {
                                                              "CartonWeight",
                                                              productData[4].ToString()
                                                           },

                                                           
                                                           {
                                                             "Carton_dimensions_" ,
                                                             productData[5].ToString()
                                                           },

                                                           {
                                                              "MasterCartonWeight",
                                                              productData[6].ToString()
                                                             },

                                                          
                                                           {
                                                             "MasterCartonQuantity",
                                                             productData[7].ToString()
                                                             
                                                             },
                                                          
                                                           {
                                                            "Master_Carton_dimensions",
                                                              productData[8].ToString()
                                                             
                                                             }
                                                          }
                                   }).ToList();

            foreach (var content in itemAttributes4)
            {
              currentItem = "Sennheiser barcodes & attributes: product > " + content.VendorItemNumber;

              var product = (from p in products
                             where p.ProductID == content.ProductID
                             select p).FirstOrDefault();
              if (product != null)
              {
                if (product.ProductBarcodes == null) product.ProductBarcodes = new List<ProductBarcode>();
                if (!product.ProductBarcodes.Any(b => b.Barcode == content.EANCode))
                {
                  //create ProductBarcode if not exists
                  _barcodeRepo.Add(new ProductBarcode
                                                           {
                                                             Product = product,
                                                             Barcode = content.EANCode.ToString(),
                                                             VendorID = vendor.VendorID,
                                                             BarcodeType = (int)BarcodeTypes.EAN
                                                           });
                }

                if (!product.ProductBarcodes.Any(b => b.Barcode == content.UPCCode))
                {
                  //create ProductBarcode if not exists
                  _barcodeRepo.Add(new ProductBarcode
                                                           {
                                                             Product = product,
                                                             Barcode = content.UPCCode.ToString(),
                                                             VendorID = vendor.VendorID,
                                                             BarcodeType = (int)BarcodeTypes.UPC
                                                           });
                }

                //  insert attributes data
                #region ProductAttributeGroupMetaData

                var guaranteeProductAttributeGroupMetaData =
                    productAttributeGroups.FirstOrDefault(c => c.GroupCode == "General");
                //create ProductAttributeGroupMetaData if not exists
                if (guaranteeProductAttributeGroupMetaData == null)
                {
                  guaranteeProductAttributeGroupMetaData = new ProductAttributeGroupMetaData
                  {
                    Index = 0,
                    GroupCode = "General",
                    VendorID = defaultVendorID
                  };
                  _attrGroupRepo.Add(guaranteeProductAttributeGroupMetaData);
                  productAttributeGroups.Add(guaranteeProductAttributeGroupMetaData);
                }
                #endregion

                #region ProductAttributeGroupName
                if (guaranteeProductAttributeGroupMetaData.ProductAttributeGroupNames == null) guaranteeProductAttributeGroupMetaData.ProductAttributeGroupNames = new List<ProductAttributeGroupName>();
                var guaranteeProductAttributeGroupName =
                  guaranteeProductAttributeGroupMetaData.ProductAttributeGroupNames.FirstOrDefault(c => c.LanguageID == languageID);
                //create ProductAttributeGroupName if not exists
                if (guaranteeProductAttributeGroupName == null)
                {
                  guaranteeProductAttributeGroupName = new ProductAttributeGroupName
                  {
                    Name = "General",
                    ProductAttributeGroupMetaData = guaranteeProductAttributeGroupMetaData,
                    LanguageID = languageID
                  };
                  _attrGroupName.Add(guaranteeProductAttributeGroupName);
                }
                //context.SubmitChanges();

                #endregion

                #region ProductAttributeMetaData

                //create ProductAttributeMetaData 

                var guaranteeProductAttributeMetaData =
                  productAttributes.FirstOrDefault(c => c.AttributeCode == "Packaging");
                //create ProductAttributeMetaData if not exists
                if (guaranteeProductAttributeMetaData == null)
                {
                  guaranteeProductAttributeMetaData = new ProductAttributeMetaData
                  {
                    ProductAttributeGroupMetaData = guaranteeProductAttributeGroupMetaData,
                    AttributeCode = "Packaging",
                    Index = 0,
                    IsVisible = true,
                    NeedsUpdate = true,
                    VendorID = defaultVendorID,
                    IsSearchable = false
                  };
                  _attrRepo.Add(guaranteeProductAttributeMetaData);
                  productAttributes.Add(guaranteeProductAttributeMetaData);
                }

                // context.SubmitChanges();
                #endregion

                //insert all the attributes if they do not exist
                foreach (var attribute in content.Attributes)
                {

                  #region ProductAttributeName
                  if (guaranteeProductAttributeMetaData.ProductAttributeNames == null) guaranteeProductAttributeMetaData.ProductAttributeNames = new List<ProductAttributeName>();
                  var guaranteeProductAttributeName =
                    guaranteeProductAttributeMetaData.ProductAttributeNames.FirstOrDefault(c => c.LanguageID == languageID);
                  //create ProductAttributeName if not exists
                  if (guaranteeProductAttributeName == null)
                  {
                    guaranteeProductAttributeName = new ProductAttributeName
                                                      {
                                                        ProductAttributeMetaData = guaranteeProductAttributeMetaData,
                                                        LanguageID = languageID,
                                                        Name = attribute.Key
                                                      };
                    _attrNameRepo.Add(guaranteeProductAttributeName);
                  }

                  #endregion

                  #region ProductAttributeValue
                  if (guaranteeProductAttributeMetaData.ProductAttributeValues == null) guaranteeProductAttributeMetaData.ProductAttributeValues = new List<ProductAttributeValue>();
                  var guaranteeProductAttributeValue =
                    guaranteeProductAttributeMetaData.ProductAttributeValues.FirstOrDefault(
                      c => c.ProductID == content.ProductID);
                  //create ProductAttributeValue 
                  if (guaranteeProductAttributeValue == null)
                  {
                    guaranteeProductAttributeValue = new ProductAttributeValue
                                                       {
                                                         ProductAttributeMetaData = guaranteeProductAttributeMetaData,
                                                         Product = product,
                                                         Value = attribute.Value,
                                                         LanguageID = languageID
                                                       };
                    _attrValueRepo.Add(guaranteeProductAttributeValue);
                  }
                }

                  #endregion
              }
              unit.Save();
            }

          }
          catch (Exception ex)
          {
            log.AuditError("Failed import Sennheiser attributes", ex);
          }
          #endregion

          #region Alleen voorbeeld verwerking
          try
          {
            var itemAttributes5 = (from productData in tables[4].Tables[0].AsEnumerable()
                                   where !productData.IsNull(1) && productData[1] != null && (from p in products where p.VendorItemNumber == productData[1].ToString().Trim() select p.ProductID).FirstOrDefault() != 0 && productData[4] != null
                                   select new
                                   {
                                     VendorItemNumber = productData[1].ToString().Trim(),
                                     Status = productData[4],
                                     ProductID = (from p in products where p.VendorItemNumber == productData[1].ToString().Trim() select p.ProductID).FirstOrDefault()
                                   }
                              ).ToList();

            foreach (var content in itemAttributes5)
            {
              currentItem = "Alleen voorbeeld statusen: product > " + content.VendorItemNumber;

              var product = _productRepo.GetSingle(p => p.ProductID == content.ProductID);

              if (product != null)
              {
                //  insert Sparepart data
                #region ProductAttributeGroupMetaData

                var guaranteeProductAttributeGroupMetaData =
                    productAttributeGroups.FirstOrDefault(c => c.GroupCode == "Sparepart");
                //create ProductAttributeGroupMetaData if not exists
                if (guaranteeProductAttributeGroupMetaData == null)
                {
                  guaranteeProductAttributeGroupMetaData = new ProductAttributeGroupMetaData
                  {
                    Index = 0,
                    GroupCode = "Sparepart",
                    VendorID = defaultVendorID
                  };
                  _attrGroupRepo.Add(guaranteeProductAttributeGroupMetaData);
                  productAttributeGroups.Add(guaranteeProductAttributeGroupMetaData);
                }
                #endregion

                #region ProductAttributeGroupName

                if (guaranteeProductAttributeGroupMetaData.ProductAttributeGroupNames == null) guaranteeProductAttributeGroupMetaData.ProductAttributeGroupNames = new List<ProductAttributeGroupName>();
                var guaranteeProductAttributeGroupName =
                  guaranteeProductAttributeGroupMetaData.ProductAttributeGroupNames.FirstOrDefault(c => c.LanguageID == languageID);
                //create ProductAttributeGroupName if not exists
                if (guaranteeProductAttributeGroupName == null)
                {
                  guaranteeProductAttributeGroupName = new ProductAttributeGroupName
                  {
                    Name = "Sparepart",
                    ProductAttributeGroupMetaData = guaranteeProductAttributeGroupMetaData,
                    LanguageID = languageID
                  };
                  _attrGroupName.Add(guaranteeProductAttributeGroupName);
                }
                //context.SubmitChanges();

                #endregion

                #region ProductAttributeMetaData

                //create ProductAttributeMetaData 

                var guaranteeProductAttributeMetaData =
                  productAttributes.FirstOrDefault(c => c.AttributeCode == "Status");
                //create ProductAttributeMetaData if not exists
                if (guaranteeProductAttributeMetaData == null)
                {
                  guaranteeProductAttributeMetaData = new ProductAttributeMetaData
                  {
                    ProductAttributeGroupMetaData = guaranteeProductAttributeGroupMetaData,
                    AttributeCode = "Status",
                    Index = 0,
                    IsVisible = true,
                    NeedsUpdate = true,
                    VendorID = defaultVendorID,
                    IsSearchable = false
                  };
                  _attrRepo.Add(guaranteeProductAttributeMetaData);
                  productAttributes.Add(guaranteeProductAttributeMetaData);
                }

                // context.SubmitChanges();
                #endregion

                #region ProductAttributeName

                if (guaranteeProductAttributeMetaData.ProductAttributeNames == null) guaranteeProductAttributeMetaData.ProductAttributeNames = new List<ProductAttributeName>();
                var guaranteeProductAttributeName =
                  guaranteeProductAttributeMetaData.ProductAttributeNames.FirstOrDefault(c => c.LanguageID == languageID);
                //create ProductAttributeName if not exists
                if (guaranteeProductAttributeName == null)
                {
                  guaranteeProductAttributeName = new ProductAttributeName
                  {
                    ProductAttributeMetaData = guaranteeProductAttributeMetaData,
                    LanguageID = languageID,
                    Name = "Status"
                  };
                  _attrNameRepo.Add(guaranteeProductAttributeName);
                }

                // context.SubmitChanges();
                #endregion

                #region ProductAttributeValue
                if (guaranteeProductAttributeMetaData.ProductAttributeValues == null) guaranteeProductAttributeMetaData.ProductAttributeValues = new List<ProductAttributeValue>();
                var guaranteeProductAttributeValue =
                  guaranteeProductAttributeMetaData.ProductAttributeValues.FirstOrDefault(c => c.ProductID == content.ProductID);
                //create ProductAttributeValue 
                if (guaranteeProductAttributeValue == null)
                {
                  guaranteeProductAttributeValue = new ProductAttributeValue
                  {
                    ProductAttributeMetaData = guaranteeProductAttributeMetaData,
                    Product = product,
                    Value = content.Status.ToString(),
                    LanguageID = languageID
                  };
                  _attrValueRepo.Add(guaranteeProductAttributeValue);
                }
                #endregion
              }
            }
            unit.Save();
          }
          catch (Exception ex)
          {
            log.AuditError("Failed import productstatus", ex);
          }
          #endregion
        }
        catch (Exception ex)
        {
          log.ErrorFormat("product: {0} error: {1}", currentItem, ex.StackTrace);
          somethingWentWrong = true;
        }

        if (somethingWentWrong)
        {
          return false;
        }
        return true;
      }
    }

    private string checkRow(object row, string productCategory)
    {
      DataRow rowToCheck = (DataRow)row;

      if (rowToCheck.IsNull(4))
      {
        Category = rowToCheck[0].ToString();
        return string.Empty;
      }
      return productCategory;
    }
  }
}
