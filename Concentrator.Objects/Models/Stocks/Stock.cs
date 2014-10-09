using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;

namespace Concentrator.Objects.Models.Stocks
{
  public class Stock
  {
    public Int32 ProductID { get; set; }

    public Int32 QuantityOnHand { get; set; }

    public DateTime PromisedDeliveryDate { get; set; }

    public Int32 QuantityToReceive { get; set; }

  }
}