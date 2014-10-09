using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects;
using System.Xml;
using System.IO;
using System.Xml.Linq;
using System.Web;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects.Models.Contents;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Models.Complex;

namespace Concentrator.Plugins.BAS.WebExport
{
  public class ConcentratorHelper
  {
    public static string GetAttributesAssortment(int connectorID, List<int> ProductIDs, DateTime? LastUpdate, IScope scope)
    {


      var _connectorRepo = scope.Repository<Connector>();
      var _funcRepo = ((IFunctionScope)scope).Repository();
      Connector con = _connectorRepo.GetSingle(x => x.ConnectorID == connectorID);

      if (con == null)
        return String.Empty;

      try
      {
        int languageID = con.ConnectorSettings.GetValueByKey<int>("LanguageID", 2);


        var contentAss = (from a in scope.Repository<Content>().GetAllAsQueryable()
                          join pg in scope.Repository<ContentProductGroup>().GetAllAsQueryable() on new { a.ProductID, a.ConnectorID } equals
                            new { pg.ProductID, pg.ConnectorID }
                          join pgn in scope.Repository<ProductGroupLanguage>().GetAllAsQueryable() on pg.ProductGroupMapping.ProductGroupID equals pgn.ProductGroupID
                          where a.ConnectorID == connectorID && pgn.LanguageID == languageID
                          select new Attributes
                                   {
                                     ManufacturerID = a.Product.VendorItemNumber,
                                     ProductID = a.ProductID,
                                     BrandID = a.Product.BrandID
                                   });

        var assortment = (from a in contentAss.InRange(x => x.ProductID, 2500, ProductIDs)
                          select a).Distinct().ToList();


        List<Attributes> list = new List<Attributes>();

        //IEnumerable<AttributeResult> attributes = null;

        //attributes = _funcRepo.GetProductAttributes(null, languageID, con.ConnectorID, LastUpdate).ToList();

        var attributes = (from a in scope.Repository<ContentAttribute>().GetAll()
                             where a.LanguageID == languageID
                              && (a.ConnectorID == null || a.ConnectorID == con.ConnectorID)
                             select a).ToList();

        var attributeDictionary =
            (from a in attributes
             group a by a.ProductID into grouped
             select grouped).ToDictionary(x => x.Key, y => y);

        foreach (var product in assortment)
        {
          var p = product;

          IGrouping<int?, ContentAttribute> productAttributes = null;
          attributeDictionary.TryGetValue(p.ProductID, out productAttributes);
          if (productAttributes != null)
            p.AttributeList = productAttributes.ToList();

          list.Add(p);
        }

        var attributeGroups = (from att in list.SelectMany(x => x.AttributeList)
                               select new ImportAttributeGroup
                                        {
                                          AttributeGroupID = att.GroupID,
                                          AttributeGroupIndex = att.GroupIndex,
                                          AttributeGroupName = att.GroupName
                                        }).Distinct().ToList();



        using (var stringWriter = new StringWriterWithEncoding(Encoding.UTF8))
        {

          using (var writer = new XmlTextWriter(stringWriter) { Formatting = Formatting.None })
          {

            writer.WriteStartDocument(true);
            writer.WriteStartElement("ProductAttributes");

            foreach (var a in list.Where(x => x.AttributeList.Count > 0))
            {
              var groupIds = a.AttributeList.Select(x => x.GroupID).Distinct().ToList();
              var productAttributeGroups =
                attributeGroups.Where(x => groupIds.Contains(x.AttributeGroupID));

              #region Element

              var element = new XElement("ProductAttribute",
                                         new XAttribute("ProductID", a.ProductID),
                                         new XAttribute("ManufacturerID", a.ManufacturerID),
                                         new XElement("Brand", new XAttribute("BrandID", a.BrandID)),
                                         new XElement("AttributeGroups", (from pag in productAttributeGroups


                                                                          select new XElement("AttributeGroup",
                                                                                              new XAttribute(
                                                                                                "AttributeGroupID",
                                                                                                pag.AttributeGroupID),
                                                                                              new XAttribute(
                                                                                                "AttributeGroupIndex",
                                                                                                pag.
                                                                                                  AttributeGroupIndex),
                                                                                              new XAttribute("Name",
                                                                                                             pag.
                                                                                                               AttributeGroupName))
                                                                         )),


                                         new XElement("Attributes",
                                                      (from p in a.AttributeList
                                                       //where !string.IsNullOrEmpty(p.AttributeValue)
                                                       select new XElement("Attribute",
                                                                           new XAttribute("AttributeID",
                                                                                          p.AttributeID),
                                                                           new XAttribute("AttributeCode",
                                                                                          !string.IsNullOrEmpty(
                                                                                             p.AttributeCode)
                                                                                            ? p.AttributeCode
                                                                                            : string.Empty),
                                                                           new XAttribute("KeyFeature", p.IsVisible),
                                                                           new XAttribute("Index", p.OrderIndex),
                                                                           new XAttribute("IsSearchable",
                                                                                          p.IsSearchable),
                                                                           new XAttribute("AttributeGroupID",
                                                                                          p.GroupID),
                                                                           new XAttribute("NeedsUpdate",
                                                                                          p.NeedsUpdate),
                                                                           new XElement("Name",
                                                                                        HttpUtility.HtmlEncode(
                                                                                          p.AttributeName)),
                                                                           new XElement("Value",
                                                                                        !string.IsNullOrEmpty(
                                                                                           p.AttributeValue)
                                                                                          ? p.AttributeValue
                                                                                          : string.Empty),
                                                                           new XElement("Sign",
                                                                                        HttpUtility.HtmlEncode(
                                                                                          !string.IsNullOrEmpty(
                                                                                             p.Sign)
                                                                                            ? p.Sign
                                                                                            : string.Empty))
                                                         )).Distinct())
                );


              element.WriteTo(writer);
              writer.Flush();

              #endregion
            }
            writer.WriteEndElement(); // ProductAttributes
            writer.WriteEndDocument();
            writer.Flush();
          }

          stringWriter.Flush();
          return stringWriter.ToString();
        }

      }

      catch (Exception ex)
      {
        throw ex;
      }

    }
  }

  public class StringWriterWithEncoding : StringWriter
  {
    Encoding encoding;
    public StringWriterWithEncoding(StringBuilder builder, Encoding encoding)
      : base(builder)
    {
      this.encoding = encoding;
    }

    public StringWriterWithEncoding(Encoding encoding)
      : base()
    {
      this.encoding = encoding;
    }
    public override Encoding Encoding { get { return encoding; } }
  }
}
