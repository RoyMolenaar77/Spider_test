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
  public class VendorService : Service<Vendor>, IVendorService
  {
   

    public IQueryable<Vendor> GetContentVendors()
    {
      var cv = base.GetAll().Where(v => (v.VendorType & (int)VendorType.Content) == (int)VendorType.Content);

      return cv;
    }


    public void CreateContentVendorSetting(ContentVendorSetting setting)
    {

    }

    public IQueryable<Vendor> GetAssortmentVendors()
    {
      return base.GetAll().Where(v => (v.VendorType & (int)VendorType.Assortment) == (int)VendorType.Assortment);
    }

    public IQueryable<Vendor> GetStockVendors()
    {
      return base.GetAll().Where(v => (v.VendorType & (int)VendorType.Stock) == (int)VendorType.Stock);
    }

    public IQueryable<Vendor> GetRetailStockVendors()
    {
      return base.GetAll().Where(v => (v.VendorType & (int)VendorType.JdeRetailStock) == v.VendorType);
    }


    public void UpdateVendorProductStatus(int vendorID, int concentratorStatusIDOld, int concentratorStatusIDNew, string vendorStatus)
    {
      ((IFunctionScope)Scope).Repository().UpdateVendorProductStatus(vendorID, concentratorStatusIDOld, concentratorStatusIDNew, vendorStatus);
    }

    public override IQueryable<Vendor> Search(string queryTerm)
    {
      if (queryTerm == null) queryTerm = string.Empty;

      return Repository().GetAll(c => c.Name.Contains(queryTerm)).AsQueryable();
    }
  }
}
