using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Plugins.PFA.Models.CC
{
	public class ProductInfoResult
	{
		public string ShortDescription { get; set; }

		public string LongDescription { get; set; }

		public string TaxRateDates { get; set; }

		public string TaxRatePercentage { get; set; }

		public string TaxCode { get; set; }

		public string GroupCode1 { get; set; }

		public string GroupName1 { get; set; }

		public string GroupCode2 { get; set; }

		public string GroupName2 { get; set; }

		public string GroupCode3 { get; set; }

		public string GroupName3 { get; set; }

		public string SeasonCode { get; set; }

		public string MtbCode1 { get; set; }

		public string MtbCode2 { get; set; }

		public string MtbCode3 { get; set; }

		public string MtbCode4 { get; set; }

		public string Material { get; set; }

    public string override_tax_code { get; set; }

    public string override_tax_dates { get; set; }

    public string override_tax_rates { get; set; }
	}
}
