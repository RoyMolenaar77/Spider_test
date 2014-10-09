using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Concentrator.Plugins.PFA.Configuration
{
  public class PfaCoolcatConfiguration : ConfigurationSection
  {
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

    private const string targetGroupAttributeID = "targetGroupAttributeID";
    [ConfigurationProperty(targetGroupAttributeID)]
    public int TargetGroupAttributeID
    {
      get
      {
        return (int)this[targetGroupAttributeID];
      }
    }

    private const string inputCodeAttributeID = "inputCodeAttributeID";
    [ConfigurationProperty(inputCodeAttributeID)]
    public int InputCodeAttributeID
    {
      get
      {
        return (int)this[inputCodeAttributeID];
      }
    }

    private const string seasonAttributeID = "seasonAttributeID";
    [ConfigurationProperty(seasonAttributeID)]
    public int SeasonAttributeID
    {
      get
      {
        return (int)this[seasonAttributeID];
      }
    }

    private PfaCoolcatConfiguration()
    {
      SectionInformation.AllowExeDefinition = ConfigurationAllowExeDefinition.MachineToApplication;
    }

    /// <summary>
    /// Gets the configuration section for the <see cref="TNTFashionSection"/>.
    /// </summary>
    public static PfaCoolcatConfiguration Current
    {
      get;
      private set;
    }

    static PfaCoolcatConfiguration()
    {
      var configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

      var section = configuration.GetSection("pfaCoolcat") as PfaCoolcatConfiguration;

      Current = section;
    }
  }
}
