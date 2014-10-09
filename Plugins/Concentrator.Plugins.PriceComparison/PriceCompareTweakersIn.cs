using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Net;
using System.IO;
using Concentrator.Objects;
using System.Globalization;
using Concentrator.Objects.Parse;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.DataAccess.UnitOfWork;

namespace Concentrator.Plugins.PriceComparison
{
  public class PriceCompareTweakersIn : BasePriceComparer<Dictionary<string, string>>
  {
    private const string url = "https://pwm.tweakers.net/pricelist";


    public override string Name
    {
      get { return "Tweaker Price Compare import"; }
    }

    protected override int ConnectorID
    {
      get { return 1; }
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
            WebRequest myReq = WebRequest.Create(url);

            string username = "17";
            string password = "r3G4zxqg";
            string usernamePassword = username + ":" + password;
            CredentialCache mycache = new CredentialCache();
            mycache.Add(new Uri(url), "Basic", new NetworkCredential(username, password));
            myReq.PreAuthenticate = true;
            myReq.AuthenticationLevel = System.Net.Security.AuthenticationLevel.MutualAuthRequested;
            myReq.Credentials = mycache;
            //myReq.Headers.Add("Authorization",
                           //   "Basic " + Convert.ToBase64String(new ASCIIEncoding().GetBytes(usernamePassword)));

            WebResponse wr = myReq.GetResponse();
            var stream = new MemoryStream();
            var buffer = new byte[30 * 1024];
            int read = 0;
            using (var inStream = wr.GetResponseStream())
            {
              do
              {
                read = inStream.Read(buffer, 0, buffer.Length);
                stream.Write(buffer, 0, read);
              } while (read > 0);
            }
            stream.Position = 0;

            this._provider = new LazyCsvParser(stream, ColumnDefinitions.Select(c => c.PropertyDescriptor).ToList(),
                                               true);
          }
          catch (Exception e)
          {
            log.Fatal("Requesting csv from tweakers failed: ", e);
          }
        }
        return this._provider;
      }
    }


    protected override void SyncProducts(IContentRecordProvider<Dictionary<string, string>> parser)
    {
      using (var unit = GetUnitOfWork())
      {
        var _compareRepo = unit.Scope.Repository<ProductCompare>();
        var _assortmentRepo = unit.Scope.Repository<VendorAssortment>();

        var productCompare = _compareRepo.GetAllAsQueryable(c => c.ProductCompareSourceID == 3).ToList();

        foreach (Dictionary<string, string> record in parser)
        {

          string ConnectorCustomItemNumber =
            record.Where(c => c.Key == TweakersProperties.ConnectorCustomItemNumber.ToString()).First().Value.Replace("\"", string.Empty).Trim();
          try
          {
            if (this.ExistingContent.Contains(ConnectorCustomItemNumber))
            {

              ProductCompare product = (from p in productCompare
                                                where
                                                  p.ConnectorCustomItemNumber == ConnectorCustomItemNumber &&
                                                  p.ConnectorID == this.ConnectorID
                                                select p).FirstOrDefault();
              if (product == null)
              {
                product = new ProductCompare();
                product.ConnectorID = this.ConnectorID;
                product.ConnectorCustomItemNumber = ConnectorCustomItemNumber;
                _compareRepo.Add(product);
                productCompare.Add(product);
              }

              product.VendorItemNumber =
                record.Where(c => c.Key == TweakersProperties.ProductId.ToString()).First().Value;
              product.MinPrice = ParseDecimalValue(TweakersProperties.MinPrice.ToString(), record);
              product.MaxPrice = ParseDecimalValue(TweakersProperties.MaxPrice.ToString(), record);
              product.AveragePrice = ParseDecimalValue(TweakersProperties.AveragePrice.ToString(), record);

              product.EAN = record.Where(c => c.Key == TweakersProperties.EAN.ToString()).First().Value.Replace("\"",
                                                                                                                string.
                                                                                                                  Empty);
              product.UPID = record.Where(c => c.Key == TweakersProperties.UPIDs.ToString()).First().Value.Replace(
                "\"", string.Empty);
              product.PriceIndex = ParseDecimalValue(TweakersProperties.PriceIndex.ToString(), record);
              product.SourceProductID =
                record.Where(c => c.Key == TweakersProperties.ProductId.ToString()).First().Value;
              if (product.ProductCompareSourceID == 0)
              {
                product.ProductCompareSourceID = unit.Scope.Repository<ProductCompareSource>().GetSingle(c => c.Source == "Tweakers").ProductCompareSourceID;
              }
              product.LastImport = DateTime.Now;
              
              unit.Save();
              ((IFunctionScope)unit.Scope).Repository().UpdateProductCompare(product.CompareProductID);

              SyncCompetitorPrice("MyCom", record.Where(c => c.Key == TweakersProperties.Price.ToString()).First().Value, product.CompareProductID, unit, ProductCompareSourceID);

              for (int i = 5; i <= 13; i += 2)
              {
                if (!string.IsNullOrEmpty(record.ElementAt(i).Value))
                {
                  SyncCompetitorPrice(record.ElementAt(i).Value, record.ElementAt(i - 1).Value, product.CompareProductID,
                                      unit, ProductCompareSourceID);
                }
              }

              unit.Save();

            }
          }
          catch (Exception e)
          {
            log.Error(
              "Processing price compare for Tweakers item with custom item number " + ConnectorCustomItemNumber +
              " failed: ", e);
          }
        }
      }
    }

    protected override List<PriceCompareProperty> ColumnDefinitions
    {
      get { return ColumnDefitionPropertyUtility.TweakersDefinitions; }
    }

    protected override int ProductCompareSourceID
    {
      get { return 3; }
    }
  }
}
