using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

using Concentrator.Objects.Environments;
using Concentrator.Objects.Model.Users;
using Concentrator.Objects.Models.Plugin;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Services.Base;
using Concentrator.Objects.Services.ServiceInterfaces;
using Concentrator.Objects.Web;
using Concentrator.Web.Shared;
using Concentrator.Web.Shared.Controllers;

using PetaPoco;

namespace Concentrator.ui.Management.Controllers
{
  public class UserController : BaseController
  {
    [RequiresAuthentication(Functionalities.ViewUser)]
    public ActionResult GetList()
    {
      int currentOrganizationID = Client.User.OrganizationID;
      return List(unit => unit.Service<User>()
        .GetAll(u => (u.OrganizationID == currentOrganizationID))
        .Select(user => new
        {
          user.UserID,
          user.Username,
          user.Firstname,
          user.Lastname,
          user.Email,
          user.IsActive,
          user.LanguageID,
          user.Timeout,
          Roles = user.UserRoles.Select(x => x.RoleID),
          Vendors = user.UserRoles.Select(x => x.VendorID),
        }));
    }

    [RequiresAuthentication(Functionalities.CreateUser)]
    public ActionResult Create()
    {
      return Create<User>((unit, user) =>
      {
        unit.Service<User>().Create(user);

        user.IsActive = Request.Params["IsActive"].Contains("on") ? true : false;
      });
    }

    [RequiresAuthentication(Functionalities.UpdateUser)]
    public ActionResult Update(int id)
    {
      Session[MvcApplication.SessionPrincipalKey] = null;

      return Update<User>(c => c.UserID == id);
    }

    [RequiresAuthentication(Functionalities.DeleteUser)]
    public ActionResult Delete(int id)
    {
      return Delete<User>(l => l.UserID == id);
    }

    [RequiresAuthentication(Functionalities.UpdateUser)]
    public ActionResult SetContentSettings(int? languageID, int? connectorID)
    {
      using (var unit = GetUnitOfWork())
      {
        ((IUserService)unit.Service<User>()).SetContentSettings(languageID, connectorID);

        unit.Save();

        Client.ReloadPrincipal();

        return Success("User settings changed successfully. Page will be refreshed in two seconds", needsRefresh: true);
      }
    }

    [RequiresAuthentication(Functionalities.ViewUser)]
    public ActionResult GetTimeout()
    {
      var userTimeout = GetUnitOfWork().Service<User>().Get(x => x.UserID == Client.User.UserID).Timeout;

      return Json(new
      {
        success = true,
        data = new { timeout = userTimeout }
      });
    }

    [RequiresAuthentication(Functionalities.SetTimeoutUser)]
    public ActionResult SetTimeout(int timeout)
    {
      return Update<User>(c => c.UserID == Client.User.UserID, (unit, userModel) =>
      {
        userModel.Timeout = timeout;
      });
    }

    [RequiresAuthentication(Functionalities.ViewUser)]
    public ActionResult GetRoles()
    {
      return SimpleList(unit => from r in unit.Service<Role>().GetAll(r => !r.isHidden)
                                select new
                                {
                                  Name = r.RoleName,
                                  ID = r.RoleID
                                });
    }

    [RequiresAuthentication(Functionalities.ChangePasswordUser)]
    public ActionResult ChangePassword(int userID, string currentPassword, string password, string password2)
    {
      using (var unit = GetUnitOfWork())
      {
        var userService = unit.Service<User>() as Concentrator.Objects.Services.UserService;
        var user = userService.Get(c => c.UserID == userID);

        if (userService.CalculatePasswordHash(user.Username, user.UserID, currentPassword).Equals(user.Password))
        {
          if (password != password2)
          {
            return Failure("The given passwords don't match");
          }

          user.Password = userService.CalculatePasswordHash(user.Username, user.UserID, password);
          unit.Save();

          return Success("Password changed");
        }
        else
        {
          return Failure("The given password is invalid");
        }
      }
    }

    [RequiresAuthentication(Functionalities.ViewUser)]
    public ActionResult GetUsers()
    {
      return SimpleList(unit => unit.Service<User>().GetAll().Select(user => new
      {
        user.UserID,
        Name = user.Firstname + " " + user.Lastname
      }));
    }

    [RequiresAuthentication(Functionalities.CreateUser)]
    public ActionResult AddPlugin()
    {

      try
      {
        UserPlugin userPluginModel = new UserPlugin();
        TryUpdateModel(userPluginModel);
        userPluginModel.SubscriptionTime = DateTime.Now;

        using (var db = new Database(Environments.Current.Connection, "System.Data.SqlClient"))
        {
          db.Execute(@"insert into UserPlugin (UserID, PluginID, TypeID, SubscriptionTime) values (@0, @1, @2, @3)", userPluginModel.UserID, userPluginModel.PluginID, userPluginModel.TypeID, DateTime.Now);
        }
        return Success("Registration complete");
      }
      catch (Exception)
      {
        return Failure("Something went wrong while registering to a plugin");
      }

    }

    [RequiresAuthentication(Functionalities.CreateUser)]
    public ActionResult GetPlugins(int userID)
    {
      var page = GetPagingParams();

      using (var db = new Database(Environments.Current.Connection, "System.Data.SqlClient"))
      {
        var queryResult = db.FetchMultiple<dynamic, int>(@"exec GetUserPlugins @0, @1, @2, @3, @4", userID, page.Skip, page.Take, string.Empty, string.Empty);

        var result = queryResult.Item1;
        var total = queryResult.Item2;


        return Json(new
        {
          results = (from i in result
                     select new
                     {
                       UserID = (int)i.UserID,
                       PluginID = (int)i.PluginID,
                       PluginName = (string)i.PluginName,
                       TypeID = (int)i.TypeID,
                       Type = (string)i.Type
                     }).ToList(),
          total,
          JsonRequestBehavior.AllowGet
        });
      }
    }
  }
}
