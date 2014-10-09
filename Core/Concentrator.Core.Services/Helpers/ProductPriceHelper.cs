using System.Collections.Generic;
using Concentrator.Objects.Environments;
using PetaPoco;

namespace Concentrator.Core.Services.Helpers
{
  internal class ProductPriceHelper
  {
    private readonly List<int> _loadedConnectorIDs = new List<int>();
    private readonly SortedDictionary<int, List<ProductPriceData>> _productPrices = new SortedDictionary<int, List<ProductPriceData>>();

    internal List<Models.Products.Price> GetProductPrices(int connectorID, int productID)
    {
      lock (_loadedConnectorIDs)
      {
        if (!_loadedConnectorIDs.Contains(connectorID))
        {
          LoadProductPrices(connectorID);
          _loadedConnectorIDs.Add(connectorID);
        }
      }

      List<ProductPriceData> filteredItems;

      if (_productPrices.ContainsKey(productID))
      {
        filteredItems = _productPrices[productID].FindAll(c => c.ConnectorID == connectorID);
      }
      else
      {
        return null;
      }

      var result = new List<Models.Products.Price>();
      foreach (var item in filteredItems)
      {
        var priceItem = new Models.Products.Price
        {
          CommercialStatus = item.CommercialStatus,
          CostPrice = item.CostPrice,
          MinimumQuantity = item.MinimumQuantity,
          SpecialPrice = item.SpecialPrice,
          TaxRate = item.TaxRate,
          UnitPrice = item.UnitPrice
        };

        result.Add(priceItem);
      }


      return result;
    }


    private void LoadProductPrices(int connectorID)
    {
      using (var db = new Database(Environments.Current.Connection, "System.Data.SqlClient"))
      {
        db.CommandTimeout = 600;

        var sql = string.Format(QueryHelper.GetProductPricesQuery(), connectorID);
        var result = db.Fetch<ProductPriceData>(sql);

        foreach (var data in result)
        {
          if (_productPrices.ContainsKey(data.ProductID))
          {
            _productPrices[data.ProductID].Add(data);
          }
          else
          {
            _productPrices.Add(data.ProductID, new List<ProductPriceData> { data });
          }
        }
      }
    }


  }

  internal class ProductPriceData
  {
    public decimal TaxRate { get; set; }
    public string CommercialStatus { get; set; }
    public int MinimumQuantity { get; set; }
    public decimal? UnitPrice { get; set; }
    public decimal CostPrice { get; set; }
    public decimal? SpecialPrice { get; set; }

    public int ProductID { get; set; }
    public int ConnectorID { get; set; }
  }
}
