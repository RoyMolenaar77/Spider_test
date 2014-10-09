using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Objects.Models.Orders
{
  public class RefundQueueHistory
  {
    public int OrderID { get; set; }

    public int OrderResponseID { get; set; }

    public DateTime CreationTime { get; set; }

    public DateTime ProcessedTime { get; set; }

    public decimal Amount { get; set; }
  }
}
