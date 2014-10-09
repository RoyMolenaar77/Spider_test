using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Data.Linq;
using System.Linq.Dynamic;
using System.Text;
using Concentrator.Objects;
using Excel;
using Concentrator.Objects.Utility;
using System.Web;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using Concentrator.Objects.Enumerations;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.Models.Brands;
using Concentrator.Objects.Models.Attributes;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Models.Media;
using Concentrator.Objects.ConcentratorService;

namespace Concentrator.Plugins.SennHeiser
{
  public class UpdateBarcodes : ConcentratorPlugin
  {
    public override string Name
    {
      get { return "Sennheiser Xls updat barcode Plugin"; }
    }
    //private int vendorID_NL;
    private const int unmappedID = -1;
    private const int languageID = 1;
    private Dictionary<int, string> vendors = new Dictionary<int, string>();

    string Category = string.Empty;
    System.Configuration.Configuration config;

    protected override void Process()
    {
      try
      {
        log.Debug("Start Barcode Import Process");
        config = GetConfiguration();

        vendors.Add(int.Parse(config.AppSettings.Settings["VendorID_NL"].Value), "NL");
        vendors.Add(int.Parse(config.AppSettings.Settings["VendorID_BE"].Value), "BE");

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

          //TODO: Load data options
          //DataLoadOptions options = new DataLoadOptions();
          //options.LoadWith<ProductAttributeGroupMetaData>(x => x.ProductAttributeGroupLabels);
          //options.LoadWith<ProductAttributeGroupMetaData>(x => x.ProductAttributes);
          //options.LoadWith<ProductAttributeMetaData>(x => x.ProductAttributeLabels);
          //options.LoadWith<ProductAttributeMetaData>(x => x.ProductAttributeValues);
          //options.LoadWith<VendorAssortment>(x => x.VendorPrice);
          //options.LoadWith<VendorAssortment>(x => x.VendorStock);
          //options.LoadWith<VendorAssortment>(x => x.VendorProductGroupAssortments);

          //context.LoadOptions = options;
          var barcodeRepo = unit.Scope.Repository<ProductBarcode>();

          int defaultVendorID = vendors.First().Key;
          var currentItem = string.Empty;

          log.Debug("Start fetch vendors");
          var vendor = unit.Scope.Repository<Vendor>().GetSingle(bv => bv.VendorID == defaultVendorID);

          log.Debug("Start fetch brandvendors");
          var brandVendors = unit.Scope.Repository<BrandVendor>().GetAll(b => (b.VendorID == vendor.VendorID || (vendor.ParentVendorID.HasValue && b.VendorID == vendor.ParentVendorID))).ToList();

          log.Debug("Start fetch products");
          var products = unit.Scope.Repository<Product>().GetAllAsQueryable();
          log.Debug("Start fetch attribute groups");
          var productAttributeGroups = unit.Scope.Repository<ProductAttributeGroupMetaData>().GetAll().ToList();

          log.Debug("Start fetch attributes");
          var productAttributes = unit.Scope.Repository<ProductAttributeMetaData>().GetAll(g => g.VendorID == defaultVendorID).ToList();


          #region EAN CODES 310810.xls
          var file = config.AppSettings.Settings["SennheiserBasePath"].Value + "Barcodes\\EAN CODES 310810.xls";
          log.DebugFormat("Start import {0}", file);

          DataSet data = new DataSet();
          using (FileStream str = File.Open(file, FileMode.Open, FileAccess.Read))
          {
            IExcelDataReader excelReader1 = ExcelReaderFactory.CreateBinaryReader(str);
            excelReader1.IsFirstRowAsColumnNames = true;
            data = excelReader1.AsDataSet();

            try
            {
              var itemAttributes4 = (from productData in data.Tables[0].AsEnumerable()
                                     //where productData.Field<double?>(0).HasValue
                                     where !productData.IsNull(0) && productData[0] != null && (from p in products where p.VendorItemNumber == productData[0].ToString().Trim() select p.ProductID).FirstOrDefault() != 0 && productData[2] != null && productData[3] != null
                                     select new
                                     {
                                       VendorItemNumber = productData[0].ToString().Trim(),
                                       EANCode = productData[2].ToString().Trim(),
                                       UPCCode = productData[3].ToString().Trim(),
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
                  if (!product.ProductBarcodes.Any(b => b.Barcode == content.EANCode))
                  {
                    //create ProductBarcode if not exists
                    barcodeRepo.Add(new ProductBarcode
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
                    barcodeRepo.Add(new ProductBarcode
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
          }
          #endregion

          #region EAN SECOM 2011.xls tab1
          file = config.AppSettings.Settings["SennheiserBasePath"].Value + "Barcodes\\EAN SECOM 2011.xls";
          log.DebugFormat("Start import {0}", file);
          data = new DataSet();
          using (FileStream str = File.Open(file, FileMode.Open, FileAccess.Read))
          {
            IExcelDataReader excelReader1 = ExcelReaderFactory.CreateBinaryReader(str);
            data = excelReader1.AsDataSet();

            try
            {
              var itemAttributes3 = (from productData in data.Tables[0].AsEnumerable()
                                     //where int.TryParse(productData.Field<string>(0), out bla)
                                     where !productData.IsNull(0) &&
                                     productData[0] != null && (from p in products where p.VendorItemNumber == productData[0].ToString().Trim() select p.ProductID).FirstOrDefault() != 0 && productData[4] != null && productData[5] != null
                                     select new
                                     {
                                       VendorItemNumber = productData[0].ToString().Trim(),
                                       EANCode = productData[4].ToString().Trim(),
                                       UPCCode = productData[5].ToString().Trim(),
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
          }
          #endregion

          #region EAN SECOM 2011.xls tab2
          file = config.AppSettings.Settings["SennheiserBasePath"].Value + "Barcodes\\EAN SECOM 2011.xls";
          log.DebugFormat("Start import {0}", file);
          data = new DataSet();
          using (FileStream str = File.Open(file, FileMode.Open, FileAccess.Read))
          {
            IExcelDataReader excelReader1 = ExcelReaderFactory.CreateBinaryReader(str);
            data = excelReader1.AsDataSet();

            try
            {
              var itemAttributes3 = (from productData in data.Tables[1].AsEnumerable()
                                     //where int.TryParse(productData.Field<string>(0), out bla)
                                     where !productData.IsNull(0) &&
                                     productData[0] != null && (from p in products where p.VendorItemNumber == productData[0].ToString().Trim() select p.ProductID).FirstOrDefault() != 0 && productData[4] != null && productData[5] != null
                                     select new
                                     {
                                       VendorItemNumber = productData[0].ToString().Trim(),
                                       EANCode = productData[3].ToString().Trim(),
                                       UPCCode = productData[4].ToString().Trim(),
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
          }
          #endregion

          #region Product info 01012010 IERLAND II.xls
          file = config.AppSettings.Settings["SennheiserBasePath"].Value + "Barcodes\\Product info 01012010 IERLAND II.xls";
          log.DebugFormat("Start import {0}", file);
          data = new DataSet();
          using (FileStream str = File.Open(file, FileMode.Open, FileAccess.Read))
          {
            IExcelDataReader excelReader1 = ExcelReaderFactory.CreateBinaryReader(str);
            data = excelReader1.AsDataSet();

            try
            {
              var itemAttributes4 = (from productData in data.Tables[0].AsEnumerable()
                                     //where productData.Field<double?>(0).HasValue
                                     where !productData.IsNull(0) && productData[0] != null && (from p in products where p.VendorItemNumber == productData[0].ToString().Trim() select p.ProductID).FirstOrDefault() != 0 && productData[2] != null && productData[3] != null
                                     select new
                                     {
                                       VendorItemNumber = productData[0].ToString().Trim(),
                                       EANCode = productData[2].ToString().Trim(),
                                       UPCCode = productData[3].ToString().Trim(),
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
          }
          #endregion

          #region Product Info 01012010 IERLAND.xls
          file = config.AppSettings.Settings["SennheiserBasePath"].Value + "Barcodes\\Product Info 01012010 IERLAND.xls";
          log.DebugFormat("Start import {0}", file);
          data = new DataSet();
          using (FileStream str = File.Open(file, FileMode.Open, FileAccess.Read))
          {
            IExcelDataReader excelReader1 = ExcelReaderFactory.CreateBinaryReader(str);
            data = excelReader1.AsDataSet();

            try
            {
              var itemAttributes4 = (from productData in data.Tables[0].AsEnumerable()
                                     //where productData.Field<double?>(0).HasValue
                                     where !productData.IsNull(0) && productData[0] != null && (from p in products where p.VendorItemNumber == productData[0].ToString().Trim() select p.ProductID).FirstOrDefault() != 0 && productData[2] != null && productData[3] != null
                                     select new
                                     {
                                       VendorItemNumber = productData[0].ToString().Trim(),
                                       EANCode = productData[2].ToString().Trim(),
                                       UPCCode = productData[3].ToString().Trim(),
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
          }
          #endregion

          #region Product Info 01012010 KG.xls
          file = config.AppSettings.Settings["SennheiserBasePath"].Value + "Barcodes\\Product Info 01012010 KG.xls";
          log.DebugFormat("Start import {0}", file);
          data = new DataSet();
          using (FileStream str = File.Open(file, FileMode.Open, FileAccess.Read))
          {
            IExcelDataReader excelReader1 = ExcelReaderFactory.CreateBinaryReader(str);
            data = excelReader1.AsDataSet();

            try
            {
              var itemAttributes4 = (from productData in data.Tables[0].AsEnumerable()
                                     //where productData.Field<double?>(0).HasValue
                                     where !productData.IsNull(0) && productData[0] != null && (from p in products where p.VendorItemNumber == productData[0].ToString().Trim() select p.ProductID).FirstOrDefault() != 0 && productData[2] != null && productData[3] != null
                                     select new
                                     {
                                       VendorItemNumber = productData[0].ToString().Trim(),
                                       EANCode = productData[2].ToString().Trim(),
                                       UPCCode = productData[3].ToString().Trim(),
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
          }
          #endregion

          #region Nuemann_EAC_UPC_ART_Codes verwerking
          file = config.AppSettings.Settings["SennheiserBasePath"].Value + "Barcodes\\Nuemann_EAC_UPC_ART_Codes.xls";
          log.DebugFormat("Start import {0}", file);
          data = new DataSet();
          using (FileStream str = File.Open(file, FileMode.Open, FileAccess.Read))
          {
            IExcelDataReader excelReader1 = ExcelReaderFactory.CreateBinaryReader(str);
            data = excelReader1.AsDataSet();
            try
            {
              var itemAttributes2 = (from productData in data.Tables[0].AsEnumerable()
                                     //where productData.Field<double?>(0).HasValue
                                     where !productData.IsNull(0)
                                     &&
                                     productData[0] != null && (from p in products where p.VendorItemNumber.Trim() == productData[0].ToString().Trim() select p.ProductID).FirstOrDefault() != null /*&&  productData[9] != null && productData[11] != null*/
                                     select new
                                     {
                                       VendorItemNumber = productData[0].ToString().Trim(),
                                       EANCode = productData[9].ToString().Trim(),
                                       UPCCode = productData[11].ToString().Trim(),
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
              }

              unit.Save();
            }
            catch (Exception ex)
            {
              log.AuditError("Failed import Neuman barcodes", ex);
            }
          }
          #endregion
        }
      }
      catch (Exception ex)
      {
        log.Error("Error", ex);
      }
      log.Debug("Finish Barcode Import Process");
    }

    private bool ParseDocument(DataSet[] tables)
    {
      using (var unit = GetUnitOfWork())
      {
        //DataLoadOptions options = new DataLoadOptions();
        //options.LoadWith<ProductAttributeGroupMetaData>(x => x.ProductAttributeGroupLabels);
        //options.LoadWith<ProductAttributeGroupMetaData>(x => x.ProductAttributes);
        //options.LoadWith<ProductAttributeMetaData>(x => x.ProductAttributeLabels);
        //options.LoadWith<ProductAttributeMetaData>(x => x.ProductAttributeValues);
        //options.LoadWith<VendorAssortment>(x => x.VendorPrice);
        //options.LoadWith<VendorAssortment>(x => x.VendorStock);
        //options.LoadWith<VendorAssortment>(x => x.VendorProductGroupAssortments);

        //context.LoadOptions = options;

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

          var brandVendors = _brandVendorRepo.GetAll(b => (b.VendorID == vendor.VendorID || (vendor.ParentVendorID.HasValue && b.VendorID == vendor.ParentVendorID))).ToList();
          var currentVendorProductGroups = _productGroupVendorRepo.GetAll(v => v.VendorID == vendor.VendorID).ToList();
          var productGroupVendors = _productGroupVendorRepo.GetAll(g => (g.VendorID == vendor.VendorID || (vendor.ParentVendorID.HasValue && g.VendorID == vendor.VendorID))).ToList();

          var products = _productRepo.GetAll().ToList();
          var medias = _mediaRepo.GetAll().ToList();
          var productDescriptions = _prodDescriptionRepo.GetAll().ToList();

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
                                   productData[0] != null && (from p in products where p.VendorItemNumber.Trim() == productData[0].ToString().Trim() select p.ProductID).FirstOrDefault() != null /*&&  productData[9] != null && productData[11] != null*/
                                   select new
                                   {
                                     VendorItemNumber = productData[0].ToString().Trim(),
                                     EANCode = productData[9].ToString().Trim(),
                                     UPCCode = productData[11].ToString().Trim(),
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
                                     EANCode = productData[4].ToString().Trim(),
                                     UPCCode = productData[5].ToString().Trim(),
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
                                     EANCode = productData[2].ToString().Trim(),
                                     UPCCode = productData[3].ToString().Trim(),
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
