using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Xml;
using System.Data;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System.Data.Common;

namespace Concentrator.Vendors.BAS
{
  /// <summary>
  /// Summary description for Service1
  /// </summary>
  [WebService(Namespace = "http://localhost/BASConnector/", Description = "Internal Service for pricing and assortment info")]
  [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
  [System.ComponentModel.ToolboxItem(false)]
  // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
  // [System.Web.Script.Services.ScriptService]
  public class JdeAssortment : System.Web.Services.WebService
  {
    [WebMethod(Description = "Returns a complete list of products with prices and stock information")]
    public DataSet GenerateStockProductList(int customerID,int internetAss, int allowDC10O)
    {
      try
      {
        return XmlHelper.FetchProducts(customerID,internetAss, allowDC10O);
      }
      catch (Exception ex)
      {
        return null;
        //return XmlHelper.GenerateErrorXml(1, ex.Message);
      }
    }

    [WebMethod(Description = "Returns a complete list of stock and non stock products with prices and stock information")]
    public DataSet GenerateFullProductList(int customerID, int internetAss, int allowDC10O,int allowDC10ObyStatus,bool bscstock,bool shopInfo)
    {
      try
      {
        return XmlHelper.FetchProducts(customerID, internetAss, allowDC10O,0,allowDC10ObyStatus,bscstock ? "ALL" : null,shopInfo);
      }
      catch (Exception ex)
      {
        return null;
        //return XmlHelper.GenerateErrorXml(1, ex.Message);
      }
    }

    [WebMethod(Description = "Returns a complete list of stock and non stock products with prices and stock information")]
    public DataSet GenerateFullProductListWithNonStock(int customerID, int internetAss, int allowDC10O, int allowDC10ObyStatus, bool bscstock, bool shopInfo)
    {
      try
      {
        return XmlHelper.FetchProducts(customerID, internetAss, allowDC10O, 1, allowDC10ObyStatus, bscstock ? "ALL" : null, shopInfo);
      }
      catch (Exception ex)
      {
        return null;
        //return XmlHelper.GenerateErrorXml(1, ex.Message);
      }
    }

    [WebMethod(Description = "Returns a complete list of stock and non stock products with prices and stock information, retail stock included")]
    public DataSet GenerateFullProductListWithRetailStock(int customerID, int internetAss, int allowDC10O, int allowDC10ObyStatus)
    {
      try
      {
        return XmlHelper.FetchProducts(customerID, internetAss, allowDC10O, 1, true,allowDC10ObyStatus);
      }
      catch (Exception ex)
      {
        return null;
        //return XmlHelper.GenerateErrorXml(1, ex.Message);
      }
    }

    [WebMethod(Description = "Get customer assortment special parameters")]
    public DataSet GenerateFullProductListSpecialAssortment(int customerID, int internetAss, int allowDC10O, int allowDC10ObyStatus, int allowNonStock, string bscStock, string costPriceDC10, string costPriceBSC, bool onlyItemsWithStock)
    {
      try
      {
        return XmlHelper.FetchProducts(customerID, internetAss, allowDC10O,allowDC10ObyStatus,allowNonStock,bscStock,costPriceDC10,costPriceBSC, onlyItemsWithStock);
      }
      catch (Exception ex)
      {
        return null;
        //return XmlHelper.GenerateErrorXml(1, ex.Message);
      }
    }

    [WebMethod(Description = "Return single product information")]
    public DataSet GetSingeProductInformation(int customerID, string productid)
    {
      try
      {
        return XmlHelper.FetchProducts(customerID, productid);
      }
      catch (Exception ex)
      {
        return null;
        //return XmlHelper.GenerateErrorXml(1, ex.Message);
      }
    }

    [WebMethod(Description = "Return single product information with shopInformation")]
    public DataSet GetSingleShopProductInformation(int customerID, string productid, bool shopInformation)
    {
      try
      {
        return XmlHelper.FetchProducts(customerID, productid, shopInformation);
      }
      catch (Exception ex)
      {
        return null;
        //return XmlHelper.GenerateErrorXml(1, ex.Message);
      }
    }

    [WebMethod(Description = "Returns all items JDE")]
    public DataSet GenerateFullItemList()
    {
      try
      {
        return XmlHelper.FetchProducts();
      }
      catch (Exception ex)
      {
        return null;
        //return XmlHelper.GenerateErrorXml(1, ex.Message);
      }
    }

    [WebMethod(Description = "Returns all Cross References between SAP and JDE")]
    public DataSet GetCrossItemNumbers()
    {
      try
      {
        Database db = DatabaseFactory.CreateDatabase();

        using (DbCommand command = db.GetSqlStringCommand(@"SELECT 
IVITM as JDE,
IVCITM as SAP
from f4104
where IVXRT = 'DC'"))
        {
          return db.ExecuteDataSet(command);
        }
      }
      catch (Exception ex)
      {
        return null;
        //return XmlHelper.GenerateErrorXml(1, ex.Message);
      }
    }

    [WebMethod(Description = "Returns all insurance JDE")]
    public DataSet GetInsurance()
    {
      try
      {
        Database db = DatabaseFactory.CreateDatabase();

        using (DbCommand command = db.GetSqlStringCommand(@"select 
imitm,
imprp8
from f4101
where imprp8 != ''"))
        {
          return db.ExecuteDataSet(command);
        }
      }
      catch (Exception ex)
      {
        return null;
        //return XmlHelper.GenerateErrorXml(1, ex.Message);
      }
    }

    [WebMethod(Description = "Returns all JDE Barcodes")]
    public DataSet GetBarcodes()
    {
      try
      {
        Database db = DatabaseFactory.CreateDatabase();

        using (DbCommand command = db.GetSqlStringCommand(@"
	SELECT IVITM, IVCITM, ivxrt
	FROM F4104
	WHERE ivxrt IN ('EA', 'UP', 'DC')"))
        {
          return db.ExecuteDataSet(command);
        }
      }
      catch (Exception ex)
      {
        return null;
        //return XmlHelper.GenerateErrorXml(1, ex.Message);
      }
    }
  }
}
