using System;
using System.Configuration;

namespace Concentrator.Plugins.PFA.Configuration
{
  /// <summary>
  /// Represents the information for handling the receipt notifications.
  /// </summary>
  public sealed class SapphReceivedNotificationElement : ConfigurationElement
  {
    private const String ArchiveUriKey = "archiveUri";

    /// <summary>
    /// Gets the archiving location.
    /// </summary>
    [ConfigurationProperty ( ArchiveUriKey )]
    public String ArchiveUri
    {
      get
      {
        return ( String ) this [ ArchiveUriKey ];
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

