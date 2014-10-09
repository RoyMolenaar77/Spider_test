using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Concentrator.Objects;
using Concentrator.Objects.Web;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Web.Shared;
using Concentrator.Web.Shared.Controllers;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.Models;

namespace Concentrator.ui.Management.Controllers
{
  public class VendorStockController : BaseController
  {
    [RequiresAuthentication(Functionalities.GetVendorStock)]
    public ActionResult GetList()
    {
      return List(unit => from c in unit.Service<VendorStock>().GetAll()
                          select new
                          {
                            c.ProductID,
                            c.Product.ProductDescriptions.Where(pd => pd.LanguageID == Client.User.LanguageID).FirstOrDefault().ProductName,
                            c.VendorID,
                            c.QuantityOnHand,
                            c.QuantityToReceive,
                            ConcentratorStatus = c.AssortmentStatus.Status,
                            c.VendorStatus
                          });
    }

    [RequiresAuthentication(Functionalities.GetVendorStock)]
    public ActionResult Create(int productID)
    {
      return Create<VendorStock>((unit, stock) =>
      {
        stock.ProductID = productID;
        unit.Service<VendorStock>().Create(stock);
      });
    }


    [RequiresAuthentication(Functionalities.GetVendorStock)]
    public ActionResult DeleteByVendorAssortment(int _VendorID, int _ProductID, int _VendorStockTypeID)
    {
      return Delete<VendorStock>(c => c.VendorID == _VendorID && c.ProductID == _ProductID && c.VendorStockTypeID == _VendorStockTypeID);
    }

    [RequiresAuthentication(Functionalities.GetVendorStock)]
    public ActionResult Search(string query)
    {
      return Search(unit => from v in unit.Service<VendorStockType>().Search(query)
                            select new
                            {
                              v.VendorStockTypeID,
                              v.StockType
                            });
    }

    [RequiresAuthentication(Functionalities.GetVendorStock)]
    public ActionResult Update(int _VendorID, int _ProductID, int _VendorStockTypeID)
    {
      return Update<VendorStock>(c => c.VendorID == _VendorID && c.ProductID == _ProductID && c.VendorStockTypeID == _VendorStockTypeID);
    }

    [RequiresAuthentication(Functionalities.GetVendorStock)]
    public ActionResult GetStockByProduct(int productID)
    {
      using (var unit = GetUnitOfWork())
      {
        var visibilityFlag = (int)VendorType.ShowsStockInManagementApplication;

        var result = 
          from vendorStock in unit.Scope
            .Repository<VendorStock>()
            .Include(vendorStock => vendorStock.Product)
            .Include(vendorStock => vendorStock.Vendor)
            .GetAll(vendorStock => vendorStock.ProductID == productID)
          where (vendorStock.Vendor.VendorType & visibilityFlag) > 0
          let vendorAssortment = vendorStock.Product.VendorAssortments.FirstOrDefault(va => va.VendorID == vendorStock.VendorID)
          select new
          {
            CustomItemNumber = vendorAssortment.CustomItemNumber,
            ProductID = vendorStock.ProductID,
            IsActive = vendorAssortment == null || vendorAssortment.IsActive,
            QuantityOnHand = vendorStock.QuantityOnHand == null ? 0 : vendorStock.QuantityOnHand,
            QuantityToReceive = vendorStock.QuantityToReceive ?? 0,
            StockStatus = vendorStock.AssortmentStatus != null
              ? vendorStock.AssortmentStatus.Status
              : "N/A",
            StockType = vendorStock.VendorStockType.StockType,
            UnitCost = vendorStock.UnitCost ?? 0M,
            Vendor = vendorStock.Vendor.Name,
            VendorID = vendorStock.VendorID,
            VendorStockTypeID = vendorStock.VendorStockTypeID,
            VendorType = vendorStock.Vendor.VendorType
          };

        return List(result.AsQueryable());
      }
    }
  }
}
