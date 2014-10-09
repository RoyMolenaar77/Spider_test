using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Concentrator.ui.Management.Filters
{
  public class AdvancedPricingPortalFilter
  {
    public const string SessionKey = "AdvancedPricingFilter";

     /// <summary>
    /// Product IDs
    /// </summary>
    public int ProductID { get; set; }

    /// <summary>
    /// BeforeDate Filter  
    /// </summary>
    public DateTime? FromDate { get; set; }


    /// <summary>
    /// AfterDate filter  
    /// </summary>
    public DateTime? UntilDate { get; set; }
  }
}