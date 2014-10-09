using Concentrator.Plugins.Sapph.Models;
using Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace Concentrator.Plugins.Sapph.Repositories
{
  class ExcelSalesPricesRepository
  {
    private string importDirectory;
    public ExcelSalesPricesRepository(string pathToImportDirectory)
    {
      importDirectory = pathToImportDirectory;
    }

    private const int _vendorItemNumberColumnIndex = 1;
    private const int _discountPercentageColumnIndex = 5;

    public List<SalesPriceModel> GetDiscountList()
    {
      if (!Directory.Exists(importDirectory))
        throw new DirectoryNotFoundException("Directory with Excel files not found: " + importDirectory);

      var excelFile = Directory.GetFiles(importDirectory, "*.xlsx").FirstOrDefault();
      if (excelFile == null)
        return null;

      using (FileStream stream = File.Open(excelFile, FileMode.Open, FileAccess.Read))
      {
        using (IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream))
        {
          DataSet result = excelReader.AsDataSet();

          System.Data.DataTable dt = result.Tables[0];

          var list = dt.AsEnumerable().ToList();

          return (from c in dt.AsEnumerable()
                  where c.Field<string>(_vendorItemNumberColumnIndex) != null && c.Field<string>(_discountPercentageColumnIndex) != null
                     && !string.IsNullOrEmpty(c.Field<string>(_vendorItemNumberColumnIndex)) && IsValidDiscount(c.Field<string>(_discountPercentageColumnIndex))
                  select new SalesPriceModel()
                  {
                    VendorItemNumber = c.Field<string>(_vendorItemNumberColumnIndex),
                    DiscountPercentage = ParseDiscountPercentage(c.Field<string>(_discountPercentageColumnIndex))
                  }).ToList();
        }
      }
    }

    private decimal ParseDiscountPercentage(string discountPercentageString)
    {
      decimal discountPercentage = 0M;
      decimal.TryParse(discountPercentageString
        , System.Globalization.NumberStyles.AllowDecimalPoint
        , System.Globalization.CultureInfo.InvariantCulture
        , out discountPercentage);

      return discountPercentage;
    }

    private bool IsValidDiscount(string discountPercentageString)
    {
      return ParseDiscountPercentage(discountPercentageString) > 0 && ParseDiscountPercentage(discountPercentageString) <= 1;
    }
  }
}
