using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Concentrator.Objects;
using System.Threading;
using System.Transactions;
using log4net;
using System.Data.Linq;
using Concentrator.Objects.Models.Vendors;
using Microsoft.Practices.ServiceLocation;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects.Vendors.Base;


namespace Concentrator.Plugins.BAS
{
  public class ProcessAssortment : IProcessVendorContent
  {
    #region IProcessVendorContent Members

    public void Process(DataSet content, Vendor vendor, ILog log)
    {
      if (content != null)
      {
        long start = DateTime.Now.Ticks;
        int totalProducts = content.Tables[0].AsEnumerable().Count();

        using (var unit = ServiceLocator.Current.GetInstance<IUnitOfWork>())
        {
          //DataLoadOptions options = new DataLoadOptions();
          //options.LoadWith<VendorAssortment>(x => x.VendorPrice);
          //options.LoadWith<Product>(x => x.ProductBarcodes);

          //context.LoadOptions = options;

          int couterProduct = 0;
          int logCount = 0;
          log.DebugFormat("Start Assortment processing for {0}, {1} Products", vendor.Name, totalProducts);
          List<double> customItemNumbers = new List<double>();

          VendorDataParser util = new VendorDataParser(unit, vendor, log);
          foreach (DataRow p in content.Tables[0].AsEnumerable())
          {
            if (!customItemNumbers.Contains(p.Field<double>("ShortItemNumber")))
            {
              couterProduct++;
              logCount++;
              if (logCount == 250)
              {
                log.DebugFormat("Products Processed : {0}/{1} for Vendor {2}", couterProduct, totalProducts, vendor.Name);
                logCount = 0;
              }

              if (util.LoadProduct(p, log))
              {
                util.ParseAssortment(log);
              }

              customItemNumbers.Add(p.Field<double>("ShortItemNumber"));
            }
            else
              log.DebugFormat("Product {0} already imported for {1}", p.Field<double>("ShortItemNumber"), vendor.Name);
          }
          unit.Save();

          log.DebugFormat("Start cleanup assortment for {0}", vendor.Name);

          VendorParserUtility util1 = new VendorParserUtility(unit, vendor.VendorID, log);

          util1.SyncUneeded(content, vendor.VendorID);
          unit.Save();

          log.DebugFormat("Finish cleanup assormtent for {0}", vendor.Name);

          long finish = DateTime.Now.Ticks - start;
          log.DebugFormat("Finish Assortment processing for {0}, {1} productlines in {2} minutes", vendor.Name, totalProducts, TimeSpan.FromTicks(finish).Minutes);
        }
      }
      else
        log.DebugFormat("Empty dataset for vendor {0}, processing assortiment failed", vendor.Name);
    }

    #endregion
  }
}
