using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Concentrator.Tasks.Vlisco.Importers.MultiMagDataReaders
{
  using Data;
  using Models;

	[Task(Constants.Vendor.Vlisco + " Item Data Reader Task")]
  public class ItemDataReader : MultiMagDataReader<Item, ItemMapping>
  {
    [ConnectorSetting(Constants.Connector.Setting.LastExecutionTime + "." + Constants.Prefixes.Items, false)]
		public DateTime? LastExecutionTime
		{
			get;
			set;
		}

    protected override FirebirdRepository<Item> CreateRepository(String connectionString)
    {
      var lastExecutionTime = TimeZoneInfo.ConvertTime(LastExecutionTime.GetValueOrDefault(DateTime.Now), TimeZoneInfo.FindSystemTimeZoneById(MultiMagTimeZone));

      return new FirebirdItemRepository(connectionString, lastExecutionTime);
    }

    protected override String GetExportFileName()
    {
      return Constants.Prefixes.Items + "_" + base.GetExportFileName();
    }
	}
}
