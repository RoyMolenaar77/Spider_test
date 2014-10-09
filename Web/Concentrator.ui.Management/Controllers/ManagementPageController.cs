using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Concentrator.Objects.Web;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Models.Management;
using Concentrator.Web.Shared; using Concentrator.Web.Shared.Controllers;
using System.Web.Script.Serialization;

namespace Concentrator.ui.Management.Controllers
{
  public class ManagementPageController : BaseController
  {
    [RequiresAuthentication(Functionalities.CreateManagementPage)]
    public ActionResult Create()
    {
      return Create<ManagementPage>();
    }

    [RequiresAuthentication(Functionalities.GetManagementPage)]
    public ActionResult GetCustomLabels()
    {

      return SimpleList(unit => 
        from managementLabel in unit.Service<ManagementLabel>().GetAll()
        select new
        {
          ID = managementLabel.Field,
          Name = managementLabel.Label,
          Grid = managementLabel.Grid
        });
    }

    [RequiresAuthentication(Functionalities.GetManagementPage)]
    public ActionResult SetCustomLabels(string data, string name)
    {
      var serializer = new JavaScriptSerializer();

      using (var unit = GetUnitOfWork())
      {
        try
        {
          foreach (var pair in serializer.Deserialize<dynamic>(data))
          {
            string field = pair.Key;
            string label = pair.Value;
            var customLabel = unit.Service<ManagementLabel>().Get(x => x.UserID == Client.User.UserID && x.Field == field && x.Grid == name);

            if (customLabel != null)
            {
              customLabel.Label = label;
            }
            else
            {
              customLabel = new ManagementLabel
              {
                Field = field,
                Grid = name,
                Label = label,
                UserID = Client.User.UserID
              };

              unit.Service<ManagementLabel>().Create(customLabel);
            }
          }

          unit.Save();
        }
        catch (Exception ex)
        {
          return Failure(string.Format("Failed to save custom label: {0}", ex.Message));
        }

        return Success(String.Empty);
      }
    }
  }
}
