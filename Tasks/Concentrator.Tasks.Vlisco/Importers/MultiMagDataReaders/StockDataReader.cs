using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Concentrator.Tasks.Vlisco.Importers.MultiMagDataReaders
{
  using Data;
  using Models;

	[Task(Constants.Vendor.Vlisco + " Stock Data Reader Task")]
  public class StockDataReader : MultiMagDataReader<Stock, StockMapping>
	{
		[ConnectorSetting(Constants.Connector.Setting.LastExecutionTime + "." + Constants.Prefixes.Stock, false)]
		public DateTime? LastExecutionTime
		{
			get;
			set;
		}

    protected override FirebirdRepository<Stock> CreateRepository(String connectionString)
    {
      var lastExecutionTime = TimeZoneInfo.ConvertTime(LastExecutionTime.GetValueOrDefault(DateTime.Now), TimeZoneInfo.FindSystemTimeZoneById(MultiMagTimeZone));

      return new FirebirdStockRepository(connectionString, lastExecutionTime);
    }

    protected override String GetExportFileName()
    {
      return Constants.Prefixes.Stock + "_" + base.GetExportFileName();
    }
	}
}
