using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Orders;
using Concentrator.Objects.Models.Vendors;


namespace Concentrator.Objects.Ordering.Vendors
{
  public class OrderLineVendor
  {
    public List<VendorRuleValue> VendorValues
    {
      get;
      set;
    }

    public OrderLine OrderLine
    {
      get;
      set;
    }
  }
}

public class VendorRuleValue
{
  /// <summary>
  /// Gets/Sets the value of this vendor for one OrderLine
  /// </summary>
  public int Value
  {
    get;
    set;
  }

  /// <summary>
  /// Gets/Sets VendorID
  /// </summary>
  public int VendorID
  {
    get;
    set;
  }

  /// <summary>
  /// Gets/Sets VendorAssortment
  /// </summary>
  public int VendorassortmentID
  {
    get;
    set;
  }

  /// <summary>
  /// Gets/Sets RuleScore
  /// </summary>
  public int Score
  {
    get;
    set;
  }
}

