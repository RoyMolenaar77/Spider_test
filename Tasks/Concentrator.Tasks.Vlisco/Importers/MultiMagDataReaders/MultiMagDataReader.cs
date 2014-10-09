using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using CsvHelper;
using CsvHelper.Configuration;

namespace Concentrator.Tasks.Vlisco.Importers.MultiMagDataReaders
{
  using Data;
  using Objects.Monitoring;

  public abstract class MultiMagDataReader<TModel, TMapping> : ConnectorTaskBase
    where TMapping : CsvClassMap<TModel>
  {
    private static readonly FeeblMonitoring Monitoring = new FeeblMonitoring();

    [ConnectorSetting(Constants.Connector.Setting.MultiMagCode)]
    public String MultiMagCode
    {
      get;
      set;
    }

    [ConnectorSetting(Constants.Connector.Setting.MultiMagTimeZone)]
    public String MultiMagTimeZone
    {
      get;
      set;
    }

    /// <summary>
    /// Target directory transaction files in the source folder of the transaction importer task
    /// </summary>
    [ConnectorSetting(Constants.Connector.Setting.Source)]
    public String Target
    {
      get;
      set;
    }

    protected abstract FirebirdRepository<TModel> CreateRepository(String connectionString);

    protected override void ExecuteTask()
    {
      Monitoring.Notify(Name, 0);

      base.ExecuteTask();

      Monitoring.Notify(Name, 1);
    }

    protected override void ExecuteConnectorTask()
    {
      if (!Directory.Exists(Target))
      {
        TraceError("{0}: The directory '{1}' does not exists!", Context.Name, Path.GetFullPath(Target));
      }
      else
      {
        using (var repository = CreateRepository(Context.Connection))
        {
          Export(repository.Execute().ToArray());
        }
      }
    }

    private void Export(IEnumerable<TModel> models)
    {
      TraceInformation("{0}: Found {1} records for export to CSV.", Context.Name, models.Count());

      if (models.Any())
      {
        using (var fileStream = File.OpenWrite(GetExportFileName()))
        using (var streamWriter = new StreamWriter(fileStream))
        using (var csvWriter = new CsvWriter(streamWriter))
        {
          csvWriter.Configuration.RegisterClassMap<TMapping>();
          csvWriter.Configuration.Delimiter = ";";
          csvWriter.WriteRecords(models);
        }
      }
    }

    protected virtual String GetExportFileName()
    {
      return Path.Combine(Target, String.Format("{0}_{1:yyyyMMdd_HHmmss}.csv", MultiMagCode, DateTime.Now));
    }

    protected override Boolean ValidateContext()
    {
      return Context.ConnectorSystemID.HasValue && Constants.Connector.System.MultiMag.Equals(Context.ConnectorSystem.Name, StringComparison.OrdinalIgnoreCase);
    }
  }
}
