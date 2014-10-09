using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
using Concentrator.Web.Objects.EDI.ChangeOrder;


namespace Concentrator.Objects.Models.Orders
{
  public class OrderResponseLine : BaseModel<OrderResponseLine>
  {
    public Int32 OrderResponseLineID { get; set; }

    public Int32 OrderResponseID { get; set; }

    public Int32? OrderLineID { get; set; }

    public Int32 Ordered { get; set; }

    public Int32 Backordered { get; set; }

    public Int32 Cancelled { get; set; }

    public Int32 Shipped { get; set; }

    public Int32 Invoiced { get; set; }

    public String Unit { get; set; }

    public Decimal Price { get; set; }

    public DateTime? DeliveryDate { get; set; }

    public String VendorLineNumber { get; set; }

    public String VendorItemNumber { get; set; }

    public String OEMNumber { get; set; }

    public String Barcode { get; set; }

    public String Remark { get; set; }

    public String Description { get; set; }

    public Boolean Processed { get; set; }

    public DateTime? RequestDate { get; set; }

    public Decimal? VatAmount { get; set; }

    public Decimal? vatPercentage { get; set; }

    public String CarrierCode { get; set; }

    public Int32? NumberOfPallets { get; set; }

    public Int32? NumberOfUnits { get; set; }

    public String TrackAndTrace { get; set; }

    public String SerialNumbers { get; set; }

    public Int32 Delivered { get; set; }

    public String TrackAndTraceLink { get; set; }

    public String ProductName { get; set; }

    public String html { get; set; }

    public Int32? ProductID { get; set; }

    public virtual OrderLine OrderLine { get; set; }

    public virtual OrderResponse OrderResponse { get; set; }

    public virtual Products.Product Product { get; set; }

    public virtual ICollection<OrderItemFullfilmentInformation> OrderItemFullfilmentInformations { get; set; }

    public void setChangeType(ChangeType type) {
      _changeType = type;
    }

    public ChangeType? GetChangeType()
    {
      return _changeType;
    }

    private ChangeType? _changeType { get; set; }

    public override System.Linq.Expressions.Expression<Func<OrderResponseLine, bool>> GetFilter()
    {
      return null;
    }
  }
}