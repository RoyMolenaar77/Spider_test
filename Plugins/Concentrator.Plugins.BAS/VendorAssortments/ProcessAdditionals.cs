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
using Ninject;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects.Vendors.Base;

namespace Concentrator.Plugins.BAS
{
  public class ProcessAdditionals : IProcessVendorContent
  {
    #region IProcessVendorContent Members

    public void Process(DataSet content, Vendor vendor, ILog log)
    {


      if (content != null)
      {
        using (var unit = ServiceLocator.Current.GetInstance<IKernel>().Get<IUnitOfWork>())
        {
          //TODO: LOAD OPTIONS
          //DataLoadOptions options = new DataLoadOptions();
          //options.LoadWith<VendorAssortment>(x => x.Product);

          //context.LoadOptions = options;

          var repoFreeGood = unit.Scope.Repository<VendorFreeGood>();
          var repoAccruel = unit.Scope.Repository<VendorAccruel>();
          VendorDataParser util = new VendorDataParser(unit, vendor, log);


          var vendorAssortment = (from a in unit.Scope.Repository<VendorAssortment>().GetAllAsQueryable(l => l.VendorID == vendor.VendorID && l.IsActive)
                                  select a).Distinct().ToDictionary(x => x.CustomItemNumber, x => x);

          #region FreeGoods
          int totalProducts = content.Tables[1].AsEnumerable().Count();
          log.DebugFormat("Start Process FreeGoods for {0}, {1} productlines", vendor.Name, totalProducts);
          int couterProduct = 0;
          int logCount = 0;

          var freeGoods = (from a in repoFreeGood.GetAllAsQueryable()
                           where a.VendorAssortment.VendorID == vendor.VendorID
                           select a).ToList();

          foreach (DataRow p in content.Tables[1].AsEnumerable())
          {
            //util.LoadProduct(p, log);
            couterProduct++;
            logCount++;
            if (logCount == 250)
            {
              log.DebugFormat("FreeGoods Processed : {0}/{1} for Vendor {2}", couterProduct, totalProducts, vendor.Name);
              logCount = 0;
            }

            if (vendorAssortment.ContainsKey(p.Field<int>("ProductID").ToString()) && vendorAssortment.ContainsKey(p.Field<int>("FreeGoodProductID").ToString()))
            {
              var vendorAssortmentProduct = vendorAssortment[p.Field<int>("ProductID").ToString()];
              var freeGoodProduct = vendorAssortment[p.Field<int>("FreeGoodProductID").ToString()];

              if (freeGoodProduct != null && vendorAssortmentProduct != null)
              {
                var freeGood = freeGoods.FirstOrDefault(x => x.VendorAssortmentID == vendorAssortmentProduct.VendorAssortmentID && x.ProductID == freeGoodProduct.ProductID);

                if (freeGood == null)
                {
                  freeGood = new VendorFreeGood()
                  {
                    VendorAssortmentID = vendorAssortmentProduct.VendorAssortmentID,
                    ProductID = freeGoodProduct.ProductID,
                    MinimumQuantity = p.Field<int>("MinimumQuantity"),
                    OverOrderedQuantity = p.Field<int>("OverOrderedQuantity"),
                    FreeGoodQuantity = p.Field<int>("FreeGoodQuantity"),
                    UnitPrice = p.Field<decimal>("FreeGoodUnitPrice"),
                    Description = p.Field<string>("FreeGoodDescription")
                  };
                  repoFreeGood.Add(freeGood);
                }
                else
                  freeGoods.Remove(freeGood);
              }
            }
          }

          repoFreeGood.Delete(freeGoods);
          unit.Save();

          log.DebugFormat("Finish FreeGoods for {0}", vendor.Name);
          #endregion

          #region Accruals
          totalProducts = content.Tables[2].AsEnumerable().Count();
          log.DebugFormat("Start Process Accruels for {0}, {1} productlines", vendor.Name, totalProducts);
          couterProduct = 0;
          logCount = 0;

          var accruels = repoAccruel.GetAll(a => a.VendorAssortment.VendorID == vendor.VendorID).ToList();


          foreach (DataRow p in content.Tables[2].AsEnumerable())
          {
            //util.LoadProduct(p, log);
            couterProduct++;
            logCount++;
            if (logCount == 250)
            {
              log.DebugFormat("Accruels Processed : {0}/{1} for Vendor {2}", couterProduct, totalProducts, vendor.Name);
              logCount = 0;
            }
            if (p.Table.Columns.Contains("ProductID"))
            {
              if (vendorAssortment.ContainsKey(p.Try(c => c.Field<int>("ProductID").ToString(), string.Empty)))
              {
                var vendorAssortmentProduct = vendorAssortment[p.Field<int>("ProductID").ToString()];

                if (vendorAssortmentProduct != null)
                {
                  var accruel = accruels.FirstOrDefault(x => x.VendorAssortmentID == vendorAssortmentProduct.VendorAssortmentID && x.AccruelCode == p.Field<string>("AccruelCode"));

                  if (accruel == null)
                  {
                    accruel = new VendorAccruel()
                    {
                      VendorAssortmentID = vendorAssortmentProduct.VendorAssortmentID,
                      AccruelCode = p.Field<string>("AccruelCode"),
                      Description = p.Field<string>("AccruelDescription"),
                      UnitPrice = p.Field<decimal>("UnitPrice"),
                      MinimumQuantity = p.Field<int>("MinimumQuantity") > 0 ? p.Field<int>("MinimumQuantity") : 0
                    };
                    repoAccruel.Add(accruel);
                  }
                  else
                    accruels.Remove(accruel);
                }
              }
            }
          }

          repoAccruel.Delete(accruels);
          unit.Save();

          log.DebugFormat("Finish Accruels for {0}", vendor.Name);
          #endregion
        }
      }
      else
        log.DebugFormat("Empty dataset for vendor {0}, processing assortiment failed", vendor.Name);
    }

    #endregion
  }
}
