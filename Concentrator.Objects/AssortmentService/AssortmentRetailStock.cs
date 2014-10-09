using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Objects.AssortmentService
{
  public class AssortmentRetailStock : AssortmentStock
  {
    public string Name { get; set; }

    public string VendorCode { get; set; }

    public decimal CostPrice { get; set; }
  }
}
