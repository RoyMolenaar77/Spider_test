using System.Collections.Generic;
using Concentrator.Objects.Environments;
using PetaPoco;

namespace Concentrator.Plugins.Wehkamp.Helpers
{
  internal static class ProductHelper
  {
    internal static readonly Dictionary<string, int> FindProductIDByWehkampDataCache = new Dictionary<string, int>();


    internal static int GetProductIDByWehkampData(string wehkampArtikelNummer, string wehkampKleurnummer, string wehkampMaat, int vendorID)
    {
      var cacheKey = string.Format("{0} {1} {2} {3}", wehkampArtikelNummer, wehkampKleurnummer, wehkampMaat, vendorID);
      if (FindProductIDByWehkampDataCache.ContainsKey(cacheKey))
        return FindProductIDByWehkampDataCache[cacheKey];

      var sql = string.Format(@"
            SELECT va.ProductID FROM 
            VendorAssortment va 
            INNER JOIN ProductBarcode pb ON va.ProductID = pb.ProductID and pb.BarcodeType = 4 
            INNER JOIN VendorTranslation vt ON va.VendorID = vt.SourceVendorID and va.VendorID = vt.DestinationVendorID and vt.DestinationVendorValue = '{3}' 
            WHERE (CustomItemNumber like '{0}%' OR CustomItemNumber like '{1}%') AND va.VendorID = {2} AND pb.Barcode = vt.SourceVendorValue", string.Format("{0} {1}", wehkampArtikelNummer, wehkampKleurnummer), string.Format("{0} {1}", wehkampArtikelNummer, wehkampKleurnummer.PadLeft(3, '0')), vendorID, wehkampMaat);
      using (var database = new Database(Environments.Current.Connection, "System.Data.SqlClient"))
      {
        database.CommandTimeout = 30;
        var productID = database.FirstOrDefault<int>(sql);
        lock (FindProductIDByWehkampDataCache)
        {
          if(!FindProductIDByWehkampDataCache.ContainsKey(cacheKey))
            FindProductIDByWehkampDataCache.Add(cacheKey, productID);
        }

        return productID;
      }
    }

    internal static int GetProductIDByWehkampData(string wehkampArtikelNummer, string wehkampKleurnummer, int vendorID)
    {
      var cacheKey = string.Format("{0} {1} {2}", wehkampArtikelNummer, wehkampKleurnummer, vendorID);
      if (FindProductIDByWehkampDataCache.ContainsKey(cacheKey))
        return FindProductIDByWehkampDataCache[cacheKey];

      var sql = string.Format("SELECT va.ProductID FROM VendorAssortment va WHERE (CustomItemNumber like '{0}' OR CustomItemNumber like '{1}') AND va.VendorID = {2}", string.Format("{0} {1}", wehkampArtikelNummer, wehkampKleurnummer), string.Format("{0} {1}", wehkampArtikelNummer, wehkampKleurnummer.PadLeft(3, '0')), vendorID);
      using (var database = new Database(Environments.Current.Connection, "System.Data.SqlClient"))
      {
        database.CommandTimeout = 30;
        var productID = database.FirstOrDefault<int>(sql);

        lock (FindProductIDByWehkampDataCache)
        {
          if (!FindProductIDByWehkampDataCache.ContainsKey(cacheKey))
            FindProductIDByWehkampDataCache.Add(cacheKey, productID);
        }

        return productID;
      }
    }
  }
}
