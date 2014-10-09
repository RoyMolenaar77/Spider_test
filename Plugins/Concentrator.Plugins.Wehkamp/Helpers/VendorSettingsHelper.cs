using System;
using System.Collections.Generic;
using System.Globalization;
using Concentrator.Objects.Environments;
using log4net;
using PetaPoco;

namespace Concentrator.Plugins.Wehkamp.Helpers
{
  internal static class VendorSettingsHelper
  {
    private static readonly Dictionary<int, int> DaysToAddForShipmentCache = new Dictionary<int, int>(); 
    private static readonly Dictionary<int, string> AlliantieNaamCache = new Dictionary<int, string>();
    private static readonly Dictionary<int, string> SFTPCache = new Dictionary<int, string>();
    private static readonly Dictionary<int, string> PrivateKeyFilenameCache = new Dictionary<int, string>();
    private static readonly Dictionary<int, string> MerkNaamCache = new Dictionary<int, string>();
    private static readonly Dictionary<int, string> RetailPartnerCodeCache = new Dictionary<int, string>();
    private static readonly Dictionary<int, int> ConnectorIDCache = new Dictionary<int, int>();

    private static string _sentToWehkampAttributeID = string.Empty;
    private static string _sentToWehkampAsDummyAttributeID = string.Empty;
    private static string _resendPriceUpdateToWehkampAttributeID = string.Empty;
    private static string _resendProductInformationToWehkampAttributeID = string.Empty;

    internal static string GetSFTPSetting(int vendorID)
    {
      if (SFTPCache.ContainsKey(vendorID))
        return SFTPCache[vendorID];

      using (var database = new Database(Environments.Current.Connection, "System.Data.SqlClient"))
      {
        database.CommandTimeout = 30;
        var sftp = database.ExecuteScalar<string>(string.Format("SELECT Value FROM VendorSetting WHERE SettingKey = 'SFTPSetting' AND VendorID = {0}", vendorID));
        SFTPCache.Add(vendorID, sftp);

        return sftp;
      }
    }
    internal static string GetPrivateKeyFilenameSetting(int vendorID)
    {
      if (PrivateKeyFilenameCache.ContainsKey(vendorID))
        return PrivateKeyFilenameCache[vendorID];

      using (var database = new Database(Environments.Current.Connection, "System.Data.SqlClient"))
      {
        database.CommandTimeout = 30;
        var privateKeyFilename = database.ExecuteScalar<string>(string.Format("SELECT Value FROM VendorSetting WHERE SettingKey = 'PrivateKeyFileName' AND VendorID = {0}", vendorID));
        PrivateKeyFilenameCache.Add(vendorID, privateKeyFilename);

        return privateKeyFilename;
      }
    }


    internal static string GetAlliantieName(int vendorID)
    {
      if (AlliantieNaamCache.ContainsKey(vendorID))
        return AlliantieNaamCache[vendorID];

      using (var database = new Database(Environments.Current.Connection, "System.Data.SqlClient"))
      {
        database.CommandTimeout = 30;
        var alliantieNaam = database.ExecuteScalar<string>(string.Format("SELECT Value FROM VendorSetting WHERE SettingKey = 'WehkampAlliantieName' AND VendorID = {0}", vendorID));
        AlliantieNaamCache.Add(vendorID, alliantieNaam);

        return alliantieNaam;
      }
    }
    internal static string GetMerkName(int vendorID)
    {
      if (MerkNaamCache.ContainsKey(vendorID))
        return MerkNaamCache[vendorID];

      using (var database = new Database(Environments.Current.Connection, "System.Data.SqlClient"))
      {
        database.CommandTimeout = 30;
        var merkNaam = database.ExecuteScalar<string>(string.Format("SELECT Value FROM VendorSetting WHERE SettingKey = 'Merknaam' AND VendorID = {0}", vendorID));
        MerkNaamCache.Add(vendorID, merkNaam);
        
        return merkNaam;
      }
    }
    internal static string GetRetailPartnerCode(int vendorID)
    {
      if (RetailPartnerCodeCache.ContainsKey(vendorID))
        return RetailPartnerCodeCache[vendorID];

      using (var database = new Database(Environments.Current.Connection, "System.Data.SqlClient"))
      {
        database.CommandTimeout = 30;
        var retailPartnerCode = database.ExecuteScalar<string>(string.Format("SELECT Value FROM VendorSetting WHERE SettingKey = 'WehkampRetailPartnerCode' AND VendorID = {0}", vendorID));
        RetailPartnerCodeCache.Add(vendorID, retailPartnerCode);

        return retailPartnerCode;
      }
    }
    internal static string GetSentToWehkampAsDummyAttributeID()
    {
      if (!string.IsNullOrEmpty(_sentToWehkampAsDummyAttributeID)) return _sentToWehkampAsDummyAttributeID;

      using (var database = new Database(Environments.Current.Connection, "System.Data.SqlClient"))
      {
        database.CommandTimeout = 30;
        var attributeID = database.ExecuteScalar<string>("SELECT AttributeID from ProductAttributeMetaData WHERE AttributeCode = 'SentToWehkampAsDummy'");
        _sentToWehkampAsDummyAttributeID = attributeID;

        return attributeID;
      }
    }
    internal static string GetSentToWehkampAttributeID()
    {
      if (!string.IsNullOrEmpty(_sentToWehkampAttributeID)) return _sentToWehkampAttributeID;

      using (var database = new Database(Environments.Current.Connection, "System.Data.SqlClient"))
      {
        database.CommandTimeout = 30;
        var attributeID = database.ExecuteScalar<string>("SELECT AttributeID from ProductAttributeMetaData WHERE AttributeCode = 'SentToWehkamp'");
        _sentToWehkampAttributeID = attributeID;

        return attributeID;
      }
    }
    internal static string GetResendPriceUpdateToWehkampAttributeID()
    {
      if (!string.IsNullOrEmpty(_resendPriceUpdateToWehkampAttributeID)) return _resendPriceUpdateToWehkampAttributeID;

      using (var database = new Database(Environments.Current.Connection, "System.Data.SqlClient"))
      {
        database.CommandTimeout = 30;
        var attributeID = database.ExecuteScalar<string>("SELECT AttributeID from ProductAttributeMetaData WHERE AttributeCode = 'ResendPriceUpdateToWehkamp'");
        _resendPriceUpdateToWehkampAttributeID = attributeID;

        return attributeID;
      }
    }

