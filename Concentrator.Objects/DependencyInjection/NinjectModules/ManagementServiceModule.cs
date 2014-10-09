using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject.Modules;
using Concentrator.Objects.Services.Base;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Services;

namespace Concentrator.Objects.DependencyInjection.NinjectModules
{
  public class ManagementServiceModule : NinjectModule, IAmApplicationSpecificModule
  {
    public override void Load()
    {
      Bind(typeof(IService<User>)).To(typeof(UserService));
    }
  }
}
