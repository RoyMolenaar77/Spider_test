using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject.Modules;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.DataAccess.Repository;
using Concentrator.Objects.DataAccess.EntityFramework;
using System.Data.Objects;
using Ninject.Activation;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects.Models.Brands;
using System.Linq.Expressions;
using Concentrator.Objects.Web;
using Concentrator.Objects.Models.Vendors;
namespace Concentrator.Objects.DependencyInjection.NinjectModules
{
  public class CommonRepositoryModule : NinjectModule
  {
    public override void Load()
    {
      Bind<IScope>().To<DefaultScope>();
      Bind(typeof(IRepository<>)).To(typeof(EFBaseRepository<>));
      Bind<IFunctionRepository>().To<EFFunctionRepository>();

      //Bind<IRepository<Brand>>().To<EFBaseRepository<Brand>>().WithConstructorArgument("predicate", predBrand);
      //Bind<IRepository<VendorAssortment>>().To<EFBaseRepository<VendorAssortment>>().WithConstructorArgument("predicate", assortmentPred);

    }
  }
}
