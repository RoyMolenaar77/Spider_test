using System.Web.Mvc;
using Concentrator.Objects.Models.Users;
using Concentrator.Web.Shared;
using Concentrator.Web.Shared.Controllers;
using Concentrator.Objects.Models.Configuration;
using System;

namespace Concentrator.ui.Management.Controllers
{
  public class ConfigController : BaseController
  {

    [RequiresAuthentication(Functionalities.GetBrand)]
    public ActionResult GetSoldenPeriod()
    {
      using (var unit = GetUnitOfWork())
      {
        bool isSolden = false;

        var test = unit.Service<Config>().GetAll();


        var config = unit.Service<Config>().Get(x => x.Name.Equals("IsSolden"));

        if (config != null)
          isSolden = bool.Parse(config.Value);

        return Json(new
        {
          success = true,
          data = new { isSolden = isSolden }
        });

      }
    }

    [RequiresAuthentication(Functionalities.GetBrand)]
    public ActionResult SetSoldenPeriod(string isSolden)
    {
      try
      {
        bool solden = !string.IsNullOrEmpty(isSolden) && isSolden.Equals("on") ? true : false;

        var baseConfig = new Config()
        {
          Name = "IsSolden",
          Value = solden.ToString(),
          Description = "If true, no solden period is alowed!"
        };

        using (var unit = GetUnitOfWork())
        {
          var config = unit.Service<Config>().Get(x => x.Name.Equals("IsSolden"));

          if (config == null)
          {
            unit.Service<Config>().Create(baseConfig);
          }
          else
          {
            //ugly fix because I cannot update Value because it belongs to the identifier of the table

            unit.Service<Config>().Delete(config);
            unit.Service<Config>().Create(baseConfig);
          }

          unit.Save();
        }
        return Success("Successfully updated solden period");
      }
      catch (Exception e)
      {
        return Failure("Could not update IsSolden period! Error: " + e.Message);
      }

    }
  }
}
