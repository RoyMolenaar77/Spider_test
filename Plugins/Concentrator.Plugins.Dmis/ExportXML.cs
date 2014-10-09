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
using Concentrator.Objects.Models.Contents;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.DataAccess.Repository;


namespace Concentrator.Plugins.Dmis
{
  public class ExportXML : ConcentratorPlugin
  {
    public override string Name
    {
      get { return "Dmis Xml Exporter"; }
    }

    private Dictionary<int, Dictionary<int, string>> ProductGroupLanguageList =
  new Dictionary<int, Dictionary<int, string>>();

    //private XElement GetProductGroupHierarchy(IEnumerable<ContentProductGroup> groups, int languageid, IRepository<ProductGroupLanguage> repoLanguage, List<ProductGroupMapping> mappingList)
    //{
    //  XElement element = new XElement("ProductGroupHierarchy");

    //  foreach (var mapping in groups)
    //  {
    //    var map = mappingList.Where(x => x.ProductGroupMappingID == mapping.ProductGroupMappingID).FirstOrDefault();

    //    if (map == null)
    //      map = mapping.ProductGroupMapping;

    //    GetProductGroupHierarchy(map, element, languageid, repoLanguage, mappingList);
    //  }
    //  return element;
    //}

    private XElement GetProductGroupHierarchy(ProductGroupMapping mapping, XElement element, int languageid, IRepository<ProductGroupLanguage> repoLanguage, List<ProductGroupMapping> mappingList)
    {
      Dictionary<int, string> productGroupLanguageList = null;

      if (!ProductGroupLanguageList.ContainsKey(languageid))
      {
        ProductGroupLanguageList.Add(languageid,
                                     repoLanguage.GetAllAsQueryable(x => x.LanguageID == languageid).ToDictionary(
                                       x => x.ProductGroupID,
                                       y => y.Name));
      }

      ProductGroupLanguageList.TryGetValue(languageid, out productGroupLanguageList);

      string name = "Unknown";
      productGroupLanguageList.TryGetValue(mapping.ProductGroupID, out name);

      var el = new XElement("ProductGroup", new XAttribute("ID", mapping.ProductGroupID),
                            new XAttribute("Name", name),
                            new XAttribute("Dmis", string.IsNullOrEmpty(mapping.CustomProductGroupLabel) ? string.Empty : mapping.CustomProductGroupLabel),
                            new XAttribute("Index", mapping.Score.HasValue ? mapping.Score.Value : mapping.ProductGroup.Score));
      element.Add(el);

      if (mapping.ParentProductGroupMappingID.HasValue)
      {
        GetProductGroupHierarchy(mapping.ParentMapping, el, languageid, repoLanguage, mappingList);
      }

      return element;
    }



