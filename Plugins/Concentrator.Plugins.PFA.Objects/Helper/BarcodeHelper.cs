using Concentrator.Objects.Enumerations;
using Concentrator.Objects.Environments;
using System;

namespace Concentrator.Plugins.PFA.Objects.Helper
{
  public static class BarcodeHelper
  {
    public static string AddCheckDigitToBarcode(string barcode)
    {
      if (barcode.Length != 12) return barcode;

      var c = 0;

      for (var i = 11; i >= 0; i--)
        c += Int32.Parse(barcode[i].ToString()) * (i % 2 == 0 ? 1 : 3);

      return barcode + ((10 - (c % 10)) % 10);
    }


    public static string GetBarcode(int productID)
    {
      using (var pDb = new PetaPoco.Database(Environments.Current.Connection, "System.Data.SqlClient"))
      {
        var barcode = pDb.FirstOrDefault<string>(@"select barcode from productbarcode where productid = @0 and barcodetype = @1", productID, (int)BarcodeTypes.Default);
        barcode.ThrowIfNull("Barcode not found for product " + productID);

        return barcode;
      }
    }
  }
}
