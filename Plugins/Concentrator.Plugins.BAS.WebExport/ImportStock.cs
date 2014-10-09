using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects;
using System.Configuration;
using System.Xml.Linq;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.ConcentratorService;

namespace Concentrator.Plugins.BAS.WebExport
{
  [ConnectorSystem(1)]
  public class ImportStock : ConcentratorPlugin
  {
    public override string Name
    {
      get { return "Website Stock Export Plugin"; }
    }

    protected override void Process()
    {
      try
      {
        foreach (Connector connector in Connectors.Where(x => ((ConnectorType)x.ConnectorType).Has(ConnectorType.WebAssortment)))
        {
          log.DebugFormat("Start {0} for {1}", Name, connector.Name);
          DateTime start = DateTime.Now;
          XDocument products;

          Concentrator.Web.ServiceClient.AssortmentService.AssortmentServiceSoapClient soap = new Concentrator.Web.ServiceClient.AssortmentService.AssortmentServiceSoapClient();
          products = new XDocument(soap.GetStockAssortment(connector.ConnectorID, null));

          Processor process = new Processor(products, log, connector);
          process.ImportStock();
        }
      }
      catch (Exception ex)
      {
        log.Error("Error import stock", ex);
      }
    }
  }
}
