using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Configuration;
using System.Xml.Linq;
using System.Globalization;
using Concentrator.Objects;
using Concentrator.Web.ServiceClient.AssortmentService;
using System.Reflection;
using System.Data.Linq.Mapping;
using log4net;
using Concentrator.Objects.ConcentratorService;
using System.IO;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.ConcentratorService;

namespace Concentrator.Plugins.XmlExport
{
  public class ExportStockXML : ConcentratorPlugin
  {
    public override string Name
    {
      get { return "Xml Stock Exporter"; }
    }

    protected override void Process()
    {
      foreach (Connector connector in base.Connectors.Where(x => ((ConnectorType)x.ConnectorType).Has(ConnectorType.FileExport)))
      {
#if DEBUG
        if (connector.ConnectorID != 10)
          continue;
#endif
        #region Assortiment
        if (((ConnectorType)connector.ConnectorType).Has(ConnectorType.ShopAssortment) || ((ConnectorType)connector.ConnectorType).Has(ConnectorType.WebAssortment))
        {
          foreach (var language in connector.ConnectorLanguages)
          {
            log.DebugFormat("Start Process XML stock export for {0}", connector.Name);

            string drive = connector.ConnectorSettings.GetValueByKey("XmlExportPath", string.Empty);
             bool networkDrive = false;
        //string drive = @"\\SOL\Company_Shares\Database Backup";
            bool.TryParse(connector.ConnectorSettings.GetValueByKey("IsNetorkDrive","false"), out networkDrive);
          NetworkExportUtility util = new NetworkExportUtility();

        if (networkDrive)
        {
          drive = util.ConnectorNetworkPath(drive, "I:");
        }

        string path = drive;

            if (!string.IsNullOrEmpty(path))
            {
              AssortmentServiceSoapClient soap = new AssortmentServiceSoapClient();

              XDocument products = new XDocument(soap.GetStockAssortment(connector.ConnectorID, null));

              string file = Path.Combine(path, string.Format("stock_{0}.xml", connector.ConnectorID));

              if (File.Exists(file))
                File.Delete(file);

              products.Save(file, SaveOptions.DisableFormatting);
            }
            else
            {
              log.AuditCritical(string.Format("Export stock XML failed for {0}, XmlExportPath not set", connector.Name));
            }

            if (networkDrive)
            {
              util.DisconnectNetworkPath(drive);
            }

            log.DebugFormat("Finish Process XML stock import for {0}", connector.Name);
          }
        }
        #endregion

      }
    }
  }
}
