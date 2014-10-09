using System.IO;
using System.Reflection;
using PetaPoco;

namespace Concentrator.Plugins.Wehkamp.Helpers
{
  internal static class CommunicatorHelper
  {
    internal static string GetSequenceNumber(int vendorID)
    {
      var sequenceNumber = -1;
      using (var database = new Database(Objects.Environments.Environments.Current.Connection, "System.Data.SqlClient"))
      {
        database.CommandTimeout = 30;
        database.EnableAutoSelect = false;
        sequenceNumber = database.SingleOrDefault<int>("exec GenerateVendorSequenceNumber @VendorID", new object[] { new { VendorID = vendorID } });
      }
      return sequenceNumber.ToString("D9");
    }

    internal static string GetWehkampPrivateKeyFilename(int vendorID)
    {
      var assembly = Assembly.GetExecutingAssembly();
      return Path.Combine(Path.GetDirectoryName(assembly.Location), "Privatekey", VendorSettingsHelper.GetPrivateKeyFilenameSetting(vendorID));
    }
  }
}
