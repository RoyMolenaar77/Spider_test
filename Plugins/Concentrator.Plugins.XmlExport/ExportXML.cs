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
using System.IO;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.ConcentratorService;


namespace Concentrator.Plugins.XmlExport
{
  public class ExportXML : ConcentratorPlugin
  {
    public override string Name
    {
      get { return "Xml Exporter"; }
    }

    protected override void Process()
    {
      foreach (Connector connector in base.Connectors.Where(x => ((ConnectorType)x.ConnectorType).Has(ConnectorType.FileExport)))
      {
        string drive = connector.ConnectorSettings.GetValueByKey("XmlExportPath", string.Empty);
        bool networkDrive = false;
        //string drive = @"\\SOL\Company_Shares\Database Backup";
        bool.TryParse(connector.ConnectorSettings.GetValueByKey("IsNetorkDrive", "false"), out networkDrive);
        NetworkExportUtility util = new NetworkExportUtility();

        if (networkDrive)
        {
          drive = util.ConnectorNetworkPath(drive, "I:");
        }

        string path = drive;
        
        #region Assortiment
        if (((ConnectorType)connector.ConnectorType).Has(ConnectorType.ShopAssortment) || ((ConnectorType)connector.ConnectorType).Has(ConnectorType.WebAssortment))
        {
          foreach (var language in connector.ConnectorLanguages)
          {
            log.DebugFormat("Start Process XML Assortment export for {0} language {1}", connector.Name, language.Language.Name);

            if (!string.IsNullOrEmpty(path))
            {
              AssortmentServiceSoapClient soap = new AssortmentServiceSoapClient();

              XDocument products = new XDocument(soap.GetAdvancedPricingAssortment(connector.ConnectorID, false, false, null, null, true, language.LanguageID, false));

              string file = Path.Combine(path, string.Format("products_{0}_{1}.xml", connector.ConnectorID, language.LanguageID));

              if (File.Exists(file))
                File.Delete(file);

              products.Save(file, SaveOptions.DisableFormatting);
            }
            else
            {
              log.AuditCritical(string.Format("Export XML failed for {0}, XmlExportPath not set", connector.Name));
            }

            log.DebugFormat("Finish Process XML Assortment import for {0}", connector.Name);
          }
        }
        #endregion

        #region Images
        if (((ConnectorType)connector.ConnectorType).Has(ConnectorType.Images))
        {
          log.DebugFormat("Start Process XML Image export for {0}", connector.Name);

          if (!string.IsNullOrEmpty(path))
          {
            AssortmentServiceSoapClient soap = new AssortmentServiceSoapClient();

            XDocument products;
            products = XDocument.Parse(soap.GetFTPAssortmentImages(connector.ConnectorID));

            string file = Path.Combine(path, string.Format("Images_{0}.xml", connector.ConnectorID));

            if (File.Exists(file))
              File.Delete(file);

            products.Save(file, SaveOptions.DisableFormatting);
          }
          else
          {
            log.AuditCritical(string.Format("Export XML failed for {0}, XmlExportPath not set", connector.Name));
          }

          log.DebugFormat("Finish Process XML Image import for {0}", connector.Name);
        }
        #endregion

        #region content
        if (((ConnectorType)connector.ConnectorType).Has(ConnectorType.Content))
        {
          foreach (var language in connector.ConnectorLanguages)
          {
            log.DebugFormat("Start Process XML Content export for {0} language {1}", connector.Name, language.Language.Name);

           if (!string.IsNullOrEmpty(path))
            {
              AssortmentServiceSoapClient soap = new AssortmentServiceSoapClient();

              XDocument content = new XDocument(soap.GetAssortmentContentDescriptionsByLanguage(connector.ConnectorID, null, language.LanguageID));

              string contentFile = Path.Combine(path, string.Format("Content_{0}_{1}.xml", connector.ConnectorID, language.LanguageID));

              if (File.Exists(contentFile))
                File.Delete(contentFile);

              content.Save(contentFile, SaveOptions.DisableFormatting);

              XDocument attributes = new XDocument(soap.GetAttributesAssortmentByLanguage(connector.ConnectorID, null, null, language.LanguageID, true));

              string attributeFile = Path.Combine(path, string.Format("Attribute_{0}_{1}.xml", connector.ConnectorID, language.LanguageID));

              if (File.Exists(attributeFile))
                File.Delete(attributeFile);

              attributes.Save(attributeFile, SaveOptions.DisableFormatting);
            }
            else
            {
              log.AuditCritical(string.Format("Export XML failed for {0}, XmlExportPath not set", connector.Name));
            }

            log.DebugFormat("Finish Process XML Content import for {0} language {1}", connector.Name, language.Language.Name);
          }
        }
        #endregion

        if (networkDrive)
        {
          util.DisconnectNetworkPath(drive);
        }
      }
    }
  }
}
