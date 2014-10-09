using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Concentrator.Web.Objects.EDI.DirectShipment;
using Concentrator.Objects.Models.Orders;
using Concentrator.Objects.Models.Vendors;

namespace Concentrator.Objects.Ordering.XmlFormats
{
  public abstract class BaseOrderExporter<T> where T : class, new()
  {
    public abstract T GetOrder(Dictionary<Concentrator.Objects.Models.Orders.Order, List<OrderLine>> orderLines, Vendor vendor);

    public abstract DirectShipmentRequest GetDirectShipmentOrder(Concentrator.Objects.Models.Orders.Order order, List<OrderLine> orderLines, Vendor administrativeVendor, Vendor vendor);
  }
}
