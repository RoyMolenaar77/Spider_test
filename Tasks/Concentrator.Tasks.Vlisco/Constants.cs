using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

[assembly: InternalsVisibleTo("Concentrator.Tasks.Vlisco.Tests")]
namespace Concentrator.Tasks.Vlisco
{ 
	internal static class Constants
	{
		public static class Attribute
		{
			public const String Collection = "Collection";
			public const String ColorCode = "ColorCode";
			public const String ColorName = "ColorName";
			public const String LabelCode = "LabelCode";
			public const String MaterialCode = "MaterialCode";
			public const String OriginCode = "OriginCode";
			public const String ReferenceCode = "ReferenceCode";
			public const String ReplenishmentMaximum = "ReplenishmentMaximum";
			public const String ReplenishmentMinimum = "ReplenishmentMinimum";
			public const String ShapeCode = "ShapeCode";
			public const String SizeCode = "SizeCode";
			public const String SupplierCode = "SupplierCode";
			public const String SupplierName = "SupplierName";
			public const String StockType = "StockType";
			public const String UnitSize = "UnitSize";
		}

		public static class Barcode
		{
			public const String Ean = "EAN";

			public static readonly Regex EanRegex = new Regex("^\\d{13}$", RegexOptions.Compiled);

			public const String Jde = "JDE";

			public static readonly Regex JdeRegex = new Regex("^VL(F\\d{5}\\.\\d{3}|\\d{6})\\.\\d{2}$", RegexOptions.Compiled);
		}

		public static class Connector
		{
			public static class Setting
			{
				public const String Destination = "Destination";
				public const String MultiMagCode = "MultiMagCode";
				public const String MultiMagTimeZone = "MultiMagTimeZone";
				public const String Source = "Source";
        public const String LastExecutionTime = "LastExecutionTime";
				public const String LastSuccessfullTransactionImport = "LastSuccessfullTransactionImport";
				public const String LastSuccessfullCustomerImport = "LastSuccessfullCustomerImport";
				public const String LastSuccessfullShopStatisticImport = "LastSuccessfullShopStatisticImport";
			}

			public static class System
			{
				public const String BusinessIntelligence = "BI";
				public const String Magento = "Magento";
				public const String MultiMag = "MultiMag";
			}
		}

		public static class Culture
		{
			public static readonly CultureInfo Dutch = new CultureInfo("nl-NL");
			public static readonly CultureInfo English = new CultureInfo("en-GB");
		}

		public static class Directories
		{
			public const String History = "History";
			public const String Repository = "Transactions";
		}

		public static class Extensions
		{
			public const String CSV = ".csv";
			public const String Failure = ".failed";
			public const String Success = ".processed";
		}

		public static class Language
		{
			public const String English = "English";
		}

		public static class Prefixes
		{
			public const String Customer = "CD";
      public const String Items = "IT";
			public const String Movements = "MV";
			public const String Statistics = "SS";
			public const String Stock = "ST";
			public const String Transaction = "TS";
		}

		public static class ProductGroup
		{
			public const Int32 UnknownID = -1;
		}

		public static class Relation
		{
			public const String Style = "Style";
		  public const String Color = "Color";
		}

		public static class Status
		{
			public const String Default = "Default";
		}

		public static class Transaction
		{
			public const String Return = "R";
			public const String Sale = "V";
		}

		public static class Vendor
		{
			public const String Vlisco = "Vlisco";

			public static class Setting
			{
				public const String CountryCode = "CountryCode";
				public const String CurrencyCode = "CurrencyCode";
				public const String IsTariff = "IsTariffVendor";
				public const String Location = "Location";
			}
		}

		public const String CountrySymbol = " @ ";
		public const String CurrencySymbol = " ¤ ";

		public const String DiversCode = "DV";
		public const String DiversName = "DIVERS";

		public const String MissingCode = "-";
		public const String IgnoreCode = ".";

		public const String Inbox = "Inbox";
		public const String Outbox = "Outbox";
	  public const String SaleDateFormat = "yyyyMMdd";
	  public const String SaleTarrif = "SLD";

		public static readonly CultureInfo VendorAssortmentImportCulture = new CultureInfo(1033);

		public const String VendorItemNumberSegmentSeparator = ".";

		public static String GetVendorItemNumber(String articleCode, String colorCode, String sizeCode)
		{
			var vendorItemNumber = articleCode;

			if (!IgnoreCode.Equals(colorCode))
			{
				vendorItemNumber = String.Join(VendorItemNumberSegmentSeparator, vendorItemNumber, colorCode);
			}

			if (!IgnoreCode.Equals(sizeCode))
			{
				vendorItemNumber = String.Join(VendorItemNumberSegmentSeparator, vendorItemNumber, sizeCode);
			}

			return vendorItemNumber;
		}
	}
}
