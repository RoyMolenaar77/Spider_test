using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Objects.AssortmentService
{
  public class AssortmentStock
  {
    public int InStock { get; set; }

    public DateTime? PromisedDeliveryDate { get; set; }

    public int QuantityToReceive { get; set; }

    public string StockStatus { get; set; }
  }
}
