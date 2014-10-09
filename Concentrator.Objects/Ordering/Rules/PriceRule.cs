using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects.Utility;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.Ordering.Vendors;

namespace Concentrator.Objects.Ordering.Rules
{
  public class PriceRule : RuleBase
  {
    public override void Apply(OrderLineVendor orderLineVendor, IUnitOfWork unit, int? score)
    {
      var avgPrice = (from m in orderLineVendor.OrderLine.Product.VendorAssortments
                      where orderLineVendor.VendorValues.Select(c => c.VendorID).Contains(m.VendorID)
                      select m).SelectMany(c => c.VendorPrices).Average(c => c.Price);

      foreach (var vendor in orderLineVendor.VendorValues)
      {
        decimal? vendorPrice = 0;
        int? productID = orderLineVendor.OrderLine.ProductID;

        if (productID.HasValue)
        {
          var va = VendorUtility.GetMatchedVendorAssortment(unit.Scope.Repository<VendorAssortment>(), vendor.VendorID, productID.Value);

          if (va != null)
            vendorPrice = va.VendorPrices.FirstOrDefault().Try<VendorPrice, decimal?>(c => c.Price.Value, null);
        }

        if (vendorPrice != null)
        {
          if (vendorPrice <= avgPrice)
          {
            vendor.Value += GetValue(unit, orderLineVendor.OrderLine.Order.ConnectorID, vendor.VendorID);
            vendor.Score += score.HasValue ? score.Value : 0;
          }
        }
      }
    }

    public override string Name
    {
      get { return "Lowest Price"; }
    }
  }
}
