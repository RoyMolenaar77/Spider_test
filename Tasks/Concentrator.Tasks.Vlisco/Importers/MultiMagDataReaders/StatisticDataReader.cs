using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Concentrator.Tasks.Vlisco.Importers.MultiMagDataReaders
{
  using Data;
  using Models;

	[Task(Constants.Vendor.Vlisco + " Shop Statistic Data Reader Task")]
	public class StatisticDataReader : MultiMagDataReader<Statistic, StatisticMapping>
  {
    [ConnectorSetting(Constants.Connector.Setting.LastExecutionTime + "." + Constants.Prefixes.Statistics, false)]
		public DateTime? LastExecutionTime
		{
			get;
			set;
		}

    protected override FirebirdRepository<Statistic> CreateRepository(String connectionString)
    {
      var lastExecutionTime = TimeZoneInfo.ConvertTime(LastExecutionTime.GetValueOrDefault(DateTime.Now), TimeZoneInfo.FindSystemTimeZoneById(MultiMagTimeZone));

      return new FirebirdStatisticRepository(connectionString, lastExecutionTime);
    }

    protected override String GetExportFileName()
    {
      return Constants.Prefixes.Statistics + "_" + base.GetExportFileName();
    }
	}
}
