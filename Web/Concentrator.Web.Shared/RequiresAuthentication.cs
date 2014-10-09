using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Concentrator.Objects.Web;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Model.Users;

namespace Concentrator.Web.Shared
{

  public class RequiresAuthentication : ActionFilterAttribute
  {
    private readonly Functionalities[] _functionalities;

    private readonly Role[] _roles;

    public RequiresAuthentication(params Role[] roles)
    {
      _roles = roles.Length > 0 ? roles : null;
    }

    public RequiresAuthentication(params Functionalities[] functionalities)
    {
      _functionalities = functionalities.Length > 0 ? functionalities : null;
    }

    public override void OnActionExecuting(ActionExecutingContext filterContext)
    {
      if (Client.User != null && Client.User.Identity.IsAuthenticated)
      {
        if (_functionalities == null)
          return; // shortcircuit because no functionalities were specified

        var inFunctionality = false;

        foreach (var functionality in _functionalities)
        {
          if (!Client.User.FunctionalityRoles.Contains(functionality.ToString())) continue;
          inFunctionality = true;
          break;
        }

        if (!inFunctionality)
        {
          if (filterContext.HttpContext.Request.IsAjaxRequest())
          {
            filterContext.HttpContext.Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
          }
          else
          {
            filterContext.HttpContext.Response.StatusCode = (int)System.Net.HttpStatusCode.Unauthorized;
          }
          filterContext.HttpContext.Response.End();
        }
      }
      else
      {
        if (filterContext.HttpContext.Request.IsAjaxRequest())
        {
          //filterContext.HttpContext.Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
          var r = new JsonResult
                    {
                      Data = new
                               {
                                 success = false,
                                 authorized = false,
                                 message = "Unauthorized"
                               }
                    };

          filterContext.Result = r;
          r.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
        }
        else
        {
          UrlHelper helper = new UrlHelper(filterContext.RequestContext);
          filterContext.RequestContext.HttpContext.Response.Redirect(helper.Action("Login", "Account", new { returnUrl = filterContext.RequestContext.HttpContext.Request.Url }));
        }
      }
    }
  }
}
