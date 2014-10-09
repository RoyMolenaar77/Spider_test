using System;
using System.Configuration;
using System.Diagnostics;
using System.Linq;

namespace Concentrator.Plugins.PFA.Configuration
{
  /// <summary>
  /// Represents the configuration section for the Stock Mutation Import.
  /// </summary>
  public sealed class TNTFashionSection : ConfigurationSection
  {
    #region Archive Uri

    private const String ArchiveUriKey = "archiveUri";

    /// <summary>
    /// Gets the archiving location.
    /// </summary>
    [ConfigurationProperty(ArchiveUriKey)]
    public String ArchiveUri
    {
      get
      {
        return (String)this[ArchiveUriKey];
      }
    }

    #endregion

    #region Destination Uri

    private const String DestinationUriKey = "destinationUri";

    /// <summary>
    /// Gets the location of the import files.
    /// </summary>
    [ConfigurationProperty(DestinationUriKey)]
    public String DestinationUri
    {
      get
      {
        return (String)this[DestinationUriKey];
      }
    }

    #endregion

    #region Source Uri

    private const String SourceUriKey = "sourceUri";

    /// <summary>
    /// Gets the location of the import files.
    /// </summary>
    [ConfigurationProperty(SourceUriKey)]
    public String SourceUri
    {
      get
      {
        return (String)this[SourceUriKey];
      }
    }

    #endregion

    #region Return Notification Element

    private const String ReturnNotificationKey = "returnNotification";

    [ConfigurationProperty(ReturnNotificationKey, IsRequired = true)]
    public ReturnNotificationElement ReturnNotification
    {
      get
      {
        return (ReturnNotificationElement)this[ReturnNotificationKey];
      }
    }

    #endregion

    #region Shipment Notification Element

    private const String ShipmentNotificationKey = "shipmentNotification";

    [ConfigurationProperty(ShipmentNotificationKey, IsRequired = true)]
    public ShippingNotificationElement ShipmentNotification
    {
      get
      {
        return (ShippingNotificationElement)this[ShipmentNotificationKey];
      }
    }

    #endregion

    #region Stock Mutation Element

    private const String StockMutationKey = "stockMutation";

    [ConfigurationProperty(StockMutationKey, IsRequired = true)]
    public StockMutationElement StockMutationImport
    {
      get
      {
        return (StockMutationElement)this[StockMutationKey];
      }
    }

    #endregion

    private TNTFashionSection()
    {
      SectionInformation.AllowExeDefinition = ConfigurationAllowExeDefinition.MachineToApplication;
    }

    /// <summary>
    /// Gets the configuration section for the <see cref="TNTFashionSection"/>.
    /// </summary>
    public static TNTFashionSection Current
    {
      get;
      private set;
    }

    static TNTFashionSection()
    {
      Current = ConfigurationManager
        .OpenExeConfiguration(ConfigurationUserLevel.None)
        .GetSection("tntFashion") as TNTFashionSection;
    }
  }
}

