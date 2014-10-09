using System;
using System.Collections.Generic;
using Concentrator.Core.Services.Models.Helper;
using Concentrator.Core.Services.Models.Products;
using Concentrator.Objects.Environments;
using PetaPoco;

namespace Concentrator.Core.Services.Helpers
{
  internal class ProductHelper
  {
    internal readonly ProductAttributeHelper AttributeHelper = new ProductAttributeHelper();
    internal readonly ProductPriceHelper PriceHelper = new ProductPriceHelper();
    internal readonly ProductStockHelper StockHelper = new ProductStockHelper();
    internal readonly RelatedProductHelper RelatedProductHelper = new RelatedProductHelper();
    internal readonly ChildProductHelper ChildProductHelper = new ChildProductHelper();

    internal List<ProductBase> GetAllProducts(int connectorID, List<string> includes, int? skipCount = -1, int? takeCount = -1, DateTime? lastModified = null)
    {
      List<ProductBase> result;

      using (var db = new Database(Environments.Current.Connection, "System.Data.SqlClient"))
      {
        var lmt = lastModified.HasValue ? string.Format("'{0}'", lastModified.Value.ToString("yyyy-MM-dd HH:mm:ss")) : "NULL";
        var skip = skipCount.HasValue ? skipCount.Value : -1;
        var take = takeCount.HasValue ? takeCount.Value : -1;

        var sql = string.Format(QueryHelper.GetProductsQuery(), connectorID, lmt, skip, take);
        var products = db.Fetch<ProductData>(sql);

        result = ProcessProducts(connectorID, products, includes);
      }

      return result;
    }

   
    private List<ProductBase> ProcessProducts(int connectorID, IEnumerable<ProductData> products, List<string> includes)
    {
      var counter = 0;
      var result = new List<ProductBase>();

      var start = DateTime.Now;
      foreach (var product in products)
      {
        counter++;

        ProductBase p;

        if (product.IsConfigurable)
        {
          p = new ConfigurableProduct();
          ((ConfigurableProduct)p).ChildProducts = ChildProductHelper.GetChildProducts(connectorID, product.ProductID, this);
        }
        else
        {
          p = new SimpleProduct();
          if (includes.Contains("prices")) 
            ((SimpleProduct)p).Prices = PriceHelper.GetProductPrices(connectorID, product.ProductID);

          if (includes.Contains("stock")) 
            ((SimpleProduct)p).Stock = StockHelper.GetProductStocks(connectorID, product.ProductID);
        }


        FillProductWithBaseAttributes(p, product, connectorID, true, true);

        result.Add(p);

        if(counter % 100 == 0)
        {
          System.Diagnostics.Debug.WriteLine("{0} Processed {1} products", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), counter);
        }
      }
      var end = DateTime.Now;

      System.Diagnostics.Debug.WriteLine("Total processingtime: {0} seconds", end.Subtract(start).TotalSeconds);

      return result;
    }

    internal void FillProductWithBaseAttributes(ProductBase p, dynamic product, int connectorID, bool includingAttributes, bool includingRelatedProduct)
    {
      p.ProductID = product.ProductID;
      p.VendorItemNumber = product.VendorItemNumber;
      p.CustomItemNumber = product.CustomItemNumber;
      p.LastModificationTime = product.LastModificationTime;
      p.IsNonAssortmentItem = product.IsNonAssortmentItem;

      if (includingAttributes)
      {
        p.Attributes = AttributeHelper.GetProductAttributes(connectorID, product.ProductID);
      }

      if (includingRelatedProduct)
      {
        p.RelatedProduct = RelatedProductHelper.GetRelatedProducts(connectorID, product.ProductID);
      }
    }
  }
}
