using System;
using System.Configuration;
using System.Diagnostics;
using System.Linq;

namespace Concentrator.Plugins.PFA.Configuration
{
  /// <summary>
  /// Represents the configuration section for the Stock Mutation Import.
  /// </summary>
  public sealed class SapphTNTFashionSection : ConfigurationSection
  {
    #region Archive Uri
    
    private const String ArchiveUriKey = "archiveUri";

    /// <summary>
    /// Gets the archiving location.
    /// </summary>
    [ConfigurationProperty ( ArchiveUriKey )]
    public String ArchiveUri
    {
      get
      {
        return ( String ) this[ ArchiveUriKey ];
      }
    }

    #endregion

    #region Destination Uri

    private const String DestinationUriKey = "destinationUri";

    /// <summary>
    /// Gets the location of the import files.
    /// </summary>
    [ConfigurationProperty ( DestinationUriKey )]
    public String DestinationUri
    {
      get
      {
        return ( String ) this[ DestinationUriKey ];
      }
    }

    #endregion

    #region Source Uri

    private const String SourceUriKey = "sourceUri";

    /// <summary>
    /// Gets the location of the import files.
    /// </summary>
    [ConfigurationProperty ( SourceUriKey )]
    public String SourceUri
    {
      get
      {
        return ( String ) this[ SourceUriKey ];
      }
    }

    #endregion

    #region Return Notification Element

    private const String ReturnNotificationKey = "returnNotification";

    [ConfigurationProperty ( ReturnNotificationKey , IsRequired = true )]
    public SapphReturnNotificationElement ReturnNotification
    {
      get
      {
        return ( SapphReturnNotificationElement ) this[ ReturnNotificationKey ];
      }
    }

    #endregion

    #region Shipment Notification Element

    private const String ShipmentNotificationKey = "shipmentNotification";

    [ConfigurationProperty ( ShipmentNotificationKey , IsRequired = true )]
    public SapphShippingNotificationElement ShipmentNotification
    {
      get
      {
        return ( SapphShippingNotificationElement ) this[ ShipmentNotificationKey ];
      }
    }

    #endregion

    #region Received Notification Element

    //private const String ReceivedNotificationKey = "receivedNotification";

    //[ConfigurationProperty(ReceivedNotificationKey, IsRequired = true)]
    //public SapphReceivedNotificationElement ReceivedNotification
    //{
    //  get
    //  {
    //    return (SapphReceivedNotificationElement)this[ReceivedNotificationKey];
    //  }
    //}

    #endregion

    #region Stock Mutation Element

    private const String StockMutationKey = "stockMutation";

    [ConfigurationProperty ( StockMutationKey , IsRequired = true )]
    public SapphStockMutationElement StockMutationImport
    {
      get
      {
				return (SapphStockMutationElement)this[StockMutationKey];
      }
    }

    #endregion

    private SapphTNTFashionSection ( )
    {
      SectionInformation.AllowExeDefinition = ConfigurationAllowExeDefinition.MachineToApplication;
    }

    /// <summary>
    /// Gets the configuration section for the <see cref="TNTFashionSection"/>.
    /// </summary>
		public static SapphTNTFashionSection Current
    {
      get;
      private set;
    }

		static SapphTNTFashionSection()
    {
      Current = ConfigurationManager
        .OpenExeConfiguration ( ConfigurationUserLevel.None )
				.GetSection("sapphTntFashion") as SapphTNTFashionSection;
    }
  }
}

