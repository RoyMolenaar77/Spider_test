using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Threading;
using Concentrator.Objects.Web.Providers;
using Microsoft.Practices.ServiceLocation;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects.Models.Management;

namespace Concentrator.Objects.Web
{
  public class Client
  {
    public static string SessionPrincipalKey = "ConcentratorPrincipal";

    public static HttpSessionStateBase Session
    {
      get
      {
        var context = HttpContextFactory.GetHttpContext();

        return context != null
          ? context.Session
          : null;
      }
    }

    public static IConcentratorPrincipal User
    {
      get
      {
        var context = HttpContextFactory.GetHttpContext();
        
        return context == null
          ? Thread.CurrentPrincipal as ConcentratorPrincipal
          : context.User as ConcentratorPrincipal;
      }
      set
      {
        var context = HttpContextFactory.GetHttpContext();

        if (context != null)
        {
          context.User = value;
        }
        else
        {
          Thread.CurrentPrincipal = value;
        }
      }
    }

    public static void Login(ConcentratorPrincipal principal)
    {
      User = principal;
    }

    public static ConcentratorMembershipUser SystemUser
    {
      get
      {
        return new ConcentratorMembershipUser
        {
          UserID = 0,
          IsApproved = true,
          Email = "system@ceyenne"
        };
      }
    }

    public static void ReloadPrincipal()
    {
      string loggedUser = User.UserName;

      var identity = new ConcentratorIdentity(loggedUser);
      var principal = new ConcentratorPrincipal(identity);

      Session[SessionPrincipalKey] = principal;
    }

    public static string GetFileName(string field, string defaultValue)
    {
      using (var unit = ServiceLocator.Current.GetInstance<IUnitOfWork>())
      {
        var label = unit.Scope.Repository<ManagementLabel>().GetSingle(x => x.Field == field);

        if (label != null && !string.IsNullOrEmpty(label.Label))
          return label.Label;
        else
          return defaultValue;
      }
    }
  }
}
