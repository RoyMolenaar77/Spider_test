using Concentrator.Objects.Models.MastergroupMapping;
using Concentrator.Objects.Models.Users;
using Concentrator.Web.Shared;
using Concentrator.Web.Shared.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Concentrator.ui.Management.Controllers
{
  public class MasterGroupMappingSettingController : BaseController
  {
    [RequiresAuthentication(Functionalities.DefaultMasterGroupMapping)]
    public ActionResult GetOptions(int? settingID)
    {
      if (settingID.HasValue)
      {
        var options = (from a in GetUnitOfWork().Scope.Repository<MasterGroupMappingSettingOption>().GetAll(x => x.MasterGroupMappingSettingID == settingID.Value)
                       select new
                       {
                         ID = a.OptionID,
                         Value = a.Value
                       }).ToList();
        return Json(new
        {
          options = options
        });
      }
      return Failure("No setting was selected.");
    }
  }
}