using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;
using Concentrator.Objects;
using System.Globalization;
using System.Net;
using Concentrator.Objects.Parse;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.DataAccess.UnitOfWork;

namespace Concentrator.Plugins.PriceComparison
{
  public class PriceCompareIceCatIn : BasePriceComparer<Dictionary<string, string>>
  {
    private DataTable CompetitorCatalog;
    private const string url = "ftp://bc_report:8Cr3p0rT@ftp.iceshop.nl/bc_report.csv";

    public override string Name
    {
      get { return "IceCat Price Compare import"; }
    }

    private IContentRecordProvider<Dictionary<string, string>> _provider;
    protected override IContentRecordProvider<Dictionary<string, string>> Provider
    {
      get
      {
        if (this._provider == null)
        {
          try
          {
            FtpWebRequest request = (FtpWebRequest)FtpWebRequest.Create(url);

            FtpWebResponse response = (FtpWebResponse)request.GetResponse();

            var stream = new MemoryStream();
            var buffer = new byte[30 * 1024];
            int read = 0;

            using (var inStream = response.GetResponseStream())
            {

              do
              {
                read = inStream.Read(buffer, 0, buffer.Length);
                stream.Write(buffer, 0, read);
              } while (read > 0);
            }
            stream.Position = 0;

            var prov = new LazyCsvParser(stream, ColumnDefinitions.Select(c => c.PropertyDescriptor).ToList(), true);
            prov.ColumnSplitter = line => line.Split('\t');
            this._provider = prov;
          }
          catch (Exception e)
          {
            log.Fatal("Requesting csv from IceCat failed: ", e);
          }
        }
        return this._provider;
      }
    }

    protected override int ConnectorID
    {
      get { return 1; }
    }

    private List<PriceCompareProperty> _columnDefinitions;
    protected override List<PriceCompareProperty> ColumnDefinitions
    {
      get
      {
        if (this._columnDefinitions == null)
          this._columnDefinitions = ColumnDefitionPropertyUtility.IceCatDefinitions;
        return this._columnDefinitions;
      }
    }

