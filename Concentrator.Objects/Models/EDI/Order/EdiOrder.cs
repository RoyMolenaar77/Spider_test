using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Models.Orders;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.Models.EDI.Post;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Models.EDI.Response;

namespace Concentrator.Objects.Models.EDI.Order
{
  public class EdiOrder : AuditObjectBase<EdiOrder>
  {
    public Int32 EdiOrderID { get; set; }

    public Int32 EdiRequestID { get; set; }

    public String Document { get; set; }

    public Int32 ConnectorID { get; set; }

    public bool IsDispatched { get; set; }

    public DateTime? DispatchToVendorDate { get; set; }

    public DateTime ReceivedDate { get; set; }

    public bool? isDropShipment { get; set; }

    public string Remarks { get; set; }

    public Int32? ShipToCustomerID { get; set; }

    public Int32? SoldToCustomerID { get; set; }

    public string CustomerOrderReference { get; set; }

    public string EdiVersion { get; set; }

    public Int32? BSKIdentifier { get; set; }

    public string WebSiteOrderNumber { get; set; }

    public string PaymentTermsCode { get; set; }

    public string PaymentInstrument { get; set; }

    public bool? BackOrdersAllowed { get; set; }

    public string RouteCode { get; set; }

    public string HoldCode { get; set; }

    public bool HoldOrder { get; set; }

    public Int32 Status { get; set; }

    public Int32 EdiOrderTypeID { get; set; }

    public DateTime? RequestDate { get; set; }

    public virtual Connector Connector { get; set; }

    public virtual ICollection<EdiOrderPost> EdiOrderPosts { get; set; }

    public virtual ICollection<EdiOrderResponse> EdiOrderResponses { get; set; }

    public virtual ICollection<EdiOrderLine> EdiOrderLines { get; set; }

    public virtual Customer ShippedToCustomer { get; set; }

    public virtual Customer SoldToCustomer { get; set; }

    public bool? PartialDelivery { get; set; }

    public int? ConnectorRelationID { get; set; }

    public EdiOrderListener EdiOrderListener { get; set; }

    public virtual ConnectorRelation ConnectorRelation { get; set; }

    public virtual EdiOrderType EdiOrderType { get; set; }

    public DateTime OrderDate { get; set; }

    public override System.Linq.Expressions.Expression<Func<EdiOrder, bool>> GetFilter()
    {
      return null;
    }
  }
}

