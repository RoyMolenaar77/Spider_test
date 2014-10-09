using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.Ordering.Vendors;

namespace Concentrator.Objects.Ordering.Rules
{
  public class PreferredVendorRule : RuleBase
  {
    public override void Apply(OrderLineVendor orderLineVendor, IUnitOfWork unit, int? score)
    {
      int connectorID = orderLineVendor.OrderLine.Order.ConnectorID;
      var contentProductGroup = orderLineVendor.OrderLine.Product.ContentProductGroups.Where(c => c.ConnectorID == connectorID).ToList();

      if (contentProductGroup != null)
      {
        var preferredAssortmentVendor = (from c in contentProductGroup
                                         where c.ProductGroupMapping.ProductGroup.ProductGroupConnectorVendors.Where(x => x.ConnectorID == connectorID && x.isPreferredAssortmentVendor).Count() > 0
                                         select c.ProductGroupMapping.ProductGroup.ProductGroupConnectorVendors.FirstOrDefault()).FirstOrDefault();

        //if any preferred vendor for this product group, assign preferrence to him
        int vendorToAssign = 0;
        if (preferredAssortmentVendor != null)
        {
          vendorToAssign = preferredAssortmentVendor.VendorID;
        }
        //assign to the preferred vendor
        else
        {
          vendorToAssign = (from v in unit.Scope.Repository<PreferredConnectorVendor>().GetAllAsQueryable() where v.isPreferred select v.VendorID).FirstOrDefault();
        }

        var vn = orderLineVendor.VendorValues.FirstOrDefault(c => c.VendorID == vendorToAssign);

        if (vn == null)
        {
          var childs = unit.Scope.Repository<Vendor>().GetAllAsQueryable(x => x.ParentVendorID.HasValue && x.ParentVendorID.Value == vendorToAssign && x.OrderDispatcherType != null).Select(x => x.VendorID);
          foreach (var child in childs)
          {
            vn = orderLineVendor.VendorValues.FirstOrDefault(c => c.VendorID == child);

            if (vn != null)
              break;
          }
        }

        if (vn != null)
        {
          vn.Value += GetValue(unit, connectorID, vendorToAssign);
          vn.Score += score.HasValue ? score.Value : 0;
        }

      }
    }

    public override string Name
    {
      get { return "Preferred vendor"; }
    }
  }
}
