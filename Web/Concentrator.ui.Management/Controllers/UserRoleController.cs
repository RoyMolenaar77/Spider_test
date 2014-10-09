using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Concentrator.Objects.Web;
using Concentrator.Objects.Services.Base;
using Concentrator.Objects.Models.Users;
using Concentrator.Web.Shared;
using Concentrator.Web.Shared.Controllers;
using Concentrator.Objects.Models;

namespace Concentrator.ui.Management.Controllers
{
  public class UserRoleController : BaseController
  {
    [RequiresAuthentication(Functionalities.GetUserRole)]
    public ActionResult GetList(int userID)
    {
      return List(unit => from u in unit.Service<User>().GetAll().SelectMany(c => c.UserRoles)
                          where u.UserID == userID
                          select new
                          {
                            u.RoleID,
                            u.VendorID
                          });
    }

    //[RequiresAuthentication(Functionalities.GetUserRole)]
    [RequiresAuthentication(Functionalities.Default)]
    public ActionResult UsersPerRole(int roleID)
    {
      return List(unit => from i in unit.Service<UserRole>().GetAll(x => x.RoleID == roleID)
                          select new
                          {
                            i.RoleID,
                            i.VendorID,
                            i.UserID,
                            Name = i.User.Firstname + " " + i.User.Lastname
                          });
    }

    [RequiresAuthentication(Functionalities.DeleteUserFromRole)]
    public ActionResult DeleteUserFromRole(int RoleID, int _UserID, int _VendorID)
    {
      return Delete<UserRole>(x => x.RoleID == RoleID && x.UserID == _UserID && x.VendorID == _VendorID);
    }

    [RequiresAuthentication(Functionalities.CreateUserRole)]
    public ActionResult Create()
    {
      return Create<UserRole>();
    }

    [RequiresAuthentication(Functionalities.DeleteUserRole)]
    public ActionResult Delete(int userID, int _RoleID, int _VendorID)
    {
      return Delete<UserRole>(x => x.UserID == userID && x.RoleID == _RoleID && x.VendorID == _VendorID);
    }
  }
}
