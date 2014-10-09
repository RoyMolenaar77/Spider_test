using System;
using System.Collections.Generic;
using Concentrator.Core.Services.Models.Helper;
using Concentrator.Objects.Environments;
using PetaPoco;

namespace Concentrator.Core.Services.Helpers
{
  internal class ProductAttributeHelper
  {
    private readonly List<int> _loadedConnectorIDs = new List<int>();
    private readonly SortedDictionary<int, List<ProductAttributeData>> _productAttributes = new SortedDictionary<int, List<ProductAttributeData>>(); 

    internal List<Models.Products.Attribute> GetProductAttributes(int connectorID, int productID)
    {
      lock (_loadedConnectorIDs)
      {
        if (!_loadedConnectorIDs.Contains(connectorID))
        {
          LoadProductAttributes(connectorID);
          _loadedConnectorIDs.Add(connectorID);
        }
      }

      List<ProductAttributeData> filteredItems;

      if (_productAttributes.ContainsKey(productID))
      {
        filteredItems = _productAttributes[productID].FindAll(c => c.ConnectorID == connectorID);
      }
      else
      {
        return null;
      }

      var result = new List<Models.Products.Attribute>();
      foreach (var item in filteredItems)
      {
        var attributeItem = new Models.Products.Attribute
        {
          AttributeID = item.AttributeID,
          Value = item.Value,
          ImageUrl = item.ImageUrl != null ? new Uri(item.ImageUrl, UriKind.RelativeOrAbsolute) : null
        };

        result.Add(attributeItem);
      }


      return result;
    }

    private void LoadProductAttributes(int connectorID)
    {
      using (var db = new Database(Environments.Current.Connection, "System.Data.SqlClient"))
      {
        db.CommandTimeout = 600;

        var sql = string.Format(QueryHelper.GetProductAttributesQuery(), connectorID);
        var result = db.Fetch<ProductAttributeData>(sql);

        foreach (var attributeData in result)
        {
          if (_productAttributes.ContainsKey(attributeData.ProductID))
          {
            _productAttributes[attributeData.ProductID].Add(attributeData);
          }
          else
          {
            _productAttributes.Add(attributeData.ProductID, new List<ProductAttributeData> { attributeData });
          }
        }

      }
    }
  }

 
}
