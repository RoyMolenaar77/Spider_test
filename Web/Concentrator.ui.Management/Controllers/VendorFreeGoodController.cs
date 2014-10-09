using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Concentrator.Objects.Web;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Web.Shared; using Concentrator.Web.Shared.Controllers;

namespace Concentrator.ui.Management.Controllers
{
  public class VendorFreeGoodController : BaseController
  {
    [RequiresAuthentication(Functionalities.GetVendorFreeGood)]
    public ActionResult GetList()
    {
      return List(unit => from v in unit.Service<VendorFreeGood>().GetAll()
                          select new
                          {
                            v.ProductID,
                            v.MinimumQuantity,
                            v.OverOrderedQuantity,
                            v.FreeGoodQuantity,
                            v.Description,
                            v.UnitPrice,
                            v.VendorAssortmentID,
                            v.VendorAssortment.CustomItemNumber,
                            VendorName = v.VendorAssortment.Vendor.Name,
                            ProductDescription = v.VendorAssortment.ShortDescription,
                            VendorID = v.VendorAssortment.VendorID
                          });
    }

    [RequiresAuthentication(Functionalities.CreateVendorFreeGood)]
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
          return Create<VendorFreeGood>(onCreatingAction: (unit, vendorFreeGood) =>
          {
            vendorFreeGood.VendorAssortmentID = assortment.VendorAssortmentID;
          });
        }
      }
    }

    [RequiresAuthentication(Functionalities.UpdateVendorFreeGood)]
    public ActionResult Update(int id)
    {
      return Update<VendorFreeGood>(c => c.VendorAssortmentID == id);
    }

    [RequiresAuthentication(Functionalities.DeleteVendorFreeGood)]
    public ActionResult Delete(int id)
    {
      return Delete<VendorFreeGood>(c => c.VendorAssortmentID == id);
    }
  }
}
