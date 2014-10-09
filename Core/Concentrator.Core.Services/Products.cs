using System;
using System.Collections.Generic;
using Concentrator.Core.Services.Models.Products;

namespace Concentrator.Core.Services
{
  public class Products
  {

    public List<ProductBase> GetAllProducts(int connectorID, List<string> includes, int? skipCount = -1, int? takeCount = -1, DateTime? lastModified = null)
    {
      var productHelper = new Helpers.ProductHelper();
      var result = productHelper.GetAllProducts(connectorID,includes, skipCount, takeCount, lastModified);

      return result;
    }


    public List<ProductBase> GetAllProducts(int connectorID, string languageCode)
    {


      return null;
    }

    public ProductBase GetProduct(int connectorID, string languageCode, int productID)
    {


      return null;
    }

  }
}
