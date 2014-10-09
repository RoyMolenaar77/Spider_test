using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Concentrator.Objects.Web;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Models.Orders;
using Concentrator.Web.Shared;
using Concentrator.Web.Shared.Controllers;
using System.Data.Entity;

namespace Concentrator.ui.Management.Controllers
{
  public class OrderResponseController : BaseController
  {
    [RequiresAuthentication(Functionalities.GetOrderResponse)]
    public ActionResult GetList(int? OrderID)
    {
      if (!Client.User.ConnectorID.HasValue) throw new InvalidOperationException("Connector needs to be applied to user");

      return List(unit => from r in unit.Service<OrderResponse>().GetAll().Include(x => x.Order)
                          where (OrderID.HasValue ? r.OrderID.Value == OrderID : true)
                          && (r.Order.ConnectorID == Client.User.ConnectorID || r.Order.Connector.ParentConnectorID == Client.User.ConnectorID)
                          select new OrderResponseDto
                          {
                            OrderID = r.OrderID,
                            OrderResponseID = r.OrderResponseID,
                            ResponseType = r.ResponseType,
                            VendorDocument = r.VendorDocument,
                            AdministrationCost = r.AdministrationCost,
                            DropShipmentCost = r.DropShipmentCost,
                            ShipmentCost = r.ShipmentCost,
                            OrderDate = r.OrderDate,
                            PartialDelivery = r.PartialDelivery,
                            VendorDocumentNumber = r.VendorDocumentNumber,
                            VendorDocumentDate = r.VendorDocumentDate,
                            VatPercentage = r.VatPercentage,
                            VatAmount = r.VatAmount,
                            TotalGoods = r.TotalGoods,
                            WebSiteOrderNumber = r.Order.WebSiteOrderNumber,
                            TotalExVat = r.TotalExVat,
                            TotalAmount = r.TotalAmount,
                            PaymentConditionDays = r.PaymentConditionDays,
                            PaymentConditionCode = r.PaymentConditionCode,
                            PaymentConditionDiscount = r.PaymentConditionDiscount,
                            PaymentConditionDiscountDescription = r.PaymentConditionDiscountDescription,
                            TrackAndTrace = r.TrackAndTrace
                          });
    }

    [RequiresAuthentication(Functionalities.GetOrderResponse)]
    public ActionResult GetLines(int? OrderResponseID)
    {
      return List(unit => from l in unit.Service<OrderResponseLine>().GetAll()
                          where (OrderResponseID.HasValue ? OrderResponseID.Value == l.OrderResponseID : true)
                          select new OrderResponseLineDto
                          {
                            OrderResponseLineID = l.OrderResponseLineID,
                            OrderResponseID = l.OrderResponseID,
                            OrderLineID = l.OrderLineID,
                            Ordered = l.Ordered,
                            Backordered = l.Backordered,
                            Cancelled = l.Cancelled,
                            Shipped = l.Shipped,
                            Invoiced = l.Invoiced,
                            Description = l.Description,
                            Unit = l.Unit,
                            Price = l.Price,
                            DeliveryDate = l.DeliveryDate,
                            VendorLineNumber = l.VendorLineNumber,
                            VendorItemNumber = l.VendorItemNumber,
                            OEMNumber = l.OEMNumber,
                            Barcode = l.Barcode,
                            Remark = l.Remark
                          });

    }
  }

  public class OrderResponseDto
  {
    public decimal? TotalExVat { get; set; }

    public decimal? TotalAmount { get; set; }

    public int? PaymentConditionDays { get; set; }
    public string PaymentConditionCode { get; set; }
    public string PaymentConditionDiscount { get; set; }
    public string PaymentConditionDiscountDescription { get; set; }
    public string TrackAndTrace { get; set; }

    public int? OrderID { get; set; }

    public int OrderResponseID { get; set; }

    public string ResponseType { get; set; }

    public string VendorDocument { get; set; }

    public decimal? AdministrationCost { get; set; }

    public decimal? DropShipmentCost { get; set; }

    public decimal? ShipmentCost { get; set; }

    public DateTime? OrderDate { get; set; }

    public bool? PartialDelivery { get; set; }

    public string VendorDocumentNumber { get; set; }

    public DateTime? VendorDocumentDate { get; set; }

    public decimal? VatPercentage { get; set; }
    public decimal? VatAmount { get; set; }
    public decimal? TotalGoods { get; set; }
    public string WebSiteOrderNumber { get; set; }
  }


  public class OrderResponseLineDto
  {
    public int OrderResponseLineID { get; set; }

    public int OrderResponseID { get; set; }

    public int? OrderLineID { get; set; }

    public int Ordered { get; set; }

    public int Backordered { get; set; }

    public int Cancelled { get; set; }

    public int Shipped { get; set; }

    public int Invoiced { get; set; }

    public string Description { get; set; }

    public string Unit { get; set; }

    public decimal Price { get; set; }

    public DateTime? DeliveryDate { get; set; }

    public string VendorLineNumber { get; set; }

    public string VendorItemNumber { get; set; }

    public string OEMNumber { get; set; }

    public string Barcode { get; set; }

    public string Remark { get; set; }
  }
}
