using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Concentrator.ui.Management.Controllers
{
  using Objects.Models;
  using Objects.Models.Brands;
  using Objects.Models.Management;
  using Objects.Models.Products;
  using Objects.Models.Statuses;
  using Objects.Models.Users;
  using Objects.Models.Vendors;
  using Objects.Services.ServiceInterfaces;
  using Objects.Web;
  using Web.Shared;
  using Web.Shared.Controllers;

  public class UserComponentStateController : BaseController
  {
    [RequiresAuthentication(Functionalities.Default)]
    public ActionResult DeleteSaveState(string name)
    {
      using (var unit = GetUnitOfWork())
      {
        unit.Service<UserState>().Delete(x => x.EntityName == name && x.UserID == Client.User.UserID);
        unit.Service<ManagementLabel>().Delete(x => x.Grid == name && x.UserID == Client.User.UserID);
        unit.Save();
      }

      return Success(String.Empty);
    }

    [RequiresAuthentication(Functionalities.Default)]
    public ActionResult GetSaveStates()
    {
      return SimpleList((unit) =>
        from s in unit.Service<UserState>().GetAll(x => x.UserID == Client.User.UserID)
        group s by s.EntityName into states
        let latest = states.Max(g => g.StateID)
        from state in states
        where state.StateID == latest
        select new
        {
          state.StateID,
          state.EntityName,
          state.SavedState
        });
    }

    [RequiresAuthentication(Functionalities.Default)]
    public ActionResult GetSaveStateByEntity(string name)
    {
      return SimpleList((unit) =>
        from s in unit.Service<UserState>().GetAll(x => x.UserID == Client.User.UserID && x.EntityName == name)
        group s by s.EntityName into states
        let latest = states.Max(g => g.StateID)
        from state in states
        where state.StateID == latest
        select new
        {
          state.StateID,
          state.EntityName,
          data = state.SavedState
        });
    }

    [RequiresAuthentication(Functionalities.Default)]
    public ActionResult Save(string data, string name)
    {
      using (var unit = GetUnitOfWork())
      {
        var state = new UserState
        {
          EntityName = name,
          SavedState = data,
          UserID = Client.User.UserID
        };

        ((IUserService)unit.Service<User>()).SaveState(state);

        unit.Save();

        return Success("Save state");
      }
    }
  }
}
