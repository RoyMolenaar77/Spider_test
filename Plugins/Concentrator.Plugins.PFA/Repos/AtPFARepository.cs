using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PetaPoco;
using Concentrator.Plugins.PFA.Models.AT;
using Concentrator.Plugins.PFA.Models;
using log4net.Core;
using AuditLog4Net.Adapter;

namespace Concentrator.Plugins.PFA.Repos
{
  public class AtPFARepository : PFARepository
  {
    public AtPFARepository(string connectionString, IAuditLogAdapter logger)
      : base(connectionString, logger)
    { }

    /// <summary>
    /// Retrieves the valid product numbers and their colors based on season code rules
    /// </summary>
    /// <returns></returns>
    public Dictionary<string, string[]> GetValidItemColors()
    {
      using (var db = new Database(ConnectionString, providerName))
      {
        return (from p in db.Query<ProductColorResult>("select distinct \"p\".\"art-code\" as \"ItemNumber\", \"bar\".\"klr-code\" as \"ColorCode\"" +
                              " from \"PUB\".\"art\" \"p\" " +
                              " inner join \"PUB\".\"bcd\" \"bar\" on \"p\".\"art-code\" = \"bar\".\"art-code\"" +
                              " inner join \"PUB\".\"szn\" \"Season\" on \"Season\".\"szn-code\" = \"p\".\"szn-code\"" +
                              " where (\"Season\".\"szn-code\" = 'YEAR' or \"Season\".\"szn-code\" = 'S12' or \"Season\".\"szn-code\" = 'w12' or \"Season\".\"szn-code\" = 'S13' or \"Season\".\"szn-code\" = 'W13' or \"Season\".\"szn-code\" = 'S14' or \"Season\".\"szn-code\" = 'W14' or \"Season\".\"szn-code\" = 'S15')")
                group p by p.ItemNumber into colorGroups
                select new
                {
                  ItemNumber = colorGroups.Key,
                  Colors = colorGroups.Select(c => c.ColorCode).Distinct().ToArray()
                }).ToDictionary(c => c.ItemNumber, c => c.Colors);
      }
    }


    
    /// <summary>
    /// Retrieves general product information
    /// </summary>
    /// <param name="itemNumber"></param>
    /// <returns></returns>
    public Concentrator.Plugins.PFA.Models.AT.ProductInfoResult GetGeneralProductInformation(string itemNumber, string countryCode)
    {
      using (var db = new Database(ConnectionString, providerName))
      {
        try
        {
          return db.FirstOrDefault<Concentrator.Plugins.PFA.Models.AT.ProductInfoResult>(ConstructGenerateProductInformationQuery(itemNumber, 2, countryCode, "art-oms"));
        }
        catch (Exception e)
        {
          Logger.Debug("Exception " + e.Message + " for item " + itemNumber);
          return null;
        }
      }
    }

    /// <summary>
    /// Retrieves the valid skus
    /// </summary>
    /// <param name="itemNumber"></param>
    /// <returns></returns>
    public List<Concentrator.Plugins.PFA.Models.AT.SkuResult> GetValidSkus(string itemNumber, string colorCode)
    {
      using (var db = new Database(ConnectionString, providerName))
      {
        return db.Query<Concentrator.Plugins.PFA.Models.AT.SkuResult>(string.Format(
             "select  \"bar\".\"klr-code\" as \"ColorCode\", \"bar\".\"mat-code\" as \"SizeCode\", \"bar\".\"bcd-code\" as \"Barcode\"" +
                                         " from \"PUB\".\"art\" \"p\"" +
                                         " inner join \"PUB\".\"bcd\" \"bar\" on \"bar\".\"art-code\" = \"p\".\"art-code\" " +
                                         " where " +
                                         " \"p\".\"art-code\" = \'{0}\' and \"bar\".\"mat-code\" != '' and \"bar\".\"klr-code\"='{1}' ", itemNumber, colorCode)
          ).ToList();
      }
    }

    public int GetWebshopStock(string itemNumber, string sizeCode, string colorCode)
    {
      return GetSkuStock(itemNumber, sizeCode, colorCode, 801);
    }

    public int GetWehkampStock(string itemNumber, string sizeCode, string colorCode)
    {
      return GetSkuStock(itemNumber, sizeCode, colorCode, 803);
    }

    /// <summary>
    /// Retrieves sku stock
    /// </summary>
    /// <param name="itemNumber"></param>
    /// <param name="sizeCode"></param>
    /// <param name="colorCode"></param>
    /// <param name="storeCode">Filiaal code in PFA</param>
    /// <returns></returns>
    public int GetSkuStock(string itemNumber, string sizeCode, string colorCode, int storeCode)
    {

      using (var db = new Database(ConnectionString, providerName))
      {

        var query = string.Format(" select \"vrd-vrd-#\" from \"PUB\".\"vrd\" where  " +
                                                " \"art-code\" = '{0}'  " +
                                                " and \"mat-code\" = '{1}' " +
                                                " and \"klr-code\" = '{2}' " +
                                                " and \"fil-code\" = '{3}' ", itemNumber, sizeCode, colorCode, storeCode);

        return db.Try(c => c.SingleOrDefault<int>(query), 0);
      }
    }
  }
}
