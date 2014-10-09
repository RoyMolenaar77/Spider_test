using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Objects.Models.Results
{
  /// <summary>
  /// Provides information on incomplete mappings
  /// </summary>
  public class IncompleteMappingInfo
  {
    public int ProductMatches { get; set; }
    public int UnmatchedProductGroupsCount { get; set; }
    public int UnmatchedVendorStatuses { get; set; }
    public int UnmatchedConnectorStatuses { get; set; }
    public int UnmatchedBrands { get; set; }
    public int ProductsNoImages { get; set; }
    public int ProductsSmallImages { get; set; }
    public int MissingSpecifications { get; set; }
  }
}
