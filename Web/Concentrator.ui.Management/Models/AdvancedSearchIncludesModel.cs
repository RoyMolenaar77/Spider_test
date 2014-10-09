using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Concentrator.ui.Management.Models
{
  public class AdvancedSearchIncludesModel
  {
    public bool includeDescriptions { get; set; }
    public bool includeBrands { get; set; }
    public bool includeProductGroups { get; set; }
    public bool includeIdentifiers { get; set; }
  }
}