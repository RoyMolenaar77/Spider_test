using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Concentrator.Tasks.Vlisco.Importers.MultiMagDataReaders
{
  using Data;
  using Models;

	[Task(Constants.Vendor.Vlisco + " Customer Data Reader Task")]
	public class CustomerDataReader : MultiMagDataReader<Customer, CustomerMapping>
  {
    [ConnectorSetting(Constants.Connector.Setting.LastExecutionTime + "." + Constants.Prefixes.Customer, false)]
		public DateTime? LastExecutionTime
		{
			get;
			set;
		}

    protected override FirebirdRepository<Customer> CreateRepository(String connectionString)
    {
      var lastExecutionTime = TimeZoneInfo.ConvertTime(LastExecutionTime.GetValueOrDefault(DateTime.Now), TimeZoneInfo.FindSystemTimeZoneById(MultiMagTimeZone));

      return new FirebirdCustomerRepository(connectionString, lastExecutionTime);
    }

    protected override String GetExportFileName()
    {
      return Constants.Prefixes.Customer + "_" + base.GetExportFileName();
    }
	}
}
