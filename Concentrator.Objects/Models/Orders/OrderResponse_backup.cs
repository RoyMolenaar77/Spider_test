using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Web;

namespace Concentrator.Objects.Models.Orders
{
  public class OrderResponse_backup : BaseModel<OrderResponse_backup>
  {
    public Int32 OrderResponseID { get; set; }
          
    public Int32 OrderID { get; set; }
          
    public String ResponseType { get; set; }
          
    public String VendorDocument { get; set; }
          
    public Int32 VendorID { get; set; }
          
    public Decimal AdministrationCost { get; set; }
          
    public Decimal DropShipmentCost { get; set; }
          
    public Decimal ShipmentCost { get; set; }
          
    public DateTime OrderDate { get; set; }
          
    public Boolean PartialDelivery { get; set; }
          
    public String VendorDocumentNumber { get; set; }
          
    public DateTime VendorDocumentDate { get; set; }
          
    public Decimal VatPercentage { get; set; }
          
    public Decimal VatAmount { get; set; }
          
    public Decimal TotalGoods { get; set; }
          
    public Decimal TotalExVat { get; set; }
          
    public Decimal TotalAmount { get; set; }
          
    public Int32 PaymentConditionDays { get; set; }
          
    public String PaymentConditionCode { get; set; }
          
    public String PaymentConditionDiscount { get; set; }
          
    public String PaymentConditionDiscountDescription { get; set; }
          
    public String TrackAndTrace { get; set; }
          
    public String InvoiceDocumentNumber { get; set; }
          
    public String ShippingNumber { get; set; }
          
    public DateTime ReqDeliveryDate { get; set; }
          
    public DateTime InvoiceDate { get; set; }
          
    public String Currency { get; set; }
          
    public String DespAdvice { get; set; }
          
    public Int32 ShipToCustomerID { get; set; }
          
    public Int32 SoldToCustomerID { get; set; }
          
    public DateTime ReceiveDate { get; set; }
          
    public String TrackAndTraceLink { get; set; }
          
    public String VendorDocumentReference { get; set; }


    public override System.Linq.Expressions.Expression<Func<OrderResponse_backup, bool>> GetFilter()
    {
      return (o => Client.User.VendorIDs.Contains(o.VendorID));
    }
  }
}