[assembly: WebActivator.PreApplicationStartMethod(typeof(Concentrator.Web.Services.App_Start.NinjectMVC3), "Start")]
[assembly: WebActivator.ApplicationShutdownMethodAttribute(typeof(Concentrator.Web.Services.App_Start.NinjectMVC3), "Stop")]

namespace Concentrator.Web.Services.App_Start
{
  using System.Reflection;
  using Microsoft.Web.Infrastructure.DynamicModuleHelper;
  using Ninject;
  using Ninject.Web.Mvc;
  using Concentrator.Objects.DependencyInjection.NinjectModules;
  using Microsoft.Practices.ServiceLocation;

  public static class NinjectMVC3
  {
    private static readonly Bootstrapper bootstrapper = new Bootstrapper();

    /// <summary>
    /// Starts the application
    /// </summary>
    public static void Start()
    {
      DynamicModuleUtility.RegisterModule(typeof(OnePerRequestModule));
      DynamicModuleUtility.RegisterModule(typeof(HttpApplicationInitializationModule));
      bootstrapper.Initialize(CreateKernel);
    }

    /// <summary>
    /// Stops the application.
    /// </summary>
    public static void Stop()
    {
      bootstrapper.ShutDown();
    }

    /// <summary>
    /// Creates the kernel that will manage your application.
    /// </summary>
    /// <returns>The created kernel.</returns>
    private static IKernel CreateKernel()
    {
      var kernel = new ConcentratorKernel();
      RegisterServices(kernel);

      ServiceLocator.SetLocatorProvider(() => new CommonServiceLocator.NinjectAdapter.NinjectServiceLocator(kernel));
      return kernel;
    }

    /// <summary>
    /// Load your modules or register your services here!
    /// </summary>
    /// <param name="kernel">The kernel.</param>
    private static void RegisterServices(IKernel kernel)
    {
      kernel.Load<RequestScopeUnitOfWorkModule>();
      kernel.Load<RequestScopeServiceUnitOfWorkModule>();
      kernel.Load<UnsecuredRepositoryModule>();
      kernel.Load<ServiceModule>();
      kernel.Load<ContextRequestScopeModule>();
      kernel.Load<ManagementServiceModule>();
    }
  }
}
