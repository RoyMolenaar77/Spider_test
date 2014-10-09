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
using System.IO;
using Concentrator.Objects.Models.Connectors;
using System.Xml;

namespace Concentrator.Plugins.BAS.WebExport
{
  [ConnectorSystem(1)]
  public class ImportProducts : ConcentratorPlugin
  {
    public override string Name
    {
      get { return "Website Assortment Export Plugin"; }
    }

    protected override void Process()
    {


      foreach (Connector connector in Connectors.Where(x => ((ConnectorType)x.ConnectorType).Has(ConnectorType.WebAssortment)))
      {
        
        log.DebugFormat("Start Process web import for {0}", connector.Name);
        try
        {
           
          DateTime start = DateTime.Now;
          log.DebugFormat("Start process products:{0}", start);
          AssortmentServiceSoapClient soap = new AssortmentServiceSoapClient();


//#if DEBUG
//          if (File.Exists(@"C:\products.xml"))
//            products = XDocument.Load(@"C:\products.xml");
//          else
//          {
//#endif
         XDocument products = new XDocument(soap.GetAssortment(connector.ConnectorID, null, true));
//#if DEBUG
//            products.Save(@"C:\products.xml");

//          }
//#endif
          Processor processor = new Processor(products, log, connector);


          log.Debug("Start import brands");
          processor.ImportBrands();
          log.Debug("Finished import brands");
          log.Debug("Start import productgroups");
          processor.ImportProductGroups(true);
          log.Debug("Finished import productgroups");
          log.Debug("Start import Products");
          bool errorImportProduct = processor.ImportProducts();
          log.Debug("Finished import Products");

          if (connector.ImportCommercialText)
          {
            log.Debug("Start import Product descriptions");
            processor.ProcessContentDescriptions(soap, connector);
            log.Debug("Finished import descriptions");
          }

          if (!errorImportProduct)
          {
            processor.CleanUpProducts();
            processor.CleanUpProductGroupMapping();
          }
         
          log.Debug("Start import Attributes");
          processor.ProcessAttributes(soap, connector, false, null, false);
          log.Debug("Finished import Attributes"); 
          
           
 
        }
        catch (Exception ex)
        {
          log.Error("Error import ProcessProducts", ex);
        }
        
        log.DebugFormat("Finish Process web import for {0}", connector.Name);

      }
    }
  }
}
