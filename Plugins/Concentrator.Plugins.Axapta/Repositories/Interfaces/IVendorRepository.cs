using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Models.Vendors;
using System;
using System.Collections.Generic;

namespace Concentrator.Plugins.Axapta.Repositories
{
  public interface IVendorRepository
  {
    Vendor GetVendor(string vendorName);
    Vendor GetVendorByID(int vendorID);
    IEnumerable<Vendor> GetVendorAndChildVendorsByID(int vendorID);
  }
}
