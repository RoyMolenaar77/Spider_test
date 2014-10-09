using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;

namespace Concentrator.Objects.Models.Orders 
{
  public class OrderLedger : BaseModel<OrderLedger>
  {
    public Int32 OrderLedgerID { get; set; }
          
    public Int32 OrderLineID { get; set; }
          
    public Int32 Status { get; set; }
          
    public DateTime LedgerDate { get; set; }
          
    public virtual OrderLine OrderLine { get;set;}

    public int? Quantity { get; set; }

    public override System.Linq.Expressions.Expression<Func<OrderLedger, bool>> GetFilter()
    {
      return null;
    }
  }
}