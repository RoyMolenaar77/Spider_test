using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

using CsvHelper;

namespace Concentrator.Tasks.Vlisco.Importers
{
  using Objects.Models;
  using Objects.Models.Orders;
  using Objects.Models.Products;
  using Objects.Models.Vendors;
  using Objects.Monitoring;
  using Objects.Vendors.Bulk;

  using Models;

  [Task(Constants.Vendor.Vlisco + " Transaction Importer Task")]
  public class TransactionImporterTask : ConnectorTaskBase
  {
    private static readonly FeeblMonitoring Monitoring = new FeeblMonitoring();

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
      var sourceDirectory = new DirectoryInfo(Source);

      if (!sourceDirectory.Exists)
      {
        TraceError("{0}: The directory '{1}' does not exists!", Context.Name, sourceDirectory.FullName);
      }
      else
      {
        var repositoryDirectory = new DirectoryInfo(Constants.Directories.Repository);

        if (!repositoryDirectory.Exists)
        {
          repositoryDirectory.Create();
        }

        TraceInformation("{0}: Copying transaction files to local repository...", Context.Name);

        foreach (var fileInfo in sourceDirectory.GetFiles(String.Format("??_{0}_*.csv", MultiMagCode), SearchOption.TopDirectoryOnly))
        {
          TraceVerbose("{0}: Copying '{1}' to '{2}.", Context.Name, fileInfo.FullName, repositoryDirectory.FullName);

          fileInfo.CopyTo(Path.Combine(repositoryDirectory.FullName, fileInfo.Name), true);
        }
      }
    }

    protected override Boolean ValidateContext()
    {
      return Context.ConnectorSystemID.HasValue && Constants.Connector.System.MultiMag.Equals(Context.ConnectorSystem.Name, StringComparison.OrdinalIgnoreCase);
    }
  }
}
