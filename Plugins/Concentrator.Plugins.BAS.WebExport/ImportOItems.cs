using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Xml.Linq;
using Concentrator.Web.ServiceClient.AssortmentService;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.ConcentratorService;

namespace Concentrator.Plugins.BAS.WebExport
{
  [ConnectorSystem(1)]
  public class ImportOItems : ConcentratorPlugin
  {
    public override string Name
    {
      get { return "Website Obsolete Products Export Plugin"; }
    }

    protected override void Process()
    {
      try
      {
        foreach (Connector connector in base.Connectors.Where(x => x.ObsoleteProducts))
        {
          if (connector.ObsoleteProducts)
          {
              
            log.DebugFormat("Start Process O items import for {0}", connector.Name);

            DateTime start = DateTime.Now;
            log.InfoFormat("Start process stock:{0}", start);
            AssortmentServiceSoapClient soap = new AssortmentServiceSoapClient();
            XDocument products = new XDocument(soap.GetOAssortment(connector.ConnectorID, false, "227782"));

            Processor process = new Processor(products, log, connector);
            process.ImportOItems(products, null);

            process.ImportStock(); ;
            
          }
        }
      }
      catch (Exception ex)
      {
        log.Error("Error import obsolete items", ex);
      }

    }
  }
}
