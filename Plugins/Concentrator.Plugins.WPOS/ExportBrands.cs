using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Threading;
using System.Configuration;
using System.Data.Linq;
using System.Globalization;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.ConcentratorService;
using Concentrator.Plugins.WPOS.Bulk;

namespace Concentrator.Plugins.WPOS
{
  [ConnectorSystem(1)]
  public class ExportBrands : ConcentratorPlugin
  {
    private XDocument brands;
    private Dictionary<string, int> customerItemNumbers = new Dictionary<string, int>();
    private Dictionary<int, List<int>> pgmList = new Dictionary<int, List<int>>();
    public string connectionString { get; set; }

    public override string Name
    {
      get { return "Concentrator Brand Export Plugin"; }
    }

    protected override void Process()
    {
      using (var unit = GetUnitOfWork())
      {
        var config = GetConfiguration();
        connectionString = config.ConnectionStrings.ConnectionStrings["WPOS"].ConnectionString;

        int connectorID = int.Parse(config.AppSettings.Settings["ConnectorID"].Value);

        Connector connector = unit.Scope.Repository<Connector>().GetSingle(x => x.ConnectorID == connectorID);

        log.DebugFormat("Starting Brand import for {0}", connector.Name);

        try
        {
          DateTime start = DateTime.Now;
          log.InfoFormat("Starting processing Brands:{0}", start);
          Concentrator.Web.ServiceClient.AssortmentService.AssortmentServiceSoapClient soap = new Concentrator.Web.ServiceClient.AssortmentService.AssortmentServiceSoapClient();
          {
            XDocument brands = new XDocument(soap.GetAssortmentContent(connectorID, false, true, null, true));

            log.Info("Start Brand import");
            ImportBrands(brands, connector);
            log.Info("Finish Brand import");
          }
        }
        catch (Exception ex)
        {
          log.Error("Error importing Products", ex);
        }
        log.DebugFormat("Finished Productimport for {0}", connector.Name);
      }
    }

    private void ImportBrands(XDocument data, Connector connector)
    {
      var Brands = (from x in data.Element("Assortment").Elements("Product").Elements("Brands").Elements("Brand")
                          select new WposBrands
                          {
                            Name = x.Try(y => y.Element("Name").Value),
                            BackendID = x.Try(y => y.Attribute("BrandID").Value)
                          });

      using (var unit = GetUnitOfWork())
      {
        using (WposBrandBulk bulkExport = new WposBrandBulk(Brands, connectionString))
        {
          bulkExport.Init(unit.Context);
          bulkExport.Sync(unit.Context);
        }
      }
    }
  }
}