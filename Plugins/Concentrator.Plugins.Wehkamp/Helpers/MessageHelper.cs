using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Concentrator.Objects.Environments;
using Concentrator.Plugins.Wehkamp.Enums;
using PetaPoco;

namespace Concentrator.Plugins.Wehkamp.Helpers
{
  internal static class MessageHelper
  {
    internal const string ISO8601 = "yyyy-MM-dd HH:mm:ss";
    private static readonly Dictionary<WehkampMessageType, string> MessageTypeLocation;

    static MessageHelper()
    {
      if (MessageTypeLocation == null)
      {
        MessageTypeLocation = new Dictionary<WehkampMessageType, string>();
        MessageTypeLocation[WehkampMessageType.ProductAttribute] = ConfigurationHelper.ProductAttributesRootFolder;
        MessageTypeLocation[WehkampMessageType.ProductInformation] = ConfigurationHelper.ProductInformationRootFolder;
        MessageTypeLocation[WehkampMessageType.ProductMedia] = ConfigurationHelper.ProductMediaRootFolder;
        MessageTypeLocation[WehkampMessageType.ProductPrice] = ConfigurationHelper.ProductPricesRootFolder;
        MessageTypeLocation[WehkampMessageType.ProductPriceUpdate] = ConfigurationHelper.ProductPricesRootFolder;
        MessageTypeLocation[WehkampMessageType.ProductRelation] = ConfigurationHelper.ProductRelationRootFolder;
        MessageTypeLocation[WehkampMessageType.SalesOrder] = ConfigurationHelper.SalesOrderRootFolder;
        MessageTypeLocation[WehkampMessageType.ShipmentConfirmation] = ConfigurationHelper.ShipmentConfirmationRootFolder;
        MessageTypeLocation[WehkampMessageType.ShipmentNotification] = ConfigurationHelper.ShipmentNotificationRootFolder;
        MessageTypeLocation[WehkampMessageType.StockMutation] = ConfigurationHelper.StockMutationRootFolder;
        MessageTypeLocation[WehkampMessageType.StockPhoto] = ConfigurationHelper.StockPhotoRootFolder;
        MessageTypeLocation[WehkampMessageType.StockReturnConfirmation] = ConfigurationHelper.StockReturnConfirmationRootFolder;
        MessageTypeLocation[WehkampMessageType.StockReturnRequest] = ConfigurationHelper.StockReturnRequestRootFolder;
        MessageTypeLocation[WehkampMessageType.StockReturnRequestConfirmation] = ConfigurationHelper.StockReturnRequestResponseRootFolder;

      }
    }

    /// <summary>
    /// Returns a list of all WehkampMessages filtered by status
    /// </summary>
    /// <param name="status">Filter</param>
    /// <returns>List of WehkampMessages</returns>
    internal static List<WehkampMessage> GetMessagesByStatus(WehkampMessageStatus status)
    {
      var sql = string.Format("SELECT MessageID, MessageType, Filename, Path, Status, Received, Sent, LastModified, Attempts, VendorID FROM WehkampMessage WHERE Status = {0}", (int)status);
      using (var db = new Database(Environments.Current.Connection, "System.Data.SqlClient"))
      {
        var results = db.Fetch<WehkampMessage>(sql);
        return results;
      }
    }

    internal static List<WehkampMessage> GetMessagesByStatusAndType(WehkampMessageStatus status, WehkampMessageType type)
    {
      var sql = string.Format("SELECT MessageID, MessageType, Filename, Path, Status, Received, Sent, LastModified, Attempts, VendorID FROM WehkampMessage WHERE Status = {0} AND MessageType = {1}", (int)status, (int)type);
      using (var db = new Database(Environments.Current.Connection, "System.Data.SqlClient"))
      {
        var results = db.Fetch<WehkampMessage>(sql);
        return results;
      }
    }

    /// <summary>
    /// Returns a list of all WehkampMessages
    /// </summary>
    /// <returns>List of WehkampMessages</returns>
    internal static List<WehkampMessage> GetMessages()
    {
      var sql = string.Format("SELECT MessageID, MessageType, Filename, Path, Status, Received, Sent, LastModified, Attempts, VendorID FROM WehkampMessage");
      using (var db = new Database(Environments.Current.Connection, "System.Data.SqlClient"))
      {
        var results = db.Fetch<WehkampMessage>(sql);
        return results;
      }
    }

    internal static WehkampMessage GetMessageByFilename(string file)
    {
      var sql = string.Format("SELECT MessageID, MessageType, Filename, Path, Status, Received, Sent, LastModified, Attempts, VendorID FROM WehkampMessage WHERE Filename ='{0}'", file.Replace("'", "''"));
      using (var db = new Database(Environments.Current.Connection, "System.Data.SqlClient"))
      {
        var result = db.FirstOrDefault<WehkampMessage>(sql);
        return result;
      }
    }

