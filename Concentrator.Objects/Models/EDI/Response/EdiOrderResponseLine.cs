using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.EDI.Order;
using Concentrator.Objects.Models.Base;

namespace Concentrator.Objects.Models.EDI.Response
{
  public class EdiOrderResponseLine : BaseModel<EdiOrderResponseLine>
  {
    public Int32 EdiOrderResponseLineID { get; set; }

    public Int32 EdiOrderResponseID { get; set; }

    public Int32? EdiOrderLineID { get; set; }

    public Int32 Ordered { get; set; }

    public Int32 Backordered { get; set; }

    public Int32 Cancelled { get; set; }

    public Int32 Shipped { get; set; }

    public Int32 Invoiced { get; set; }

    public string Unit { get; set; }

    public Decimal Price { get; set; }

    public DateTime? DeliveryDate { get; set; }

    public string VendorLineNumber { get; set; }

    public string VendorItemNumber { get; set; }

    public string OEMNumber { get; set; }

    public string Barcode { get; set; }

    public string Remark { get; set; }

    public string Description { get; set; }

    public bool processed { get; set; }

    public DateTime? RequestDate { get; set; }

    public Decimal? VatAmount { get; set; }

    public Decimal? vatPercentage { get; set; }

    public string CarrierCode { get; set; }

    public Int32? NumberOfPallets { get; set; }

    public Int32? NumberOfUnits { get; set; }

    public string TrackAndTrace { get; set; }

    public string SerialNumbers { get; set; }

    public Int32 Delivered { get; set; }

    public string TrackAndTraceLink { get; set; }

    public string ProductName { get; set; }

    public List<ReturnError> ResponseErrors { get; set; }
    
    public string html { get; set; }

    public virtual EdiOrderResponse EdiOrderResponse { get; set; }

    public virtual EdiOrderLine EdiOrderLine { get; set; }

    public override System.Linq.Expressions.Expression<Func<EdiOrderResponseLine, bool>> GetFilter()
    {
      return null;
    }
  }
}
