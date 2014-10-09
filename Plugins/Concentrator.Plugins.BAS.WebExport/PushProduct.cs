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
using Concentrator.Plugins.BAS.WebExport.Shop;


namespace Concentrator.Plugins.BAS.WebExport
{
  [ConnectorSystem(1)]
  public class PushProduct : ConcentratorPlugin
  {
    public override string Name
    {
      get { return "Push Product"; }
    }

    protected override void Process()
    {
      foreach (Connector connector in Connectors.Where(x => ((ConnectorType)x.ConnectorType).Has(ConnectorType.WebAssortment)))
      {
         
        log.DebugFormat("Start process push product for {0}", connector.Name);
        using (var unit = GetUnitOfWork())
        {
          try
          {
            var _pushRepo = unit.Scope.Repository<Concentrator.Objects.Models.Products.PushProduct>();
             
            DateTime start = DateTime.Now;
            log.DebugFormat("Start process products:{0}", start);
            AssortmentServiceSoapClient soap = new AssortmentServiceSoapClient();

            XDocument products = null;

            var pushProducts = _pushRepo.GetAll(x => !x.Processed && x.ConnectorID == connector.ConnectorID).ToList();

            foreach (var product in pushProducts)
            {
              if (product.ProductID.HasValue)
                products = new XDocument(soap.GetAssortment(connector.ConnectorID, product.ProductID.Value.ToString(), false));
              else
                products = new XDocument(soap.GetOAssortment(connector.ConnectorID, false, product.CustomItemNumber));


              Processor process = new Processor(products, log, connector);
              process.ImportOItems(products, product.CustomItemNumber);

              product.Processed = true;
              unit.Save();
            }
            
          }
          catch (Exception ex)
          {
            log.Error("Error push product", ex);
          }
        }
        
        log.DebugFormat("Finish push product import for {0}", connector.Name);

      }

      foreach (Connector connector in Connectors.Where(x => ((ConnectorType)x.ConnectorType).Has(ConnectorType.ShopAssortment)))
      {

        log.DebugFormat("Start process push product for {0}", connector.Name);
        using (var unit = GetUnitOfWork())
        {
          try
          {
            var _pushRepo = unit.Scope.Repository<Concentrator.Objects.Models.Products.PushProduct>();

            DateTime start = DateTime.Now;
            log.DebugFormat("Start process products:{0}", start);
            AssortmentServiceSoapClient soap = new AssortmentServiceSoapClient();

            XDocument products = null;

            var pushProducts = _pushRepo.GetAll(x => !x.Processed && x.ConnectorID == connector.ConnectorID).ToList();

            foreach (var product in pushProducts)
            {
             // if (product.ProductID.HasValue)
               // products = new XDocument(soap.GetAssortment(connector.ConnectorID, product.ProductID.Value.ToString(), false));
              //else
               // products = new XDocument(soap.GetOAssortment(connector.ConnectorID, false, product.CustomItemNumber));


              ShopImportOProduct processor = new ShopImportOProduct();
              processor.ImportOItems(products, connector);

              //Processor process = new Processor(products, log, connector);
              //process.ImportOItems(products, product.CustomItemNumber);

              product.Processed = true;
              unit.Save();
            }

          }
          catch (Exception ex)
          {
            log.Error("Error push product", ex);
          }
        }

        log.DebugFormat("Finish push product import for {0}", connector.Name);

      }
    }
  }
}
