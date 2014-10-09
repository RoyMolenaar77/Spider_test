using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject;
using Ninject.Planning.Bindings;
using Ninject.Modules;

namespace Concentrator.Objects.DependencyInjection.NinjectModules
{
  public class ConcentratorKernel : StandardKernel
  {
    public ConcentratorKernel(params INinjectModule[] modules)
      : base(modules)
    {

    }

    public ConcentratorKernel(INinjectSettings settings, params INinjectModule[] modules) : base(settings, modules) { }

    public override IEnumerable<IBinding> GetBindings(Type service)
    {
      var bindings = base.GetBindings(service);

      if (bindings.Count() > 1)
      {
        bindings = bindings.Where(c => !c.Service.IsGenericTypeDefinition);
      }

      return bindings;
    }
  }
}
