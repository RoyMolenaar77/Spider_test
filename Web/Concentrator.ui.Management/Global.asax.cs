using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Concentrator.Objects.Web;
using Concentrator.Objects.DependencyInjection.NinjectModules;
using Ninject;
using CommonServiceLocator.NinjectAdapter;
using Microsoft.Practices.ServiceLocation;
using Concentrator.Web.Shared.Binders;
using System.IO;
using System.Threading;
using System.Configuration;
using Concentrator.Objects.Configuration;
using Concentrator.Objects.Models.Forms;

namespace Concentrator.ui.Management
{
  // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
  // visit http://go.microsoft.com/?LinkId=9394801

  public class MvcApplication : System.Web.HttpApplication
  {
    public override void Init()
    {
      this.PostAcquireRequestState += MvcApplication_PostAcquireRequestState;
      base.Init();
    }

    void MvcApplication_PostAcquireRequestState(object sender, EventArgs e)
    {
      if (HttpContext.Current.Handler is System.Web.SessionState.IRequiresSessionState || HttpContext.Current.Handler is System.Web.SessionState.IReadOnlySessionState)
      {
        if (Request.IsAuthenticated)
        {
          //get the username which we previously set in  
          //forms authentication ticket in our login1_authenticate event  
          string loggedUser = HttpContext.Current.User.Identity.Name;

          if (Session[SessionPrincipalKey] == null)
          {

            System.Diagnostics.Debug.WriteLine("Getting User Info From DB");
            //build a custom identity and custom principal object based on this username  
            var identity = new ConcentratorIdentity(loggedUser);
            var principal = new ConcentratorPrincipal(identity);

            #region not working yet
            var timeout = identity.Timeout;

            Session.Timeout = timeout;
            #endregion

            Session[SessionPrincipalKey] = principal;
          }
          //set the principal to the current context  
          HttpContext.Current.User = Session[SessionPrincipalKey] as ConcentratorPrincipal;

        }
        else
        {
          Session[SessionPrincipalKey] = null;
        }

      }
    }

    public static string SessionPrincipalKey = "ConcentratorPrincipal";

    public static void RegisterGlobalFilters(GlobalFilterCollection filters)
    {
      filters.Add(new HandleErrorAttribute());
    }

    public static void RegisterRoutes(RouteCollection routes)
    {
      routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

      routes.MapRoute(
          "Default", // Route name
          "{controller}/{action}/{id}", // URL with parameters
          new { controller = "Application", action = "Index", id = UrlParameter.Optional }


      );
      
      var extraRoutesSetting = ConfigurationManager.AppSettings["ClientModules"]; //comma separated customer name
      if (extraRoutesSetting != null)
      {
        var clientModulesToLoad = extraRoutesSetting.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (string mod in clientModulesToLoad)
        {

          Route externalRoute = new Route(
          mod + "/{controller}/{action}/{id}",
           new MvcRouteHandler()
        );
          externalRoute.DataTokens = new RouteValueDictionary(
            new
            {
              namespaces = new[] { string.Format("Concentrator.Web.CustomerSpecific.{0}.Controllers", mod) }
            });

          routes.Add(externalRoute);

        }
      }
    }

    protected void Application_Start()
    {
      AreaRegistration.RegisterAllAreas();
      ModelMetadataProviders.Current = new DataAnnotationsModelMetadataProvider();
      RegisterGlobalFilters(GlobalFilters.Filters);
      RegisterRoutes(RouteTable.Routes);
      RegisterModelBinders();

      var k = CreateKernel();
      ServiceLocator.SetLocatorProvider(() => new NinjectServiceLocator(k));
    }

    private void RefreshPlugins(string pluginPath)
    {
      string[] files = Directory.GetFiles(pluginPath, "Concentrator.*.dll");
      foreach (string file in files)
      {
        FileInfo info = new FileInfo(file);
        File.Copy(info.FullName, Path.Combine(AppDomain.CurrentDomain.RelativeSearchPath, info.Name), true);
      }
    }

    public IKernel CreateKernel()
    {
      //do the ninja
      IKernel kernel = new ConcentratorKernel();

      kernel.Load<RequestScopeServiceUnitOfWorkModule>();
      kernel.Load<CommonRepositoryModule>();
      kernel.Load<ServiceModule>();
      kernel.Load<ContextRequestScopeModule>();
      kernel.Load<ManagementServiceModule>();


      return kernel;
    }

    private void RegisterModelBinders()
    {
      ModelBinders.Binders[typeof(string)] = new StringModelBinder();
      ModelBinders.Binders[typeof(decimal)] = new DecimalModelBinder();
      ModelBinders.Binders[typeof(System.Nullable<decimal>)] = new DecimalModelBinder();
      ModelBinders.Binders.Add(typeof(long[]), new ArrayModelBinder<long>());
      ModelBinders.Binders.Add(typeof(int[]), new ArrayModelBinder<int>());
      ModelBinders.Binders.Add(typeof(DateTime[]), new ArrayModelBinder<DateTime>());
      ModelBinders.Binders.Add(typeof(FormNotificationType), new FlagEnumBinder());
    }
  }
}