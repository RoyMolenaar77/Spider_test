using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.Ordering.Vendors;

namespace Concentrator.Objects.Ordering.Rules
{
  public class DeliveryTimeRule : RuleBase
  {
    public override void Apply(OrderLineVendor orderLineVendor, IUnitOfWork unit, int? score)
    {
      var vendors = unit.Scope.Repository<Vendor>().GetAll().ToList().Where(v => orderLineVendor.VendorValues.Select(c => c.VendorID).Contains(v.VendorID));
      var avgDeliveryTime = new
      {
        DeliveryHours = vendors.Average(c => c.DeliveryHours)
      };

      foreach (var v in orderLineVendor.VendorValues)
      {
        var vendor = vendors.FirstOrDefault(c => c.VendorID == v.VendorID);

        var deliverTime = vendor.DeliveryHours;
        if (deliverTime < avgDeliveryTime.DeliveryHours)
        {
          v.Value += GetValue(unit, orderLineVendor.OrderLine.Order.ConnectorID, vendor.VendorID);
          v.Score += score.HasValue ? score.Value : 0;
        }
      }
    }

    public override string Name
    {
      get { return "Delivery Time"; }
    }
  }
}
