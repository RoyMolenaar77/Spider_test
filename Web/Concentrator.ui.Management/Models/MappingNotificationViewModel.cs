using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Concentrator.ui.Management.Models
{
  public class MappingNotificationViewModel
  {
    public int ProductMatches { get; set; }
    public int UnmatchedProductGroupsCount { get; set; }
    public int UnmatchedVendorStatuses { get; set; }
    public int UnmatchedConnectorStatuses { get; set; }
    public int UnmatchedBrands { get; set; }
    public int ProductsNoImages { get; set; }
    public int ProductsSmallImages { get; set; }
    public int MissingSpecifications { get; set; }

    public int OverlayProducts { get; set; }
    public int BaseProducts { get; set; }
    public int MissingPreferredContent { get; set; }
    
  }
}
