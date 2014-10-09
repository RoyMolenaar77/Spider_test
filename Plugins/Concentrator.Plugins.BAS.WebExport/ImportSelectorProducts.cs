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
  public class ImportSelectorProducts : ConcentratorPlugin
  {
    public override string Name
    {
      get { return "Website Selector Product Export Plugin"; }
    }

    protected override void Process()
    {
      foreach (Connector connector in Connectors.Where(x => ((ConnectorType)x.ConnectorType).Has(ConnectorType.WebAssortment) && x.Selectors))
      {
#if DEBUG
        if (connector.ConnectorID != 1)
          continue;
#endif
         
        using (
           WebsiteDataContext context =
             new WebsiteDataContext(
               ConfigurationManager.ConnectionStrings[connector.Connection].ConnectionString))
        {
          log.DebugFormat("Start Selector Product import for {0}", connector.Name);
          try
          {
            DateTime start = DateTime.Now;
            log.DebugFormat("Start Selector Product:{0}", start);
            AssortmentServiceSoapClient soap = new AssortmentServiceSoapClient();

            XDocument products;

            var selectorIDs = (from s in context.Selectors select s.SelectorID).ToList();

            foreach (int selectorID in selectorIDs)
            {
              products = new XDocument(soap.GetSelectorProducts(connector.ConnectorID, selectorID));

              Processor processor = new Processor(products, log, connector);
              
              log.Debug("Start import brands");
              processor.ImportBrands();
              log.Debug("Finished import brands");
              log.Debug("Start import productgroups");
              processor.ImportProductGroups(false);
              log.Debug("Finished import productgroups");
              log.Debug("Start import Products");
              processor.ImportSelectorProducts();
              log.Debug("Finished import Products");

              //log.Debug("Start import Attributes");
              //processor.ProcessAttributes(soap, connector, false, null, false);
              //log.Debug("Finished import Attributes");

              //processor.CleanUpProducts();
              //processor.CleanUpProductGroupMapping();
            }
          }
          catch (Exception ex)
          {
            log.Error("Error import Selector Products", ex);
          }
          
          log.DebugFormat("Finish Process Selector Product for {0}", connector.Name);
        }
      }
    }
  }
}
