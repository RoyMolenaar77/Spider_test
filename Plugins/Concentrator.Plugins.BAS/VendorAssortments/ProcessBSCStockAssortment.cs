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
using Ninject;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects.Vendors.Base;

namespace Concentrator.Plugins.BAS
{
  public class ProcessBSCStockAssortment : IProcessVendorContent
  {
    #region IProcessVendorContent Members

    public void Process(DataSet content, Vendor vendor, ILog log)
    {
      Process(content, vendor, log, false);
    }
    public void Process(DataSet content, Vendor vendor, ILog log, bool processStock)
    {
      using (var unit = ServiceLocator.Current.GetInstance<IKernel>().Get<IUnitOfWork>())
      {
        if (content != null)
        {
          long start = DateTime.Now.Ticks;
          int totalProducts = content.Tables[0].AsEnumerable().Count();
          var stockRepo = unit.Scope.Repository<VendorStock>();

          //TODO: Load options
          //var options = new DataLoadOptions();
          //  options.LoadWith<VendorAssortment>(v => v.VendorPrice);
          //  options.LoadWith<VendorAssortment>(v => v.Product);
          //  context.LoadOptions = options;

          int couterProduct = 0;
          int logCount = 0;
          log.DebugFormat("Start ProcessBSCStockAssortment for {0}, {1} productlines", vendor.Name, totalProducts);
          List<string> vendorItemsInResultSet = stockRepo.GetAll(vs => vs.VendorID == vendor.VendorID).Select(c => c.Product.VendorItemNumber.Trim().ToUpper()).Distinct().ToList();

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
                log.DebugFormat("Procces product {0}/{1} for vendor {2}", couterProduct, totalProducts, vendor.Name);
                logCount = 0;
              }

              if (processStock)
                util.UpdateBSCStockLevels(p, log);
              else
              {
               // util.LoadProduct(p, log);
                //util.ParseAssortment(log);
              }
              customItemNumbers.Add(p.Field<double>("ShortItemNumber"));
              string vendorItemNumber = p.Field<string>("VendorItemNumber").Trim().ToUpper();
              vendorItemsInResultSet.Remove(vendorItemNumber);
            }
            else
              log.DebugFormat("Product {0} already importerted for {1}", p.Field<double>("ShortItemNumber"), vendor.Name);
          }
          unit.Save();

          log.DebugFormat("Start cleanup ProcessBSCStockAssortment for {0}", vendor.Name);
          

          foreach (var vendorItemNumber in vendorItemsInResultSet)
          {
            string vin = vendorItemNumber;
            var records = stockRepo.GetAllAsQueryable(vs => vs.VendorID == vendor.VendorID
                                 //&& vs.VendorStockTypeID == 1//(int)VendorStockType.VendorStockRegularID
                                 && vs.Product.VendorItemNumber.Trim().ToUpper() == vin);

            stockRepo.Delete(records);
          }
          unit.Save();


          //VendorParserUtility util1 = new VendorParserUtility(unit, vendor.VendorID, log);

          //util1.SyncUneeded(content, vendor.VendorID);
          //unit.Save();

          log.DebugFormat("Finish cleanup ProcessBSCStockAssortment for {0}", vendor.Name);

          long finish = DateTime.Now.Ticks - start;
          log.DebugFormat("Finish ProcessBSCStockAssortment for {0}, {1} productlines in {2} minutes", vendor.Name, totalProducts, TimeSpan.FromTicks(finish).Minutes);
        }
        else
          log.DebugFormat("Empty dataset for vendor {0} processing assorment failed", vendor.Name);
      }
    }
    #endregion
  }
}
