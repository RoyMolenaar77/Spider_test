using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Concentrator.Objects;
using Concentrator.Objects.Service;

namespace Concentrator.Plugins.AlatestProductReview
{
  public class AlatestContentUpdate : ConcentratorPlugin
  {
    public override string Name
    {
      //get { throw new NotImplementedException(); }
      get { return "Alatest update service"; }
    }

    protected override void Process()
    {
    }

    private void ImportFeed(ConcentratorDataContext context, XDocument reviewsDoc)
    {
      var products = (from c in context.VendorAssortments
                      select c.CustomItemNumber).Distinct().ToList();

      var reviewSources = (from rs in context.ReviewSources
                           select rs).ToList();

      products.Sort();

      var rev = (from pr in reviewsDoc.Element("reviewsfeed").Element("products").Elements("product")
                 let root = reviewsDoc.Element("reviewsfeed")
                 let alid = pr.Element("alid").Value
                 let reviews = reviewsDoc.Element("reviewsfeed").Element("reviews").Elements("review").Where(c => c.Element("alid").Value == alid)
                 select new
                 {
                   CustomItemNumber = (from r in root.Element("productids").Elements("productid").Where(c => c.Element("alid").Value == alid)
                                       select r.Element("idvalue").Value).FirstOrDefault(),
                   ReviewsCollection = (from m in reviews
                                        select new
                                        {
                                          masterLineID = m.Element("masterlineid").Try(c => c.Value, string.Empty),
                                          title = m.Try(c => c.Element("testtitle").Value, string.Empty),
                                          sourceTestRating = m.Try<XElement, int?>(c => int.Parse(c.Element("sourcetestrating").Value), null),
                                          language = m.Try(c => c.Element("language").Value, string.Empty),
                                          author = m.Try(c => c.Element("author").Value, string.Empty),
                                          date = m.Try<XElement, DateTime?>(c => DateTime.Parse(c.Element("testdate").Value), null),
                                          source = (from s in reviewsDoc.Element("reviewsfeed").Element("sources").Elements("source")
                                                    where s.Element("sourceid").Value == m.Element("sourceid").Value
                                                    select new
                                                    {
                                                      sourceID = s.Element("sourceid").Try<XElement, int>(c => int.Parse(c.Value, 0)),
                                                      name = s.Element("name").Value,
                                                      countryCode = s.Try(c => c.Element("countrycode").Value, string.Empty),
                                                      languageCode = s.Try(c => c.Element("languagecode").Value, string.Empty),
                                                      sourceURL = s.Try(c => c.Element("sourcewww").Value, string.Empty),
                                                      sourceLogo = s.Try(c => c.Element("sourcelogo").Value, string.Empty),
                                                      sourceRank = s.Try<XElement, int?>(c => int.Parse(c.Element("sourcerank").Value), null)

                                                    }).FirstOrDefault(),
                                          advanced = (m.Attribute("issummary") != null && m.Attribute("issummary").Try(c => bool.Parse(c.Value), false)) ?
                                                     new
                                                     {
                                                       ratingImageURL = m.Try(c => c.Element("ratingimage").Value, string.Empty),
                                                       summary = m.Try(c => c.Element("testsummary").Value, string.Empty),
                                                       verdict = m.Try(c => c.Element("testverdict").Value, string.Empty),
                                                       reviewUrl = m.Try(c => c.Element("reviewurl").Value, string.Empty)
                                                     } : null

                                        })




                 });

    }
  }
}
