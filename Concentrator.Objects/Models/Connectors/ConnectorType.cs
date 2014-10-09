using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Objects.Models.Connectors
{
  [Flags]
  public enum ConnectorType
  {
    Content = 1,
    WebAssortment = 2,
    ShopAssortment = 4,
    OrderHandling = 8,
    Images = 16,
    Reviews = 32,
    FileExport = 64,
    RetailStock = 128,
    Customers = 256,
    ExcelExport = 512,
    EDI = 1024,
    DhlTool = 2048,
    Sepa = 4096
  }
}
