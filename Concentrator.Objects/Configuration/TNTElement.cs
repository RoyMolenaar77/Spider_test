using System;
using System.Configuration;

namespace Concentrator.Plugins.PFA.Configuration
{
  /// <summary>
  /// Represents the information for a FTP-connection.
  /// </summary>
  public sealed class StockMutationImportElement : ConfigurationElement
  {
    private const String ArchiveUriKey = "archiveUri";

    /// <summary>
    /// Gets the archiving location.
    /// </summary>
    [ConfigurationProperty ( ArchiveUriKey , DefaultValue = "file://Archive" )]
    public String ArchiveUri
    {
      get
      {
        return ( String ) this [ ArchiveUriKey ];
      }
    }

    private const String VendorNameKey = "vendorName";

    /// <summary>
    /// Gets the name of the vendor.
    /// </summary>
    [ConfigurationProperty ( VendorNameKey , DefaultValue = "PFA AT" )]
    public String VendorName
    {
      get
      {
        return ( String ) this[ VendorNameKey ];
      }
    }

    private const String VendorStockTypeKey = "vendorStockType";

    /// <summary>
    /// Gets the type of the vendor stock.
    /// </summary>
    [ConfigurationProperty ( VendorStockTypeKey , DefaultValue = "Webshop" )]
    public String VendorStockType
    {
      get
      {
        return ( String ) this[ VendorStockTypeKey ];
      }
      set
      {
        this[ VendorStockTypeKey ] = value;
      }
    }

    private const String SourceUriKey = "sourceUri";

    /// <summary>
    /// The location of the stock mutation files.
    /// </summary>
    [ConfigurationProperty ( SourceUriKey )]
    public String SourceUri
    {
      get
      {
        return ( String ) this [ SourceUriKey ];
      }
    }

    private const String ValidationFileNameKey = "validationFileName";

    /// <summary>
    /// Gets the file name of the XML-schema used for validation.
    /// </summary>
    [ConfigurationProperty ( ValidationFileNameKey , DefaultValue = "" )]
    public String ValidationFileName
    {
      get
      {
        return this[ ValidationFileNameKey ] as String;
      }
    }
  }
}

