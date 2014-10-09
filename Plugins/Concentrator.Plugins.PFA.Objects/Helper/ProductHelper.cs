using Concentrator.Objects.Enumerations;
using Concentrator.Objects.Environments;
using System;

namespace Concentrator.Plugins.PFA.Objects.Helper
{
  public static class ProductHelper
  {
    public static string GetPFAItemNumber(string articleNumber, string colorCode, int productID)
    {

      using (var pDb = new PetaPoco.Database(Environments.Current.Connection, "System.Data.SqlClient"))
      {

        if (string.IsNullOrEmpty(articleNumber) || string.IsNullOrEmpty(colorCode))
        {
          var vendorItemNumber = pDb.FirstOrDefault<string>("select VendorItemNumber from product where productid = @0", productID);
          if (!vendorItemNumber.Contains(" "))
            throw new InvalidOperationException("Something went wrong with trying to parse the article and color codes");

          var parts = vendorItemNumber.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
          articleNumber = parts[0];
          colorCode = parts[1];
        }

        var size = pDb.FirstOrDefault<String>("select barcode from productbarcode where productid = @0 and barcodetype = @1", productID, (int)BarcodeTypes.PFA) ?? "990";

        String pfaItemNumber = string.Format("{0}{1}{2}", articleNumber, colorCode.PadLeft(3, '0'), size.PadLeft(4, '0'));

        return pfaItemNumber;
      }
    }
  }
}
