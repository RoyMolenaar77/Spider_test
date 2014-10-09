using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Configuration;

namespace Concentrator.Configuration
{
  public class ConcentratorSection : ConfigurationSection
  {
    private const String ConcentratorSectionName = "concentrator";

    public static ConcentratorSection Default
    {
      get;
      private set;
    }

    static ConcentratorSection()
    {
      var section = HttpContext.Current != null
        ? WebConfigurationManager.GetSection(ConcentratorSectionName)
        : ConfigurationManager.GetSection(ConcentratorSectionName);

      Default = section as ConcentratorSection ?? new ConcentratorSection();
    }
    
    private const String ManagementElementName = "management";

    [ConfigurationProperty(ManagementElementName)]
    public ManagementElement Management
    {
      get
      {
        return this[ManagementElementName] as ManagementElement ?? new ManagementElement();
      }
    }

    private const String SearchingElementName = "searching";

    [ConfigurationProperty(SearchingElementName)]
    public SearchingElement Searching
    {
      get
      {
        return this[SearchingElementName] as SearchingElement ?? new SearchingElement();
      }
    }

    //private const String TransferElementName = "transfer";

    //[ConfigurationProperty(TransferElementName)]
    //public TransferElement Transfer
    //{
    //  get
    //  {
    //    return this[TransferElementName] as TransferElement ?? new TransferElement();
    //  }
    //}
  }
}
