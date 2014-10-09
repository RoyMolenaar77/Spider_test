using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject.Modules;
using Ninject;
using Concentrator.Objects.DataAccess.EntityFramework;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects.DataAccess.Repository;

namespace Concentrator.Objects.DependencyInjection.NinjectModules
{
  public class UnsecuredRepositoryModule : NinjectModule
  {
    public override void Load()
    {
      Bind<IScope>().To<DefaultScope>();
      Bind(typeof(IRepository<>)).To(typeof(EFBaseRepository<>)).WithConstructorArgument("filterOnVendor", false);
      Bind<IFunctionRepository>().To<EFFunctionRepository>();
    }
  }
}
