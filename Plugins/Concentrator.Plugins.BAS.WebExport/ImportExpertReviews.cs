using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using Concentrator.Objects.ConcentratorService;
using Concentrator.Objects;
using System.Xml.Linq;
using System.Transactions;
using Concentrator.Web.ServiceClient.AssortmentService;
using Concentrator.Objects.Models.Connectors;

namespace Concentrator.Plugins.BAS.WebExport
{
  [ConnectorSystem(1)]
  public class ImportExpertReviews : ConcentratorPlugin
  {
    public override string Name
    {
      get { return "Alatest expert reviews export plugin"; }
    }

    private void ProcessXML()
    {

      log.Info("Start import of expert product reviews");
      AssortmentServiceSoapClient client = new AssortmentServiceSoapClient();

      var message = client.GetExpertProductReviews();

      XDocument reviews = XDocument.Parse(message);

      var productReviews = (from p in reviews.Element("ExpertReviews").Element("Products").Elements("Product")
                            select new
                            {

                              ConcentratorID = int.Parse(p.Attribute("ID").Value),
                              Reviews = from r in p.Element("Reviews").Elements("Review")
                                        let sourceID = int.Parse(r.Attribute("SourceID").Value)
                                        select new
                                        {
                                          CustomID = r.Attribute("ConcentratorID").Value,
                                          SourceID = sourceID,
                                          isSummary = bool.Parse(r.Attribute("isSummary").Value),
                                          Author = r.Element("Author").Value,
                                          Date = r.Element("Date").Value,
                                          Title = r.Element("Title").Value,
                                          Summary = r.Element("Summary").Value,
                                          Verdict = r.Element("Verdict").Value,
                                          ReviewURL = r.Element("ReviewURL").Value,
                                          RatingImageURL = r.Element("RatingImageURL").Value,
                                          Rating = r.Element("Rating").Try<XElement, int?>(c => int.Parse(c.Value), null),
                                          Source = (from s in reviews.Element("ExpertReviews").Element("Sources").Elements("Source")
                                                    where int.Parse(s.Attribute("ID").Value) == sourceID
                                                    select new
                                                    {
                                                      Name = s.Element("Name").Value,
                                                      LanguageCode = s.Element("LanguageCode").Value,
                                                      CountryCode = s.Element("CountryCode").Value,
                                                      SourceURL = s.Element("SourceURL").Value,
                                                      SourceLogoURL = s.Element("SourceLogoURL").Value,
                                                      SourceRank = s.Element("SourceRank").Try<XElement, int?>(c => int.Parse(c.Value), null)
                                                    }).FirstOrDefault()
                                        }

                            }
                              );

      foreach (Connector connector in base.Connectors.Where(x => ((ConnectorType)x.ConnectorType).Has(ConnectorType.Reviews)))
      {
          
        log.InfoFormat("Start expert product review import for connector {0}", connector.Name);
        DateTime start = DateTime.Now;

        using (WebsiteDataContext context = new WebsiteDataContext(ConfigurationManager.ConnectionStrings[connector.Connection].ConnectionString))
        {
          var productIDs = (from p in context.Products select p.ConcentratorProductID).ToList();
          var sources = (from s in context.ReviewSources select s).ToList();
          productIDs.Sort();

          foreach (var product in productReviews)
          {
            if (!productIDs.Contains(product.ConcentratorID)) continue;

            var productItem = (from p in context.Products where p.ConcentratorProductID == product.ConcentratorID select p).FirstOrDefault();

            foreach (var review in product.Reviews)
            {
              #region Source
              var src = sources.FirstOrDefault(c => c.CustomSourceID == review.SourceID);
              if (src == null)
              {
                src = new ReviewSource
                {
                  CustomSourceID = review.SourceID
                };
                context.ReviewSources.InsertOnSubmit(src);
                sources.Add(src);
              }
              src.CountryCode = review.Source.CountryCode;
              src.LanguageCode = review.Source.LanguageCode;
              src.Name = review.Source.Name;
              src.SourceLogoUrl = review.Source.SourceLogoURL;
              src.SourceRank = review.Source.SourceRank;
              src.SourceUrl = review.Source.SourceURL;
              #endregion

              var reviewItem =
                (from s in context.AlatestExpertReviews where s.CustomID == review.CustomID select s).FirstOrDefault();

              if (reviewItem == null)
              {
                reviewItem = new AlatestExpertReview()
                {
                  Product = productItem,
                  ReviewSource = src,
                  CustomID = review.CustomID
                };
                context.AlatestExpertReviews.InsertOnSubmit(reviewItem);
              }

              reviewItem.Author = review.Author;

              DateTime date;
              DateTime.TryParse(review.Date, out date);
              reviewItem.Date = review.Date.Try<string, DateTime?>(c => DateTime.Parse(c), null);
              reviewItem.isSummary = review.isSummary;
              reviewItem.Rating = review.Rating;
              reviewItem.RatingImageURL = review.RatingImageURL;
              reviewItem.ReviewURL = review.ReviewURL;
              reviewItem.Summary = review.Summary;
              reviewItem.Title = review.Title;
              reviewItem.Verdict = review.Verdict;
            }
          }
          try
          {
            using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Suppress, TimeSpan.FromMinutes(5)))
            {
              context.SubmitChanges();
            }
          }
          catch (Exception e)
          {
            log.AuditError("Import of alatest expert reviews failed for connector: " + connector.Name, e,
                           "Import Expert Reviews");
          }

        }
         
      }
    }


    protected override void Process()
    {
      try
      {
        ProcessXML();
        log.AuditComplete("Finished import of Expert Product Reviews");
      }
      catch (Exception e)
      {
        log.AuditFatal("Import of Expert Product Reviews failed", e, "Alates Expert product reviews");
      }
    }
  }
}
