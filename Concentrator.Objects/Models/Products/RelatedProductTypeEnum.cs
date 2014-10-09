using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Objects.Models.Products
{
  public enum RelatedProductTypeEnum
  {
    /// <summary>
    /// Model/Variant of a product
    /// </summary>
    Model = 2,

    /// <summary>
    /// Accessory to a product
    /// </summary>
    Accessory = 3,

    /// <summary>
    /// A product related to a product
    /// </summary>
    Related = 4 
  }
}
