using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Tasks.Vlisco.Repositories
{
  public interface IProductRepository
  {
    bool GetProductIDBySku(string SKU, out int? productID);
  }
}
