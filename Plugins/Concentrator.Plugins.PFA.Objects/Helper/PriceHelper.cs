using Concentrator.Objects.Environments;
using System;

namespace Concentrator.Plugins.PFA.Objects.Helper
{
  public static class PriceHelper
  {
    public static decimal GetPrice(int productID, Int32 vendorId)
    {
      using (var pDb = new PetaPoco.Database(Environments.Current.Connection, "System.Data.SqlClient"))
      {
        var vendorAssortmentId = pDb.FirstOrDefault<Int32>(String.Format("select vendorassortmentid from vendorassortment where productid = {0} and vendorid = {1}", productID, vendorId));

        var price = pDb.FirstOrDefault<Decimal>(String.Format("select isnull(specialprice, price) from vendorprice  where vendorassortmentid = {0}", vendorAssortmentId));

        return price;
      }
    }
  }
}
