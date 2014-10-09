using Concentrator.Objects.Environments;
using PetaPoco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Plugins.PFA.Objects.Helper
{
  public static class GenericSlipNumberHelper
  {
    public static int GetSlipNumberForTransfer(int vendorID, string vendorSettingKey)
    {
      var sequenceNumber = -1;
      using (var database = new Database(Environments.Current.Connection, "System.Data.SqlClient"))
      {
        database.CommandTimeout = 30;
        database.EnableAutoSelect = false;
        sequenceNumber = database.SingleOrDefault<int>("exec GenerateSlipNumber @VendorID, @SettingKey", new object[] { new { VendorID = vendorID, SettingKey = vendorSettingKey } });
      }
      return sequenceNumber;
    }
  }
}
