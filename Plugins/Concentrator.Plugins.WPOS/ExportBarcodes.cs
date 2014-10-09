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
  public class ExportBarcodes : ConcentratorPlugin
  {
    private XDocument barcodes;
    private Dictionary<string, int> customerItemNumbers = new Dictionary<string, int>();
    private Dictionary<int, List<int>> pgmList = new Dictionary<int, List<int>>();
    public string connectionString { get; set; }

    public override string Name
    {
      get { return "Concentrator Product Export Plugin"; }
    }

    protected override void Process()
    {
      using (var unit = GetUnitOfWork())
      {
        var config = GetConfiguration();
        connectionString = config.ConnectionStrings.ConnectionStrings["WPOS"].ConnectionString;

        int connectorID = int.Parse(config.AppSettings.Settings["ConnectorID"].Value);

        Connector connector = unit.Scope.Repository<Connector>().GetSingle(x => x.ConnectorID == connectorID);

        log.DebugFormat("Starting Barcode import for {0}", connector.Name);

        try
        {
          DateTime start = DateTime.Now;
          log.InfoFormat("Starting processing barcodes:{0}", start);
          using(Concentrator.Web.ServiceClient.AssortmentService.AssortmentServiceSoapClient soap = new Concentrator.Web.ServiceClient.AssortmentService.AssortmentServiceSoapClient())
          {
            XDocument barcodes;
            barcodes = new XDocument(soap.GetAssortmentContent(connectorID, false, true, null, true));

            log.Info("Start Barcode import");
            ImportBarcodes(barcodes, connector);
            log.Info("Finish Barcode import");
          }
        }
        catch (Exception ex)
        {
          log.Error("Error importing Barcodes", ex);
        }
        log.DebugFormat("Finished Barcodeimport for {0}", connector.Name);
      }
    }

    private void ImportBarcodes(XDocument data, Connector connector)
    {
      List<WposBarcodes> Barcodes = new List<WposBarcodes>();

      foreach (var product in data.Element("Assortment").Elements("Product"))
      {
        foreach (var barcode in product.Elements("Barcodes").Elements("Barcode"))
        {
          Barcodes.Add(new WposBarcodes()
          {
            BarcodeStandardID = "1",
            BarcodeTypeID = "1",
            Value = barcode.Value,
            Product_ID = product.Try(y => y.Attribute("ProductID").Value)
          });
        }
      }

      using (var unit = GetUnitOfWork())
      {
        using (WposBarcodeBulk bulkExport = new WposBarcodeBulk(Barcodes, connectionString))
        {
          bulkExport.Init(unit.Context);
          bulkExport.Sync(unit.Context);
        }
      }
    }
  }
}