    internal static string GetResendProductInformationToWehkampAttributeID()
    {
      if (!string.IsNullOrEmpty(_resendProductInformationToWehkampAttributeID)) return _resendProductInformationToWehkampAttributeID;

      using (var database = new Database(Environments.Current.Connection, "System.Data.SqlClient"))
      {
        database.CommandTimeout = 30;
        var attributeID = database.ExecuteScalar<string>("SELECT AttributeID from ProductAttributeMetaData WHERE AttributeCode = 'ResendProductInformationToWehkamp'");
        _resendProductInformationToWehkampAttributeID = attributeID;

        return attributeID;
      }
    }

    internal static int GetConnectorIDByVendorID(int vendorID)
    {
      if (ConnectorIDCache.ContainsKey(vendorID))
        return ConnectorIDCache[vendorID];

      using (var database = new Database(Environments.Current.Connection, "System.Data.SqlClient"))
      {
        database.CommandTimeout = 30;
        var connectorID =  database.ExecuteScalar<int>(string.Format("SELECT top 1 ConnectorID from Connectorpublicationrule WHERE VendorID = '{0}'", vendorID));
        ConnectorIDCache.Add(vendorID, connectorID);

        return connectorID;
      }
    }

    internal static List<int> GetVendorIDsToExportToWehkamp(ILog log)
    {
      List<int> vendorIDs;

      try
      {
        using (var database = new Database(Environments.Current.Connection, "System.Data.SqlClient"))
        {
          database.CommandTimeout = 30;
          vendorIDs = database.Fetch<int>("SELECT VendorID FROM VendorSetting WHERE SettingKey = 'ExportToWehkamp' AND Value = 1");
        }
      }
      catch (Exception exception)
      {
        log.Error("Error while retrieving VendorID's to export to Wehkamp.", exception);
        vendorIDs = null;
      }
      return vendorIDs;
    }

    internal static int GetDaysToAddForShipmentDate(int vendorID)
    {
      if (DaysToAddForShipmentCache.ContainsKey(vendorID))
        return DaysToAddForShipmentCache[vendorID];

      using (var database = new Database(Environments.Current.Connection, "System.Data.SqlClient"))
      {
        database.CommandTimeout = 30;
        var days = database.ExecuteScalar<int>(string.Format("SELECT Value FROM VendorSetting WHERE SettingKey = 'ShipmentNotificationAddDaysFromToday' AND VendorID = {0}", vendorID));
        DaysToAddForShipmentCache.Add(vendorID, days);

        return days;
      }
    }

    internal static int ExportOnlyProductsOlderThanXXXMinutes(int vendorID)
    {
      using (var database = new Database(Environments.Current.Connection, "System.Data.SqlClient"))
      {
        database.CommandTimeout = 30;
        var minutes = database.ExecuteScalar<int>(string.Format("SELECT Value FROM VendorSetting WHERE SettingKey = 'ExportOnlyProductsOlderThanXXXMinutes' AND VendorID = {0}", vendorID));

        return minutes;
      }
    }

    internal static DateTime GetLastPriceExportDateTime(int vendorID)
    {
      using (var database = new Database(Environments.Current.Connection, "System.Data.SqlClient"))
      {
        database.CommandTimeout = 30;
        var result = database.ExecuteScalar<string>(string.Format("SELECT Value FROM VendorSetting WHERE SettingKey = 'PricesExportedDatetime' AND VendorID = {0}", vendorID));

        if (string.IsNullOrEmpty(result))
        {
          result = "2013-10-01 00:00:00";
        }

        return DateTime.ParseExact(result, MessageHelper.ISO8601, CultureInfo.InvariantCulture);
      }
    }
    internal static bool SetLastPriceExportDateTime(int vendorID, DateTime lastExportDateTime)
    {
      using (var database = new Database(Environments.Current.Connection, "System.Data.SqlClient"))
      {
        database.CommandTimeout = 30;
        var result = database.Execute(string.Format("UPDATE VendorSetting SET Value = '{0}' WHERE SettingKey = 'PricesExportedDatetime' AND VendorID = {1}", lastExportDateTime.ToString(MessageHelper.ISO8601, CultureInfo.InvariantCulture), vendorID));
        return result == 1;
      }
    }
  }
}

