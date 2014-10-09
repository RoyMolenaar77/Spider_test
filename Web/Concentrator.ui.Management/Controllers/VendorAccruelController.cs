using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Concentrator.Objects.Web;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Models.Brands;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.Models.Statuses;
using Concentrator.Web.Shared;
using Concentrator.Web.Shared.Controllers;

namespace Concentrator.ui.Management.Controllers
{
  public class VendorAccruelController : BaseController
  {
    [RequiresAuthentication(Functionalities.GetVendorAccruel)]
    public ActionResult GetList()
    {
      return List(unit => from v in unit.Service<VendorAccruel>().GetAll()
                          select new
                          {
                            v.VendorAssortmentID,
                            v.AccruelCode,
                            v.MinimumQuantity,
                            v.Description,
                            v.UnitPrice
                          });
    }

    public ActionResult Create(int ProductID, int VendorID, int? vendorAssortmentID)
    {
      using (var unitOfWork = GetUnitOfWork())
      {
        var assortment = unitOfWork.Service<VendorAssortment>().Get(c => c.VendorID == VendorID && c.ProductID == ProductID);

        if (assortment == null)
        {
          return Failure("The selected vendor doesn't have the selected product in its assortment");
        }
        else
        {
          return Create<VendorAccruel>(onCreatingAction: (unit, vendorAccruel) =>
          {
            vendorAccruel.VendorAssortmentID = assortment.VendorAssortmentID;
          });
        }
      }
    }

    [RequiresAuthentication(Functionalities.DeleteVendorAccruel)]
    public ActionResult Delete(int id)
    {
      return Delete<VendorAccruel>(v => v.VendorAssortmentID == id);
    }

    [RequiresAuthentication(Functionalities.UpdateVendorAccruel)]
    public ActionResult Update(int id)
    {
      return Update<VendorAccruel>(v => v.VendorAssortmentID == id);
    }
  }
}
