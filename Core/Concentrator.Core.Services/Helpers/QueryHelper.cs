using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Concentrator.Core.Services.Helpers
{
  internal static class QueryHelper
  {
    internal static string GetProductsQuery()
    {
      var assembly = Assembly.GetExecutingAssembly();
      const string resourceName = "Concentrator.Core.Services.Queries.Products.txt";

      using (var stream = assembly.GetManifestResourceStream(resourceName))
      {
        if (stream != null)
        {
          using (var reader = new StreamReader(stream))
          {
            var result = reader.ReadToEnd();
            return result;
          }
        }
      }
      return string.Empty;
    }

    internal static string GetChildProductsQuery()
    {
      var assembly = Assembly.GetExecutingAssembly();
      const string resourceName = "Concentrator.Core.Services.Queries.ChildProducts.txt";

      using (var stream = assembly.GetManifestResourceStream(resourceName))
      {
        if (stream != null)
        {
          using (var reader = new StreamReader(stream))
          {
            var result = reader.ReadToEnd();
            return result;
          }
        }
      }
      return string.Empty;
    }

    internal static string GetRelatedProductsQuery()
    {
      var assembly = Assembly.GetExecutingAssembly();
      const string resourceName = "Concentrator.Core.Services.Queries.RelatedProducts.txt";

      using (var stream = assembly.GetManifestResourceStream(resourceName))
      {
        if (stream != null)
        {
          using (var reader = new StreamReader(stream))
          {
            var result = reader.ReadToEnd();
            return result;
          }
        }
      }
      return string.Empty;
    }

    internal static string GetProductAttributesQuery()
    {
      var assembly = Assembly.GetExecutingAssembly();
      const string resourceName = "Concentrator.Core.Services.Queries.ProductAttributes.txt";

      using (var stream = assembly.GetManifestResourceStream(resourceName))
      {
        if (stream != null)
        {
          using (var reader = new StreamReader(stream))
          {
            var result = reader.ReadToEnd();
            return result;
          }
        }
      }
      return string.Empty;
    }

    internal static string GetProductPricesQuery()
    {
      var assembly = Assembly.GetExecutingAssembly();
      const string resourceName = "Concentrator.Core.Services.Queries.ProductPrices.txt";

      using (var stream = assembly.GetManifestResourceStream(resourceName))
      {
        if (stream != null)
        {
          using (var reader = new StreamReader(stream))
          {
            var result = reader.ReadToEnd();
            return result;
          }
        }
      }
      return string.Empty;
    }

    internal static string GetProductStocksQuery()
    {
      var assembly = Assembly.GetExecutingAssembly();
      const string resourceName = "Concentrator.Core.Services.Queries.ProductStock.txt";

      using (var stream = assembly.GetManifestResourceStream(resourceName))
      {
        if (stream != null)
        {
          using (var reader = new StreamReader(stream))
          {
            var result = reader.ReadToEnd();
            return result;
          }
        }
      }
      return string.Empty;
    }



  }
}
