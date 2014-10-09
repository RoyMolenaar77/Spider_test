using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.Services.Base;
using Concentrator.Objects.Services.ServiceInterfaces;
using Concentrator.Objects.Models.Contents;
using Concentrator.Objects.DataAccess.UnitOfWork;

namespace Concentrator.Objects.Services
{
  public class VendorAssortmentService : Service<VendorAssortment>
  {
    public override IQueryable<VendorAssortment> Search(string queryTerm)
    {
      if (queryTerm == null) queryTerm = string.Empty;

      return Repository().GetAll(c => c.Vendor.Name.Contains(queryTerm)).AsQueryable();
    }

  }
}
