using Concentrator.Objects.ConcentratorService;
using Concentrator.Objects.Models.Vendors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Plugins.Axapta.Repositories
{
  public class VendorRepository : UnitOfWorkPlugin, IVendorRepository
  {
    public Vendor GetVendor(string vendorName)
    {
      using (var unit = GetUnitOfWork())
      {
        return unit.Scope.Repository<Vendor>().GetSingle(x => x.Name.Equals(vendorName));
      }
    }

    public Vendor GetVendorByID(int vendorID)
    {
      using (var unit = GetUnitOfWork())
      {
        return unit.Scope.Repository<Vendor>().GetSingle(x => x.VendorID == vendorID);
      }
    }

    public IEnumerable<Vendor> GetVendorAndChildVendorsByID(int vendorID)
    {
      using (var unit = GetUnitOfWork())
      {
        var listOfVendors = new List<Vendor>();

        var parentVendor = unit.Scope.Repository<Vendor>().GetSingle(x => x.VendorID == vendorID);
        listOfVendors.Add(parentVendor);

        if (parentVendor != null)
        {
          var listOfChildVendors = unit.Scope.Repository<Vendor>().GetAll(x => x.ParentVendorID == vendorID);

          listOfVendors.AddRange(listOfChildVendors);
        }

        return listOfVendors;
      }
    }

  }
}
