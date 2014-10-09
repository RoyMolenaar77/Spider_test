using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Concentrator.ui.Management.Models
{
  public class PriceHierarchyNode : TreeNodeBase
  {
    public int? BrandID { get; set; }

    public int? ProductID { get; set; }
  }
}