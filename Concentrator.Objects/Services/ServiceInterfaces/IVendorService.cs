using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.Models.Contents;

namespace Concentrator.Objects.Services.ServiceInterfaces
{
  public interface IVendorService
  {
    /// <summary>
    /// Retrieves all vendors of type Content vendor
    /// </summary>
    /// <returns></returns>
    IQueryable<Vendor> GetContentVendors();

    /// <summary>
    /// Creates a vendor setting for a specific connector
    /// </summary>
    /// <param name="setting"></param>
    /// <param name="connectorID"></param>
    void CreateContentVendorSetting(ContentVendorSetting setting);

    /// Retrieves all vendors of type Assortment vendor
    /// </summary>
    /// <returns></returns>
    IQueryable<Vendor> GetAssortmentVendors();

    /// <summary>
    /// Retrieves all vendors of type Stock vendor
    /// </summary>
    /// <returns></returns>
    IQueryable<Vendor> GetStockVendors();

    /// <summary>
    /// Retrieves all vendors of type Retail stock vendor
    /// </summary>
    /// <returns></returns>
    IQueryable<Vendor> GetRetailStockVendors();

    void UpdateVendorProductStatus(int vendorID, int concentratorStatusIDOld, int concentratorStatusIDNew, string vendorStatus);
  }
}
