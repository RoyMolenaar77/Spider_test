using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Concentrator.Objects.Web;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Models.Orders;
using Concentrator.Objects.Services.ServiceInterfaces;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.Enumerations;
using Concentrator.Web.Shared;
using Concentrator.Web.Shared.Controllers;

namespace Concentrator.ui.Management.Controllers
{
  public class OrderLineController : BaseController
  {
    [RequiresAuthentication(Functionalities.GetOrderLine)]
    public ActionResult GetList(int orderID)
    {
      return List(unit => (from ol in unit.Service<OrderLine>().GetAll(c => c.OrderID == orderID).ToList()
                           let productName = ol.Product != null ?
                            ol.Product.ProductDescriptions.FirstOrDefault(x => x.LanguageID == Client.User.LanguageID) ??
                            ol.Product.ProductDescriptions.FirstOrDefault() : null
                           where ol.OrderID == orderID
                           select new
                           {
                             ol.OrderID,
                             ol.OrderLineID,
                             ol.Quantity,
                             ol.ProductID,
                             ProductName = productName != null ? (productName.ProductName ?? productName.ShortContentDescription) :
                              ol.Product != null ? ol.Product.VendorItemNumber : string.Empty,
                             ol.Product.VendorItemNumber,
                             ol.CustomerOrderLineNr,
                             ol.CustomerOrderNr,
                             ol.CustomerItemNumber,
                             ol.isDispatched,
                             ol.DispatchedToVendorID,
                             ol.VendorOrderNumber,
                             CurrentState = ol.CurrentState()
                           }).AsQueryable());
    }

    [RequiresAuthentication(Functionalities.ViewOrders)]
    public ActionResult Dispatch(int orderLineID)
    {
      try
      {
        using (var unit = GetUnitOfWork())
        {
          var vendorID = ((IOrderService)unit.Service<Order>()).DispatchOrderLine(orderLineID, (IUnitOfWork)unit);

          return Success("OrderLine dispatched to: " + unit.Service<Vendor>().Get(c => c.VendorID == vendorID).Name);
        }
      }
      catch (Exception e)
      {
        return Failure(e.Message);
      }
    }

    [RequiresAuthentication(Functionalities.ViewOrders)]
    public ActionResult GetVendorList()
    {
      return List(unit => from ol in unit.Service<Vendor>().GetAll(c => c.OrderDispatcherType != null)
                          select new
                          {
                            ol.VendorID,
                            ol.VendorType,
                            ol.Name,
                            ol.Description,
                            ol.BackendVendorCode,
                            ol.ParentVendorID,
                            ol.OrderDispatcherType,
                            ol.CDPrice,
                            ol.DSPrice,
                            ol.PurchaseOrderType,
                            ol.IsActive,
                            ol.CutOffTime,
                            ol.DeliveryHours
                          });
    }

    [RequiresAuthentication(Functionalities.ViewOrders)]
    public ActionResult DispatchEverything(int orderID, int vendorID)
    {
      using (var unit = GetUnitOfWork())
      {
        ((IOrderService)unit.Service<Order>()).QueueForDispatch(orderID, vendorID);

        unit.Save();

        return Success("Orders are queued for dispatch");
      }
    }

    [RequiresAuthentication(Functionalities.ViewOrders)]
    public ActionResult GetOrderLineStates()
    {
      List<OrderLineStatus> enums = EnumHelper.EnumToList<OrderLineStatus>();

      return Json(new
      {
        results = (from e in enums
                   select new
                   {
                     ID = (int)e,
                     Name = Enum.GetName(typeof(OrderLineStatus), e)
                   }).ToArray()
      });
    }
  }
}
