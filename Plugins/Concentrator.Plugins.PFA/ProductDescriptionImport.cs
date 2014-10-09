using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.ConcentratorService;
using System.IO;
using Excel;
using System.Data;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Models.Vendors;

namespace Concentrator.Plugins.PFA
{
  public class ProductDescriptionImport : ConcentratorPlugin
  {

    public override string Name
    {
      get { return "Product description import"; }
    }

    protected override void Process()
    {
      using (var unit = GetUnitOfWork())
      {
        Int32 vendovendorFourtyEight = Int32.Parse(GetConfiguration().AppSettings.Settings["ConcentratorVendorID"].Value);
        Int32 vendorTwo = Int32.Parse(GetConfiguration().AppSettings.Settings["ATVendorID"].Value);
        Int32 language = Int32.Parse(GetConfiguration().AppSettings.Settings["Language"].Value);

        String path = GetConfiguration().AppSettings.Settings["DescriptionPath"].Value;
        Int32 descCounter = 0;

        foreach (String file in Directory.GetFiles(path))
        {
          String fullPath = Path.Combine(path, file);

          using (FileStream stream = File.Open(fullPath, FileMode.Open, FileAccess.Read))
          {
            IExcelDataReader excelReader = ExcelReaderFactory.CreateBinaryReader(stream);
            excelReader.IsFirstRowAsColumnNames = true;

            DataSet result = excelReader.AsDataSet();

            System.Data.DataTable dt = result.Tables[0];

            Int32 rowCounter = 0;
            var allProductDescriptions = unit.Scope.Repository<ProductDescription>().GetAll(c => c.VendorID == 48 && c.LanguageID == 1 && c.Product.SourceVendorID == 2).ToList();
            foreach (DataRow dr in dt.Rows)
            {
              if (!dr.IsNull(0))
              {
                rowCounter++;

                String articleCode = dr.Field<String>(0);

                String description = dr.Field<String>(1);

                log.Info(String.Format("Adding data for row number {0} : {1} {2}", rowCounter, articleCode, description));

                List<VendorAssortment> vendorAssortment = unit.Scope.Repository<VendorAssortment>().GetAll(c => c.VendorID == 2).ToList().Where(c => c.CustomItemNumber.StartsWith(articleCode)).ToList();

                foreach (var item in vendorAssortment)
                {
                  ProductDescription productRepo = allProductDescriptions.FirstOrDefault(x => x.ProductID == item.ProductID && x.VendorID == vendovendorFourtyEight && x.LanguageID == 1);

                  if (productRepo == null)
                  {
                    ProductDescription altRepo = GetUnitOfWork().Scope.Repository<ProductDescription>().GetSingle(x => x.VendorID == vendorTwo && x.LanguageID == 1 && x.ProductID == item.ProductID);

                    ProductDescription prodDescription = new ProductDescription
                    {
                      VendorID = vendovendorFourtyEight,
                      LanguageID = language,
                      ProductID = item.ProductID,
                      LongContentDescription = description,
                      ShortContentDescription = altRepo.Try(c => c.ShortContentDescription, string.Empty)
                    };

                    unit.Scope.Repository<ProductDescription>().Add(prodDescription);
                    allProductDescriptions.Add(prodDescription);
                    descCounter++;
                  }
                  else
                  {
                    productRepo.LongContentDescription = description;
                  }


                }
              }
            }
            unit.Save();
          }

          log.Info(String.Format("Added {0} new descriptions", descCounter));

          String pPath = Path.Combine(path, "Processed");

          if (!Directory.Exists(pPath))
            Directory.CreateDirectory(pPath);

          FileInfo inf = new FileInfo(file);
          String nPath = Path.Combine(pPath, inf.Name);

          inf.MoveTo(nPath);
        }
      }
    }
  }
}
