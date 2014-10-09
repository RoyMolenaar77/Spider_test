using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.AssortmentService;

namespace Concentrator.Plugins.Magento.Models
{
  public struct RelatedProductWithIndex
  {
    public string RelatedProductManufacturerID;
    public int Index;
    public int CatalogProductLinkTypeID { get; set; }
  }
}
