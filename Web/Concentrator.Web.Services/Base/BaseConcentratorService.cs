using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Ninject;
using Concentrator.Objects.DependencyInjection.NinjectModules;
using Microsoft.Practices.ServiceLocation;
using CommonServiceLocator.NinjectAdapter;
using System.Web.Services.Protocols;
using System.Web.Services;
using System.Security.Principal;
using System.Threading;
using Concentrator.Objects.Web;
using System.Web.Security;
using System.Security.Authentication;

namespace Concentrator.Web.Services.Base
{
  /// <summary>
  /// Soap Header for the Secured Web Service.
  /// Username and Password are required for AuthenticateUser(),
  ///   and AuthenticatedToken is required for everything else.
  /// </summary>

  //public class SecuredWebServiceHeader : System.Web.Services.Protocols.SoapHeader
  //{
  //  public string Username;
  //  public string Password;
  //  public string AuthenticatedToken;
  //}

  public class BaseConcentratorService : System.Web.Services.WebService
  {
    public BaseConcentratorService()
    {
      //var kernel = new StandardKernel(new UnitOfWorkModule(), new ServiceUnitOfWorkModule());
      //kernel.Bind<IAmApplicationSpecificModule>().To<ManagementServiceModule>();
      //_unit = kernel.Get<IUnitOfWork>();

      //ServiceLocator.SetLocatorProvider(() => new NinjectServiceLocator(kernel));

      //string userName = string.Empty;
      //if (base.Context.Request.Params["UserName"] != null)
      //  userName = base.Context.Request.Params["UserName"];

      //string password = string.Empty;
      //if (base.Context.Request.Params["Password"] != null)
      //  password = base.Context.Request.Params["Password"];

      //if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(password))
      //{
      //  if (!IsUserValid(userName, password))
      //  {
      //    throw new AuthenticationException("Please call AuthenitcateUser() first.");
      //  }
      //}
      //else if (!IsUserValid(SoapHeader))
      //{
      //  throw new AuthenticationException("Please call AuthenitcateUser() first.");
      //}
    }

    //[WebMethod]
    //public string Login(string UserName, string Password)
    //{
    //  return string.Empty;
    //}

    //public SecuredWebServiceHeader SoapHeader;
    //[WebMethod]
    //[System.Web.Services.Protocols.SoapHeader("SoapHeader")]
    //public string AuthenticateUser()
    //{
    //  if (SoapHeader == null)
    //    return "Please provide a Username and Password";
    //  if (string.IsNullOrEmpty(SoapHeader.Username) || string.IsNullOrEmpty(SoapHeader.Password))
    //    return "Please provide a Username and Password";
    //  // Are the credentials valid?
    //  if (!IsUserValid(SoapHeader.Username, SoapHeader.Password))
    //    return "Invalid Username or Password";
    //  // Create and store the AuthenticatedToken before returning it
    //  string token = Guid.NewGuid().ToString();
    //  HttpRuntime.Cache.Add(
    //      token,
    //      SoapHeader.Username,
    //      null,
    //      System.Web.Caching.Cache.NoAbsoluteExpiration,
    //      TimeSpan.FromMinutes(60),
    //      System.Web.Caching.CacheItemPriority.NotRemovable,
    //      null);
    //  return token;
    //}

    //private bool IsUserValid(string Username, string Password)
    //{
    //  bool loginSuccessful;
    //  try
    //  {
    //    loginSuccessful = ConcentratorPrincipal.Login(Username, Password);
    //  }
    //  catch (Exception ex)
    //  {
    //    loginSuccessful = false;
    //  }

    //  return loginSuccessful;
    //}

    //internal bool IsUserValid(SecuredWebServiceHeader SoapHeader)
    //{
    //  if (SoapHeader == null)
    //    return false;
    //  // Does the token exists in our Cache?
    //  if (!string.IsNullOrEmpty(SoapHeader.AuthenticatedToken))
    //    return (HttpRuntime.Cache[SoapHeader.AuthenticatedToken] != null);
    //  return false;
    //}

    private IUnitOfWork _unit;

    protected IUnitOfWork GetUnitOfWork()
    {
      if (_unit == null)
      {
        //var kernel = new StandardKernel(new UnitOfWorkModule(), new ServiceUnitOfWorkModule());
       // var kernel = new ConcentratorKernel(new UnitOfWorkModule(), new ServiceUnitOfWorkModule(), new CommonRepositoryModule(), new ServiceModule());
        //kernel.Bind<IAmApplicationSpecificModule>().To<ManagementServiceModule>();

        _unit = ServiceLocator.Current.GetInstance<IUnitOfWork>();
        //_unit = kernel.Get<IUnitOfWork>();

//        ServiceLocator.SetLocatorProvider(() => new NinjectServiceLocator(kernel));
      }
      return _unit;
    }
  }
}