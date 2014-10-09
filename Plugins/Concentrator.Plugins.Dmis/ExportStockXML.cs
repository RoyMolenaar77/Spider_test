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
using Concentrator.Objects.Ftp;

namespace Concentrator.Plugins.Dmis
{
  [ConnectorSystem(4)]
  public class ExportStockXML : ConcentratorPlugin
  {
    public override string Name
    {
      get { return "Dmis Stock Exporter"; }
    }

    protected override void Process()
    {
      foreach (Connector connector in base.Connectors.Where(x => ((ConnectorType)x.ConnectorType).Has(ConnectorType.FileExport)))
      {
        #region Stock
        if (((ConnectorType)connector.ConnectorType).Has(ConnectorType.ShopAssortment) || ((ConnectorType)connector.ConnectorType).Has(ConnectorType.WebAssortment))
        {
          log.DebugFormat("Start Process Dmis stock export for {0}", connector.Name);

          AssortmentServiceSoapClient soap = new AssortmentServiceSoapClient();

          try
          {
            XDocument products = null;
            products = XDocument.Parse(soap.GetStockAssortment(connector.ConnectorID, null));

            string fileName = string.Format("WEBVRRD_{0}_{1}", connector.ConnectorID, DateTime.Now.ToString("yyyyMMddHHmmss"));

            using (var stream = new MemoryStream())
            {
              using (StreamWriter sw = new StreamWriter(stream))
              {
              sw.WriteLine("INIT;" + DateTime.Now.ToString("YYMMddHHmm"));

              var csv = (from p in products.Root.Elements("Product")
                         select new
                         {
                           ProductID = DmisUtilty.GetCustomItemNumber(p.Attribute("CustomProductID").Value,p.Attribute("ProductID").Value),
                           Stock = p.Element("Stock").Attribute("InStock").Value
                         }).ToList();

              csv.ForEach(x =>
              {
                sw.WriteLine(string.Format("{0};{1}",x.ProductID,x.Stock));
              });
              sw.WriteLine("ENDOFFILE");

           
              }
              
              DmisUtilty.UploadFiles(connector, stream, fileName, log);
            }
          }
          catch (Exception ex)
          {
            log.Error("FTP upload Dmis stockfile failed for connector" + connector.ConnectorID, ex);
          }
        }
        else
        {
          log.AuditCritical(string.Format("Export stock Dmis failed for {0}, XmlExportPath not set", connector.Name));
        }

        log.DebugFormat("Finish Process Dmis stock import for {0}", connector.Name);
      
        #endregion

       #region RetailStock
        if (((ConnectorType)connector.ConnectorType).Has(ConnectorType.ShopAssortment) || ((ConnectorType)connector.ConnectorType).Has(ConnectorType.WebAssortment))
        {
          log.DebugFormat("Start Process Dmis retail stock export for {0}", connector.Name);

          AssortmentServiceSoapClient soap = new AssortmentServiceSoapClient();

          try
          {
            XDocument products = null;
            products = XDocument.Parse(soap.GetStockAssortment(connector.ConnectorID, null));

            var retailProducts = (from p in products.Root.Elements("Product")
                                  from g in p.Element("Stock").Element("Retail").Elements("RetailStock")
                                  select new
                                  {
                                    ProductID = DmisUtilty.GetCustomItemNumber(p.Attribute("CustomProductID").Value, p.Attribute("ProductID").Value),
                                    VendorCode = g.Attribute("VendorCode").Value,
                                    Stock = g.Attribute("InStock").Value
                                  }).Distinct().ToList();

            var vendors = retailProducts.Select(x => x.VendorCode).Distinct().ToList();

            vendors.ForEach(x =>
            {
              var vendorStock = retailProducts.Where(v => v.VendorCode == x);

              string fileName = string.Format("WEBFILVRRD{0}_{1}_{2}", connector.ConnectorID,x, DateTime.Now.ToString("yyyyMMddHHmmss"));

              using (var stream = new MemoryStream())
              {
                using (StreamWriter sw = new StreamWriter(stream))
                {
                  sw.WriteLine("INIT;" + DateTime.Now.ToString("YYMMddHHmm"));

                  var csv = (from p in vendorStock
                             select new
                             {
                               ProductID = p.ProductID,
                               Stock = p.Stock
                             }).ToList();

                  csv.ForEach(z =>
                  {
                    sw.WriteLine(string.Format("{0};{1}", z.ProductID, z.Stock));
                  });
                  sw.WriteLine("ENDOFFILE");

                }

                DmisUtilty.UploadFiles(connector, stream, fileName, log);
              }
            });
          }
          catch (Exception ex)
          {
            log.Error("FTP upload Dmis stockfile failed for connector" + connector.ConnectorID, ex);
          }
        }
        else
        {
          log.AuditCritical(string.Format("Export stock Dmis failed for {0}, XmlExportPath not set", connector.Name));
        }

        log.DebugFormat("Finish Process Dmis stock import for {0}", connector.Name);
      }
        #endregion

    }
  }
}
