using System.IO;
using System.Reflection;

namespace Concentrator.Plugins.Wehkamp.Helpers
{
  internal static class QueryHelper
  {
    internal static string GetProductInformationQuery()
    {
      var assembly = Assembly.GetExecutingAssembly();
      const string resourceName = "Concentrator.Plugins.Wehkamp.Queries.ProductInformation.txt";

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
    internal static string GetProductInformationSizesQuery()
    {
      var assembly = Assembly.GetExecutingAssembly();
      const string resourceName = "Concentrator.Plugins.Wehkamp.Queries.ProductInformationSizes.txt";

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
    internal static string GetProductsWithPriceChangesQuery()
    {
      var assembly = Assembly.GetExecutingAssembly();
      const string resourceName = "Concentrator.Plugins.Wehkamp.Queries.ProductsWithPriceChanges.txt";

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
    internal static string GetShipmentNotificationProductInformationQuery()
    {
      var assembly = Assembly.GetExecutingAssembly();
      const string resourceName = "Concentrator.Plugins.Wehkamp.Queries.ShipmentNotificationProductInformation.txt";

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
    internal static string GetStockReturnRequestQuery()
    {
      var assembly = Assembly.GetExecutingAssembly();
      const string resourceName = "Concentrator.Plugins.Wehkamp.Queries.StockReturnRequest.txt";

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
