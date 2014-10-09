using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Services.Base;
using Concentrator.Objects.Models.Vendors;

namespace Concentrator.Objects.Services
{
  public class VendorStockTypeService : Service<VendorStockType>
  {
    public override IQueryable<VendorStockType> Search(string queryTerm)
    {
      var query = queryTerm.IfNullOrEmpty("").ToLower();
      return Repository().GetAllAsQueryable(c => c.StockType.ToLower().Contains(query));
    }
  }
}
