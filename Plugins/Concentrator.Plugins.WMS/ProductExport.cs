using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.ConcentratorService;
using Concentrator.Web.ServiceClient.AssortmentService;
using System.Xml.Linq;
using System.IO;
using Concentrator.Web.Services;

namespace Concentrator.Plugins.WMS
{
  public class ProductExport : ConcentratorPlugin
  {
    public override string Name
    {
      get { return "WMS Product export"; }
    }

    protected override void Process()
    {
      try
      {
        AssortmentService soap = new AssortmentService();

        log.Info("Start product export WMS");
        XDocument products;
        products = XDocument.Parse(soap.GetFullConcentratorProductsByVendor(1));

        var config = GetConfiguration();

        string path = config.AppSettings.Settings["FtpWms"].Value;
        string file = Path.Combine(path, string.Format("{0}_{1}", DateTime.Now.ToString("yyyyMMddHHmm"), "ConcentratorProducts.xml"));

        if (File.Exists(file))
          File.Delete(file);

        products.Save(file, SaveOptions.DisableFormatting);
        log.Info("Finish product export WMS");

      }
      catch (Exception ex)
      {
        log.Error("Error product export WMS", ex);
      }
    }
  }
}
