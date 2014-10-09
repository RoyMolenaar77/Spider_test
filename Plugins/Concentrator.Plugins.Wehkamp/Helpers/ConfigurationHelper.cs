using System.Configuration;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Linq;

namespace Concentrator.Plugins.Wehkamp.Helpers
{
  internal static class ConfigurationHelper
  {
    private static readonly Configuration ConfigurationData;

    static ConfigurationHelper()
    {
      if (ConfigurationData == null)
      {
        ConfigurationData = ConfigurationManager.OpenExeConfiguration(Assembly.GetExecutingAssembly().Location);
      }

      if (!Directory.Exists(WehkampFilesRootFolder)) { Directory.CreateDirectory(WehkampFilesRootFolder); }

      foreach (var vendorID in VendorIdsToDownload)
      {
        CheckFolders(vendorID.ToString(CultureInfo.InvariantCulture));
      }
    }

    internal static void CheckFolders(string vendorID)
    {
      if (!Directory.Exists(Path.Combine(IncomingFilesRootFolder, vendorID, vendorID))) { Directory.CreateDirectory(Path.Combine(IncomingFilesRootFolder, vendorID)); }
      if (!Directory.Exists(Path.Combine(OutgoingFilesRootFolder, vendorID))) { Directory.CreateDirectory(Path.Combine(OutgoingFilesRootFolder, vendorID)); }
      if (!Directory.Exists(Path.Combine(FailedFilesRootFolder, vendorID))) { Directory.CreateDirectory(Path.Combine(FailedFilesRootFolder, vendorID)); }
      if (!Directory.Exists(Path.Combine(ArchivedRootFolder, vendorID))) { Directory.CreateDirectory(Path.Combine(ArchivedRootFolder, vendorID)); }

      if (!Directory.Exists(Path.Combine(ProductInformationRootFolder, vendorID))) { Directory.CreateDirectory(Path.Combine(ProductInformationRootFolder, vendorID)); }
      if (!Directory.Exists(Path.Combine(ProductAttributesRootFolder, vendorID))) { Directory.CreateDirectory(Path.Combine(ProductAttributesRootFolder, vendorID)); }
      if (!Directory.Exists(Path.Combine(ProductRelationRootFolder, vendorID))) { Directory.CreateDirectory(Path.Combine(ProductRelationRootFolder, vendorID)); }

      if (!Directory.Exists(Path.Combine(ShipmentNotificationRootFolder, vendorID))) { Directory.CreateDirectory(Path.Combine(ShipmentNotificationRootFolder, vendorID)); }
      if (!Directory.Exists(Path.Combine(ShipmentConfirmationRootFolder, vendorID))) { Directory.CreateDirectory(Path.Combine(ShipmentConfirmationRootFolder, vendorID)); }
      if (!Directory.Exists(Path.Combine(SalesOrderRootFolder, vendorID))) { Directory.CreateDirectory(Path.Combine(SalesOrderRootFolder, vendorID)); }
      if (!Directory.Exists(Path.Combine(ProductPricesRootFolder, vendorID))) { Directory.CreateDirectory(Path.Combine(ProductPricesRootFolder, vendorID)); }
      if (!Directory.Exists(Path.Combine(StockMutationRootFolder, vendorID))) { Directory.CreateDirectory(Path.Combine(StockMutationRootFolder, vendorID)); }
      if (!Directory.Exists(Path.Combine(StockPhotoRootFolder, vendorID))) { Directory.CreateDirectory(Path.Combine(StockPhotoRootFolder, vendorID)); }

      if (!Directory.Exists(Path.Combine(ProductMediaRootFolder, vendorID))) { Directory.CreateDirectory(Path.Combine(ProductMediaRootFolder, vendorID)); }
      if (!Directory.Exists(Path.Combine(StockReturnRequestRootFolder, vendorID))) { Directory.CreateDirectory(Path.Combine(StockReturnRequestRootFolder, vendorID)); }
      if (!Directory.Exists(Path.Combine(StockReturnRequestResponseRootFolder, vendorID))) { Directory.CreateDirectory(Path.Combine(StockReturnRequestResponseRootFolder, vendorID)); }
      if (!Directory.Exists(Path.Combine(StockReturnConfirmationRootFolder, vendorID))) { Directory.CreateDirectory(Path.Combine(StockReturnConfirmationRootFolder, vendorID)); }

    }

