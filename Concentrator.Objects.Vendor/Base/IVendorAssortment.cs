using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Vendors;

namespace Concentrator.Objects.Vendors.Base
{
  interface IVendorAssortment
  {
    /// <summary>
    /// Import Brands for Vendor
    /// </summary>
    /// <param name="vendorBrandCodes"></param>
    /// <param name="importVendorID"></param>
    /// <returns></returns>
    void ImportVendorBrands(List<string> vendorBrandCodes, int importVendorID);

    /// <summary>
    /// Import Productgroups for Vendor
    /// </summary>
    /// <param name="vendorBrandCodes"></param>
    /// <param name="importVendorID"></param>
    /// <returns></returns>
    void ImportVendorProductGroups(Dictionary<string, List<string>> vendorProductGroupCodes, int importVendorID);

    /// <summary>
    /// Update Stock for Vendor
    /// </summary>
    /// <param name="vendorBrandCodes"></param>
    /// <param name="importVendorID"></param>
    /// <returns></returns>
    void UpdateStock(Dictionary<string, List<VendorStock>> vendorStockInput, int importVendorID);

    /// <summary>
    /// Update Price for Vendor
    /// </summary>
    /// <param name="vendorBrandCodes"></param>
    /// <param name="importVendorID"></param>
    /// <returns></returns>
    void UpdatePrice(Dictionary<string, List<VendorPrice>> vendorPriceInput, int importVendorID);
  }
}
