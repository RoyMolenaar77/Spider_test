using System.IO;
using System.Reflection;

namespace Concentrator.Plugins.DhlTool.Helpers
{
  internal static class QueryHelper
  {
    internal static string GetDhlOrdersQuery()
    {
      var assembly = Assembly.GetExecutingAssembly();
      const string resourceName = "Concentrator.Plugins.DhlTool.Queries.GetDhlOrders.txt";

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

    internal static string UpdateRetrievedDhlOrders()
    {
      var assembly = Assembly.GetExecutingAssembly();
      const string resourceName = "Concentrator.Plugins.DhlTool.Queries.UpdateRetrievedDhlOrders.txt";

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

    internal static string UpdateStatusSuccessInMagentoQuery()
    {
      var assembly = Assembly.GetExecutingAssembly();
      const string resourceName = "Concentrator.Plugins.DhlTool.Queries.UpdateStatusSuccessInMagento.txt";

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

    internal static string UpdateStatusFailureInMagentoQuery()
    {
      var assembly = Assembly.GetExecutingAssembly();
      const string resourceName = "Concentrator.Plugins.DhlTool.Queries.UpdateStatusFailureInMagento.txt";

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
