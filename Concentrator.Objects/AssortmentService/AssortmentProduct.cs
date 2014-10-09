using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Objects.AssortmentService
{
  public abstract class AssortmentProduct
  {
    /// <summary>
    /// Vendor item number
    /// </summary>
    public string ManufacturerID { get; set; }

    public string CustomProductID { get; set; }

    public int ProductID { get; set; }

    public bool IsNonAssortmentItem { get; set; }

    public bool IsConfigurable { get; set; }

    public bool Visible { get; set; }
  }
}
