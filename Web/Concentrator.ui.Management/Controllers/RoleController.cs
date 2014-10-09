using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Concentrator.Objects.Web;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Model.Users;
using Concentrator.Objects.Services.ServiceInterfaces;
using Concentrator.Web.Shared;
using Concentrator.Web.Shared.Controllers;
using Concentrator.Objects.Models;

namespace Concentrator.ui.Management.Controllers
{
  public class RoleController : BaseController
  {
    [RequiresAuthentication(Functionalities.ViewRoles)]
    public ActionResult GetList()
    {
      return List(unit => from r in unit.Service<Role>().GetAll()
                          select new
                          {
                            r.RoleID,
                            r.RoleName,
                            r.isHidden
                          });
    }

    [RequiresAuthentication(Functionalities.CreateRole)]
    public ActionResult Create(bool? isHidden)
    {
      return Create<Role>();
    }

    //[RequiresAuthentication(Functionalities.cr)]
    public ActionResult SearchRoles()
    {
      return Search(unit => from r in unit.Service<Role>().GetAll()
                            select new
                            {
                              RoleName = r.RoleName,
                              RoleID = r.RoleID
                            });
    }

    [RequiresAuthentication(Functionalities.DeleteRole)]
    public ActionResult Delete(int id)
    {
      using (var unit = GetUnitOfWork())
      {
        var functionalityRoles = unit.Service<FunctionalityRole>().GetAll(c => c.RoleID == id).ToList();

        functionalityRoles.ForEach((functionality, idx) =>
        {
          unit.Service<FunctionalityRole>().Delete(functionality);
        });

        return Delete<Role>(r => r.RoleID == id);
      }
    }

    [RequiresAuthentication(Functionalities.UpdateRole)]
    public ActionResult Update(int id, bool? IsHidden)
    {
      return Update<Role>(r => r.RoleID == id, (unit, role) =>
      {
        role.isHidden = IsHidden.HasValue ? IsHidden.Value : false;
      });
    }

    [RequiresAuthentication(Functionalities.GetFunctionalities)]
    public ActionResult GetFunctionalities(int roleID)
    {
      using (var unit = GetUnitOfWork())
      {
        var result = (from c in ((IUserService)unit.Service<User>()).GetFunctionalitiesPerRole(roleID)
                      select new
                      {
                        c.DisplayName,
                        c.FunctionalityName,
                        c.Group,
                        c.IsEnabled
                      });

        var pagingParams = GetPagingParams();

        return Json(new
        {
          results = GetPagedResult(result),
          total = result.Count()
        });
      }
    }

    [RequiresAuthentication(Functionalities.UpdateFunctionalities)]
    public ActionResult UpdateFunctionalities(int roleID, string[] enabledfunctionalities, string[] disabledfunctionalities)
    {
      using (var unit = GetUnitOfWork())
      {
        ((IUserService)unit.Service<User>()).UpdateFunctionalitiesPerRole(roleID, enabledfunctionalities, disabledfunctionalities);

        unit.Save();

        Client.ReloadPrincipal();

        return Success("User roles updated successfully.", needsRefresh: true);
      }

    }
  }
}
