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
  public class ExportImages : ConcentratorPlugin
  {
    private XDocument images;
    private Dictionary<string, int> customerItemNumbers = new Dictionary<string, int>();
    private Dictionary<int, List<int>> pgmList = new Dictionary<int, List<int>>();
    public string connectionString { get; set; }

    public override string Name
    {
      get { return "Concentrator Image Export Plugin"; }
    }

    protected override void Process()
    {
      using (var unit = GetUnitOfWork())
      {
        var config = GetConfiguration();
        connectionString = config.ConnectionStrings.ConnectionStrings["WPOS"].ConnectionString;

        int connectorID = int.Parse(config.AppSettings.Settings["ConnectorID"].Value);

        Connector connector = unit.Scope.Repository<Connector>().GetSingle(x => x.ConnectorID == connectorID);

        log.DebugFormat("Starting Image import for {0}", connector.Name);

        try
        {
          DateTime start = DateTime.Now;
          log.InfoFormat("Starting processing images:{0}", start);
          using(Concentrator.Web.ServiceClient.AssortmentService.AssortmentServiceSoapClient soap = new Concentrator.Web.ServiceClient.AssortmentService.AssortmentServiceSoapClient())
          {
            XDocument images;
            images = XDocument.Parse(soap.GetFTPAssortmentImages(connectorID));

            log.Info("Start Images import");
            ImportImages(images, connector);
            log.Info("Finish Images import");
          }
        }
        catch (Exception ex)
        {
          log.Error("Error importing Images", ex);
        }
        log.DebugFormat("Finished Image Import for {0}", connector.Name);
      }
    }

    private void ImportImages(XDocument data, Connector connector)
    {
      var brandImages = (from x in data.Element("ProductMedia").Elements("Brands").Elements("BrandImage")
                          select new WposImages
                          {
                            ImageType = "Brand",
                            ImagePath = x.Try(y => y.Value),
                            SequenceNo = "0",
                            CreatedByID = "1",
                            CreationTime = DateTime.Now,
                            Product_ID = "0",
                            Brand_ID = x.Try(y => y.Attribute("BrandID").Value)
                          });

      var productImages = (from x in data.Element("ProductMedia").Elements("Products").Elements("ProductMedia")
                           select new WposImages
                           {
                             ImageType = x.Try(y => y.Attribute("Type").Value),
                             ImagePath = x.Try(y => y.Value),
                             SequenceNo = x.Try(y => y.Attribute("Sequence").Value),
                             CreatedByID = "1",
                             CreationTime = DateTime.Now,
                             Product_ID = x.Try(y => y.Attribute("ProductID").Value),
                             Brand_ID = x.Try(y => y.Attribute("BrandID").Value)
                           });

      var Images = brandImages.Union(productImages);

      using (var unit = GetUnitOfWork())
      {
        using (WposImagesBulk bulkExport = new WposImagesBulk(Images, connectionString))
        {
          bulkExport.Init(unit.Context);
          bulkExport.Sync(unit.Context);
        }
      }
    }
  }
}