using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject.Modules;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects.DataAccess.EntityFramework;

namespace Concentrator.Objects.DependencyInjection.NinjectModules
{
  public class RequestScopeServiceUnitOfWorkModule : NinjectModule
  {
    public override void Load()
    {
      Bind<IServiceUnitOfWork>().To<DefaultUnitOfWork>().InRequestScope();
    }
  }
}
