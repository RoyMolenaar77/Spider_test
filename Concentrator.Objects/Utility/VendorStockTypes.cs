using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.DataAccess.Repository;
using Concentrator.Objects.Models.Vendors;

namespace Concentrator.Objects.Utility
{
  public class VendorStockTypes
  {
    private IRepository<VendorStockType> Table;
    private List<VendorStockType> _vendorStockTypes;

    /// <summary>
    /// Initializes a new instance of product status mapper
    /// </summary>
    /// <param name="statusCollection">Collection of all vendorProductStatuses (context.VendorProductStatus)</param>
    public VendorStockTypes(IRepository<VendorStockType> types)
    {
      Table = types;
      _vendorStockTypes = types.GetAll().ToList();
    }

    public VendorStockType SyncVendorStockTypes(string type)
    {
      //if (string.IsNullOrEmpty(status)) return defaultStatus;

      var rType = _vendorStockTypes.FirstOrDefault(c => c.StockType == type);

      if (rType == null)
      {
        rType = new VendorStockType
        {
          StockType = type
        };
        Table.Add(rType);
        _vendorStockTypes.Add(rType);
      }

      return rType;
    }
  }
}
