using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

using CsvHelper;
using CsvHelper.Configuration;

namespace Concentrator.Tasks.Vlisco.Importers
{
  using Objects.Monitoring;

  public abstract class MultiMagImporterTask<TModel, TModelMapping> : ConnectorTaskBase
    where TModelMapping : CsvClassMap
  {
    protected static readonly FeeblMonitoring Monitoring = new FeeblMonitoring();

    protected abstract String ImportFilePrefix
    {
      get;
    }

    [ConnectorSetting(Constants.Connector.Setting.MultiMagCode)]
    public String MultiMagCode
    {
      get;
      set;
    }

    [ConnectorSetting(Constants.Connector.Setting.Source)]
    public String Source
    {
      get;
      set;
    }

    protected override void ExecuteTask()
    {
      Monitoring.Notify(Name, 0);

      base.ExecuteTask();

      Monitoring.Notify(Name, 1);
    }

    protected override void ExecuteConnectorTask()
    {
      var locationInfo = new DirectoryInfo(Source);

      if (!locationInfo.Exists)
      {
        TraceError("{0}: The directory '{1}' does not exists!", Context.Name, locationInfo.FullName);
      }
      else
      {
        var pattern = String.Format("{0}_{1}_*.csv", ImportFilePrefix, MultiMagCode);

        foreach (var fileInfo in locationInfo.GetFiles(pattern, SearchOption.TopDirectoryOnly))
        {
          TraceInformation("{0}: Importing '{1}'...", Context.Name, fileInfo.Name);

          try
          {
            ImportFile(fileInfo);

            fileInfo.CopyTo(fileInfo.FullName + Constants.Extensions.Success);
          }
          catch (Exception exception)
          {
            TraceCritical(exception);

            fileInfo.CopyTo(fileInfo.FullName + Constants.Extensions.Failure);
          }

          fileInfo.Delete();
        }
      }
    }

    protected virtual void ImportFile(FileInfo file)
    {
      var models = new Dictionary<TModel, String>();

      try
      {
        using (var streamReader = new StreamReader(file.FullName))
        using (var csvReader = new CsvReader(streamReader))
        {
          csvReader.Configuration.Delimiter = ";";
          csvReader.Configuration.RegisterClassMap<TModelMapping>();

          while (csvReader.Read())
          {
            try
            {
              models[csvReader.GetRecord<TModel>()] = String.Join(csvReader.Configuration.Delimiter, csvReader.CurrentRecord);
            }
            catch (CsvHelperException exception)
            {
              var stringBuilder = new StringBuilder();

              stringBuilder.AppendFormat("{0}: Unable to read the row {1}.", Context.Name, csvReader.Row);

              if (exception.Data.Contains("CsvHelper"))
              {
                stringBuilder.AppendLine();
                stringBuilder.AppendLine(exception.Data["CsvHelper"].ToString());
              }

              TraceWarning(stringBuilder.ToString());
            }
          }
        }
      }
      catch (Exception exception)
      {
        TraceCritical(exception);
      }

      Import(models);
    }

    protected virtual void Import(IDictionary<TModel, String> models)
    {
    }

    protected override Boolean ValidateContext()
    {
      return Context.ConnectorSystemID.HasValue && Constants.Connector.System.MultiMag.Equals(Context.ConnectorSystem.Name, StringComparison.OrdinalIgnoreCase);
    }
  }
}