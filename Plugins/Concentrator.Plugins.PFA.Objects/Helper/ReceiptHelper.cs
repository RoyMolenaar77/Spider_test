using Concentrator.Objects.Environments;
using System;

namespace Concentrator.Plugins.PFA.Objects.Helper
{
  public static class ReceiptHelper
  {
    public const string MANCO_SALESSLIP_NUMBER_SETTINGKEY = "ReceivedTransferSalesslipNumber";
    public const string STOCK_SALESSLIP_NUMBER_SETTINGKEY = "StockSalesslipNumber";
    public const string STOCK_SALESSLIP_RECEIPT_NUMBER_SETTING_KEY = "StockReceiptSalesslipNumber";
    public const string STOCK_SALESSLIP_RECEIPT_NUMBER_SETTING_KEY_SURPLUS = "StockReceiptSalesslipNumber_Surplus";
    public const string STOCK_SALESSLIP_RECEIPT_NUMBER_SETTING_KEY_OTHER_FILIAL = "StockReceiptSalesslipNumber_OTHER_FILIAL";
    public const string RETURN_SALESSLIP_RECEIPT_NUMBER_SETTING_KEY = "WehkampReturnSalesslipNumber";

    public static void IncrementSalesSlipNumber(ref int currentSalesSlip, int vendorID, string salesSlipNumberSetting = "")
    {
      if (currentSalesSlip == 9999)
        currentSalesSlip = 0;

      currentSalesSlip++;

      using (var pDb = new PetaPoco.Database(Environments.Current.Connection, "System.Data.SqlClient"))
      {
        pDb.Execute(@"if exists (select * from vendorsetting where vendorid = @1 and settingkey = @2) 
                         update VendorSetting set value = @0 where vendorid = @1 and settingkey = @2
                      else 
                          insert into VendorSetting (VendorID, SettingKey, Value) values (@1, @2, @0)", currentSalesSlip, vendorID, string.IsNullOrEmpty(salesSlipNumberSetting) ? MANCO_SALESSLIP_NUMBER_SETTINGKEY : salesSlipNumberSetting);
      }
    }

    public static int GetSlipNumber(int vendorId, string salesSlipNumberSetting = "")
    {
      using (var pDb = new PetaPoco.Database(Environments.Current.Connection, "System.Data.SqlClient"))
      {
        var slipNumber = pDb.FirstOrDefault<int>(String.Format("select value from vendorsetting where vendorid = {0} and settingkey = '{1}'", vendorId, string.IsNullOrEmpty(salesSlipNumberSetting) ? MANCO_SALESSLIP_NUMBER_SETTINGKEY : salesSlipNumberSetting));

        return slipNumber == 0 ? 1 : slipNumber; //start always from 1
      }
    }
  }
}
