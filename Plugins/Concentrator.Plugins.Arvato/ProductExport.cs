using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using AuditLog4Net.Adapter;
using Concentrator.Objects;
using Concentrator.Web.ServiceClient.RED;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects.Models.Vendors;

namespace Concentrator.Plugins.Arvato
{
  public class ProductExport
  {

    public static void doResponse(RetailerSoapClient client, AuthenticationHeader authHeader, IUnitOfWork unit, IAuditLogAdapter log, string id, string feed)
    {
      log.AuditInfo("Start catalog response");

      var Feed = GetXML(feed);
     
      
      if (Feed != null)
      {
        var ConcentratorProducts = (from product in Feed.Element("Producten").Elements("Product")
                                    select new
                                             {
                                               ConnectorID = product.Element("ConnectorID").Value,
                                               ProductID = product.Element("ProductID").Value,
                                               MycomProductID = product.Element("MycomProductID").Value,
                                               URL = product.Element("URL").Value
                                             }).ToList();

        var Offers = unit.Scope.Repository<VendorAssortment>().GetAll(c => c.VendorID == 50).ToList();
                      

        //Create Responce XML
        Catalog_Response response = new Catalog_Response();
        response.Version = byte  .Parse("2");
        response.Reference_ID = id;

        Offer[] offers = new Offer[Offers.Count];
        Trial_Purchase_URL trial_url = new Trial_Purchase_URL();
         

        int counter = 0;
        foreach (var Offer in Offers)
        {

          Offer offer = new Offer();
          var prod = (from product in ConcentratorProducts
                      where product.ProductID == Offer.ProductID.ToString()
                      select product).FirstOrDefault();

          if (prod != null)
          {
            Catalog_URL url = new Catalog_URL();
            url.Value = prod.URL;
            offer.Catalog_URL = new Catalog_URL[1];
            offer.Catalog_URL[0] = url;
            offer.Offer_ID = Offer.ActivationKey;
            offer.Retailer_Item_Number = prod.ProductID;
            offer.Status = redState.Active;
            trial_url.Value = "";
            offer.Trial_Purchase_URL = new Trial_Purchase_URL[1];
            offer.Trial_Purchase_URL[0] = trial_url;
            offers[counter] = offer;
            counter++;
          }
        }
        response.Offer = offers;

        StringBuilder requestString = new StringBuilder();
        XmlWriterSettings settings = new XmlWriterSettings();
        settings.Encoding = Encoding.UTF8;

        using (XmlWriter xw = XmlWriter.Create(requestString, settings))
        {
          xw.WriteProcessingInstruction("xml", "version=\"1.0\" encoding=\"utf-8\"");
          XmlSerializer serializer = new XmlSerializer(response.GetType());
          XmlSerializerNamespaces nm = new XmlSerializerNamespaces();
          nm.Add("xsi", "http://www.w3.org/2001/XMLSchema-instance");
          serializer.Serialize(xw, response, nm);

          //XmlDocument document = new XmlDocument();
          //document.LoadXml(requestString.ToString());
        }

        if (client.CatalogResponseRequest(authHeader, requestString.ToString()) == "OK")
        {
          log.AuditInfo("Catalog response successfully");
        }
        else
        {
          log.AuditInfo("Catalog response failed");
        }
      }
    }

    public static XDocument GetXML(string url)
    {
    
      HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

      request.CookieContainer = new CookieContainer();
      request.Method = "GET";

      HttpWebResponse response = (HttpWebResponse)request.GetResponse();
      Stream receiveStream = response.GetResponseStream();

      if (!response.ContentType.Equals("text/xml") && !response.ContentType.Equals("text/xml; charset=utf-8"))
      {
        Console.WriteLine("Could not fetch xml file: {0}", url);
        return null;
      }

      XmlDocument xmlDoc = new XmlDocument();
      xmlDoc.Load(receiveStream);

      return XDocument.Parse(xmlDoc.OuterXml);
     
    }
  }
}