    protected override void SyncProducts(IContentRecordProvider<Dictionary<string, string>> parser)
    {

      using (var unit = GetUnitOfWork())
      {
        var _comparRepo = unit.Scope.Repository<ProductCompare>();
        foreach (Dictionary<string, string> record in parser)
        {
          string ConnectorCustomItemNumber =
             record.Where(c => c.Key == IceCatProperties.YourProductCode.ToString()).First().Value.Replace("\"",
                                                                                                           string.Empty)
               .Trim();
          try
          {


            if (this.ExistingContent.Contains((ConnectorCustomItemNumber)))
            {
              var product = (from p in _comparRepo.GetAllAsQueryable(c => c.ProductCompareSource.Source == "IceCat")
                             where
                               p.ConnectorCustomItemNumber == ConnectorCustomItemNumber &&
                               p.ConnectorID == this.ConnectorID
                             select p).FirstOrDefault();
              if (product == null)
              {
                product = new ProductCompare();
                product.ConnectorID = this.ConnectorID;
                product.ConnectorCustomItemNumber = ConnectorCustomItemNumber;
                product.ProductCompareSource = (from p in unit.Scope.Repository<ProductCompareSource>().GetAllAsQueryable(c => c.Source == "IceCat")
                                                  select p).FirstOrDefault();
              
                _comparRepo.Add(product);
              }

              product.VendorItemNumber =
                record.Where(c => c.Key == IceCatProperties.VendorProductCode.ToString()).First().Value;
              product.MinPrice = ParseDecimalValue(IceCatProperties.MinPrice.ToString(), record);
              product.MaxPrice = ParseDecimalValue(IceCatProperties.MaxPrice.ToString(), record);
              product.PriceGroup1Percentage = ParseDecimalValue(IceCatProperties.PriceGroup1.ToString(), record);
              product.PriceGroup2Percentage = ParseDecimalValue(IceCatProperties.PriceGroup2.ToString(), record);
              product.PriceGroup3Percentage = ParseDecimalValue(IceCatProperties.PriceGroup3.ToString(), record);
              product.PriceGroup4Percentage = ParseDecimalValue(IceCatProperties.PriceGroup4.ToString(), record);
              product.PriceGroup5Percentage = ParseDecimalValue(IceCatProperties.PriceGroup5.ToString(), record);
              product.TotalSales = ParseDecimalValue(IceCatProperties.TotalSales.ToString(), record);
              product.MinStock = ParseIntValue(IceCatProperties.MinStock.ToString(), record).Value;
              product.MaxStock = ParseIntValue(IceCatProperties.MaxStock.ToString(), record).Value;
              product.Popularity = ParseDecimalValue(IceCatProperties.IceCatPopularity.ToString(), record);

              product.LastImport = DateTime.Now;

              unit.Save();

              ((IFunctionScope)unit.Scope).Repository().UpdateProductCompare(product.CompareProductID);
            }
          }
          catch (Exception e)
          {
            log.Error(
              "Processing price compare for Ice cat item with custom item number " + ConnectorCustomItemNumber +
              " failed: ", e);
          }
        }
      }
      #region commented

      //using (ConcentratorDataContext context = new ConcentratorDataContext())
      //{
      //  foreach (Dictionary<string, string> record in parser)
      //  {
      //    #region notabstracted
      //    string ConnectorCustomItemNumber =
      //      record.Where(c => c.Key == IceCatProperties.YourProductCode.ToString()).First().Value.Replace("\"", string.Empty).Trim();
      //    #endregion

      //    if (this.ExistingContent.Contains(ConnectorCustomItemNumber))
      //    {
      //      #region Abstracted
      //      ProductCompare product = (from p in context.ProductCompares
      //                                where
      //                                  p.ConnectorCustomItemNumber == ConnectorCustomItemNumber && p.ConnectorID ==
      //                                                                                              this.ConnectorID &&
      //                                  p.ProductCompareSource.Source == this.Source.ToString()
      //                                select p).FirstOrDefault();

      //      if (product == null)
      //      {
      //        product = new ProductCompare();
      //        product.ProductCompareSource =
      //          (from s in context.ProductCompareSources where s.Source == this.Source.ToString() select s).FirstOrDefault();
      //        context.ProductCompares.InsertOnSubmit(product);
      //      }


      //      product.ConnectorID = this.ConnectorID;
      //      product.ConnectorCustomItemNumber = ConnectorCustomItemNumber;
      //      #endregion

      //      #region not abstracted
      //      product.VendorItemNumber =
      //        record.Where(c => c.Key == IceCatProperties.VendorProductCode.ToString()).First().Value;
      //      product.MinPrice =
      //        decimal.Parse(record.Where(c => c.Key == IceCatProperties.MinPrice.ToString()).First().Value,
      //                      NumberStyles.Any, this.Cultures);
      //      product.MaxPrice =
      //        decimal.Parse(record.Where(c => c.Key == IceCatProperties.MaxPrice.ToString()).First().Value,
      //                      NumberStyles.Any, this.Cultures);
      //      #endregion

      //      context.SubmitChanges();

      //      #region Abstracted
      //      List<string> DynamicProperties = this.ColumnDefinitions.Where(c => c.isDynamic).ToList().Select(c => c.PropertyDescriptor).ToList();
      //      foreach (string s in DynamicProperties)
      //      {
      //        //fetch property
      //        ProductCompareProperty property = (from p in context.ProductCompareProperties
      //                                           where p.PropertyDescriptor == s
      //                                                 &&
      //                                                 p.ProductCompareSourceID ==
      //                                                 (from c in context.ProductCompareSources
      //                                                  where c.Source == this.Source.ToString()
      //                                                  select c).FirstOrDefault().ProductCompareSourceID
      //                                           select p).FirstOrDefault();

      //        //check value of property
      //        ProductComparePropertyValue propertyValue =
      //          (from v in property.ProductComparePropertyValues
      //           where v.CompareProductID == product.CompareProductID
      //           select v).FirstOrDefault();


      //        if (propertyValue == null)
      //        {
      //          propertyValue = new ProductComparePropertyValue()
      //          {

      //            CompareProductID = product.CompareProductID,
      //            PropertyID = property.PropertyID
      //          };
      //          context.ProductComparePropertyValues.InsertOnSubmit(propertyValue);
      //        }

      //        propertyValue.Value = record.Where(c => c.Key == s).First().Value;
      //      }
      //      context.SubmitChanges();
      //      #endregion
      //    }
      //  }
      //} 
      #endregion
    }

    protected override CultureInfo Cultures
    {
      get
      {
        return new CultureInfo("nl-NL");
      }
    }

    protected override int ProductCompareSourceID
    {
      get { return 2; }
    }
  }
}
