using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.ConcentratorService;
using Concentrator.Objects.DataAccess.UnitOfWork;
using System.Data;
using System.IO;
using Excel;
using Concentrator.Objects.Models.Vendors;
using System.Text.RegularExpressions;
using System.Globalization;
using Concentrator.Objects.Logic;
using Concentrator.Objects.Models.Attributes;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Enumerations;

namespace Concentrator.Plugins.SennHeiser
{
  public class PriceImport : ConcentratorPlugin
  {
    private decimal _taxRate = 0.00m;
    private decimal _taxCalculation = 0.00m;

    public override string Name
    {
      get { return "Sennheiser Price Import Plugin"; }
    }

    protected override void Process()
    {
      int vendorID_NL;
      int vendorID_BE;
      int vendorID_USA;

      var config = GetConfiguration();

      if (config.AppSettings.Settings["SennheiserVendorID_NL"] == null)
        throw new Exception("SennheiserVendorID_NL not set in config for Sennheiser Price Import");

      if (config.AppSettings.Settings["SennheiserVendorID_BE"] == null)
        throw new Exception("SennheiserVendorID_BE not set in config for Sennheiser Price Import");

      if (config.AppSettings.Settings["SennheiserVendorID_USA"] == null)
        throw new Exception("SennheiserVendorID_UAS not set in config for Sennheiser Price Import");

      vendorID_NL = int.Parse(config.AppSettings.Settings["SennheiserVendorID_NL"].Value);
      vendorID_BE = int.Parse(config.AppSettings.Settings["SennheiserVendorID_BE"].Value);
      vendorID_USA = int.Parse(config.AppSettings.Settings["SennheiserVendorID_USA"].Value);

      try
      {
        using (var unit = GetUnitOfWork())
        {
          var path = config.AppSettings.Settings["SennheiserBasePath"].Value;

          if (!Directory.Exists(path))
            throw new NullReferenceException("The directory doesn't exist for SennheiserBasePath");

          #region NL
          log.Debug("Check for NL pricefile");
          var pricePath = Path.Combine(path, config.AppSettings.Settings["SennheiserPriceFilesPath"].Value);
          //Handling NL Excel file
          config.AppSettings.Settings["ExcelFileNL"].Value.Split(',').ForEach((fileNameNL, idx) =>
          {
            //string fileNameNL = config.AppSettings.Settings["ExcelFileNL"].Value;
            var excelFilePath_NL = Path.Combine(path, fileNameNL);

            if (File.Exists(excelFilePath_NL))
            {
              FileStream stream_NL = File.Open(excelFilePath_NL, FileMode.Open, FileAccess.Read);
              IExcelDataReader excelReader_NL = null;

              var extensionNL = Path.GetExtension(excelFilePath_NL);

              if (extensionNL == ".xls")
              {
                excelReader_NL = ExcelReaderFactory.CreateBinaryReader(stream_NL);
                _taxRate = 19.00m;
                _taxCalculation = 1.19m;
              }
              else
              {
                excelReader_NL = ExcelReaderFactory.CreateOpenXmlReader(stream_NL);
                _taxRate = 19.00m;
                _taxCalculation = 1.19m;
              }

              DataSet result_NL = excelReader_NL.AsDataSet();
              ParseDocuments(unit, vendorID_NL, result_NL, true);
              log.Debug("Save NL prices");
              unit.Save();
              stream_NL.Close();

              var destpath = Path.Combine(pricePath, DateTime.Now.ToString("yyyyMMddmmss"));
              if (!Directory.Exists(destpath))
                Directory.CreateDirectory(destpath);

              File.Move(excelFilePath_NL, Path.Combine(destpath, fileNameNL));
            }
          });
          #endregion

          #region BE
          log.Debug("Check for BE pricefile");
          // Handling BE Excel file
          config.AppSettings.Settings["ExcelFileBE"].Value.Split(',').ForEach((fileNameBE, idx) =>
         {
           //string fileNameBE = config.AppSettings.Settings["ExcelFileBE"].Value;
           var excelFilePath_BE = Path.Combine(path, fileNameBE);

           if (File.Exists(excelFilePath_BE))
           {
             FileStream stream_BE = File.Open(excelFilePath_BE, FileMode.Open, FileAccess.Read);
             IExcelDataReader excelReader_BE = null;

             var extensionBE = Path.GetExtension(excelFilePath_BE);

             if (extensionBE == ".xls")
             {
               excelReader_BE = ExcelReaderFactory.CreateBinaryReader(stream_BE);
               _taxRate = 21.00m;
               _taxCalculation = 1.21m;
             }
             else
             {
               excelReader_BE = ExcelReaderFactory.CreateOpenXmlReader(stream_BE);
               _taxRate = 21.00m;
               _taxCalculation = 1.21m;
             }

             DataSet result_BE = excelReader_BE.AsDataSet();
             ParseDocuments(unit, vendorID_BE, result_BE, false);
             log.Debug("Save BE prices");
             unit.Save();
             stream_BE.Close();

             var destpath = Path.Combine(pricePath, DateTime.Now.ToString("yyyyMMddmmss"));
             if (!Directory.Exists(destpath))
               Directory.CreateDirectory(destpath);

             File.Move(excelFilePath_BE, Path.Combine(destpath, fileNameBE));
           }
         });
          #endregion

          #region USA
          log.Debug("Check for USA pricefile");
          // Handling BE Excel file
          config.AppSettings.Settings["ExcelFileUSA"].Value.Split(',').ForEach((fileNameUSA, idx) =>
        {
          // string fileNameUSA = config.AppSettings.Settings["ExcelFileUSA"].Value;
          var excelFilePath_USA = Path.Combine(path, fileNameUSA);

          if (File.Exists(excelFilePath_USA))
          {
            FileStream stream_USA = File.Open(excelFilePath_USA, FileMode.Open, FileAccess.Read);
            IExcelDataReader excelReader_USA = null;

            var extensionUSA = Path.GetExtension(excelFilePath_USA);

            if (extensionUSA == ".xls")
            {
              excelReader_USA = ExcelReaderFactory.CreateBinaryReader(stream_USA);
              _taxRate = 0;
              _taxCalculation = 0;
            }
            else
            {
              excelReader_USA = ExcelReaderFactory.CreateOpenXmlReader(stream_USA);
              _taxRate = 0;
              _taxCalculation = 0;
            }

            DataSet result_USA = excelReader_USA.AsDataSet();
            ParseDocuments(unit, vendorID_USA, result_USA, false);
            log.Debug("Save USA prices");
            unit.Save();
            stream_USA.Close();

            var destpath = Path.Combine(pricePath, DateTime.Now.ToString("yyyyMMddmmss"));
            if (!Directory.Exists(destpath))
              Directory.CreateDirectory(destpath);


            File.Move(excelFilePath_USA, Path.Combine(destpath, fileNameUSA));
          }
        });
          #endregion
        }
      }
      catch (Exception e)
      {
        log.AuditError("Something went wrong during the price import", e);
      }
    }

