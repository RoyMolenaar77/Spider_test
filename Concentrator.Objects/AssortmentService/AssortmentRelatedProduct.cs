using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Objects.AssortmentService
{
  public class AssortmentRelatedProduct
  {
    public int RelatedProductID { get; set; }

    public bool IsConfigured { get; set; }

    public string Type { get; set; }

    public int TypeID { get; set; }

    public int Index { get; set; }

    public int? MapsToMagentoTypeID { get; set; } 
  }
}
