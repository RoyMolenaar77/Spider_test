using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Threading;
using System.Configuration;
using System.Data.Linq;
using System.Globalization;
using Concentrator.Objects.ConcentratorService;
using Concentrator.Objects;
using Concentrator.Web.ServiceClient.AssortmentService;
using Concentrator.Objects.Models.Connectors;

namespace Concentrator.Plugins.BAS.WebExport.Shop
{
  [ConnectorSystem(1)]
  public class ShopImportStock : ConcentratorPlugin
  {
    private XDocument products;
    private Dictionary<string, int> customerItemNumbers = new Dictionary<string, int>();
    private Dictionary<int, List<int>> pgmList = new Dictionary<int, List<int>>();

    public override string Name
    {
      get { return "MyCom Shop Stock Product Export Plugin"; }
    }

    protected override void Process()
    {

      foreach (Connector connector in base.Connectors.Where(c => ((ConnectorType)c.ConnectorType).Has(ConnectorType.ShopAssortment)))
      {

        log.DebugFormat("Start Process shop Stock import for {0}", connector.Name);
        

        try
        {
           
          DateTime start = DateTime.Now;
          log.InfoFormat("Start process products:{0}", start);
          using (Concentrator.Web.ServiceClient.AssortmentService.AssortmentServiceSoapClient soap = new Concentrator.Web.ServiceClient.AssortmentService.AssortmentServiceSoapClient())
          {

            products = new XDocument(soap.GetAssortmentContent(connector.ConnectorID, false, true, null, false));

            log.Info("Start import Stock");
            ShopUtility util = new ShopUtility();
            util.ProcessStock(connector, log, products);
            log.Info("Finish import Stock");
          }
          
        }
        catch (Exception ex)
        {
          log.Error("Error import shop ProcessProducts", ex);
        }
       
        log.DebugFormat("Finish Process shop import for {0}", connector.Name);

      }
    }
  }
}

