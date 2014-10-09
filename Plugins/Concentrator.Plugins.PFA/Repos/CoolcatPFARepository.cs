using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Plugins.PFA.Configuration;
using PetaPoco;
using Concentrator.Plugins.PFA.Models;
using Concentrator.Plugins.PFA.Models.CC;
using AuditLog4Net.Adapter;

namespace Concentrator.Plugins.PFA.Repos
{
  public class CoolcatPFARepository : PFARepository
  {
    private readonly string _vrdsConnectionString;


    public CoolcatPFARepository(string connectionString, string vrdsConnectionString, IAuditLogAdapter logger)
      : base(connectionString, logger)
    {
      _vrdsConnectionString = vrdsConnectionString;
    }

    /// <summary>
    /// Retrieves the last execution time for the vrds processes
    /// </summary>
    /// <param name="vrdsConnectionString"></param>
    /// <returns></returns>
    public DateTime GetLastVrdsExecutionTime()
    {
      DateTime lastExecutionTime = DateTime.Now;

      using (Database db = new Database(ConnectionString, providerName))
      {
        var hasRunQ = string.Format("select \"stb-bijw-dd\" FROM \"PUB\".\"stb\" where \"stb-code\"='VRDS'");
        lastExecutionTime = db.SingleOrDefault<DateTime>(hasRunQ);
      }

      return lastExecutionTime;
    }

    /// <summary>
    /// Retrieves the CM stock from PFA
    /// </summary>
    /// <param name="nextStockDate">The current date on which the stock is maintained. Usually next Saturday's date</param>
    /// <returns></returns>
    public List<StockResult> GetCMStock(DateTime nextStockDate)
    {
      using (var db = new Database(_vrdsConnectionString, providerName))
      {
        return db.Query<StockResult>(string.Format("select  \"art-code\" as \"ItemNumber\", \"klr-code\" as \"ColorCode\", \"mat-code\" as \"SizeCode\",  " +
                                     " (\"vrds-vrd-#\" - \"vrds-tfr-te-vrz-#\")  as 'Quantity'" +
                                     " from \"PUB\".\"vrds\" where \"vrds-dd\" = '{0}' and \"fil-code\"= '1'", GetFormattedDateForComparison(nextStockDate))).ToList();
      }
    }

    /// <summary>
    /// Retrieves the Transfer stock from PFA
    /// </summary>
    /// <param name="nextStockDate">The current date on which the stock is maintained. Usually next Saturday's date</param>
    /// <returns></returns>
    public List<StockResult> GetTransferStock(DateTime nextStockDate)
    {
      using (var db = new Database(_vrdsConnectionString, providerName))
      {
        return db.Query<StockResult>(string.Format("select  \"art-code\" as \"ItemNumber\", \"klr-code\" as \"ColorCode\", \"mat-code\" as \"SizeCode\",  " +
                                     " \"vrds-tfr-te-otv-#\" as 'Quantity'" +
                                     " from \"PUB\".\"vrds\" where \"vrds-dd\" = '{0}' and \"fil-code\"= '890'", GetFormattedDateForComparison(nextStockDate))).ToList();
      }
    }

    /// <summary>
    /// Retrieves the 890 stock from PFA
    /// </summary>
    /// <param name="nextStockDate">The current date on which the stock is maintained. Usually next Saturday's date</param>
    /// <returns></returns>
    public List<StockResult> GetWmsStock(DateTime nextStockDate)
    {
      return GetSpecificStoreStock(nextStockDate, 890);
    }

    /// <summary>
    /// Retrieves the 892 (Wehkamp) stock from PFA
    /// </summary>
    /// <param name="nextStockDate">The current date on which the stock is maintained. Usually next Saturday's date</param>
    /// <returns></returns>
    public List<StockResult> GetWehkampStock(DateTime nextStockDate)
    {
      return GetSpecificStoreStock(nextStockDate, 892);
    }

