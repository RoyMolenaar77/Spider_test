using System;

namespace Concentrator.Plugins.Axapta.Models
{
  public class OrderNotification
  {
    public Int32 OrderID { get; set; }
    public string Document { get; set; }
    public string WebSiteOrderNumber { get; set; }
    public int OrderType { get; set; }
    public Int32 OrderLineID { get; set; }
    public int Quantity { get; set; }
    public string OriginalLine { get; set; }
    public Int32? OrderResponseLineID { get; set; }
    public int? Ordered { get; set; }
    public int? Cancelled { get; set; }
    public int? Shipped { get; set; }
    public string Html { get; set; }
  }
}
