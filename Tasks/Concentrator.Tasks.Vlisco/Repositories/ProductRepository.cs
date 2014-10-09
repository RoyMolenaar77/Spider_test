using Concentrator.Objects.ConcentratorService;
using Concentrator.Objects.Models.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Tasks.Vlisco.Repositories
{
  public class ProductRepository : UnitOfWorkPlugin, IProductRepository
  {
    public bool GetProductIDBySku(string SKU, out int? productID)
    {
      using (var db = GetUnitOfWork())
      {
        var product = db
          .Scope
          .Repository<Product>()
          .GetSingle(x => x.VendorItemNumber == SKU);

        if (product != null)
        {
          productID = product.ProductID;
          return true;
        }
        else
        {
          productID = null;
          return false;
        }
      }
    }
  }
}
