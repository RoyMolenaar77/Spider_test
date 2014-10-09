using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Concentrator.Objects;
using log4net;
using System.Data.Linq;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Microsoft.Practices.ServiceLocation;
using Concentrator.Objects.Vendors.Base;

namespace Concentrator.Plugins.BAS
{
  public class ProcessRetailStock : IProcessVendorContent
  {
    #region IProcessVendorContent Members

    public void Process(DataSet content, Vendor vendor, ILog log)
    {
      if (content != null)
      {

         using (var unit = ServiceLocator.Current.GetInstance<IUnitOfWork>())
        {
        var list = (from p in content.Tables[3].AsEnumerable()
                    select new Concentrator.Plugins.BAS.VendorRetailStockBulk.VendorImportRetailStock{
                      CustomItemNumber = p.Field<int>("ProductID").ToString(),
                      defaultVendorID = vendor.VendorID,
                      QuantityOnHand = p.Field<int>("InStock"),
                      VendorStockType = "Assortment",
                      VendorBackendCode = p.Field<int>("relationID").ToString()
                    });

        using (VendorRetailStockBulk vrs = new VendorRetailStockBulk(list,vendor.VendorID,vendor.VendorID,"BAS_Retail"))
        {
          vrs.Init(unit.Context);
          vrs.Sync(unit.Context);
        };
         

        //TODO: Data load options
        //DataLoadOptions options = new DataLoadOptions();
        //options.LoadWith<VendorAssortment>(x => x.Product);

        //context.LoadOptions = options;
       
          //var vendorRepo = unit.Scope.Repository<Vendor>();

          //int totalProducts = content.Tables[0].AsEnumerable().Count();
          //log.DebugFormat("Start Process Retail Stock for {0}, {1} productlines", vendor.Name, totalProducts);
          //var dbRetailVendors = vendorRepo.GetAll().ToList();

          //var retailvendors = dbRetailVendors.Where(r => ((VendorType)r.VendorType).Has(VendorType.Stock));

          //int couterProduct = 0;
          //int logCount = 0;

          //var vendors = (from v in unit.Scope.Repository<VendorStock>().GetAll()
          //               join vm in unit.Scope.Repository<VendorMapping>().GetAll(x => x.VendorID == vendor.VendorID) on v.VendorID equals vm.MapVendorID
          //               select v).ToList();

          //var originalVendors = vendors.ToList();

          //VendorDataParser util = new VendorDataParser(unit, vendor, log);
          //foreach (DataRow p in content.Tables[0].AsEnumerable())
          //{
          //  //util.LoadProduct(p, log);
          //  couterProduct++;
          //  logCount++;
          //  if (logCount == 250)
          //  {
          //    log.DebugFormat("Products Processed : {0}/{1} for Vendor {2}", couterProduct, totalProducts, vendor.Name);
          //    logCount = 0;
          //  }

          //  var retailStock = (from rs in content.Tables[3].AsEnumerable()
          //                     where rs.Field<int>("ProductID") == p.Field<int>("ProductID")
          //                     select rs).ToList();

          //  foreach (DataRow dr in retailStock)
          //  {
          //    int? relationVendorID = (from rv in retailvendors
          //                             join v in originalVendors on rv.VendorID equals v.VendorID
          //                             where rv.BackendVendorCode == dr.Field<int>("relationID").ToString()
          //                             select rv.VendorID).FirstOrDefault();

          //    if (relationVendorID.HasValue && relationVendorID.Value > 0)
          //    {
          //      VendorStock vendorstock = util.UpdateRetailStockLevels(relationVendorID.Value, dr, log, originalVendors.Where(x => x.VendorID == relationVendorID).ToList());
               
          //      if (vendorstock != null)
          //        vendors.Remove(vendorstock);
          //    }
          //  }
          //}
          //unit.Save();

          //unit.Scope.Repository<VendorStock>().Delete(vendors);
          //unit.Save();
          
          log.DebugFormat("Finish ProcessRetailStock for {0}", vendor.Name);
        }
      }
      else
        log.DebugFormat("Empty dataset for vendor {0} processing assorment failed", vendor.Name);
    }

    #endregion
  }
}