    internal static WehkampMessage GetMessage(decimal messageID)
    {
      var sql = string.Format("SELECT MessageID, MessageType, Filename, Path, Status, Received, Sent, LastModified, Attempts, VendorID FROM WehkampMessage WHERE MessageID = '{0}'", messageID);
      using (var db = new Database(Environments.Current.Connection, "System.Data.SqlClient"))
      {
        var result = db.FirstOrDefault<WehkampMessage>(sql);
        return result;
      }
    }

    /// <summary>
    /// Updates the status of a single message
    /// </summary>
    /// <param name="messageID">Message primary key</param>
    /// <param name="status">new status</param>
    internal static void UpdateMessageStatus(decimal messageID, WehkampMessageStatus status, PetaPoco.Database db = null)
    {
      var sql = string.Format("UPDATE WehkampMessage SET Status={1}, LastModified='{2}' WHERE MessageID = {0}", messageID, (int)status, DateTime.Now.ToUniversalTime().ToString(ISO8601, CultureInfo.InvariantCulture));
      ExecuteSQL(sql, db);
    }

    /// <summary>
    /// Updates the file path of a single message
    /// </summary>
    /// <param name="messageID">Message primary key</param>
    /// <param name="path">new path</param>
    internal static void UpdateMessagePath(decimal messageID, string path)
    {
      var sql = string.Format("UPDATE WehkampMessage SET Path='{1}', LastModified='{2}' WHERE MessageID = {0}", messageID, (path ?? "NULL").Replace("'", "''"), DateTime.Now.ToUniversalTime().ToString(ISO8601, CultureInfo.InvariantCulture));
      ExecuteSQL(sql);
    }

    /// <summary>
    /// Updates the received timestamp of the message
    /// </summary>
    /// <param name="messageID">Message primary key</param>
    /// <param name="received">received timestamp</param>
    internal static void UpdateMessageRecieved(decimal messageID, DateTime received)
    {
      var sql = string.Format("UPDATE WehkampMessage SET Received='{1}', LastModified='{2}' WHERE MessageID = {0}", messageID, received.ToString(ISO8601, CultureInfo.InvariantCulture), DateTime.Now.ToUniversalTime().ToString(ISO8601, CultureInfo.InvariantCulture));
      ExecuteSQL(sql);
    }

    /// <summary>
    /// Updates the sent timestamp of the message
    /// </summary>
    /// <param name="messageID">Message primary key</param>
    /// <param name="sent">sent timestamp</param>
    internal static void UpdateMessageSent(decimal messageID, DateTime sent)
    {
      var sql = string.Format("UPDATE WehkampMessage SET Sent='{1}', LastModified='{2}' WHERE MessageID = {0}", messageID, sent.ToString(ISO8601, CultureInfo.InvariantCulture), DateTime.Now.ToString(ISO8601, CultureInfo.InvariantCulture));
      ExecuteSQL(sql);
    }

    /// <summary>
    /// Increases the "Attempt" counter of a message by one
    /// </summary>
    /// <param name="messageID">Message primary key</param>
    internal static void UpdateMessageAttempt(decimal messageID)
    {
      var sql = string.Format("UPDATE WehkampMessage SET Attempts = Attempts + 1, LastModified='{1}' WHERE MessageID = {0}", messageID, DateTime.Now.ToUniversalTime().ToString(ISO8601, CultureInfo.InvariantCulture));
      ExecuteSQL(sql);
    }

    internal static void ResetMessageAttempt(decimal messageID)
    {
      var sql = string.Format("UPDATE WehkampMessage SET Attempts = 0, LastModified='{1}' WHERE MessageID = {0}", messageID, DateTime.Now.ToUniversalTime().ToString(ISO8601, CultureInfo.InvariantCulture));
      ExecuteSQL(sql);
    }

    /// <summary>
    /// Creates a new message
    /// </summary>
    /// <param name="type">Message Type</param>
    /// <param name="filename">Filename of Message data</param>
    /// <param name="vendorID">Vendor ID</param>
    /// <returns>Primary key of message, -1 if error</returns>
    internal static decimal InsertMessage(WehkampMessageType type, string filename, int vendorID, PetaPoco.Database db = null)
    {
      var disposeDatabase = (db == null);
      if (db == null)
      {
        db = new Database(Environments.Current.Connection, "System.Data.SqlClient");
      }

      var message = new WehkampMessage
      {
        MessageType = type,
        Filename = filename,
        Status = WehkampMessageStatus.Created,
          LastModified = DateTime.Now.ToUniversalTime(),
        VendorID = vendorID
      };

      var result = (decimal)db.Insert(message);
      
      if (disposeDatabase)
      {
        db.Dispose();
      }

      return result;
    }

