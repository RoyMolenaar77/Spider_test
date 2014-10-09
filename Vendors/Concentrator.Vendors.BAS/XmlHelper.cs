using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.IO;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System.Data.Common;

namespace Concentrator.Vendors.BAS
{
  public class XmlHelper
  {

    private DataSet FetchAssortment(int customerID, int internetAss, int allowDC10O, int allowNonStock, int allowDC10ObyStatus, string bscStock, bool shopInfo, string productID, bool retailStock, string costPriceDC10, string costPriceBSC, bool onlyItemsWithStock)
    {
      try
      {
        DataSet ds = new DataSet();

        Database db = DatabaseFactory.CreateDatabase();

        //using (DbCommand command = db.GetStoredProcCommand("ConcentratorGetAssortment"))
        using (DbCommand command = db.GetStoredProcCommand(ConfigurationManager.AppSettings["SP_ConcentratorGetAssortment"]))
        {
          command.CommandTimeout = 120;
          db.AddInParameter(command, "CustomerID", DbType.Int32, customerID);
          db.AddInParameter(command, "InternetAss", DbType.Int32, internetAss);
          db.AddInParameter(command, "AllowDC10O", DbType.Int32, allowDC10O);
          db.AddInParameter(command, "AllowNonStock", DbType.Int32, allowNonStock);
          db.AddInParameter(command, "AllowDC10ObyStatus", DbType.Int32, allowDC10ObyStatus);

          if (!string.IsNullOrEmpty(bscStock))
            db.AddInParameter(command, "ShowBSCStock", DbType.String, bscStock);

          if (shopInfo)
            db.AddInParameter(command, "ShowShopInfo", DbType.String, "YES");

          if (!string.IsNullOrEmpty(productID))
            db.AddInParameter(command, "BasItemNumber", DbType.Int32, productID);

          if (retailStock)
            db.AddInParameter(command, "ShowRetailStock", DbType.String, "YES");

          if (!string.IsNullOrEmpty(costPriceBSC))
            db.AddInParameter(command, "@ShowCostPriceBSC", DbType.String, costPriceBSC);

          if (!string.IsNullOrEmpty(costPriceDC10))
            db.AddInParameter(command, "ShowCostPriceDC10", DbType.String, costPriceDC10);

          if(onlyItemsWithStock)
            db.AddInParameter(command, "ShowStockItemsOnly", DbType.Boolean, onlyItemsWithStock);


          ds = db.ExecuteDataSet(command);
        }

        return ds;
      }
      catch (SqlException sqlex)
      {
        return null;
      }
    }

    private DataSet FetchAssortment()
    {
      try
      {
        DataSet ds = new DataSet();

        Database db = DatabaseFactory.CreateDatabase();

        using (DbCommand command = db.GetStoredProcCommand("ConcentratorFetchContent"))
        {
          ds = db.ExecuteDataSet(command);
        }

        return ds;
      }
      catch (SqlException sqlex)
      {
        return null;
      }
    }


    public static DataSet FetchProducts(int customerID, string productID, bool shopInfo)
    {
      XmlHelper helper = new XmlHelper();
      return helper.FetchAssortment(customerID, 0, 1, 1, 0, null, shopInfo, productID, false, null, null, false);
    }

    public static DataSet FetchProducts(int customerID, string productID)
    {
      XmlHelper helper = new XmlHelper();
      return helper.FetchAssortment(customerID, 0, 1, 1, 0, null, false, productID, false, null, null,false);
    }

    public static DataSet FetchProducts(int customerID, int internetAss, int allowDC10O)
    {
      XmlHelper helper = new XmlHelper();
      return helper.FetchAssortment(customerID, 0, 1, 0, 0, null, false, null, false, null, null, false);
    }

    public static DataSet FetchProducts(int customerID, int internetAss, int allowDC10O, int allowNonStock, int allowDC10ObyStatus, string bscstock, bool shopInfo)
    {
      XmlHelper helper = new XmlHelper();
      return helper.FetchAssortment(customerID, internetAss, allowDC10O, allowNonStock, allowDC10ObyStatus, bscstock, shopInfo, null, false, null, null, false);
    }

    public static DataSet FetchProducts(int customerID, int internetAss, int allowDC10O, int allowNonStock, bool retailStock, int allowDC10ObyStatus)
    {
      XmlHelper helper = new XmlHelper();
      return helper.FetchAssortment(customerID, internetAss, allowDC10O, allowNonStock, allowDC10ObyStatus, null, false, null, retailStock, null, null, false);
    }

    public static DataSet FetchProducts()
    {
      XmlHelper helper = new XmlHelper();
      return helper.FetchAssortment();
    }

    /* 
* Errors
* 0 - None
* 1 - General Error
* 2 - Access Denied (username,password)
* 3 - Manufacturer part number Invalid
* 4 - Product Number Invalid
* 5 - Invalid Country
* 6 - No products found
* 7 - No access to product specifications
*/

    /// <summary>
    /// Generates a XML error message based on the specified input parameters.
    /// </summary>
    /// <param name="errorCode"></param>
    /// <param name="errorMessage"></param>
    /// <returns></returns>
    public static XmlDocument GenerateErrorXml(int errorCode, string errorMessage)
    {
      string message = "<Products><Error ID=\"{0}\" Message=\"{1}\" /></Products>";
      XmlDocument doc = new XmlDocument();
      //switch (errorCode)
      //{
      //  case 6:
      //    message = "<Product />";
      //    break;
      //  default:
      //    break;
      //}

      try
      {
        doc.LoadXml(String.Format(message, errorCode, errorMessage));
      }
      catch (XmlException xmlEx)
      {
        //ExceptionPolicy.HandleException(xmlEx, "Xtract Policy");
      }
      return doc;
    }


    internal static DataSet FetchProducts(int customerID, int internetAss, int allowDC10O, int allowDC10ObyStatus, int allowNonStock, string bscStock, string costPriceDC10, string costPriceBSC, bool onlyItemsWithStock)
    {
      XmlHelper helper = new XmlHelper();
      return helper.FetchAssortment(customerID, internetAss, allowDC10O, allowNonStock, allowDC10ObyStatus, bscStock, false, null, false, costPriceDC10, costPriceBSC, onlyItemsWithStock);
    }
  }
}
