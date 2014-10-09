using Concentrator.Objects.Models.Vendors;
using System;
using System.Collections.Generic;
using Concentrator.Plugins.Axapta.Helpers;

namespace Concentrator.Plugins.Axapta.Repositories
{
  public interface IVendorStockRepository
  {
    IEnumerable<VendorStock> GetListOfVendorStockByVendorID(int vendorID);

    VendorStock GetVendorStock(int productID, int vendorID);
    Boolean IsVendorStockExists(int productID, int vendorID);

    Boolean InsertVendorStock(VendorStock vendorStock);
    void UpdateVendorStockQuantity(int productID, int vendorID, int quantityOnHand);

    Boolean InsertVendorStocks(IEnumerable<VendorStock> listOfVendorStocks);
    Boolean UpdateVendorStocksQuantity(IEnumerable<VendorStock> listOfVendorStocks);
  }
}
