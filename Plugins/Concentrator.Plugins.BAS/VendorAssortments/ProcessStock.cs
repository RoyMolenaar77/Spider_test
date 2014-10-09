using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Concentrator.Objects;
using log4net;
using System.Data.Linq;
using Concentrator.Objects.Models.Vendors;
using Microsoft.Practices.ServiceLocation;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects.Vendors.Base;

namespace Concentrator.Plugins.BAS
{
  public class ProcessStock : IProcessVendorContent
  {
    #region IProcessVendorContent Members

    public void Process(DataSet content, Vendor vendor, ILog log)
    {
      if (content != null)
      {
        using (var unit = ServiceLocator.Current.GetInstance<IUnitOfWork>())
        {

          var stockRepo = unit.Scope.Repository<VendorStock>();
          //DataLoadOptions options = new DataLoadOptions();
          //options.LoadWith<VendorAssortment>(x => x.Product);

          //context.LoadOptions = options;

          int totalProducts = content.Tables[0].AsEnumerable().Count();
          log.DebugFormat("Start Process Stock for {0}, {1} productlines", vendor.Name, totalProducts);

          var parentVendorID = unit.Scope.Repository<Vendor>().GetSingle(c => c.VendorID == vendor.VendorID).Try(l => l.ParentVendorID, null);



          List<string> vendorItemsInResultSet = stockRepo.GetAll(vs => vs.VendorID == vendor.VendorID
                                                       && vs.VendorStockTypeID == 1).Select(c => c.Product.VendorItemNumber).ToList();


          int couterProduct = 0;
          int logCount = 0;

          VendorDataParser util = new VendorDataParser(unit, vendor, log);
          foreach (DataRow p in content.Tables[0].AsEnumerable())
          {
            //util.LoadProduct(p, log);

            couterProduct++;
            logCount++;
            if (logCount == 250)
            {
              log.DebugFormat("Products Processed : {0}/{1} for Vendor {2}", couterProduct, totalProducts, vendor.Name);
              logCount = 0; 
            }

            string vendorItemNumber = p.Field<string>("VendorItemNumber").Trim().ToUpper();

            util.UpdateStockLevels(p, log);
            vendorItemsInResultSet.Remove(vendorItemNumber);

          }

          unit.Save();
          log.DebugFormat("Cleaning up old stock records for vendor {0}", vendor.Name);
          foreach (var vendorItemNumber in vendorItemsInResultSet)
          {
            string vin = vendorItemNumber;
            var records = stockRepo.GetAllAsQueryable(vs => vs.VendorID == vendor.VendorID
                                 && vs.VendorStockTypeID == 1//(int)VendorStockType.VendorStockRegularID
                                 && vs.Product.VendorItemNumber.Trim().ToUpper() == vin);

            stockRepo.Delete(records);
          }
          unit.Save();


        }
        log.DebugFormat("Finished Process Stock for {0}", vendor.VendorID);
      }
      else
        log.DebugFormat("Empty dataset for vendor {0} processing assorment failed", vendor.Name);
    }

    #endregion
  }
}
