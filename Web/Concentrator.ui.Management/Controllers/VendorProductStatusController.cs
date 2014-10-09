using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Concentrator.Objects.Web;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Models.Vendors;
using System.Data.SqlClient;
using Concentrator.Objects.Services.ServiceInterfaces;
using Concentrator.Web.Shared;
using Concentrator.Web.Shared.Controllers;

namespace Concentrator.ui.Management.Controllers
{
  public class VendorProductStatusController : BaseController
  {
    [RequiresAuthentication(Functionalities.GetVendorProductStatus)]
    public ActionResult GetList(int? ConcentratorStatusID)
    {
      return List(unit => from v in unit.Service<VendorProductStatus>().GetAll()
                          where ConcentratorStatusID.HasValue ? v.ConcentratorStatusID == ConcentratorStatusID : true
                          select new
                          {
                            v.VendorID,
                            v.VendorStatus,
                            v.ConcentratorStatusID,
                            Vendor = v.Vendor.Name,
                            ConcentratorStatus = v.AssortmentStatus.Status
                          });
    }

    [RequiresAuthentication(Functionalities.UpdateVendorProductStatus)]
    public ActionResult Update(int _VendorID, string _VendorStatus, int _ConcentratorStatusID)
    {
      try
      {
        using (var unit = GetUnitOfWork())
        {
          ((IVendorService)unit.Service<Vendor>()).UpdateVendorProductStatus(_VendorID,
                                            _ConcentratorStatusID,
                                            int.Parse(Request["ConcentratorStatusID"]),
                                            _VendorStatus);
        }
        return Success("Successfully updated vendor product statuses");
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

    [RequiresAuthentication(Functionalities.UpdateVendorProductStatus)]
    public ActionResult Delete(int _VendorID, string _VendorStatus, int _ConcentratorStatusID)
    {
      return Delete<VendorProductStatus>(x => x.VendorID == _VendorID && x.VendorStatus == _VendorStatus && x.ConcentratorStatusID == _ConcentratorStatusID);
    }
  }
}
