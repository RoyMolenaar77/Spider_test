using Concentrator.Objects.Environments;
using PetaPoco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Plugins.PFA.Objects.Helper
{
  public static class VendorHelper
  {
    public const string PFA_CONNECTION_STRING_SETTING_KEY = "PfaConnectionString";
    public const string VENDOR_COUNTRY_CODE_SETTING_KEY = "PfaCountryCode";
    public const string VENDOR_DIFFERENCE_SHOP_NUMBER_SETTING_KEY = "Stock/DifferenceShop";
    public const string RETURN_VENDOR_DIFFERENCE_SHOP_NUMBER_SETTING_KEY = "Return/DifferenceShop";
    public const string EMPLOYEE_NUMBER_SETTING_KEY = "DatcolEmployeeNumber";
    public const string WEHKAMP_IMPORT_RETURN_EXCEL_PATH = "PathForWehkampReturnOrders";

    public static string GetEmployeeNumber(int vendorID)
    {
      using (var db = new Database(Environments.Current.Connection, "System.Data.SqlClient"))
      {
        var connectionString = db.FirstOrDefault<string>("select value from vendorsetting where vendorid = @0 and settingkey = @1", vendorID, EMPLOYEE_NUMBER_SETTING_KEY);
        if (string.IsNullOrEmpty(connectionString))
        {
          throw new ArgumentException("Missing Employee number setting for vendorid " + vendorID);
        }
        return connectionString;
      }
    }

    public static string GetConnectionStringForPFA(int vendorID)
    {
      using (var db = new Database(Environments.Current.Connection, "System.Data.SqlClient"))
      {
        var connectionString = db.FirstOrDefault<string>("select value from vendorsetting where vendorid = @0 and settingkey = @1", vendorID, PFA_CONNECTION_STRING_SETTING_KEY);
        if (string.IsNullOrEmpty(connectionString))
        {
          throw new ArgumentException("Missing PfaConnectionString setting for vendorid " + vendorID);
        }
        return connectionString;
      }
    }

    public static string GetCountryCode(int vendorID)
    {
      using (var db = new Database(Environments.Current.Connection, "System.Data.SqlClient"))
      {
        var countryCode = db.FirstOrDefault<string>("select value from vendorsetting where vendorid = @0 and settingkey = @1", vendorID, VENDOR_COUNTRY_CODE_SETTING_KEY);
        if (string.IsNullOrEmpty(countryCode))
        {
          throw new ArgumentException("Missing " + VENDOR_COUNTRY_CODE_SETTING_KEY + " setting for vendorid " + vendorID);
        }
        return countryCode;
      }
    }

    public static int GetDifferenceShopNumber(int vendorID)
    {
      using (var db = new Database(Environments.Current.Connection, "System.Data.SqlClient"))
      {
        var shopNumber = db.FirstOrDefault<int>("select value from vendorsetting where vendorid = @0 and settingkey = @1", vendorID, VENDOR_DIFFERENCE_SHOP_NUMBER_SETTING_KEY);

        return shopNumber;
      }
    }

    public static int GetReturnDifferenceShopNumber(int vendorID)
    {
      using (var db = new Database(Environments.Current.Connection, "System.Data.SqlClient"))
      {
        var shopNumber = db.FirstOrDefault<int>("select value from vendorsetting where vendorid = @0 and settingkey = @1", vendorID, RETURN_VENDOR_DIFFERENCE_SHOP_NUMBER_SETTING_KEY);

        return shopNumber;
      }
    }

    public static string GetImportPathOfWehkampOrders(int vendorID)
    {
      using (var db = new Database(Environments.Current.Connection, "System.Data.SqlClient"))
      {
        var path = db.FirstOrDefault<string>("select value from vendorsetting where vendorid = @0 and settingkey = @1", vendorID, WEHKAMP_IMPORT_RETURN_EXCEL_PATH);
        if (string.IsNullOrEmpty(path))
        {
          throw new ArgumentException("Missing " + WEHKAMP_IMPORT_RETURN_EXCEL_PATH + " setting for vendorid " + vendorID);
        }
        return path;
      }
    }

    public static int GetConnectorIDByVendorID(int vendorID)
    {
      using (var db = new Database(Environments.Current.Connection, "System.Data.SqlClient"))
      {
        var connectorID = db.FirstOrDefault<int>("SELECT ConnectorID FROM ConnectorPublicationRule WHERE VendorID = @0", vendorID);
        return connectorID;
      }
    }
  }
}
