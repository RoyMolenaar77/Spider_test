using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Concentrator.Plugins.PFA.Configuration
{
	public class PfaSapphConfiguration : ConfigurationSection
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

		private PfaSapphConfiguration()
		{
			SectionInformation.AllowExeDefinition = ConfigurationAllowExeDefinition.MachineToApplication;
		}

		/// <summary>
		/// Gets the configuration section for the <see cref="TNTFashionSection"/>.
		/// </summary>
		public static PfaSapphConfiguration Current
		{
			get;
			private set;
		}

		static PfaSapphConfiguration()
		{
			var configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

			var section = configuration.GetSection("pfaSapph") as PfaSapphConfiguration;

			Current = section;
		}
	}
}
