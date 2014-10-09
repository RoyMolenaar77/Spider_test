using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.ConcentratorService;
using System.IO;
using System.Net;

namespace Concentrator.Plugins.SennHeiser
{
  class ExportPDF : ConcentratorPlugin
  {
    public override string Name
    {
      get { return "Sennheiser PDF export"; }
    }

    protected override void Process()
    {
      var config = GetConfiguration();

      string outputPath = config.AppSettings.Settings["OutPutPath"].Value;
      string path = Path.Combine(outputPath, "PriceList.pdf");
      string url = config.AppSettings.Settings["SennheiserPriceList"].Value;
      
      #region Brand PDF
      using (var unit = GetUnitOfWork())
      {
        var brands = unit.Scope.Repository<Concentrator.Objects.Models.Contents.Content>().GetAll(cp => cp.ConnectorID == 2).Select(c => c.Product.Brand).OrderByDescending(x => x.Products.Count()).ToList().Distinct(new BrandEqualityComparer());

        brands.ForEach((brand, idx) =>
        {
          try
          {
            

            using (var wc = new MyWebClient())
            {
              log.InfoFormat("Start generate PDF for {0}", brand.Name);
              var brandPath = Path.Combine(outputPath, brand.BrandID + "_" + brand.Name + ".pdf");

              if (File.Exists(brandPath))
                File.Delete(brandPath);

              wc.Headers.Add(HttpRequestHeader.KeepAlive, "TRUE");
              wc.DownloadFile(string.Format(config.AppSettings.Settings["SennheiserBrandPriceList"].Value, brand.BrandID), brandPath);
              log.InfoFormat("Finish generate PDF for {0}", brand.Name);
            }
          }
          catch (Exception ex)
          {
            log.Error("Error generation Brand PDF for" + brand.BrandID, ex);
          }
        });
      }
      #endregion

      #region Full pdf
      try
      {
        if (File.Exists(path))
        {
          var latest = Path.Combine(outputPath, "PriceList_Latest.pdf");
          if (File.Exists(latest))
          {
            File.Delete(latest);
            File.Move(path, latest);
          }
          
          File.Delete(path);
        }

        using (var wc = new MyWebClient())
        {
          log.Info("Start generate Full PDF");
          wc.Headers.Add(HttpRequestHeader.KeepAlive, "TRUE");
          wc.DownloadFile(config.AppSettings.Settings["SennheiserPriceList"].Value, path);
          log.Info("Finish generate Full PDF");
        }
      }
      catch (Exception ex)
      {
        log.Error("Error generation full PDF", ex);
      }
      #endregion


    }
  }

  public class MyWebClient : WebClient
  {
    public MyWebClient()
    {

    }

    protected override WebRequest GetWebRequest(Uri uri)
    {
      WebRequest w = base.GetWebRequest(uri);
      w.Timeout = int.Parse(new TimeSpan(0, 30, 0).TotalMilliseconds.ToString());
      return w;
    }
  }
}
