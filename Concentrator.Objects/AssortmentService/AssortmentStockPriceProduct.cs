using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Objects.AssortmentService
{
  public class AssortmentStockPriceProduct : AssortmentProductWithRelatedProducts
  {
    public AssortmentStock Stock { get; set; }

    public List<AssortmentRetailStock> RetailStock { get; set; }

    public List<AssortmentPrice> Prices { get; set; }
  }
}
