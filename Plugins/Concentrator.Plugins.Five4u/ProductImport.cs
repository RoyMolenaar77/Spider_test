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
using System.Data;
using System.IO;
using Excel;
using Concentrator.Objects.Vendors.Bulk;
namespace Concentrator.Plugins.Five4u
{
  public class ProductImport : ConcentratorPlugin
  {
    private int vendorID;
    private const int unmappedID = -1;
    private const int languageID = 1;
    private System.Configuration.Configuration config;

    public override string Name
    {
      get { return "Five4u Product Import Plugin"; }
    }

    protected override void Process()
    {
      var config = GetConfiguration();

      if (config.AppSettings.Settings["VendorID"] == null)
        throw new Exception("VendorID not set in config for Five4u ProductImport");

      vendorID = int.Parse(config.AppSettings.Settings["VendorID"].Value.ToString());

      try
      {
        using (var unit = GetUnitOfWork())
        {
          var productXlsFilePath = config.AppSettings.Settings["Five4uBasePath"].Value;

          DataSet[] datas = new DataSet[0];

          using (FileStream stream1 = File.Open(productXlsFilePath, FileMode.Open, FileAccess.Read))
          {
            //1. Reading from a OpenXml Excel file (2007 format; *.xlsx)
            IExcelDataReader excelReader1 = ExcelReaderFactory.CreateOpenXmlReader(stream1);
            excelReader1.IsFirstRowAsColumnNames = true;
            //...
            //2. DataSet - The result of each spreadsheet will be created in the result.Tables
            DataSet result1 = excelReader1.AsDataSet();
            ParseDocuments(unit, result1);
          }
        }
      }
      catch (Exception ex)
      {
        log.AuditError("Error getting Five4u files from Excel", ex);
      }
    }

    private void ParseDocuments(IUnitOfWork unit, DataSet ds)
    {
      int totalProducts = ds.Tables[0].AsEnumerable().Count();
      log.AuditInfo(string.Format("Start Assortment processing for AW import, {0} Products", totalProducts));

      BulkImport(vendorID, ds, null);
    }

    private void BulkImport(int vendorID, DataSet content, int? parentVendorID)
    {
      using (var unit = GetUnitOfWork())
      {
        //var itemProducts = (from d in ds.Tables[0].AsEnumerable()
        //                    select new
        //                    {
        //                      EAN = d.Field<string>("EAN"),
        //                      Brand = d.Field<string>("Brand"),
        //                      Original_Partnumber = d.Field<string>("Original Partnumber"),
        //                      Distributor_Partnumber = d.Field<string>("Distributor Partnumber"),
        //                      Product_Name = d.Field<string>("Product Name"),
        //                      Groep_L1 = d.Field<string>("Groep_L1"),
        //                      Groep_L2 = d.Field<string>("Groep_L2"),
        //                      Groep_L3 = d.Field<string>("Groep_L3"),


        //                    }).ToList();

        var dataList = (from d in content.Tables[0].AsEnumerable()
                        let defaultVendorID = parentVendorID.HasValue ? parentVendorID.Value : vendorID
                        select new Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorAssortmentItem
                        {
                          #region BrandVendor
                          BrandVendors = new List<Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportBrand>()
                          {
                            new Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportBrand(){
                              Name = SetDataSetValue("Brand", d),
                              VendorBrandCode = SetDataSetValue("Brand", d),
                              VendorID = defaultVendorID
                            }
                          },
                          #endregion
                          #region GeneralProductInfo
                          VendorProduct = new VendorAssortmentBulk.VendorProduct
                          {
                            CustomItemNumber = SetDataSetValue("Distributor Partnumber", d),                            
                            VendorItemNumber = SetDataSetValue("Original Partnumber", d),
                            ShortDescription = SetDataSetValue("Original Partnumber", d),
                            LongDescription = null,
                            LineType = null,
                            LedgerClass = null,
                            ProductDesk = null,
                            ExtendedCatalog = null,
                            VendorID = vendorID,
                            DefaultVendorID = defaultVendorID,
                            VendorBrandCode = SetDataSetValue("Brand", d),
                            Barcode = SetDataSetValue("EAN", d),
                            VendorProductGroupCode1 = SetDataSetValue("Groep_L1", d),
                            VendorProductGroupCodeName1 = string.Empty,
                            VendorProductGroupCode2 = SetDataSetValue("Groep_L2", d),
                            VendorProductGroupCodeName2 = string.Empty,
                            VendorProductGroupCode3 = SetDataSetValue("Groep_L3", d),
                            VendorProductGroupCodeName3 = string.Empty,
                          },
                          #endregion
                          #region RelatedProducts
                          RelatedProducts = new List<VendorAssortmentBulk.VendorImportRelatedProduct>(),
                          #endregion
                          #region Attribures
                          VendorImportAttributeValues = new List<VendorAssortmentBulk.VendorImportAttributeValue>(),
                          #endregion
                          #region Prices
                          VendorImportPrices = new List<Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportPrice>()
                          {
                            new Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportPrice(){
                              VendorID = vendorID,
                              DefaultVendorID = vendorID,
                              CustomItemNumber = SetDataSetValue("Distributor Partnumber", d),
                              Price = "0",
                              CostPrice = "0",
                              TaxRate = "19",
                              MinimumQuantity = 0,
                              CommercialStatus = "S"
                            }
                          },

                          #endregion
                          #region Stock
                          VendorImportStocks = new List<Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportStock>()
                          {
                            new Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportStock(){
                              VendorID = vendorID,
                              DefaultVendorID = vendorID,
                              CustomItemNumber = SetDataSetValue("Distributor Partnumber", d),
                              QuantityOnHand = 0,
                              StockType = "Assortment",
                              StockStatus = "S"
                            }
                          },

                          #endregion
                        });

        log.Debug("dataset to object");

        using (var vendorAssortmentBulk = new VendorAssortmentBulk(dataList, vendorID, parentVendorID))
        {
          vendorAssortmentBulk.Init(unit.Context);
          vendorAssortmentBulk.Sync(unit.Context);
        }
      }
    }

    private string SetDataSetValue(string name, DataRow row)
    {
      try
      {
        return !string.IsNullOrEmpty(row.Field<object>(name).ToString()) ? row.Field<object>(name).ToString().Trim() : string.Empty;
      }
      catch
      {
        return string.Empty;
      }
    }
  }
}
