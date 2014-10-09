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
  public class StockRule : RuleBase
  {
    public override void Apply(OrderLineVendor orderLineVendor, IUnitOfWork unit, int? score)
    {
      foreach (var vendorValue in orderLineVendor.VendorValues)
      {
        if (orderLineVendor.OrderLine.Quantity <= unit.Scope.Repository<VendorStock>().GetSingle(c => c.VendorID == vendorValue.VendorID && c.ProductID == orderLineVendor.OrderLine.ProductID).Try(c => c.QuantityOnHand, 0))
        {
          vendorValue.Value += GetValue(unit, orderLineVendor.OrderLine.Order.ConnectorID, vendorValue.VendorID);
          vendorValue.Score += score.HasValue ? score.Value : 0;
        }
      }
    }

    public override string Name
    {
      get { return "Vendor has Stock"; }
    }
  }
}
