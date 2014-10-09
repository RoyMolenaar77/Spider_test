using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Models.EDI.Order;
using Concentrator.Objects.Models.Orders;
using Concentrator.Objects.Models.EDI.Vendor;
using Concentrator.Objects.Models.Connectors;

namespace Concentrator.Objects.Models.EDI.Response
{
  public class EdiOrderResponse : AuditObjectBase<EdiOrderResponse>
  {
    public Int32 EdiOrderResponseID { get; set; }

    public Int32 ResponseType { get; set; }

    public string VendorDocument { get; set; }

    public decimal? AdministrationCost { get; set; }

    public decimal? DropShipmentCost { get; set; }

    public decimal? ShipmentCost { get; set; }

    public DateTime? OrderDate { get; set; }

    public bool? PartialDelivery { get; set; }

    public string VendorDocumentNumber { get; set; }

    public DateTime? VendorDocumentDate { get; set; }

    public decimal? VatPercentage { get; set; }

    public Int32 EdiVendorID { get; set; }

    public decimal? VatAmount { get; set; }

    public decimal? TotalGoods { get; set; }

    public decimal? TotalExVat { get; set; }

    public decimal? TotalAmount { get; set; }

    public Int32? PaymentConditionDays { get; set; }

    public string PaymentConditionCode { get; set; }

    public string PaymentConditionDiscount { get; set; }

    public string PaymentConditionDiscountDescription { get; set; }

    public string TrackAndTrace { get; set; }

    public string InvoiceDocumentNumber { get; set; }

    public string ShippingNumber { get; set; }

    public DateTime? ReqDeliveryDate { get; set; }

    public DateTime? InvoiceDate { get; set; }

    public string Currency { get; set; }

    public string InvoiCurrencyceDate { get; set; }

    public string DespAdvice { get; set; }

    public Int32? ShipToCustomerID { get; set; }

    public Int32? SoldToCustomerID { get; set; }

    public DateTime ReceiveDate { get; set; }

    public string TrackAndTraceLink { get; set; }

    public string VendorDocumentReference { get; set; }

    public Int32? EdiOrderID { get; set; }

    public Int32? ConnectorRelationID { get; set; }

    public virtual ICollection<EdiOrderResponseLine> EdiOrderResponseLines { get; set; }

    public virtual EdiOrder EdiOrder { get; set; }

    public virtual Customer ShippedToCustomer { get; set; }

    public virtual Customer SoldToCustomer { get; set; }

    public List<ReturnError> ResponseErrors { get; set; }

    public virtual ConnectorRelation ConnectorRelation { get; set; }

    public override System.Linq.Expressions.Expression<Func<EdiOrderResponse, bool>> GetFilter()
    {
      return null;
    }
  }
}
