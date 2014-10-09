using System;
using System.Configuration;
using System.Linq;
using System.Web.Services;
using System.Xml;
using System.Xml.Linq;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.Models.Contents;
using Concentrator.Objects.Models.Products;
using Concentrator.Web.Services.Base;

//namespace Concentrator.Vendors.Jumbo
namespace Concentrator.Web.Services
{
  /// <summary>
  /// Summary description for MediaService
  /// </summary>
  [WebService(Namespace = "http://Concentrator.diract-it.nl/")]
  [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
  [System.ComponentModel.ToolboxItem(false)]
  // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
  // [System.Web.Script.Services.ScriptService]
  public class MediaService : BaseConcentratorService
  {
    public MediaService() : base() { }


    [WebMethod(Description = "Custom export for all media from a specific media type", BufferResponse = false)]
    public string ExportMedia(int connectorID, string type)
    {
      return ExportMediaFull(connectorID, type, 0);
    }

    [WebMethod(Description = "Custom export for all media from a specific media type by connector", BufferResponse = false)]
    public string ExportMediaFull(int connectorID, string type, int productID)
    {
      using (var unit = GetUnitOfWork())
      {
        try
        {
          XmlDocument xml = new XmlDocument();
          Connector con = unit.Scope.Repository<Connector>().GetSingle(c => c.ConnectorID == connectorID);
          type = type.IfNullOrEmpty("").ToLower();
          con.ThrowIfNull("Connector can't be null");

          var imageUrl = new Uri(ConfigurationManager.AppSettings["MediaImageUrl"].ToString());

          var productMedia = (from c in unit.Scope.Repository<ProductMedia>().GetAll()
                              join z in unit.Scope.Repository<ContentProductGroup>().GetAll(z => z.IsCustom == true && z.ConnectorID == connectorID) on c.ProductID equals z.ProductID
                              where productID > 0 ? c.ProductID == productID : true
                              &&
                              c.MediaType.Type.ToLower().Contains(type)
                              select new
                              {
                                c.ProductID,
                                c.MediaPath,
                                c.MediaID,
                                c.FileName
                              }).ToList();

          XElement media = new XElement("Productmedia",
                    from p in productMedia
                    orderby p.MediaID
                    select new XElement("Media",
                              new XElement("MediaID", p.MediaID),
                              new XElement("MediaPath", p.MediaPath),
                              new XElement("FileName", p.FileName))
                    );


          xml.LoadXml(media.ToString());
          return xml.InnerXml;
        }
        catch (Exception ex)
        {
          XElement doc = new XElement("ProductImages",
             new XElement("Error", ex.StackTrace));

          return doc.ToString();
        }
      }
    }

    [WebMethod(Description = "Custom export for all media from a specific media type", BufferResponse = false)]
    public string ExportMediaFullByType(int connectorID, string type)
    {
      using (var unit = GetUnitOfWork())
      {
        try
        {
          XmlDocument xml = new XmlDocument();
          Connector con = unit.Scope.Repository<Connector>().GetSingle(c => c.ConnectorID == connectorID);
          type = type.IfNullOrEmpty("").ToLower();
          con.ThrowIfNull("Connector can't be null");

          var language = con.ConnectorLanguages.FirstOrDefault();
          var pref = con.PreferredConnectorVendors.FirstOrDefault(x => x.isPreferred);

          var imageUrl = new Uri(ConfigurationManager.AppSettings["MediaImageUrl"].ToString());

          var productMedia = (from c in unit.Scope.Repository<ProductMedia>().GetAll()
                              let description = c.Product.ProductDescriptions.OrderByDescending(x => x.LanguageID == language.LanguageID).FirstOrDefault()                              
                              let va = c.Product.VendorAssortments.OrderByDescending(x => x.VendorID == pref.VendorID).FirstOrDefault()                              
                             where c.MediaType.Type.ToLower().Contains(type)
                              select new
                              {
                                c.ProductID,
                                c.MediaPath,
                                c.MediaID,
                                c.FileName,
                                ProductName = description != null ? description.ProductName : va != null ? va.ShortDescription : string.Empty,
                                CustomItemNumber = va != null ? va.CustomItemNumber : string.Empty
                              }).ToList();

          XElement media = new XElement("Productmedia",
                    from p in productMedia
                    orderby p.MediaID
                    select new XElement("Media",
                              new XElement("MediaID", p.MediaID),
                              new XElement("MediaPath", p.MediaPath),
                              new XElement("FileName", p.FileName),
                              new XAttribute("CustomItemNumber", p.CustomItemNumber),
                              new XAttribute("ProductName", p.ProductName))

                    );


          xml.LoadXml(media.ToString());
          return xml.InnerXml;
        }
        catch (Exception ex)
        {
          XElement doc = new XElement("ProductImages",
             new XElement("Error", ex.StackTrace));

          return doc.ToString();
        }
      }
    }
  }
}