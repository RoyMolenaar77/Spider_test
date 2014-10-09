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
  [ConnectorSystem(3)]
  public class ImportAuctionAttributes : ConcentratorPlugin
  {
    public override string Name
    {
      get { return "Website auction attribute processor"; }
    }

    protected override void Process()
    {
      log.DebugFormat("Start auction attribute processing config connecotors");

      foreach (Connector connector in base.Connectors.Where(c => ((ConnectorType)c.ConnectorType).Has(ConnectorType.WebAssortment)))
      {

        log.DebugFormat("Start auction attribute Process web import for {0}", connector.Name);
        try
        {
          
          DateTime start = DateTime.Now;
          log.InfoFormat("Start process auction attributes:{0}", start);

          Concentrator.Web.ServiceClient.AssortmentService.AssortmentServiceSoapClient soap = new Concentrator.Web.ServiceClient.AssortmentService.AssortmentServiceSoapClient();
          XDocument products;

          products = new XDocument(soap.GetAssortment(connector.ConnectorID, null, false));

          Processor processor = new Processor(products, log, connector);

          processor.ConcentratorItemNumbers.AddRange(products.Root.Elements("Product").Select(x => int.Parse(x.Attribute("ProductID").Value)));

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
