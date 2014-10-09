using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Web;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.Services;
using Concentrator.Objects.Services.ServiceInterfaces;
using Concentrator.Web.Shared;
using Concentrator.Web.Shared.Controllers;
using System.Data.SqlClient;
using Concentrator.Objects.Models.Statuses;


namespace Concentrator.ui.Management.Controllers
{
  public class ConnectorProductStatusController : BaseController
  {
    [RequiresAuthentication(Functionalities.GetConnectorProductStatus)]
    public ActionResult GetList(int? _ConcentratorStatusID)
    {
      return List(unit => from v in unit.Service<ConnectorProductStatus>().GetAll()
                          where _ConcentratorStatusID.HasValue ? v.ConcentratorStatusID == _ConcentratorStatusID : true
                          && Client.User.ConnectorID.HasValue ? v.ConnectorID == Client.User.ConnectorID : true
                          select new
                          {
                            v.ConnectorProductStatusID,
                            v.ConnectorID,
                            v.ConnectorStatus,
                            v.ConcentratorStatusID,                            
                            ConnectorStatusID = v.AssortmentStatus.StatusID,
                            Connector = v.Connector.Name,
                            ConcentratorStatus = v.AssortmentStatus.Status
                          });
    }

    [RequiresAuthentication(Functionalities.CreateConnectorProductStatus)]
    public ActionResult Create(int ConcentratorStatusID)
    {
      return Create<ConnectorProductStatus>(onCreatingAction: (unit, cps) =>
      {
        cps.ConnectorStatus = unit.Service<AssortmentStatus>().Get(x => x.StatusID == ConcentratorStatusID).Status;
      });
    }

    [RequiresAuthentication(Functionalities.DeleteConnectorProductStatus)]
    public ActionResult Delete(int _ConnectorProductStatusID)
    {
      return Delete<ConnectorProductStatus>(c => c.ConnectorProductStatusID == _ConnectorProductStatusID);
    }

    [RequiresAuthentication(Functionalities.UpdateConnectorProductStatus)]
    public ActionResult Update(int _ConnectorID, int? _ConcentratorStatusID, int? _ConnectorStatusID, string ConnectorStatus)
    {
      try
      {
        using (var unit = GetUnitOfWork())
        {
          ((IConnectorService)unit.Service<Connector>()).UpdateConnectorProductStatus(_ConnectorID, _ConnectorStatusID.Value,
            int.Parse(Request["ConnectorStatusID"]), _ConcentratorStatusID.Value, int.Parse(Request["ConcentratorStatusID"]), ConnectorStatus);

          unit.Save();
        }
        return Success("Successfully updated connector product statuses");
      }
      catch (SqlException e)
      {
        return HandleSqlException(e);
      }
      catch (Exception e)
      {
        return Failure("Something went wrong: " + e.Message);
      }
    }
  }
}
