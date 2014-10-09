using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using System.Configuration;
using Concentrator.Objects;
using Concentrator.Objects.ConcentratorService;
using Concentrator.Objects.Models.Connectors;

namespace Concentrator.Plugins.BAS.WebExport
{
  [ConnectorSystem(1)]
  public class ImportAttributes : ConcentratorPlugin
  {
    public override string Name
    {
      get { return "Website attribute processor"; }
    }

    protected override void Process()
    {
      log.DebugFormat("Start Product processing config connecotors");

      foreach (Connector connector in base.Connectors.Where(c => ((ConnectorType)c.ConnectorType).Has(ConnectorType.WebAssortment)))
      {
                

        DateTime start = DateTime.Now;
        log.DebugFormat("Start Attribute Process web import for {0}", connector.Name);
        try
        {
          log.InfoFormat("Start process products and attributes:{0}", start);

          Concentrator.Web.ServiceClient.AssortmentService.AssortmentServiceSoapClient soap = new Concentrator.Web.ServiceClient.AssortmentService.AssortmentServiceSoapClient();
          XDocument products;

#if DEBUG
          if (System.IO.File.Exists(@"c:\products.xml"))
            products = XDocument.Parse(System.IO.File.ReadAllText(@"c:\products.xml"));
          else
          {
#endif
          products = new XDocument(soap.GetAssortment(connector.ConnectorID, null, true));
#if DEBUG
            products.Save(@"C:\products.xml", SaveOptions.DisableFormatting);
          }
#endif
          Processor processor = new Processor(products, log, connector);

          processor.ConcentratorItemNumbers.AddRange(products.Root.Elements("Product").Select(x => int.Parse(x.Attribute("ProductID").Value)));

          //log.Info("Start import brands");
          //processor.ImportBrands();
          //log.Info("Finish import brands");
          //log.Info("Start import productgroups");
          //processor.ImportProductGroups();
          //log.Info("Finish import productgroups");
          //log.Info("Start import Products");
          //processor.ImportProducts();
          //log.Info("Finish import Products");

          //processor.CleanUpProducts();
          //processor.CleanUpProductGroupMapping();

          try
          {
            processor.ProcessAttributes(soap, connector, false, null, true);

          }
          catch (Exception ex)
          {
            log.Error("ProductAttributes error", ex);
          }
        }
        catch (Exception ex)
        {
          log.Fatal(ex);
        }

       
        log.DebugFormat("Finish Attribute Process web import for {0}", connector.Name);
      }
    }
  }
}
