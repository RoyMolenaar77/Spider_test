using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.ConcentratorService;
using Concentrator.Objects.Models.Vendors;

namespace Concentrator.Plugins.PFA
{
  public class StockReset : ConcentratorPlugin
  {
    public override string Name
    {
      get { return "CM stock reset"; }
    }

    protected override void Process()
    {
      using (var unit = GetUnitOfWork())
      {
        var qWebshopStock = @"update vendorstock
                              set vendorstock.quantityOnHand = vendorstock.quantityonhand - vsCM.quantityOnHand
                              from vendorstock , vendorstock vsCM 
                              where 
	                            vendorstock.productid = vsCM.productid 
                              and vendorstock.vendorid = vsCM.vendorid 
                              and vendorstock.vendorid = 1 and 
                              vendorstock.vendorstocktypeid = 1 
                              and vsCM.vendorstocktypeid = 2"
;

        var qCMReset = @" update vendorstock 
                          set QuantityOnHand = 0 where
                          vendorid = 1 and vendorstocktypeid = 2";

        unit.ExecuteStoreCommand(qWebshopStock);
        unit.ExecuteStoreCommand(qCMReset);

      }
    }
  }
}