    protected override void Process()
    {

      foreach (Connector connector in base.Connectors.Where(x => ((ConnectorType)x.ConnectorType).Has(ConnectorType.FileExport)))
      {

        if (connector.ConnectorID != 666)
          continue;

        //#region Assortiment

        string path = "F:/tmp/BEX";

        //if (!string.IsNullOrEmpty(path))
        //{
        //  #region ProductGroups
        //  using (var unit = GetUnitOfWork())
        //  {
        //    var repoContentProductGroup = unit.Scope.Repository<ContentProductGroup>().Include(c => c.ProductGroupMapping);
        //    var repoProductGroupMapping = unit.Scope.Repository<ProductGroupMapping>().Include(c => c.ProductGroup);

        //    var pra = repoContentProductGroup.GetAll(x => x.ConnectorID == 666).ToList();
        //    List<ProductGroupMapping> mappingslist = repoProductGroupMapping.GetAll(x => x.ConnectorID == 666).ToList();


        //    List<int> pgList = new List<int>();

        //    //foreach(var a in assortment){
        //    //  var el = GetProductGroupHierarchy(a, 2, unit.Scope.Repository<ProductGroupLanguage>(), mappingslist);
        //    //}


        //    XElement element = new XElement("ProductGroupHierarchy");
        //    foreach (var mapping in pra)
        //    {

        //      if (!pgList.Contains(mapping.ProductGroupMappingID))
        //      {

        //        var map = mappingslist.Where(x => x.ProductGroupMappingID == mapping.ProductGroupMappingID && mapping.ConnectorID == 666).FirstOrDefault();

        //        if (map == null)
        //          continue;

        //        GetProductGroupHierarchy(map, element, 2, unit.Scope.Repository<ProductGroupLanguage>(), mappingslist);

        //        pgList.Add(mapping.ProductGroupMappingID);
        //      }
        //    }

        //    var elements = (from e in element.Elements("ProductGroup")
        //                    select e).Distinct();

        //    XElement element2 = new XElement("ProductGroupHierarchy");
        //    element2.Add(elements);

        //    XDocument productGroupsDoc = new XDocument();
        //    productGroupsDoc.Add(element2);

        //    string fileName2 = string.Format("ProductGroups_{0}_{1}.xml", connector.ConnectorID, DateTime.Now.ToString("yyyyMMddHHmmss"));
        //    productGroupsDoc.Save(Path.Combine(path, fileName2));


        //    using (var stream = new MemoryStream())
        //    {
        //      productGroupsDoc.Save(stream, SaveOptions.DisableFormatting);
        //      //DmisUtilty.UploadFiles(connector, stream, fileName2, log);
        //    }
        //  }
        //  #endregion

        //  AssortmentServiceSoapClient soap = new AssortmentServiceSoapClient();

        //  XDocument products;
        //  products = XDocument.Parse(soap.GetAdvancedPricingAssortment(connector.ConnectorID, false, false, null, null, true, 2));

        //  #region Products
        //  string fileName = string.Format("Assortment_{0}_{1}.xml", connector.ConnectorID, DateTime.Now.ToString("yyyyMMddHHmmss"));
        //  products.Save(Path.Combine(path, fileName));

        //  using (var stream = new MemoryStream())
        //  {
        //    products.Save(stream, SaveOptions.DisableFormatting);
        //    DmisUtilty.UploadFiles(connector, stream, fileName, log);
        //  }
        //  #endregion
        //}
        //else
        //{
        //  log.AuditCritical(string.Format("Export XML failed for {0}, XmlExportPath not set", connector.Name));
        //}

        //log.DebugFormat("Finish Process XML Assortment import for {0}", connector.Name);

        //#endregion

        #region Images
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
        #endregion

          log.DebugFormat("Start Process XML Content export for {0} language {1}", connector.Name, 2);


          if (!string.IsNullOrEmpty(path))
          {
            AssortmentServiceSoapClient soap = new AssortmentServiceSoapClient();

            XDocument content = XDocument.Parse(soap.GetAssortmentContentDescriptionsByLanguage(connector.ConnectorID, null, 2));

            string contentFile = Path.Combine(path, string.Format("Content_{0}_{1}.xml", connector.ConnectorID, 2));

            if (File.Exists(contentFile))
              File.Delete(contentFile);

            content.Save(contentFile, SaveOptions.DisableFormatting);

            XDocument attributes = XDocument.Parse(soap.GetAttributesAssortmentByLanguage(connector.ConnectorID, null, null, 2));

            string attributeFile = Path.Combine(path, string.Format("Attribute_{0}_{1}.xml", connector.ConnectorID, 2));

            if (File.Exists(attributeFile))
              File.Delete(attributeFile);

            attributes.Save(attributeFile, SaveOptions.DisableFormatting);
          }
          else
          {
            log.AuditCritical(string.Format("Export XML failed for {0}, XmlExportPath not set", connector.Name));
          }

          log.DebugFormat("Finish Process XML Content import for {0} language {1}", connector.Name, 2);

      }
    }
  }
}
