using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Concentrator.Web.Shared.Controllers;
using Concentrator.Web.CustomerSpecific.Coolcat.Repositories;
using Concentrator.Web.CustomerSpecific.Coolcat.Models;
using System.Xml.Linq;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Web.Shared;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Web;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects;
using Concentrator.Objects.Models.Products;

namespace Concentrator.Web.CustomerSpecific.Coolcat.Controllers
{
  public class ExcludeProductController : BaseController
  {
    public int WehkampConnectorID
    {
      get
      {
        using (var unit = GetUnitOfWork())
        {
          if (Client.User.ConnectorID.HasValue)
          {
            var connector = unit.Service<Connector>().Get(x => x.ConnectorID == Client.User.ConnectorID.Value);

            var wehkampConnectorIDsetting = connector.ConnectorSettings.GetValueByKey("WehkampConnectorID", "");

            if (string.IsNullOrEmpty(wehkampConnectorIDsetting))
              return -1;
            else
              return int.Parse(wehkampConnectorIDsetting);
          }
          else
          {
            return -1;
          }
        }
      }
    }


    [RequiresAuthentication(Functionalities.GetExcludeProducts)]
    public ActionResult GetAll()
    {
      return List(unit => from e in unit.Service<ExcludeProduct>().GetAll(x => x.ConnectorID == WehkampConnectorID)
                          select new
                          {
                            e.ExcludeProductID,
                            e.Value
                          }
        );
    }

    [RequiresAuthentication(Functionalities.CreateExcludeProducts)]
    public ActionResult Create()
    {
      if (WehkampConnectorID == -1)
        return Failure("You are unauthorized to create Excluded products!");

      return Create<ExcludeProduct>(onCreatingAction: (unit, excludeProductModel) =>
      {
        excludeProductModel.ConnectorID = WehkampConnectorID;
      });
    }


    [RequiresAuthentication(Functionalities.DeleteExcludeProducts)]
    public ActionResult Delete(int _ExcludeProductID)
    {
      if (WehkampConnectorID == -1)
        return Failure("You are unauthorized to delete Excluded products!");

      return Delete<ExcludeProduct>(x => x.ConnectorID == WehkampConnectorID && x.ExcludeProductID == _ExcludeProductID);
    }
  }
}
