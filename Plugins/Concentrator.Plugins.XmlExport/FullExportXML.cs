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

namespace Concentrator.Plugins.XmlExport
{
  public class FullExportXML : ConcentratorPlugin
  {
    public override string Name
    {
      get { return "Full Xml Exporter"; }
    }

    protected override void Process()
    {
      foreach (Connector connector in base.Connectors.Where(x => ((ConnectorType)x.ConnectorType).Has(ConnectorType.FileExport)))
      {
        #region Assortiment
        if (((ConnectorType)connector.ConnectorType).Has(ConnectorType.ShopAssortment) || ((ConnectorType)connector.ConnectorType).Has(ConnectorType.WebAssortment))
        {
          foreach (var language in connector.ConnectorLanguages)
          {
            log.DebugFormat("Start Process Full XML export for {0} language {1}", connector.Name, language.Language.Name);

            string path = connector.ConnectorSettings.GetValueByKey("XmlExportPath", string.Empty);

            if (!string.IsNullOrEmpty(path))
            {
              AssortmentServiceSoapClient soap = new AssortmentServiceSoapClient();

              XDocument products;
              products = XDocument.Parse(soap.GetFullInformation(connector.ConnectorID, true, false, null, null, true, language.LanguageID));

              string file = Path.Combine(path, string.Format("products_{0}_{1}.xml", connector.ConnectorID, language.LanguageID));

              if (File.Exists(file))
                File.Delete(file);

              products.Save(file, SaveOptions.DisableFormatting);
            }
            else
            {
              log.AuditCritical(string.Format("Export XML failed for {0}, XmlExportPath not set", connector.Name));
            }

            log.DebugFormat("Finish Process Full XML import for {0}", connector.Name);
          }
        }
        #endregion
      }

    }
  }
}
