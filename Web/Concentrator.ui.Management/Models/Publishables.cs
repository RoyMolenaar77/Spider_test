using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Concentrator.ui.Management.Models
{
  public class Publishables
  {
    public bool? Attributes { get; set; }
    public bool? ContentPrices { get; set; }
    public bool? ContentProducts { get; set; }
    public bool? ContentVendorSettings { get; set; }
    public bool? ConnectorPublications { get; set; }
    public bool? ConnectorProductStatuses { get; set; }
    public bool? PreferredContentSettings { get; set; }
  }
}