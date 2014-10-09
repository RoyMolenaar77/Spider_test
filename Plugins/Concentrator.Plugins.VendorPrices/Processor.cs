using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.ConcentratorService;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects.Logic;

namespace Concentrator.Plugins.Pricing
{
  public class Processor : ConcentratorPlugin
  {
    public override string Name
    {
      get { return "Vendor price calculator"; }
    }

    protected override void Process()
    {
      log.Info("Start calculating prices");
      using (var unit = GetUnitOfWork())
      {
        var functionScope = ((IFunctionScope)unit.Scope).Repository();

        //calculate all vendor prices
        functionScope.CalculateVendorPrices();
        log.Info("Price have been calculated. Start rounding prices");
        //fetch all of them and if needed round them
        var pricesToBeRounded = unit.Scope.Repository<VendorPrice>().GetAll(c => c.VendorPriceRuleID != null && c.VendorPriceRule.VendorPriceCalculationID != null).ToList();

        PriceRounder rounder = new PriceRounder();

        foreach (var vendorPrice in pricesToBeRounded)
        {
          decimal? margin = null;
          if (vendorPrice.Price.HasValue)
          {
            var price = vendorPrice.Price.Try<decimal?, decimal?>(x => x.Value, null);
            var costPrice = vendorPrice.CostPrice.Try<decimal?, decimal?>(x => x.Value, null);

            if (costPrice.HasValue && price.HasValue)
              margin = (price.Value - costPrice.Value) / (costPrice.Value / 100);

            if (price.HasValue)
            {
              var roundedPrice = rounder.RoundPrice(vendorPrice.VendorPriceRule.VendorPriceCalculation.Calculation, price.Value, margin);
              vendorPrice.Price = roundedPrice;
            }
          }
        }

        log.Info("Finish rounding pricing");
        unit.Save();
      }
    }
  }
}
