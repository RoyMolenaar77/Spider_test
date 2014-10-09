using System;
using System.Linq;
using System.Data;
using Concentrator.Objects;
using Concentrator.Objects.ConcentratorService;
using Concentrator.Plugins.BAS.Vendors.BAS.WebService;
using Concentrator.Objects.Models.Vendors;

namespace Concentrator.Plugins.BAS
{
  public class RetailStockImport : ConcentratorPlugin
  {
    public override string Name
    {
      get { return "BAS Import Retail Stock Plugin"; }
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
        content = cl.GenerateFullProductList(
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
      using (var unit = GetUnitOfWork())
      {
        foreach (Vendor vendor in Vendors.Where(x => ((VendorType)x.VendorType).Has(VendorType.JdeRetailStock) && x.IsActive))
        {
          if (vendor.VendorSettings.GetValueByKey<int>("AssortmentImportID", 0) < 1)
            continue;

          log.DebugFormat("Start import BAS stock import, vendor {0} en vendorID {1}", vendor.Name, vendor.VendorID);


          _retailStock = vendor.VendorSettings.GetValueByKey<bool>("RetailStock", false);
          _auctionStock = vendor.VendorSettings.GetValueByKey<bool>("AuctionStock", false);
          _shopAssortment = vendor.VendorSettings.GetValueByKey<bool>("ShopAssortment", false);

          using (var content = GetContent(vendor))
          {

            if (_retailStock)
            {
              var rs = new ProcessRetailStock();
              rs.Process(content, vendor, log);
            }
          }

          log.DebugFormat("Finish import BAS stock import, vendor {0} en vendorID {1}", vendor.Name, vendor.VendorID);
        }
      }
    }
  }
}
