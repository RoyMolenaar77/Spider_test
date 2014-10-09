using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Objects.AssortmentService
{
  public abstract class AssortmentProductWithRelatedProducts : AssortmentProduct
  {
    public List<AssortmentRelatedProduct> RelatedProducts { get; set; }
  }
}
