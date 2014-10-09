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

namespace Concentrator.Vendors.Jumbo
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


    [WebMethod(Description = "Custom Jumbo export for all media from a specific media type", BufferResponse = false)]
    public string ExportMedia(int connectorID, int typeID)
    {
      return ExportMediaFull(connectorID, typeID, 0);
    }

    [WebMethod(Description = "Custom Jumbo export for all media from a specific media type", BufferResponse = false)]
    public string ExportMediaFull(int connectorID, int typeID, int productID)
    {
      using (var unit = GetUnitOfWork())
      {
        try
        {
          XmlDocument xml = new XmlDocument();
          Connector con = unit.Scope.Repository<Connector>().GetSingle(c => c.ConnectorID == connectorID);
          con.ThrowIfNull("Connector can't be null");

          var imageUrl = new Uri(ConfigurationManager.AppSettings["JumboImageUrl"].ToString());

          var productMedia = (from c in unit.Scope.Repository<ProductMedia>().GetAll()
                              join z in unit.Scope.Repository<ContentProductGroup>().GetAll() on c.ProductID equals z.ProductID
                              where z.IsCustom == true //&& productID.HasValue ? c.ProductID == productID : true
                              select new
                              {
                                c.ProductID,
                                c.MediaPath,
                                c.MediaID,
                                c.FileName
                              });

          XElement media = new XElement("ProductMedia",
             new XElement("Media",
             from p in productMedia
             let uri = new Uri(imageUrl, p.MediaPath)
             select new XElement("MediaImage",
                 new XAttribute("MediaID", p.MediaID),
                 new XAttribute("FileName", p.FileName),
                 uri)));

          return media.ToString();
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