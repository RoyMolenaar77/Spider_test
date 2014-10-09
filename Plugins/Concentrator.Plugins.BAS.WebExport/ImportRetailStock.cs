using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects;
using System.Configuration;
using System.Xml.Linq;
using Concentrator.Web.ServiceClient.AssortmentService;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.ConcentratorService;

namespace Concentrator.Plugins.BAS.WebExport
{
  public class ImportRetailStock : ConcentratorPlugin
  {
    public override string Name
    {
      get { return "Website Retail Stock Export Plugin"; }
    }

    protected override void Process()
    {
      log.DebugFormat("Start Retail Stock Export processing Connectors");

      try
      {
        foreach (Connector connector in Connectors.Where(x => ((ConnectorType)x.ConnectorType).Has(ConnectorType.RetailStock)))
        {
          log.InfoFormat("Start Retail Stock Export for {0}", connector.Name);

          DateTime start = DateTime.Now;
          log.InfoFormat("Start process stock:{0}", start);
          AssortmentServiceSoapClient soap = new AssortmentServiceSoapClient();
          XDocument products = new XDocument(soap.GetStockAssortment(connector.ConnectorID, null));

          Processor process = new Processor(products, log, connector);
          process.ImportRetailStock();

          log.InfoFormat("Finish Retail Stock Export: {0} duration {1} minutes", DateTime.Now, TimeSpan.FromMilliseconds(DateTime.Compare(DateTime.Now, start)).TotalMinutes);
        }
      }
      catch (Exception ex)
      {
        log.Error("Error import retail stock", ex);
      }
    }
  }
}
