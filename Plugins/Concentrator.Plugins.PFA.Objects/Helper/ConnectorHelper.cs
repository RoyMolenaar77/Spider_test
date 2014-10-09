using Concentrator.Objects.Environments;
using PetaPoco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Plugins.PFA.Objects.Helper
{
  public static class ConnectorHelper
  {
    public const string STORE_NUMBER_SETTING_KEY = "DatcolShopNumber";

    public static int GetStoreNumber(int connectorID)
    {
      using (var db = new Database(Environments.Current.Connection, "System.Data.SqlClient"))
      {
        var shopNumber = db.FirstOrDefault<int>("select Value from connectorsetting where connectorid = @0 and settingkey = @1", connectorID, STORE_NUMBER_SETTING_KEY);
        if (shopNumber == 0)
        {
          throw new ArgumentException("Missing DatcolShopNumber setting for connector " + connectorID);
        }
        return shopNumber;
      }
    }
  }
}
