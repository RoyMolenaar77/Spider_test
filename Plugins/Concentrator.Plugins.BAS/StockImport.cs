using System;
using System.Linq;
using System.Data;
using Concentrator.Objects;
using Concentrator.Objects.ConcentratorService;
using Concentrator.Plugins.BAS.Vendors.BAS.WebService;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.Vendors;
using Concentrator.Objects.Vendors.Bulk;

namespace Concentrator.Plugins.BAS
{
  public class StockImport : ConcentratorPlugin
  {
    public override string Name
    {
      get { return "BAS Import Stock Plugin"; }
    }

    private bool _retailStock;
    private bool _auctionStock;
    private bool _shopAssortment;

    private DataSet GetContent(Vendor vendor)
    {
      DataSet content = null;

      JdeAssortmentSoapClient cl = new JdeAssortmentSoapClient();
      if (_retailStock)
      {
        content = cl.GenerateFullProductListWithRetailStock(
          int.Parse(vendor.VendorSettings.Where(c => c.SettingKey == "AssortmentImportID").FirstOrDefault().Value),
          int.Parse(vendor.VendorSettings.Where(c => c.SettingKey == "InternetAssortment").FirstOrDefault().Value),
          int.Parse(vendor.VendorSettings.Where(c => c.SettingKey == "AllowDC10OItems").FirstOrDefault().Value),
          int.Parse(vendor.VendorSettings.Where(c => c.SettingKey == "AllowDC10ObyStatus").FirstOrDefault().Value));
      }
      else if (_auctionStock)
      {

        content = cl.GenerateFullProductListSpecialAssortment(
                  int.Parse(vendor.VendorSettings.Where(c => c.SettingKey == "AssortmentImportID").FirstOrDefault().Value),
                  int.Parse(vendor.VendorSettings.Where(c => c.SettingKey == "InternetAssortment").FirstOrDefault().Value),
                  int.Parse(vendor.VendorSettings.Where(c => c.SettingKey == "AllowDC10OItems").FirstOrDefault().Value),
                  int.Parse(vendor.VendorSettings.Where(c => c.SettingKey == "AllowDC10ObyStatus").FirstOrDefault().Value),
      0,
                  vendor.VendorSettings.Where(c => c.SettingKey == "BSCStock").FirstOrDefault().Value,
                  vendor.VendorSettings.Where(c => c.SettingKey == "CostPriceDC10").FirstOrDefault().Value,
                 vendor.VendorSettings.Where(c => c.SettingKey == "CostPriceBSC").FirstOrDefault().Value,
true
                  );
      }
      else if (_shopAssortment)
      {
        content = cl.GenerateFullProductListWithNonStock(
          int.Parse(vendor.VendorSettings.Where(c => c.SettingKey == "AssortmentImportID").FirstOrDefault().Value),
          int.Parse(vendor.VendorSettings.Where(c => c.SettingKey == "InternetAssortment").FirstOrDefault().Value),
          int.Parse(vendor.VendorSettings.Where(c => c.SettingKey == "AllowDC10OItems").FirstOrDefault().Value),
          int.Parse(vendor.VendorSettings.Where(c => c.SettingKey == "AllowDC10ObyStatus").FirstOrDefault().Value),
          false,
          true
          );


      }
      else
      {

        content = cl.GenerateFullProductList(
          int.Parse(vendor.VendorSettings.Where(c => c.SettingKey == "AssortmentImportID").FirstOrDefault().Value),
          int.Parse(vendor.VendorSettings.Where(c => c.SettingKey == "InternetAssortment").FirstOrDefault().Value),
          int.Parse(vendor.VendorSettings.Where(c => c.SettingKey == "AllowDC10OItems").FirstOrDefault().Value),
          int.Parse(vendor.VendorSettings.Where(c => c.SettingKey == "AllowDC10ObyStatus").FirstOrDefault().Value),
          false,
          false
          );
      }

      //if (vendor.VendorSettings.Where(c => c.SettingKey == "CostPriceImportID").FirstOrDefault() != null)
      //{

      //  DataSet contentCostPrice = cl.GenerateFullProductList(
      //              int.Parse(vendor.VendorSettings.Where(c => c.SettingKey == "CostPriceImportID").FirstOrDefault().Value),
      //              int.Parse(vendor.VendorSettings.Where(c => c.SettingKey == "InternetAssortment").FirstOrDefault().Value),
      //              int.Parse(vendor.VendorSettings.Where(c => c.SettingKey == "AllowDC10OItems").FirstOrDefault().Value),
      //              int.Parse(vendor.VendorSettings.Where(c => c.SettingKey == "AllowDC10ObyStatus").FirstOrDefault().Value),
      //              false,
      //              false
      //              );
      //  content.Tables[0].Columns.Add("CostPrice", typeof(decimal));
      //  content.Tables[0].Columns["CostPrice"].AllowDBNull = true;

      //  foreach (DataRow row in content.Tables[0].AsEnumerable())
      //  {
      //    row["CostPrice"] = (from ccp in contentCostPrice.Tables[0].AsEnumerable()
      //                        where ccp.Field<double>("ShortItemNumber") == row.Field<double>("ShortItemNumber")
      //                        && ccp.Field<int>("MinimumQuantity") == row.Field<int>("MinimumQuantity")
      //                        select ccp.Field<decimal>("UnitPrice")).FirstOrDefault();
      //  }
      //}
      return content;

    }

