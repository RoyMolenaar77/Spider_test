using System;
using System.Collections.Generic;
using Concentrator.Objects.Environments;
using PetaPoco;

namespace Concentrator.Core.Services.Helpers
{
  internal class ProductStockHelper
  {
    private readonly List<int> _loadedConnectorIDs = new List<int>();
    private readonly SortedDictionary<int, List<ProductStockData>> _productStocks = new SortedDictionary<int, List<ProductStockData>>();

    internal List<Models.Products.Stock> GetProductStocks(int connectorID, int productID)
    {
      lock (_loadedConnectorIDs)
      {
        if (!_loadedConnectorIDs.Contains(connectorID))
        {
          LoadProductStocks(connectorID);
          _loadedConnectorIDs.Add(connectorID);
        }
      }

      List<ProductStockData> filteredItems;

      if (_productStocks.ContainsKey(productID))
      {
        filteredItems = _productStocks[productID].FindAll(c => c.ConnectorID == connectorID);
      }
      else
      {
        return null;
      }

      var result = new List<Models.Products.Stock>();
      foreach (var item in filteredItems)
      {
        var stockItem = new Models.Products.Stock
        {
          InStock = item.InStock,
          PromisedDeliveryDate = item.PromisedDeliveryDate,
          QuantityToReceive = item.QuantityToReceive,
          StockStatus = item.StockStatus
        };

        result.Add(stockItem);
      }


      return result;
    }


    private void LoadProductStocks(int connectorID)
    {
      using (var db = new Database(Environments.Current.Connection, "System.Data.SqlClient"))
      {
        db.CommandTimeout = 600;

        var sql = string.Format(QueryHelper.GetProductStocksQuery(), connectorID);
        var result = db.Fetch<ProductStockData>(sql);

        foreach (var data in result)
        {
          if (_productStocks.ContainsKey(data.ProductID))
          {
            _productStocks[data.ProductID].Add(data);
          }
          else
          {
            _productStocks.Add(data.ProductID, new List<ProductStockData> { data });
          }
        }
      }
    }


  }

  internal class ProductStockData
  {
    public int InStock { get; set; }
    public DateTime? PromisedDeliveryDate { get; set; }
    public int QuantityToReceive { get; set; }
    public string StockStatus { get; set; }

    public int ProductID { get; set; }
    public int ConnectorID { get; set; }
  }
}
