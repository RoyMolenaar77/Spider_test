using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.ConcentratorService;
using Concentrator.Objects;
using System.Transactions;
using System.Timers;
using System.Xml.Linq;
using System.Net;
using System.Configuration;
using System.IO;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.Models.Products;

namespace Concentrator.Plugins.Alatest
{
  public class AlatestContentUpdater : ConcentratorPlugin
  {
    public override string Name
    {
      get { return "Alatest Updater Service"; }
    }

    protected override void Process()
    {
      try
      {
        log.AuditInfo("Starting import of Alatest reviews");
        using (var unit = GetUnitOfWork())
        {
          ImportFeed(unit, GetReview());

          using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Suppress, TimeSpan.FromMinutes(5)))
          {
            unit.Save();
          }
        }
      }
      catch (Exception e)
      {
        log.AuditFatal("Alatest import failed", e);
      }
      log.AuditInfo("Finished Alatest reviews import");
    }

    private void ImportFeed(IUnitOfWork unit, XDocument reviewsDoc)
    {
      var assortmentRepo = unit.Scope.Repository<VendorAssortment>();

      var products = assortmentRepo.GetAllAsQueryable().Select(c => c.CustomItemNumber).Distinct().ToList();

      var reviewSources = unit.Scope.Repository<ReviewSource>().GetAll().ToList();

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

      foreach (var productReview in rev)
      {
        string customItemNumber = productReview.CustomItemNumber;
        if (!products.Contains(customItemNumber))
          continue;

        var product = assortmentRepo.GetSingle(v => v.CustomItemNumber == customItemNumber).Product;

        foreach (var review in productReview.ReviewsCollection)
        {
          ReviewSource src = null;
          if (review.source != null)
          {
            #region source

            src = reviewSources.FirstOrDefault(c => c.CustomSourceID == review.source.sourceID);

            if (src == null)
            {
              src = new ReviewSource()
                      {
                        CustomSourceID = review.source.sourceID
                      };
              unit.Scope.Repository<ReviewSource>().Add(src);

              reviewSources.Add(src);
            }
            src.Name = review.source.name;
            src.LanguageCode = review.source.languageCode;
            src.SourceLogoUrl = review.source.sourceLogo;
            src.SourceUrl = review.source.sourceURL;
            src.SourceRank = review.source.sourceRank;
            src.CountryCode = review.source.countryCode;

            #endregion
          }
          #region review
          var repoReview = unit.Scope.Repository<ProductReview>();
          var rv = repoReview.GetSingle(r => r.CustomID == review.masterLineID);
                    
          if (rv == null)
          {
            rv = new ProductReview()
                   {
                     ReviewSource = src,
                     Product = product,
                     CustomID = review.masterLineID
                   };
           repoReview.Add(rv);
          }

          rv.Author = review.author;
          rv.Date = review.date;
          if (review.advanced != null)
          {
            rv.IsSummary = true;
            rv.Summary = review.advanced.summary;
            rv.Verdict = review.advanced.verdict;
            rv.RatingImageURL = review.advanced.ratingImageURL;
            rv.ReviewURL = review.advanced.reviewUrl;

          }
          rv.Rating = review.sourceTestRating;
          rv.SourceRating = review.sourceTestRating;
          #endregion

        }

      }
    }

    private XDocument GetReview()
    {
      var config = GetConfiguration().AppSettings.Settings;
      WebRequest request = WebRequest.Create(config["AlatestReviewContentUrl"].Value + "&key=" + config["AlatestReviewContentKey"].Value);

      using (StreamReader reader = new StreamReader(request.GetResponse().GetResponseStream()))
      {
        return new XDocument(XDocument.Parse(reader.ReadToEnd()));
      }
    }

  }
}