    protected override void Process()
    {
      foreach (Vendor vendor in Vendors.Where(x => ((VendorType)x.VendorType).Has(VendorType.Stock) && x.IsActive))
      {
#if DEBUG
        if (vendor.VendorID != 31)
          continue;
#endif

        if (vendor.VendorSettings.GetValueByKey<int>("AssortmentImportID", 0) < 1)
          continue;

        log.DebugFormat("Start import BAS stock import, vendor {0} en vendorID {1}", vendor.Name, vendor.VendorID);


        _retailStock = false;// vendor.VendorSettings.GetValueByKey<bool>("RetailStock", false);
        _auctionStock = vendor.VendorSettings.GetValueByKey<bool>("AuctionStock", false);
        _shopAssortment = vendor.VendorSettings.GetValueByKey<bool>("ShopAssortment", false);

        using (var content = GetContent(vendor))
        {

          int totalProducts = content.Tables[0].AsEnumerable().Count();
          log.DebugFormat("Start Stock processing for {0}, {1} Products", vendor.Name, totalProducts);

          //if (totalProducts > 0)
          //  BulkImport(vendor.VendorID, content, vendor.ParentVendorID);
          //else
          //  log.Debug("Stop processing with empty dataset");

          if (totalProducts > 0)
          {
            if (_auctionStock)
            {
              var bscStock = new ProcessBSCStockAssortment();
              bscStock.Process(content, vendor, log, true);
            }
            else if (_retailStock)
            {
              var rs = new ProcessRetailStock();
              rs.Process(content, vendor, log);
            }
            else
            {
              var ass = new ProcessStock();
              ass.Process(content, vendor, log);
            }
          }
          else
            log.Debug("Stop processing with empty dataset");
        }

        log.DebugFormat("Finish import BAS stock import, vendor {0} en vendorID {1}", vendor.Name, vendor.VendorID);
      }
    }

    private void BulkImport(int vendorID, DataSet content, int? parentVendorID)
    {
      using (var unit = GetUnitOfWork())
      {
        var dataList = (from d in content.Tables[0].AsEnumerable()
                        let defaultVendorID = parentVendorID.HasValue ? parentVendorID.Value : vendorID
                        select new Concentrator.Objects.Vendors.Bulk.VendorStockBulk.VendorImportStock
                        {
                          VendorID = vendorID,
                          VendorStockType = "Assortment",
                          QuantityOnHand = (int)d.Field<double>("QuantityOnHand") > 0 ? (int)d.Field<double>("QuantityOnHand") : 0,
                          CustomItemNumber = VendorImportUtility.SetDataSetValue("ShortItemNumber", d),
                          defaultVendorID = defaultVendorID,
                          PromisedDeliveryDate = VendorImportUtility.SetDataSetValue("PromisedDeliveryDate", d),
                          QuantityToReceive = VendorImportUtility.SetDataSetValue("QuantityToReceive", d) != null ? int.Parse(VendorImportUtility.SetDataSetValue("QuantityToReceive", d)) : 0,
                          StockStatus = VendorImportUtility.SetDataSetValue("StockStatus", d),
                          UnitCost = null,
                          VendorBrandCode = VendorImportUtility.SetDataSetValue("Brand", d),
                          VendorItemNumber = VendorImportUtility.SetDataSetValue("VendorItemNumber", d),
                          VendorStatus = VendorImportUtility.SetDataSetValue("StockStatus", d)
                        });

        log.Debug("dataset to object");

        using (var vendorStockBulk = new VendorStockBulk(dataList, vendorID, parentVendorID, "BAS"))
        {
          vendorStockBulk.Init(unit.Context);
          vendorStockBulk.Sync(unit.Context);
        }
      }
    }
  }
}
