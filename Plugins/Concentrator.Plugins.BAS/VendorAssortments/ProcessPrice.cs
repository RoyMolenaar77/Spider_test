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
  public class ProcessPrice : IProcessVendorContent
  {
    #region IProcessVendorContent Members

    public void Process(DataSet content, Vendor vendor, ILog log)
    {
      if (content != null)
      {
        //DataLoadOptions options = new DataLoadOptions();
        //options.LoadWith<VendorAssortment>(x => x.Product);
        //options.LoadWith<VendorAssortment>(x => x.VendorPrices);

        //context.LoadOptions = options;
        using (var unit = ServiceLocator.Current.GetInstance<IUnitOfWork>())
        {
          int totalProducts = content.Tables[0].AsEnumerable().Count();
          log.DebugFormat("Start Process Prices for {0}, {1} productlines", vendor.Name, totalProducts);

          var parentVendorID = unit.Scope.Repository<Vendor>().GetSingle(v => v.VendorID == vendor.VendorID).ParentVendorID;


          int couterProduct = 0;
          int logCount = 0;

          VendorDataParser util = new VendorDataParser(unit, vendor, log);
          foreach (DataRow p in content.Tables[0].AsEnumerable())
          {
            couterProduct++;
            logCount++;
            if (logCount == 250)
            {
              log.DebugFormat("Products prices Processed : {0}/{1} for Vendor {2}", couterProduct, totalProducts, vendor.Name);
              logCount = 0;
            }

            string vendorItemNumber = p.Field<string>("VendorItemNumber").Trim().ToUpper();
            util.UpdatePrices(p, log);
          }
          unit.Save();
          log.DebugFormat("Finished Process prices for {0}", vendor.VendorID);
        }
      }
      else
        log.DebugFormat("Empty dataset for vendor {0} processing prices failed", vendor.Name);
    }

    #endregion
  }
}
