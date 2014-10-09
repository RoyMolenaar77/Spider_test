using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Concentrator.Tasks.Vlisco.Importers.MultiMagDataReaders
{
  using Data;
  using Models;

  [Task(Constants.Vendor.Vlisco + " Movement Data Reader Task")]
  public class MovementDataReader : MultiMagDataReader<Movement, MovementMapping>
	{
    [ConnectorSetting(Constants.Connector.Setting.LastExecutionTime + "." + Constants.Prefixes.Movements, false)]
		public DateTime? LastExecutionTime
		{
			get;
			set;
		}

    protected override FirebirdRepository<Movement> CreateRepository(String connectionString)
    {
      var lastExecutionTime = TimeZoneInfo.ConvertTime(LastExecutionTime.GetValueOrDefault(DateTime.Now), TimeZoneInfo.FindSystemTimeZoneById(MultiMagTimeZone));

      return new FirebirdMovementRepository(connectionString, lastExecutionTime);
    }

    protected override String GetExportFileName()
    {
      return Constants.Prefixes.Movements + "_" + base.GetExportFileName();
    }
	}
}
