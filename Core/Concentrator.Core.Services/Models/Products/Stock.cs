using System;

namespace Concentrator.Core.Services.Models.Products
{
  public class Stock
  {
    public int InStock { get; set; }

    public DateTime? PromisedDeliveryDate { get; set; }

    public int QuantityToReceive { get; set; }

    public string StockStatus { get; set; }
  }
}
