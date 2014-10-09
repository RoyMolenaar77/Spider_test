using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Concentrator.Plugins.PFA.Configuration
{
  public class PfaAmericaTodayConfiguration : ConfigurationSection
  {
    private const string ftpDestinationURLKey = "ftpDestinationUrl";
    [ConfigurationProperty(ftpDestinationURLKey)]
    public String FtpDestinationURL
    {
      get
      {
        return (String)this[ftpDestinationURLKey];
      }
    }

    private const string returnCostsProductKey = "returnCostsProduct";
    [ConfigurationProperty(returnCostsProductKey)]
    public String ReturnCostsProduct
    {
      get
      {
        return (string)this[returnCostsProductKey];
      }
    }


    private const string shipmentCostsProductKey = "shipmentCostsProduct";
    [ConfigurationProperty(shipmentCostsProductKey)]
    public String ShipmentCostsProduct
    {
      get
      {
        return (string)this[shipmentCostsProductKey];
      }
    }

    private const string kialaShipmentCostsProductKey = "kialaShipmentCostsProduct";
    [ConfigurationProperty(kialaShipmentCostsProductKey)]
    public String KialaShipmentCostsProduct
    {
      get
      {
        return (string)this[kialaShipmentCostsProductKey];
      }
    }

    private const string kialaReturnCostsProductKey = "kialaReturnCostsProduct";
    [ConfigurationProperty(kialaReturnCostsProductKey)]
    public String KialaReturnCostsProduct
    {
      get
      {
        return (string)this[kialaReturnCostsProductKey];
      }
    }

    private PfaAmericaTodayConfiguration()
    {
      SectionInformation.AllowExeDefinition = ConfigurationAllowExeDefinition.MachineToApplication;
    }

    /// <summary>
    /// Gets the configuration section for the <see cref="TNTFashionSection"/>.
    /// </summary>
    public static PfaAmericaTodayConfiguration Current
    {
      get;
      private set;
    }

    static PfaAmericaTodayConfiguration()
    {
      var configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

      var section = configuration.GetSection("pfaAmericaToday") as PfaAmericaTodayConfiguration;

      Current = section;
    }
  }
}
