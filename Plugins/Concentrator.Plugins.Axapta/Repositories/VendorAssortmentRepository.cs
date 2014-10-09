using Concentrator.Objects.ConcentratorService;
using Concentrator.Objects.Models.Vendors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Plugins.Axapta.Repositories
{
  public class VendorAssortmentRepository : UnitOfWorkPlugin, IVendorAssortmentRepository 
  {
    public VendorAssortment GetVendorAssortment(int vendorID, string customItemNumber)
    {
      using (var db = GetUnitOfWork())
      {
        return db
          .Scope
          .Repository<VendorAssortment>()
          .GetSingle(x => x.VendorID == vendorID && x.CustomItemNumber == customItemNumber);
      }
    }

    public IEnumerable<VendorAssortment> GetListOfVendorAssortmentByVendorID(int vendorID)
    {
      using (var db = new PetaPoco.Database(Connection, "System.Data.SqlClient"))
      {
        var query = string.Format(@"
          SELECT VendorAssortmentID
	          ,ProductID
	          ,CustomItemNumber
	          ,va.VendorID
	          ,va.IsActive
          FROM VendorAssortment va
          inner join vendor v on v.vendorid = va.vendorid
          WHERE va.VendorID = {0} or (v.parentvendorid is not null and v.parentvendorid = {0})
        ", vendorID);
          
        IEnumerable<VendorAssortment> vendorAssortments = db.Query<VendorAssortment>(query);        

        return vendorAssortments;
      }
    }
  }
}
