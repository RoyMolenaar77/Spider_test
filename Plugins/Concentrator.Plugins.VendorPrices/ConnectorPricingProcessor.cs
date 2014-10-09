using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.ConcentratorService;
using Concentrator.Objects.DataAccess.UnitOfWork;

namespace Concentrator.Plugins.VendorPrices
{
  public class ConnectorPricingProcessor : ConcentratorPlugin
  {
    public override string Name
    {
      get { return "Calculate connector prices"; }
    }

    protected override void Process()
    {
      log.Info("Start calculating prices");
      using (var unit = GetUnitOfWork())
      {
        var functionScope = ((IFunctionScope)unit.Scope).Repository();
        functionScope.CalculateConnectorPrices();
        log.Info("Price have been calculated.");
      }
    }
  }
}
