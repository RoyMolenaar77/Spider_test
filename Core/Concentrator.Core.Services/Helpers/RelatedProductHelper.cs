using System.Collections.Generic;
using Concentrator.Core.Services.Models.Helper;
using Concentrator.Core.Services.Models.Products;
using Concentrator.Objects.Environments;
using PetaPoco;

namespace Concentrator.Core.Services.Helpers
{
  internal class RelatedProductHelper
  {
    private readonly List<int> _loadedConnectorIDs = new List<int>();
    private readonly SortedDictionary<int, List<RelatedProductData>> _relatedProducts = new SortedDictionary<int, List<RelatedProductData>>();

    internal List<RelatedProduct> GetRelatedProducts(int connectorID, int productID)
    {
      lock (_loadedConnectorIDs)
      {
        if (!_loadedConnectorIDs.Contains(connectorID))
        {
          LoadRelatedProducts(connectorID);
          _loadedConnectorIDs.Add(connectorID);
        }
      }

      List<RelatedProductData> filteredItems;

      if (_relatedProducts.ContainsKey(productID))
      {
        filteredItems = _relatedProducts[productID].FindAll(c => c.ConnectorID == connectorID);
      }
      else
      {
        return null;
      }

      var result = new List<RelatedProduct>();
      foreach (var item in filteredItems)
      {
        var relatedProductItem = new RelatedProduct
        {
          Index = item.Index,
          RelatedProductID = item.RelatedProductID,
          RelationType = item.RelationType
        };

        result.Add(relatedProductItem);
      }

      return result;
    }



    private void LoadRelatedProducts(int connectorID)
    {
      using (var db = new Database(Environments.Current.Connection, "System.Data.SqlClient"))
      {
        db.CommandTimeout = 600;

        var sql = string.Format(QueryHelper.GetRelatedProductsQuery(), connectorID);
        var result = db.Fetch<RelatedProductData>(sql);

        foreach (var data in result)
        {
          if (_relatedProducts.ContainsKey(data.ProductID))
          {
            _relatedProducts[data.ProductID].Add(data);
          }
          else
          {
            _relatedProducts.Add(data.ProductID, new List<RelatedProductData> { data });
          }
        }
      }
    }
  }


}
