using Concentrator.Objects.Models.Vendors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Plugins.Axapta.Repositories
{
  public interface IVendorAssortmentRepository
  {
    VendorAssortment GetVendorAssortment(int vendorID, string customItemNumber);
    IEnumerable<VendorAssortment> GetListOfVendorAssortmentByVendorID(int vendorID);
  }
}
