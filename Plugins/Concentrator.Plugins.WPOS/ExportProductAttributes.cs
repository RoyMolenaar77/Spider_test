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
  public class ExportProductAttributes : ConcentratorPlugin
  {
    private XDocument products;
    private Dictionary<string, int> customerItemNumbers = new Dictionary<string, int>();
    private Dictionary<int, List<int>> pgmList = new Dictionary<int, List<int>>();
    public string connectionString { get; set; }

    public override string Name
    {
      get { return "Concentrator ProductAttribute Export Plugin"; }
    }

    protected override void Process()
    {
      using (var unit = GetUnitOfWork())
      {
        var config = GetConfiguration();
        connectionString = config.ConnectionStrings.ConnectionStrings["WPOS"].ConnectionString;

        int connectorID = int.Parse(config.AppSettings.Settings["ConnectorID"].Value);

        Connector connector = unit.Scope.Repository<Connector>().GetSingle(x => x.ConnectorID == connectorID);

        log.DebugFormat("Starting ProductAttribute import for {0}", connector.Name);

        try
        {
          DateTime start = DateTime.Now;
          log.InfoFormat("Starting processing ProductAttributes:{0}", start);
          using (Concentrator.Web.ServiceClient.AssortmentService.AssortmentServiceSoapClient soap = new Concentrator.Web.ServiceClient.AssortmentService.AssortmentServiceSoapClient())
          {
            XDocument products;
            products = new XDocument(soap.GetAttributesAssortment(connectorID, null, null));
            log.Info("Start ProductAttribute import");

            log.Info("Start ProductAttributeCategories import");
            ImportProductAttributeCategories(products, connector);
            log.Info("Start ProductAttributeCategories import");

            log.Info("Start ProductAttributeTypes import");
            ImportProductAttributeTypes(products, connector);
            log.Info("Start ProductAttributeTypes import");

            log.Info("Start ProductAttributeValues import");
            ImportProductAttributeValues(products, connector);
            log.Info("Start ProductAttributeValues import");

            log.Info("Finish ProductAttribute import");
          }
        }
        catch (Exception ex)
        {
          log.Error("Error importing ProductAttributes", ex);
        }
        log.DebugFormat("Finish ProductAttribute import for {0}", connector.Name);
      }
    }

    private void ImportProductAttributeCategories(XDocument data, Connector connector)
    {
      var productAttributeCategories = (from x in data.Element("ProductAttributes").Elements("ProductAttribute").Elements("AttributeGroups").Elements("AttributeGroup")
                                        select new WposProductAttributeCategories
                                        {
                                          Name = x.Try(y => y.Attribute("Name").Value),
                                          BackendID = x.Try(y => y.Attribute("AttributeGroupID").Value),
                                        });
      using (var unit = GetUnitOfWork())
      {
        using (WposProductAttributeCategoryBulk bulkExport = new WposProductAttributeCategoryBulk(productAttributeCategories, connectionString))
        {
          bulkExport.Init(unit.Context);
          bulkExport.Sync(unit.Context);
        }
      }
    }

    private void ImportProductAttributeTypes(XDocument data, Connector connector)
    {
      var productAttributeTypes = (from x in data.Element("ProductAttributes").Elements("ProductAttribute").Elements("Attributes").Elements("Attribute")
                                   select new WposProductAttributeTypes
                                   {
                                     Name = x.Try(y => y.Element("Name").Value),
                                     Unit = x.Try(y => y.Element("Sign").Value),
                                     IsVisible = x.Try(y => y.Attribute("KeyFeature").Value),
                                     IsSearchable = x.Try(y => y.Attribute("IsSearchable").Value),
                                     CategoryID = x.Try(y => y.Attribute("AttributeGroupID").Value),
                                     BackendID = x.Try(y => y.Attribute("AttributeID").Value)
                                   });
      using (var unit = GetUnitOfWork())
      {
        using (WposProductAttributeTypeBulk bulkExport = new WposProductAttributeTypeBulk(productAttributeTypes, connectionString))
        {
          bulkExport.Init(unit.Context);
          bulkExport.Sync(unit.Context);
        }
      }
    }

    private void ImportProductAttributeValues(XDocument data, Connector connector)
    {
      var productAttributeValues = (from x in data.Element("ProductAttributes").Elements("ProductAttribute")
                                    let productID = x.Try(y => y.Attribute("ProductID").Value)
                                    from att in x.Element("Attributes").Elements("Attribute")
                                    select new WposProductAttributeValues
                                    {
                                      ProductID = productID,
                                      TypeID = att.Try(y => y.Attribute("AttributeID").Value),
                                      Value = att.Try(y => y.Element("Value").Value),
                                      CreatedByID = 1,
                                      CreationTime = DateTime.Now
                                    });
      using (var unit = GetUnitOfWork())
      {
        using (WposProductAttributeValueBulk bulkExport = new WposProductAttributeValueBulk(productAttributeValues, connectionString))
        {
          bulkExport.Init(unit.Context);
          bulkExport.Sync(unit.Context);
        }
      }
    }
  }
}