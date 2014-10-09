using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject.Modules;
using Concentrator.Plugins.ConnectorProductSync.Repositories;
using Concentrator.Plugins.ConnectorProductSync.Services;
using Concentrator.Objects.DataAccess.UnitOfWork;
using PetaPoco;
using Concentrator.Plugins.ConnectorProductSync.Helpers;

namespace Concentrator.Plugins.ConnectorProductSync.Binding
{
  public class Bindings : NinjectModule
  {
    private Func<Ninject.Activation.IContext, IDatabase> petaPoco;
    public Bindings(Func<Ninject.Activation.IContext, IDatabase> _petaPoco)
    {
      petaPoco = _petaPoco;
      
    }

    public override void Load()
    {
      Bind<IDatabase>().ToMethod(petaPoco).InRequestScope();

      Bind<ILogging>().To<Logging>();
      Bind<IGenerateUpdateProperties>().To<GenerateUpdateProperties>();

      Bind<IProcessService>().To<ProcessService>();
      Bind<ISyncContentService>().To<SyncContentService>();
      Bind<ISyncProductService>().To<SyncProductService>();
      Bind<IProcessImportService>().To<ProcessImportService>();
      Bind<ISyncProductGroupMappingService>().To<SyncProductGroupMappingService>();
      Bind<ISyncContentProductGroupService>().To<SyncContentProductGroupService>();
      Bind<IFilterByParentProductGroupService>().To<FilterByParentProductGroupService>();
      Bind<IFlattenHierachyProductGroupService>().To<FlattenHierachyProductGroupService>();
      Bind<IGenerateSqlSetterForType>().To<ComposedSqlSetterForTypeGenerator>();
      
      Bind<IContentRepository>().To<ContentRepository>();
      Bind<IProductRepository>().To<ProductRepository>();
      Bind<IConnectorRepository>().To<ConnectorRepository>();
      Bind<IProductGroupRepository>().To<ProductGroupRepository>();
      Bind<IMasterGroupMappingRepository>().To<MasterGroupMappingRepository>();
      Bind<IContentProductGroupRepository>().To<ContentProductGroupRepository>();
      Bind<IProductGroupMappingRepository>().To<ProductGroupMappingRepository>();
      Bind<IConnectorPublicationRuleRepository>().To<ConnectorPublicationRuleRepository>();
      Bind<IMagentoProductGroupSettingRepository>().To<MagentoProductGroupSettingRepository>();
    }
  }
}
