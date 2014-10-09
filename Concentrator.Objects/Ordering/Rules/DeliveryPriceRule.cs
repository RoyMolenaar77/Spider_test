using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.Ordering.Vendors;


namespace Concentrator.Objects.Ordering.Rules
{
  public class DeliveryPriceRule : RuleBase
  {
    public override void Apply(OrderLineVendor orderLineVendor, IUnitOfWork unit, int? score)
    {
      var isDrop = orderLineVendor.OrderLine.Order.isDropShipment;
      var vendors = unit.Scope.Repository<Vendor>().GetAll().ToList().Where(v => orderLineVendor.VendorValues.Select(c => c.VendorID).Contains(v.VendorID));
      var avgPrices = new
                        {
                          DSPrice = vendors.Average(c => c.DSPrice),
                          CDPrice = vendors.Average(c => c.CDPrice)
                        };

      foreach (var v in orderLineVendor.VendorValues)
      {
        var vendor = vendors.FirstOrDefault(c => c.VendorID == v.VendorID);
        if (orderLineVendor.OrderLine.Order.isDropShipment.HasValue && orderLineVendor.OrderLine.Order.isDropShipment.Value)
        {
          var price = vendor.DSPrice;
          if (price < avgPrices.DSPrice)
          {
            v.Value += GetValue(unit, orderLineVendor.OrderLine.Order.ConnectorID, vendor.VendorID);
            v.Score += score.HasValue ? score.Value : 0;
          }
        }
        else
        {
          var price = vendor.CDPrice;
          if (price < avgPrices.CDPrice)
          {
            v.Value += GetValue(unit, orderLineVendor.OrderLine.Order.ConnectorID, vendor.VendorID);
            v.Score += score.HasValue ? score.Value : 0;
          }
        }
      }
    }

    public override string Name
    {
      get { return "Delivery Price"; }
    }
  }
}
