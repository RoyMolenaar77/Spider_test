using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.DataAccess.Repository;

namespace Concentrator.Objects.Models.Orders
{
  [DebuggerDisplay("ID: {OrderID}")]
  public class Order : BaseModel<Order>
  {
    public Int32 OrderID { get; set; }

    public String Document { get; set; }

    public Int32 ConnectorID { get; set; }

    public Boolean IsDispatched { get; set; }

    public DateTime? DispatchToVendorDate { get; set; }

    public DateTime ReceivedDate { get; set; }

    public Boolean? isDropShipment { get; set; }

    public String Remarks { get; set; }

    public Int32? ShipToCustomerID { get; set; }

    public String CustomerOrderReference { get; set; }

    public String EdiVersion { get; set; }

    public String BSKIdentifier { get; set; }

    public String WebSiteOrderNumber { get; set; }

    public String PaymentTermsCode { get; set; }

    public String PaymentInstrument { get; set; }

    public Boolean? BackOrdersAllowed { get; set; }

    public String RouteCode { get; set; }

    public String HoldCode { get; set; }

    public Int32? SoldToCustomerID { get; set; }

    public Boolean HoldOrder { get; set; }

    public Int32 OrderType { get; set; }

    public string OrderLanguageCode { get; set; }

    public string PhysicalIdentifier { get; set; }

    public virtual Connector Connector { get; set; }

    public virtual Customer ShippedToCustomer { get; set; }

    public virtual Customer SoldToCustomer { get; set; }

    public virtual ICollection<OrderLine> OrderLines { get; set; }

    public virtual ICollection<Outbound> Outbounds { get; set; }

    public virtual ICollection<OrderResponse> OrderResponses { get; set; }

    public virtual ICollection<DatcolLink> DatcolLinks { get; set; }

    public virtual ICollection<RefundQueue> RefundQueues { get; set; }

    public List<OrderResponse> GetOrderResponses(OrderResponseTypes type, IRepository<OrderResponse> repo)
    {
      string responseType = type.ToString();
      return repo.GetAll(x => x.OrderResponseLines.Any(y => y.OrderLineID.HasValue && y.OrderLine.OrderID == OrderID) && x.ResponseType == responseType).ToList();
    }

    public override System.Linq.Expressions.Expression<Func<Order, bool>> GetFilter()
    {
      return null;
    }
  }
}