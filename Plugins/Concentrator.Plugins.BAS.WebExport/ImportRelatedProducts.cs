using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Service;
using Concentrator.Plugins.BAS.WebExport;
using Spider.Objects.Concentrator;

namespace Spider.WebsiteImport
{
  public class ImportRelatedProducts : ConcentratorPlugin
  {
    public override string Name
    {
      get { return "Import Related Products"; }
    }

    protected override void Process()
    {
      ConcentratorSelectorService.SelectorServiceSoapClient serviceClient = new Spider.WebsiteImport.ConcentratorSelectorService.SelectorServiceSoapClient();
      using (WebsiteDataContext context = new WebsiteDataContext())
      {
        try
        {
          log.InfoFormat("Starting import of related products");
          ProcessRelatedProducts(context, serviceClient, 1);
          context.SubmitChanges();
          log.InfoFormat("Finished import of related products");

        }
        catch (Exception e)
        {
          log.Error("Importing of related products for connector " + Name + "failed ", e);
        }
      }
    }


    public void ProcessRelatedProducts(WebsiteDataContext context, ConcentratorSelectorService.SelectorServiceSoapClient client, int connectorID)
    {
      
      var selectorIDs = (from s in context.Selectors select s.SelectorID).ToList();
      foreach (int selectorID in selectorIDs)
      {
        var products = client.FindProduct(selectorID, string.Empty, connectorID);
        foreach (var manufacturerID in products)
        {
          #region Product

          var product = (from p in context.Products where p.ManufacturerID == manufacturerID select p).FirstOrDefault();

          if (product == null) continue;

          #region related products

          var relatedProducts = client.FindCompatibleProducts(selectorID, product.ManufacturerID, connectorID);

          foreach (var relatedProductID in relatedProducts)
          {
            var relatedProduct = (from rp in context.Products where rp.ManufacturerID == relatedProductID.ToString() select rp).FirstOrDefault();

            if (relatedProduct == null) continue;//if no product exists move on to next one

            //insert related product 
            RelatedProduct relatedProd = (from rp in context.RelatedProducts
                                          where
                                          rp.ProductID == product.ProductID
                                          &&
                                          rp.RelatedProductID == relatedProduct.ProductID
                                          select rp).FirstOrDefault();

            if (relatedProd == null)
              context.RelatedProducts.InsertOnSubmit(new RelatedProduct
                                                       {
                                                         ProductID = product.ProductID,
                                                         RelatedProductID = relatedProduct.ProductID,
                                                         SelectorID = selectorID
                                                       });

          }
          #endregion

          #endregion
        }

      }
    }
  }
}