    private void ParseDocuments(IUnitOfWork unit, int vendorID, DataSet excel, bool firstRun)
    {
      var content = (from p in excel.Tables[0].AsEnumerable().Skip(1)
                     let vpn = p.Field<string>(0)
                     select new
                     {
                       UnpaddedVendorItemNumber = vpn,
                       PaddedVendorItemNumber = vpn.Length <= 6 ? vpn.PadLeft(6, '0') : vpn,
                       Price = p.Field<String>(3),
                       PriceGroup = p.Field<String>(1),
                       EAN = p.Field<String>(2)
                     }).ToList();

      int counter = 0;
      int total = content.Count();
      int totalNumberOfProductsToProcess = total;
      log.InfoFormat("Start import {0} products", total);
      var attribute = unit.Scope.Repository<ProductAttributeMetaData>().GetSingle(x => x.AttributeCode == "PriceGroup");

      foreach (var item in content)
      {
        try
        {
          if (counter == 50)
          {
            counter = 0;
            log.InfoFormat("Still need to process {0} of {1}; {2} done;", totalNumberOfProductsToProcess, total, total - totalNumberOfProductsToProcess);
          }
          totalNumberOfProductsToProcess--;
          counter++;

          decimal dec = 0;
          decimal.TryParse(item.Price, NumberStyles.Any, CultureInfo.InvariantCulture, out dec);

          if (dec <= 0)
            continue;

          if (item.Price == String.Empty)
            continue;

          var _vendorAssortment = unit.Scope.Repository<VendorAssortment>().GetSingle(x => (x.Product.VendorItemNumber == item.PaddedVendorItemNumber || x.Product.VendorItemNumber == item.UnpaddedVendorItemNumber) && x.VendorID == vendorID);

          if (_vendorAssortment == null)
          {
            _vendorAssortment = unit.Scope.Repository<VendorAssortment>().GetSingle(x => (x.Product.VendorItemNumber == item.PaddedVendorItemNumber || x.Product.VendorItemNumber == item.UnpaddedVendorItemNumber));
            if (_vendorAssortment == null)
              continue;
            else
            {
              var va = new VendorAssortment()
              {
                VendorID = vendorID,
                ProductID = _vendorAssortment.ProductID,
                CustomItemNumber = _vendorAssortment.CustomItemNumber,
                ShortDescription = _vendorAssortment.ShortDescription,
                LongDescription = _vendorAssortment.LongDescription,
                IsActive = false,
                ProductGroupVendors = _vendorAssortment.ProductGroupVendors
              };
              unit.Scope.Repository<VendorAssortment>().Add(va);
              unit.Save();
              _vendorAssortment = va;
            }
          }

          var _vendorPrice = unit.Scope.Repository<VendorPrice>().GetSingle(x => x.VendorAssortmentID == _vendorAssortment.VendorAssortmentID);

          // Remove trailing zeros and Parsing
          //String currencyFormat = String.Format("{0:C4}", Decimal.Parse(item.Price.Replace('.', ',')));
          var costPrice = dec; //Decimal.Parse(currencyFormat, NumberStyles.Currency);
          var priceWithTax = (costPrice * _taxCalculation);

          if (_vendorPrice == null)
          {
            VendorPrice price = new VendorPrice()
            {
              VendorAssortmentID = _vendorAssortment.VendorAssortmentID,
              Price = priceWithTax,
              MinimumQuantity = 0,
              CostPrice = costPrice,
              TaxRate = _taxRate
            };

            unit.Scope.Repository<VendorPrice>().Add(price);
          }
          else
          {
            _vendorPrice.CostPrice = costPrice;
            _vendorPrice.TaxRate = _taxRate;
            _vendorPrice.Price = null;
          }

          var _vendorStock = unit.Scope.Repository<VendorStock>().GetSingle(x => x.ProductID == _vendorAssortment.ProductID && x.VendorID == vendorID);

          if (_vendorStock == null)
          {
            VendorStock stock = new VendorStock()
            {
              ProductID = _vendorAssortment.ProductID,
              VendorID = vendorID,
              QuantityOnHand = 0,
              VendorStockTypeID = 1
            };

            unit.Scope.Repository<VendorStock>().Add(stock);
          }

          if (firstRun)
          {
            #region Attribute
            if (!string.IsNullOrEmpty(item.PriceGroup))
            {


              if (attribute == null)
                log.AuditCritical("PriceGroup attribute Removed");
              else
              {
                var attributevalue = attribute.ProductAttributeValues.FirstOrDefault(x => x.ProductID == _vendorAssortment.ProductID);

                if (attributevalue == null)
                {
                  attributevalue = new ProductAttributeValue()
                  {
                    LanguageID = 1,
                    AttributeID = attribute.AttributeID,
                    ProductID = _vendorAssortment.ProductID
                  };
                  unit.Scope.Repository<ProductAttributeValue>().Add(attributevalue);
                }
                attributevalue.Value = item.PriceGroup;
              }
            }
            #endregion

            //#region Barcode
            //var productBarcode = unit.Scope.Repository<ProductBarcode>().GetSingle(x => x.ProductID == _vendorAssortment.ProductID && x.BarcodeType == 1);

            //if (productBarcode == null)
            //{
            //  productBarcode = new ProductBarcode()
            //  {
            //    ProductID = _vendorAssortment.ProductID,
            //    BarcodeType = (int)BarcodeTypes.EAN,
            //    VendorID = vendorID
            //  };
            //  unit.Scope.Repository<ProductBarcode>().Add(productBarcode);
            //}
            //productBarcode.Barcode = item.EAN;

            //#endregion
          }
          unit.Save();
        }
        catch (Exception ex)
        {
          log.AuditError("Error update price", ex);
        }
      }
    }
  }



}