    private List<StockResult> GetSpecificStoreStock(DateTime nextStockDate, int storeNumber)
    {
      using (var db = new Database(_vrdsConnectionString, providerName))
      {
        return db.Query<StockResult>(string.Format("select  \"art-code\" as \"ItemNumber\", \"klr-code\" as \"ColorCode\", \"mat-code\" as \"SizeCode\",  " +
                                     " \"vrds-vrd-#\"as 'Quantity'" +
                                     " from \"PUB\".\"vrds\" where \"vrds-dd\" = '{0}' and \"fil-code\"= '{1}'", GetFormattedDateForComparison(nextStockDate), storeNumber)).ToList();
      }
    }

    /// <summary>
    /// Retrieves the valid product numbers based on season code rules
    /// </summary>
    /// <returns></returns>
    public List<string> GetValidItemNumbers()
    {
      using (var db = new Database(ConnectionString, providerName))
      {
        var ccShipmentCostsProduct = PfaCoolcatConfiguration.Current.ShipmentCostsProduct;
        var ccReturnCostsProduct = PfaCoolcatConfiguration.Current.ReturnCostsProduct;
        var ccKialaShipmentCostsProduct = PfaCoolcatConfiguration.Current.KialaShipmentCostsProduct;
        var ccKialaReturnCostsProduct = PfaCoolcatConfiguration.Current.KialaReturnCostsProduct;

        string query = string.Format("select distinct \"p\".\"art-code\" " +
                                "  from \"PUB\".\"art\" \"p\" " +
                                "  inner join \"PUB\".\"szn\" \"Season\" on \"Season\".\"szn-code\" = \"p\".\"szn-code\"" +
                                "  where (\"Season\".\"szn-code\" = 'Z13'" + 
                                "  or \"Season\".\"szn-code\" = 'W13'" + 
                                "  or \"Season\".\"szn-code\" = 'Z14'" + 
                                "  or \"Season\".\"szn-code\" = 'W14'" +
                                "  or \"Season\".\"szn-code\" = 'Z15'" + 
                                "  or \"Season\".\"szn-code\" = 'NHG' )" +
                                "  or (\"p\".\"art-code\" = '{0}'" +
                                "  or \"p\".\"art-code\" = '{1}'" +
                                "  or \"p\".\"art-code\" = '{2}'" +
                                "  or \"p\".\"art-code\" = '{3}')"
                                , ccShipmentCostsProduct
                                , ccReturnCostsProduct
                                , ccKialaShipmentCostsProduct
                                , ccKialaReturnCostsProduct);

        return db.Query<string>(query).ToList();
      }
    }

    /// <summary>
    /// Retrieves the general product information
    /// </summary>
    /// <param name="itemNumber"></param>
    /// <returns></returns>
    public ProductInfoResult GetGeneralProductInformation(string itemNumber, string countryCode)
    {
      using (var db = new Database(ConnectionString, providerName))
      {
        return db.SingleOrDefault<ProductInfoResult>(ConstructGenerateProductInformationQuery(itemNumber, 1, countryCode));
      }
    }

    /// <summary>
    /// Get the valid skus of a product
    /// </summary>
    /// <param name="itemNumber"></param>
    /// <returns></returns>
    public List<SkuResult> GetValidSkus(string itemNumber)
    {
      using (var db = new Database(ConnectionString, providerName))
      {
        return db.Query<SkuResult>(string.Format("select  \"bar\".\"klr-code\" as \"ColorCode\", \"bar\".\"mat-code\" as \"SizeCode\" , \"bar\".\"bcd-code\" as \"Barcode\", \"Color\".\"klr-oms\" as \"BCDCode\" " +
                                     " from \"PUB\".\"art\" \"p\"" +
                                     " inner join \"PUB\".\"bcd\" \"bar\" on \"bar\".\"art-code\" = \"p\".\"art-code\" " +
                                     " left join \"PUB\".\"klr\" \"Color\" on \"Color\".\"klr-code\" = \"bar\".\"klr-code\"" +
                                     " where " +
                                     " \"p\".\"art-code\" = \'{0}\' and \"bar\".\"bcs-code\" = '1' and \"bar\".\"lot-code\" = '' and \"bar\".\"mat-code\" != ''", itemNumber)).ToList();
      }
    }

    private string GetFormattedDateForComparison(DateTime date)
    {
      return date.ToString("yyyy-MM-dd");
    }
  }
}
