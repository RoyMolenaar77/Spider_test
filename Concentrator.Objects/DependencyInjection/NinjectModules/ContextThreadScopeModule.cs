using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject.Modules;
using System.Data.Objects;
using Concentrator.Objects.DataAccess.EntityFramework;

namespace Concentrator.Objects.DependencyInjection.NinjectModules
{
  public class ContextThreadScopeModule : NinjectModule
  {
    public override void Load()
    {
      //Bind(typeof(ObjectContext)).To(typeof(ConcentratorDataContext));
    }
  }
}
