using Concentrator.Objects.Models.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Objects.Models.Orders
{
  public class RefundQueue : BaseModel<RefundQueue>
  {
    public int OrderID { get; set; }

    public int OrderResponseID { get; set; }

    public bool Valid { get; set; }

    public DateTime CreationTime { get; set; }

    public virtual Order Order { get; set; }

    public virtual OrderResponse OrderResponse { get; set; }

    public decimal Amount { get; set; }    
  }
}