    internal static WehkampMessageType DetermineType(string type)
    {
      switch (type.ToLower())
      {
        case "aankomst": return WehkampMessageType.ShipmentNotification;
        case "aankomstbevestiging": return WehkampMessageType.ShipmentConfirmation;
        case "administratievevoorraad": return WehkampMessageType.StockPhoto;
        case "artikeleigenschap": return WehkampMessageType.ProductAttribute;
        case "artikelinformatie": return WehkampMessageType.ProductInformation;
        case "artikelrelatie": return WehkampMessageType.ProductRelation;
        case "kassainformatie": return WehkampMessageType.SalesOrder;
        case "prijsaanpassing": return WehkampMessageType.ProductPriceUpdate;
        case "retouraanvraag": return WehkampMessageType.StockReturnRequest;
        case "retouruitslag": return WehkampMessageType.StockReturnRequestConfirmation;
        case "retourbevestiging": return WehkampMessageType.StockReturnConfirmation;
        case "voorraadmutaties": return WehkampMessageType.StockMutation;

        default: throw new NotImplementedException(string.Format("Message type {0} is not defined", type));
      }
    }

    internal static string GetMessageFolderByType(WehkampMessageType type, int vendorID)
    {
      if (MessageTypeLocation.ContainsKey(type))
        return Path.Combine(MessageTypeLocation[type], vendorID.ToString(CultureInfo.InvariantCulture));

      return Path.Combine(ConfigurationHelper.WehkampFilesRootFolder, vendorID.ToString(CultureInfo.InvariantCulture), "other");
    }

    internal static string[] GetMessageFolders(string filter, int vendorID)
    {
      var folders = new List<string>();
      foreach (var value in MessageTypeLocation.Values)
      {
        if (string.IsNullOrEmpty(filter) ||
            (value.Length > ConfigurationHelper.WehkampFilesRootFolder.Length + 1 && value.Substring(ConfigurationHelper.WehkampFilesRootFolder.Length + 1).StartsWith(filter)))
        {
          folders.Add(Path.Combine(value, vendorID.ToString(CultureInfo.InvariantCulture)));
        }
      }
      return folders.ToArray();
    }

    internal static void Error(WehkampMessage message)
    {
      UpdateMessageStatus(message.MessageID, WehkampMessageStatus.Error);
      File.Move(Path.Combine(message.Path, message.Filename), Path.Combine(ConfigurationHelper.FailedFilesRootFolder, message.VendorID.ToString(CultureInfo.InvariantCulture), message.Filename));
      UpdateMessagePath(message.MessageID, Path.Combine(ConfigurationHelper.FailedFilesRootFolder, message.VendorID.ToString(CultureInfo.InvariantCulture)));
    }

    internal static void Error(decimal messageid)
    {
      var message = GetMessage(messageid);
      Error(message);
    }

    internal static void Archive(WehkampMessage message)
    {
      UpdateMessageStatus(message.MessageID, WehkampMessageStatus.Archived);
      File.Move(Path.Combine(message.Path, message.Filename), Path.Combine(ConfigurationHelper.ArchivedRootFolder, message.VendorID.ToString(CultureInfo.InvariantCulture), message.Filename));
      UpdateMessagePath(message.MessageID, Path.Combine(ConfigurationHelper.ArchivedRootFolder, message.VendorID.ToString(CultureInfo.InvariantCulture)));
    }

    internal static void Archive(decimal messageid)
    {
      var message = GetMessage(messageid);
      Archive(message);
    }

    private static void ExecuteSQL(string sql, PetaPoco.Database db = null)
    {
      if (db == null)
      {
        using (db = new Database(Environments.Current.Connection, "System.Data.SqlClient"))
        {
          db.Execute(sql);
        }
      }
      else
      {
        db.Execute(sql);
      }
    }

    [TableName("WehkampMessage")]
    [PrimaryKey("MessageID")]
    internal class WehkampMessage
    {
      public decimal MessageID { get; set; }
      public WehkampMessageType MessageType { get; set; }
      public string Filename { get; set; }
      public string Path { get; set; }
      public WehkampMessageStatus Status { get; set; }
      public DateTime? Received { get; set; }
      public DateTime? Sent { get; set; }
      public DateTime? LastModified { get; set; }
      public int Attempts { get; set; }
      public int VendorID { get; set; }
    }


    internal enum WehkampMessageType
    {
      ProductPriceUpdate = 0,
      ProductAttribute = 1,
      ProductInformation = 2,
      ProductMedia = 3,
      ProductPrice = 4,
      ProductRelation = 5,
      SalesOrder = 6,
      ShipmentConfirmation = 7,
      ShipmentNotification = 8,
      StockMutation = 9,
      StockPhoto = 10,
      StockReturnConfirmation = 11,
      StockReturnRequestConfirmation = 12,
      StockReturnRequest = 13
    }
  }
}
