using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Configuration;
using Concentrator.Objects;
using Concentrator.Objects.ConcentratorService;
using System.Xml.Linq;
using Concentrator.Web.ServiceClient.AssortmentService;
using Concentrator.Objects.Models.Connectors;

namespace Concentrator.Plugins.BAS.WebExport
{
  [ConnectorSystem(1)]
  public class ImportPrices : ConcentratorPlugin
  {
    public override string Name
    {
      get { return "Website Price Export Plugin"; }
    }

    protected override void Process()
    {
      foreach (Connector connector in Connectors.Where(x => ((ConnectorType)x.ConnectorType).Has(ConnectorType.WebAssortment)))
      {
        log.DebugFormat("Start Process price import for {0}", connector.Name);
        try
        {
          DateTime start = DateTime.Now;
          log.DebugFormat("Start process products:{0}", start);
          AssortmentServiceSoapClient soap = new AssortmentServiceSoapClient();

          XDocument products;

          products = new XDocument(soap.GetPriceAssortment(connector.ConnectorID, null, null));

          Processor processor = new Processor(products, log, connector);


          log.Debug("Start import ProductPrice");
          bool errorImportProduct = processor.ImportPrice();
          log.Debug("Finished import ProductPrice");

          if (!errorImportProduct)
          {
            processor.CleanUpProducts();
          }
        }
        catch (Exception ex)
        {
          log.Error("Error import ProcessProducts", ex);
        }
        
        log.DebugFormat("Finish Process price import for {0}", connector.Name);

      }
    }
  }
}
