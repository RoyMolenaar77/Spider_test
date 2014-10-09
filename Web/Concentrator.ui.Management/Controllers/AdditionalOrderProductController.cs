using System;
using System.Linq;
using System.Web.Mvc;
using Concentrator.Objects.Models.Orders;
using Concentrator.Objects.Models.Users;
using Concentrator.Web.Shared.Controllers;
using Concentrator.Web.Shared;

namespace Concentrator.ui.Management.Controllers
{
  public class AdditionalOrderProductController : BaseController
  {
    [RequiresAuthentication(Functionalities.GetAdditionalOrderProduct)]
    public ActionResult GetList()
    {
      return List(unit =>
                  (from a in unit.Service<AdditionalOrderProduct>().GetAll().ToList()
                   select new
                   {
                     a.AdditionalOrderProductID,
                     a.ConnectorID,
                     Connector = a.Connector.Name,
                     a.ConnectorProductID,
                     a.VendorID,
                     Vendor = a.Vendor.Name,
                     a.VendorProductID,
                     a.UnitPrice,
                     a.CreatedBy,
                     CreationTime = a.CreationTime.ToLocalTime(),
                     a.LastModifiedBy,
                     LastModificationTime = a.LastModificationTime.ToNullOrLocal()
                   }).AsQueryable());
    }

    [RequiresAuthentication(Functionalities.CreateAdditionalOrderProduct)]
    public ActionResult Create()
    {
      return Create<AdditionalOrderProduct>();
    }

    [RequiresAuthentication(Functionalities.UpdateAdditionalOrderProduct)]
    public ActionResult Update(int ID)
    {
      return Update<AdditionalOrderProduct>(a => a.AdditionalOrderProductID == ID);
    }

    [RequiresAuthentication(Functionalities.DeleteAdditionalOrderProduct)]
    public ActionResult Delete(int ID)
    {
      return Delete<AdditionalOrderProduct>(a => a.AdditionalOrderProductID == ID);
    }

  }
}
