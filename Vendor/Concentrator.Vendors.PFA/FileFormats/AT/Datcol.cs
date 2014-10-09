using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FileHelpers;
using Concentrator.Vendors.PFA.Helpers;

namespace Concentrator.Vendors.PFA.FileFormats.AT
{
	[DelimitedRecord("|")]
	public class DatColReceiveRegular : Concentrator.Vendors.PFA.FileFormats.DatColReceiveRegular
	{
		public DatColReceiveRegular()
		{
			EmployeeNumber = 8011;
			EmployeeNumber2 = 8011;
			FixedField6 = "00";
		}

	}

	[DelimitedRecord("|")]
	public class DatColTransfer : Concentrator.Vendors.PFA.FileFormats.DatColTransfer
	{
		public DatColTransfer()
		{
			EmployeeNumber = 8011;
			EmployeeNumber2 = 8011;
		}
	}

	[DelimitedRecord("|")]
	public class DatColNormalSales : Concentrator.Vendors.PFA.FileFormats.DatColNormalSales
	{
		public DatColNormalSales()
		{
			EmployeeNumber = 8011;
			EmployeeNumber2 = 8011;
			FixedField6 = "00";
		}
	}

	[DelimitedRecord("|")]
	public class DatColReturn : Concentrator.Vendors.PFA.FileFormats.DatColReturn
	{
		public DatColReturn()
		{
			EmployeeNumber = 8011;
			EmployeeNumber2 = 8011;
			FixedField6 = "04";
		}
	}
}
