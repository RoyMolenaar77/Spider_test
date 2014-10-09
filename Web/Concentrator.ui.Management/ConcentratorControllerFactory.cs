using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.SessionState;
using Ninject;
using System.Reflection;



namespace Concentrator.ui.Management
{
  public class ConcentratorControllerFactory : IControllerFactory
  {
    public IController CreateController(RequestContext requestContext, string controllerName)
    {
      return null;
    }

    public SessionStateBehavior GetControllerSessionBehavior(RequestContext requestContext, string controllerName)
    {    
      return SessionStateBehavior.Default;
    }

    public void ReleaseController(IController controller)
    {
      var disposable = controller as IDisposable;

      if (disposable != null)
      {
        disposable.Dispose();
      }
    }
  }
}