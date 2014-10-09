using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using Concentrator.Objects.Web;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.Services.ServiceInterfaces;
using Concentrator.Web.Shared;
using Concentrator.Web.Shared.Controllers;
using Concentrator.Objects.Models.Products;

namespace Concentrator.ui.Management.Controllers
{
  public class VendorController : BaseController
  {
    [RequiresAuthentication(Functionalities.GetVendor)]
    public ActionResult GetList()
    {
      return List(unit => (from v in unit.Service<Vendor>().GetAll().ToList()
                           select new
                           {
                             v.VendorID,
                             v.BackendVendorCode,
                             v.IsActive,
                             v.CutOffTime,
                             v.DeliveryHours,
                             v.ParentVendorID,
                             v.DSPrice,
                             v.CDPrice,
                             isAssortment = ((VendorType)v.VendorType).Has(VendorType.Assortment),
                             isContent = ((VendorType)v.VendorType).Has(VendorType.Content),
                              VendorName = v.Name
                           }).AsQueryable());
    }

    [RequiresAuthentication(Functionalities.GetProduct)]
    public ActionResult GetByProduct(int productID, bool? includeConcentratorVendor)
    {
      using (var unit = GetUnitOfWork())
      {
        var vendors = ((IProductService)unit.Service<Product>()).GetContentVendors(productID, includeConcentratorVendor);

        return Json(new
        {
          results = (from r in vendors
                     where r != null
                     select new
                     {
                       r.VendorID,
                       VendorName = r.Name
                     }).ToList()
        });
      }
    }

    [RequiresAuthentication(Functionalities.GetProduct)]
    public ActionResult GetEligibleForProduct(int productID, int excludeVendorID)
    {
      const int vendorType = (Int32)VendorType.Content;

      using (var unit = GetUnitOfWork())
      {
        var contentVendors = unit.Scope
          .Repository<Vendor>()
          .GetAll(vendor => (vendor.VendorType & vendorType) > 0 && vendor.IsActive && vendor.VendorID != excludeVendorID)
          .Select(vendor => new
          {
            VendorID = vendor.VendorID,
            VendorName = vendor.Name
          })
          .ToArray();

        return List(contentVendors.AsQueryable());
      }
    }

    //[RequiresAuthentication(Functionalities.Default)]
    //public ActionResult Search(string query)
    //{
    //  if (query == null) query = string.Empty;

    //  return SimpleList((unit) => from c in ((IVendorService)unit.Service<Vendor>()).GetAssortmentVendors()
    //                              .Search(query, l => l.Name == "Name")
    //                              select new
    //                              {
    //                                c.VendorID,
    //                                VendorName = c.Name
    //                              });
    //}

    [RequiresAuthentication(Functionalities.GetVendor)]
    public ActionResult Search(string query)
    {
      return Search((unit) => from v in unit.Service<Vendor>().Search(query)
                              select new
                              {
                                v.VendorID,
                                VendorName = v.Name
                              });
    }

    /// <summary>
    /// Returns all vendors without the  Concentrator one
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    [RequiresAuthentication(Functionalities.GetVendor)]
    public ActionResult SearchDefault(string query)
    {
      return Search((unit) => (from v in unit.Service<Vendor>().Search(query).ToList().Where(c => c.Name != "Concentrator")
                               select new
                               {
                                 v.VendorID,
                                 VendorName = v.Name
                               }).AsQueryable());
    }

    [RequiresAuthentication(Functionalities.UpdateVendor)]
    public ActionResult Update(int id)
    {
      return Update<Vendor>(c => c.VendorID == id, (unit, vendor) =>
      {
        string isContent = Request["isContent"];
        string isAssortment = Request["isAssortment"];

        VendorType va = (VendorType)vendor.VendorType;

        if (!string.IsNullOrEmpty(isContent) && (bool.Parse(isContent)))
        {
          va = va | VendorType.Content;
        }
        if (!string.IsNullOrEmpty(isAssortment) && (bool.Parse(isAssortment)))
        {
          va = va | VendorType.Assortment;
        }

        vendor.VendorType = (int)va;
      });
    }

    [RequiresAuthentication(Functionalities.GetVendor)]
    public ActionResult GetVendors()
    {
      int vendorType = (int)VendorType.Assortment | (int)VendorType.Content;

      return SimpleList(unit => unit
        .Service<Vendor>()
        .GetAll(vendor => (vendor.VendorType & vendorType) > vendorType)
        .Select(vendor => new { vendor.VendorID, VendorName = vendor.Name })
        .AsQueryable());
    }

    [RequiresAuthentication(Functionalities.GetVendor)]
    public ActionResult GetAllVendors()
    {
      using (var pDb = new PetaPoco.Database(Objects.Environments.Environments.Current.Connection, "System.Data.SqlClient"))
      {
        var vendors = pDb.Query<VendorView>("SELECT Name as VendorName, VendorID FROM Vendor").ToList();
        return SimpleList(vendors);
      }
    }

    private class VendorView
    {
      public int VendorID { get; set; }
      public string VendorName { get; set; }
    }

  }
}
