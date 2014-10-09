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
  public class ImportProductGropups : ConcentratorPlugin
  {
    public override string Name
    {
      get { return "Website Product Groups Export Plugin"; }
    }

    protected override void Process()
    {


      foreach (Connector connector in Connectors.Where(x => ((ConnectorType)x.ConnectorType).Has(ConnectorType.WebAssortment)))
      {
#if DEBUG
        if (connector.ConnectorID != 1)
          continue;
#endif
        log.DebugFormat("Start Process web productgroup import for {0}", connector.Name);
        DateTime start = DateTime.Now;
        try
        {
          AssortmentServiceSoapClient soap = new AssortmentServiceSoapClient();

          XDocument products;

          products = new XDocument(soap.GetAssortment(connector.ConnectorID, null,true).Value);

          Processor processor = new Processor(products, log, connector);
          
          log.Debug("Start import productgroups");
          processor.ImportProductGroups(true);
          log.Debug("Finished import productgroups");
        }
        catch (Exception ex)
        {
          log.Error("Error import ProcessProducts", ex);
        }
        log.DebugFormat("Finish Process web  productgroup import for {0}", connector.Name);

      }
    }
  }
}
