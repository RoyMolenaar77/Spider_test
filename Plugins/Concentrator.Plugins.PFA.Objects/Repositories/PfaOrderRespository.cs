using Concentrator.Plugins.PFA.Objects.Model;
using PetaPoco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Plugins.PFA.Objects.Repositories
{
  public class PfaOrderRespository
  {
    protected string ConnectionString { get; private set; }
    protected const string providerName = "System.Data.Odbc";

    public PfaOrderRespository(string connectionString)
    {
      ConnectionString = connectionString;
    }

    /// <summary>
    /// Retrieves the shipped quantity of a order line from PFA
    /// </summary>
    /// <param name="storeNumber"></param>
    /// <param name="transferNumber"></param>
    /// <returns></returns>
    public List<TransferOrderModel> GetShippedQuantitiesPerOrder(int storeNumber, string transferNumber)
    {
      using (var db = new Database(ConnectionString, providerName))
      {
        return db.Fetch<TransferOrderModel>(string.Format("select \"art-code\" as \"ArtikelNumber\", \"klr-code\" as \"ColorCode\", \"mat-code\" as \"SizeCode\", sum(\"tfr-verz-#\") as \"Shipped\" from \"PUB\".\"tfr\" where \"tfr-fil-naar-#\" = {0} and \"tfb-code\" = '{1}' group by \"art-code\", \"klr-code\",  \"mat-code\" ", storeNumber, transferNumber));
      }
    }
  }
}
