using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Objects.Web.Models
{
  public class ContentPortalFilter
  {

    public const string SessionKey = "ContentFilter";

    /// <summary>
    /// Connector IDs 
    /// </summary>
    public int[] Connectors { get; set; }

    /// <summary>
    /// Vendor IDs
    /// </summary>
    public int[] Vendors { get; set; }

    /// <summary>
    /// BeforeDate Filter  
    /// </summary>
    public DateTime? BeforeDate { get; set; }


    /// <summary>
    /// AfterDate filter  
    /// </summary>
    public DateTime? AfterDate { get; set; }

    /// <summary>
    /// OnDate Filter  
    /// </summary>
    public DateTime? OnDate { get; set; }

    /// <summary>
    /// Filters on active products
    /// </summary>
    public bool? IsActive { get; set; }

    /// <summary>
    /// Product Group IDs
    /// </summary>
    public int[] ProductGroups { get; set; }

    /// <summary>
    /// Brand IDs
    /// </summary>
    public int[] Brands { get; set; }

    /// <summary>
    ///  Lower Stock Count Filter 
    /// </summary>
    public Int32? LowerStockCount { get; set; }

    /// <summary>
    /// Greater Stock Count filter
    /// </summary>
    public Int32? GreaterStockCount { get; set; }

    /// <summary>
    /// Equal Stock Count Filter  
    /// </summary>
    public Int32? EqualStockCount { get; set; }

    /// <summary>
    /// Stock Status IDs
    /// </summary>
    public int[] Statuses { get; set; }
  }
}
