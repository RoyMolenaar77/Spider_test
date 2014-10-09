using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects;
using Concentrator.Objects.Connectors;
using Concentrator.Objects.Service;
using System.Xml;
using System.Configuration;
using System.Xml.Linq;
using System.Transactions;
using Concentrator.Web.ServiceClient.AssortmentService;
using Connector = Concentrator.Objects.Connectors.Connector;
namespace Concentrator.Plugins.BAS.WebExport
{
  [ConnectorSystem(1)]
  public class ImportProductReviews : ConcentratorPlugin
  {
    public override string Name
    {
      get { return "Import Product Reviews"; }
    }

    protected override void Process()
    {
      log.Info("Starting import of AlaTest product reviews");

      AssortmentServiceSoapClient client = new AssortmentServiceSoapClient();

      try
      {
        var reviews = client.GetProductReviews();

        foreach (Connector connector in base.Connectors.Where(c => c.ConnectorType.Has(ConnectorType.WebAssortment)))
        {
          using (WebsiteDataContext context = new WebsiteDataContext(ConfigurationManager.ConnectionStrings[connector.Connection].ConnectionString))
          {
            XDocument reviewsDoc = XDocument.Parse(reviews.OuterXml);
            foreach (var productReviewElement in reviewsDoc.Element("ProductReviews").Elements("Product"))
            {
              #region Product reviews
              var product = (from p in context.Products where p.ManufacturerID == productReviewElement.Attribute("vendorItemNumber").Value select p).FirstOrDefault();

              if (product != null)
              {
                var ProductReview = (from pr in context.AlatestReviews
                                     where pr.ProductID == product.ProductID
                                     select pr).FirstOrDefault();

                if (ProductReview == null)
                {
                  ProductReview = new AlatestReview
                                    {
                                      Product = product
                                    };
                  context.AlatestReviews.InsertOnSubmit(ProductReview);
                }
                ProductReview.Review = productReviewElement.Try(c => c.Element("Review").Value, string.Empty);
                ProductReview.ReviewSnippet = productReviewElement.Try(c => c.Element("ReviewSnippet").Value, string.Empty);
              }
              #endregion
            }
            try
            {
              using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Suppress, TimeSpan.FromMinutes(3)))
              {
                context.SubmitChanges();
                ts.Complete();
              }
            }
            catch (Exception e)
            {
              log.Error("Saving product reviews to the database failed for connector" + connector.Name, e);
            }
          }
        }

      }
      catch (Exception e)
      {
        log.Fatal("Downloading product reviews failed", e);
      }
    }
  }
}