    internal static bool ListFTPFilesCheck { get { return bool.Parse(ConfigurationData.AppSettings.Settings["ListFTPFilesCheck"].Value); } }
    internal static string FTPMediaDirectory { get { return ConfigurationData.AppSettings.Settings["FTPMediaDirectory"].Value; } }

    internal static string WehkampFilesRootFolder { get { return ConfigurationData.AppSettings.Settings["WehkampFilesRootFolder"].Value; } }

    internal static string IncomingFilesRootFolder { get { return Path.Combine(WehkampFilesRootFolder, ConfigurationData.AppSettings.Settings["IncomingFilesRootFolder"].Value); } }
    internal static string OutgoingFilesRootFolder { get { return Path.Combine(WehkampFilesRootFolder, ConfigurationData.AppSettings.Settings["OutgoingFilesRootFolder"].Value); } }
    internal static string FailedFilesRootFolder { get { return Path.Combine(WehkampFilesRootFolder, ConfigurationData.AppSettings.Settings["FailedFilesRootFolder"].Value); } }
    internal static string ArchivedRootFolder { get { return Path.Combine(WehkampFilesRootFolder, ConfigurationData.AppSettings.Settings["ArchivedFilesRootFolder"].Value); } }

    internal static string ProductInformationRootFolder { get { return Path.Combine(WehkampFilesRootFolder, ConfigurationData.AppSettings.Settings["ProductInformationRootFolder"].Value); } }
    internal static string ProductAttributesRootFolder { get { return Path.Combine(WehkampFilesRootFolder, ConfigurationData.AppSettings.Settings["ProductAttributesRootFolder"].Value); } }
    internal static string ProductRelationRootFolder { get { return Path.Combine(WehkampFilesRootFolder, ConfigurationData.AppSettings.Settings["ProductRelationRootFolder"].Value); } }

    internal static string ShipmentNotificationRootFolder { get { return Path.Combine(WehkampFilesRootFolder, ConfigurationData.AppSettings.Settings["ShipmentNotificationRootFolder"].Value); } }
    internal static string ShipmentConfirmationRootFolder { get { return Path.Combine(WehkampFilesRootFolder, ConfigurationData.AppSettings.Settings["ShipmentConfirmationRootFolder"].Value); } }
    internal static string SalesOrderRootFolder { get { return Path.Combine(WehkampFilesRootFolder, ConfigurationData.AppSettings.Settings["SalesOrderRootFolder"].Value); } }
    internal static string ProductPricesRootFolder { get { return Path.Combine(WehkampFilesRootFolder, ConfigurationData.AppSettings.Settings["ProductPricesRootFolder"].Value); } }
    internal static string StockMutationRootFolder { get { return Path.Combine(WehkampFilesRootFolder, ConfigurationData.AppSettings.Settings["StockMutationRootFolder"].Value); } }
    internal static string StockPhotoRootFolder { get { return Path.Combine(WehkampFilesRootFolder, ConfigurationData.AppSettings.Settings["StockPhotoRootFolder"].Value); } }

    internal static string ProductMediaRootFolder { get { return Path.Combine(WehkampFilesRootFolder, ConfigurationData.AppSettings.Settings["ProductMediaRootFolder"].Value); } }
    internal static string StockReturnRequestRootFolder { get { return Path.Combine(WehkampFilesRootFolder, ConfigurationData.AppSettings.Settings["StockReturnRequestRootFolder"].Value); } }
    internal static string StockReturnRequestResponseRootFolder { get { return Path.Combine(WehkampFilesRootFolder, ConfigurationData.AppSettings.Settings["StockReturnRequestResponseRootFolder"].Value); } }
    internal static string StockReturnConfirmationRootFolder { get { return Path.Combine(WehkampFilesRootFolder, ConfigurationData.AppSettings.Settings["StockReturnConfirmation"].Value); } }

    internal static int[] VendorIdsToDownload { get { return ConfigurationData.AppSettings.Settings["VendorIdsToDownload"].Value.Split(',').Select(int.Parse).ToArray(); } }
  }
}
