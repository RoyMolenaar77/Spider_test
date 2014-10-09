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
  public class ExportProducts : ConcentratorPlugin
  {
    private XDocument products;
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

        log.DebugFormat("Starting Product import for {0}", connector.Name);

        try
        {
          DateTime start = DateTime.Now;
          log.InfoFormat("Starting processing products:{0}", start);
          using(Concentrator.Web.ServiceClient.AssortmentService.AssortmentServiceSoapClient soap = new Concentrator.Web.ServiceClient.AssortmentService.AssortmentServiceSoapClient())
          {
            XDocument products = new XDocument(soap.GetAssortmentContent(connectorID, false, true, null, true));

            log.Info("Start ProductGroups import");
            ImportProductGroups(products, connector);
            log.Info("Finish ProductGroups import");

            log.Info("Start Productimport");
            ImportProducts(products, connector);
            log.Info("Finish Productimport");

            log.Info("Start ProductGroupProducts import");
            ImportProductGroupProducts(products, connector);
            log.Info("Finish ProductGroupProducts import");
          }
        }
        catch (Exception ex)
        {
          log.Error("Error importing Products", ex);
        }
        log.DebugFormat("Finished Productimport for {0}", connector.Name);
      }
    }

    private void ImportProducts(XDocument data, Connector connector)
    {
      var itemProducts = (from x in data.Element("Assortment").Elements("Product")
                          select new WposProducts
                          {
                            BrandID = x.Try(y => y.Element("Brands").Element("Brand").Attribute("BrandID").Value),
                            BackendProductID = x.Try(y => y.Attribute("ProductID").Value),
                            CustomItemNumber = x.Try(z => z.Attribute("CustomProductID").Value),
                            VendorProductID = x.Try(z => z.Attribute("ManufacturerID").Value),                 
                            ShortDescription = x.Try(y => y.Element("Content").Attribute("ShortDescription").Value),
                            LongDescription = x.Try(y => y.Element("Content").Attribute("LongDescription").Value),
                            LongContentDescription = null,
                            UnitCost = x.Try(y => y.Element("Price").Element("CostPrice").Value, "0"),
                            UnitPrice = x.Try(y => y.Element("Price").Element("UnitPrice").Value, "0"),
                            TaxRate = x.Try(y => y.Element("Price").Attribute("TaxRate").Value, "0"),
                            PromisedDeliveryDate = x.Try(y => string.IsNullOrEmpty(y.Element("Stock").Attribute("PromisedDeliveryDate").Value) ? null : DateTime.Parse(y.Element("Stock").Attribute("PromisedDeliveryDate").Value).ToString("yyyyMMdd")),
                            Status = x.Try(y => y.Element("Price").Attribute("CommercialStatus").Value),
                            CreatedByID = 1,
                            CreationTime = DateTime.Now,
                            Rating = "0",
                            NrOfReviews = 0,
                            Popularity = "0"
                          });

      using (var unit = GetUnitOfWork())
      {
        using (WposProductBulk bulkExport = new WposProductBulk(itemProducts, connectionString))
        {
          bulkExport.Init(unit.Context);
          bulkExport.Sync(unit.Context);
        }
      }
    }

    private void ImportProductGroupProducts(XDocument data, Connector connector)
    {
      List<WposProductGroupProducts> pgps = new List<WposProductGroupProducts>();

      foreach (var product in data.Element("Assortment").Elements("Product"))
      {
        string concentratorProductID = product.Attribute("ProductID").Value;

        foreach (var visibleGroup in product.Elements("ProductGroupHierarchy").Elements("ProductGroup"))
        {
          pgps.Add(new WposProductGroupProducts()
          {
            ConcentratorProductID = int.Parse(concentratorProductID),
            ConcentratorProductGroupID = int.Parse(visibleGroup.Attribute("ID").Value)
          });
        }
      }

      using (var unit = GetUnitOfWork())
      {
        using (WposProductGroupProductsBulk bulkExport = new WposProductGroupProductsBulk(pgps, connectionString))
        {
          bulkExport.Init(unit.Context);
          bulkExport.Sync(unit.Context);
        }
      }
    }

    private List<WposProductGroups> wPostProductGroups = new List<WposProductGroups>();

    private void ImportProductGroups(XDocument data, Connector connector)
    {
      foreach (var hierarchy in data.Element("Assortment").Elements("Product").Elements("ProductGroupHierarchy"))
      {
        foreach (var visibleGroup in hierarchy.Elements("ProductGroup"))
        {
          level = 0;
          ProcessProductGroupNode(visibleGroup, wPostProductGroups);
        }
      }

      using (var unit = GetUnitOfWork())
      {
        using (WposProductGroupBulk bulkExport = new WposProductGroupBulk(wPostProductGroups, connectionString))
        {
          bulkExport.Init(unit.Context);
          bulkExport.Sync(unit.Context);
        }
      }
    }

    int level;
    private WposProductGroups ProcessProductGroupNode(XElement currentNode, List<WposProductGroups> productGroups)
    {
      var nodes = currentNode.Elements("ProductGroup");
      var concentratorProductGroupID = currentNode.Try(c => c.Attribute("ID").Value, string.Empty);

      WposProductGroups productGroup = null;
      //depth first
      foreach (var node in nodes)
      {
        var parentProductGroup = ProcessProductGroupNode(node, productGroups);

        var parentConcentratorProductGroupID = node.Try(c => c.Attribute("ID").Value, string.Empty);

        if (!String.IsNullOrEmpty(parentConcentratorProductGroupID))
        {
          productGroup = productGroups.FirstOrDefault(c => c.ConcentratorProductGroupID == concentratorProductGroupID
                                                           &&
                                                           c.ParentConcentratorProductGroupID == parentProductGroup.ParentConcentratorProductGroupID
                                                           );
        }
        else
        {
          productGroup = productGroups.FirstOrDefault(c => c.ConcentratorProductGroupID == concentratorProductGroupID
                                                           &&
                                                           string.IsNullOrEmpty(c.ParentConcentratorProductGroupID)
            );
        }

        if (productGroup == null)
        {
          level++;
          productGroup = new WposProductGroups
          {
            ConcentratorProductGroupID = concentratorProductGroupID,
            ParentConcentratorProductGroupID = parentConcentratorProductGroupID,
            Name = currentNode.Attribute("Name").Value,
            CreatedByID = 1,
            CreationTime = DateTime.Now,
            Level = level
          };
          productGroups.Add(productGroup);
        }
        return productGroup;
      }

      //self
      if (nodes.Count() == 0)
      {
        //root level
        productGroup = productGroups.FirstOrDefault(c => c.ConcentratorProductGroupID == concentratorProductGroupID
                                                           &&
                                                           string.IsNullOrEmpty(c.ParentConcentratorProductGroupID));
        if (productGroup == null)
        {
          productGroup = new WposProductGroups
          {
            ConcentratorProductGroupID = concentratorProductGroupID,
            ParentConcentratorProductGroupID = string.Empty,
            Name = currentNode.Attribute("Name").Value,
            CreatedByID = 1,
            CreationTime = DateTime.Now,
            Level = level
          };
          productGroups.Add(productGroup);
        }
        return productGroup;
      }
      return null;
    }
  }
}