using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects;
using Concentrator.Objects.ConcentratorService;
using System.Configuration;
using System.Xml.Linq;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.ConcentratorService;

namespace Concentrator.Plugins.BAS.WebExport
{
  [ConnectorSystem(1)]
  public class ImportBarcodes : ConcentratorPlugin
  {
    public override string Name
    {
      get { return "Website Barcodes Export Plugin"; }
    }

    protected override void Process()
    {
      log.DebugFormat("Start barcode processing config connecotors");

      foreach (Connector connector in base.Connectors.Where(c => ((ConnectorType)c.ConnectorType).Has(ConnectorType.WebAssortment)))
      {
        DateTime start = DateTime.Now;
        log.DebugFormat("Start Process barcode import for {0}", connector.Name);
        try
        {
          Concentrator.Web.ServiceClient.AssortmentService.AssortmentServiceSoapClient soap = new Concentrator.Web.ServiceClient.AssortmentService.AssortmentServiceSoapClient();

          XDocument products;
          products = new XDocument(soap.GetAssortment(connector.ConnectorID, null, false));

          Processor processor = new Processor(products, log, connector);
          processor.ImportBarcode();
        }
        catch (Exception ex)
        {
          log.Fatal(ex);
        }
        log.DebugFormat("Finish Process barcode import for {0}", connector.Name);

      }
    }

    private ProductBarcode InsertBarcode(int productID, string barcode)
    {
      ProductBarcode productBarcode = new ProductBarcode();
      productBarcode.LastModificationTime = DateTime.Now;
      productBarcode.ProductID = productID;
      productBarcode.BarcodeType = "Sp";
      productBarcode.Barcode = barcode;
      return productBarcode;
    }
  }
}
