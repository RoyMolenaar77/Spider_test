using Nancy.Authentication.Basic;
using Nancy.Bootstrapper;
using Nancy.TinyIoc;

namespace Concentrator.Web.API
{
  using Nancy;

  public class ConcentratorBootstrapper : DefaultNancyBootstrapper
  {
    protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
    {
      container.Register(typeof(ConcentratorModule));

      base.ApplicationStartup(container, pipelines);
      
      pipelines.EnableBasicAuthentication(new BasicAuthenticationConfiguration(container.Resolve<IUserValidator>(), "MyRealm"));
    }
  }
}