using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft;
using Microsoft.Practices;
using Microsoft.Practices.ServiceLocation;

using NinjectAdapter;

namespace Concentrator.Tasks
{
  using Objects.DependencyInjection;
  using Objects.DependencyInjection.NinjectModules;
  using Objects.Web;

  public static class ConcentratorService
  {
    private readonly static Object GlobalLock = new Object();

    public static Boolean IsInitialized
    {
      get;
      private set;
    }

    public static ConcentratorKernel Kernel
    {
      get;
      private set;
    }

    /// <summary>
    /// Initializes the Concentrator Kernel.
    /// </summary>
    public static void Initialize()
    {
      lock (GlobalLock)
      {
        if (!IsInitialized)
        {
          Kernel = new ConcentratorKernel(new ServiceModule(), new ThreadScopeUnitOfWorkModule(), new UnsecuredRepositoryModule());

          ServiceLocator.SetLocatorProvider(() => new NinjectServiceLocator(Kernel));

          Client.User = new ConcentratorPrincipal(new ConcentratorIdentity(new UserIdentityModel(1)));

          IsInitialized = true;
        }
      }
    }
  }
}
