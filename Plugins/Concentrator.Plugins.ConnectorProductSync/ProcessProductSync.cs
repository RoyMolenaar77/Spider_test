using System.Reflection;
using Concentrator.Objects.ConcentratorService;
using Concentrator.Plugins.ConnectorProductSync.Services;
using Ninject;
using Concentrator.Plugins.ConnectorProductSync.Binding;
using Concentrator.Objects.DataAccess.UnitOfWork;
using PetaPoco;
using System;

namespace Concentrator.Plugins.ConnectorProductSync
{
  public class ProcessProductSync : ConcentratorPlugin
  {
    public override string Name
    {
      get { return "Connector Products Sync Plugin"; }
    }

    protected override void Process()
    {
      using (IKernel kernel = new StandardKernel())
      {
        var bindings = new Bindings(x =>
        {
          var db = new PetaPoco.Database(Connection, "System.Data.SqlClient");
          db.CommandTimeout = 300;
          return db;
        });

        kernel.Load(bindings);
        IProcessService processService = kernel.Get<IProcessService>();

        processService.Process();
        bindings.Dispose();
      }
    }
  }

  public class ImportProductGroupAndProductGroupMapping : ConcentratorPlugin
  {
    public override string Name
    {
      get { return "Importing Product Groups And Product Group Mappings To Master Group Mapping Plugin"; }
    }

    protected override void Process()
    {
      using (IKernel kernel = new StandardKernel())
      {
        var bindings = new Bindings(x => new PetaPoco.Database(Connection, "System.Data.SqlClient"));
        kernel.Load(bindings);
        IProcessImportService processImportService = kernel.Get<IProcessImportService>();

        // Import Old Product Groups To Master Group Mapping
        //processImportService.ImportProductGroups();
        
        // Import Old Product Group Mapping to Connector Mapping(Master Group Mapping)
        //processImportService.ImportProductGroupMappings();

        bindings.Dispose();
      }
    }
  }
}