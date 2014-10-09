using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Concentrator.Tasks.Vlisco.Importers.MultiMagDataReaders
{
  using Data;
  using Models;

  [Task(Constants.Vendor.Vlisco + " Order Data Reader Task")]
  public class OrderDataReader : MultiMagDataReader<Order, OrderMapping>
  {
    [ConnectorSetting(Constants.Connector.Setting.LastExecutionTime + "." + Constants.Prefixes.Transaction, false)]
    public DateTime? LastExecutionTime
    {
      get;
      set;
    }

    protected override FirebirdRepository<Order> CreateRepository(String connectionString)
    {
      var lastExecutionTime = TimeZoneInfo.ConvertTime(LastExecutionTime.GetValueOrDefault(DateTime.Now), TimeZoneInfo.FindSystemTimeZoneById(MultiMagTimeZone));

      return new FirebirdOrderRepository(connectionString, lastExecutionTime);
    }

    protected override String GetExportFileName()
    {
      return Constants.Prefixes.Transaction + "_" + base.GetExportFileName();
    }
  }
}
