using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.Ordering.Vendors;

namespace Concentrator.Objects.Ordering.Rules
{
  public abstract class RuleBase
  {
    /// <summary>
    /// Execute the rule
    /// </summary>
    public abstract void Apply(OrderLineVendor orderLineVendor, IUnitOfWork unit, int? score);

    /// <summary>
    /// Get the name of the Price Rule
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// Returns the value of a rule for a specific connector
    /// </summary>
    /// <param name="context">An instance of ConcentratorDataContext</param>
    /// <param name="connectorID">ConnectorID</param>
    /// <returns>Value of rule</returns>
    protected int GetValue(IUnitOfWork unit, int connectorID, int vendorID)
    {
      return (from v in unit.Scope.Repository<ConnectorRuleValue>().GetAllAsQueryable()
              where
              (v.ConnectorID == connectorID || (v.Connector.ParentConnectorID.HasValue && v.Connector.ParentConnectorID.Value == connectorID))
              && v.OrderRule.Name == Name
              && (v.VendorID == vendorID || (v.Vendor.ParentVendorID.HasValue && v.Vendor.ParentVendorID.Value == vendorID))
              select v).FirstOrDefault().Try(c => c.Value, 1);
    }
  }
}
