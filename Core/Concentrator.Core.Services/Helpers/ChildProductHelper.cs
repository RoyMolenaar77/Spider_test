using System.Collections.Generic;
using Concentrator.Core.Services.Models.Helper;
using Concentrator.Core.Services.Models.Products;
using Concentrator.Objects.Environments;
using PetaPoco;

namespace Concentrator.Core.Services.Helpers
{
  internal class ChildProductHelper
  {
    private readonly SortedDictionary<int, List<ChildProductData>> _childProducts = new SortedDictionary<int, List<ChildProductData>>();

    internal List<SimpleProduct> GetChildProducts(int connectorID, int productID, ProductHelper productHelper)
    {
      lock (_childProducts)
      {
        if (_childProducts.Count == 0)
        {
          LoadChildProducts();
        }
      }

      List<ChildProductData> filteredItems;
      if (_childProducts.ContainsKey(productID))
      {
        filteredItems = _childProducts[productID];
      }
      else
      {
        return null;
      }

      var result = new List<SimpleProduct>();
      foreach (var product in filteredItems)
      {
        var p = new SimpleProduct();


        productHelper.FillProductWithBaseAttributes(p, product, connectorID, true, true);

        result.Add(p);
      }

      return result;
    }



    private void LoadChildProducts()
    {
      using (var db = new Database(Environments.Current.Connection, "System.Data.SqlClient"))
      {
        db.CommandTimeout = 600;

        var sql = QueryHelper.GetChildProductsQuery();
        var result = db.Fetch<ChildProductData>(sql);

        _childProducts.Clear();

        foreach (var data in result)
        {
          if (_childProducts.ContainsKey(data.ParentProductID))
          {
            _childProducts[data.ParentProductID].Add(data);
          }
          else
          {
            _childProducts.Add(data.ParentProductID, new List<ChildProductData> { data });
          }
        }
      }
    }
  }
}
