using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Plugins.Magento
{
  public enum MagentoOrderState
  {
    complete,
    canceled,
    Processing
  }
  public enum MagentoOrderStatus
  {
    complete,
    canceled,
    shipped_to_shop,
    shipped,
    In_Transit,
    Acknowledged,
    backorder,
    concentrator,
    returned
  }
}
