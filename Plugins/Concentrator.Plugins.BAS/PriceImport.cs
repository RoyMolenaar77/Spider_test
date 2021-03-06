﻿using System;
using System.Linq;
using System.Data;
using Concentrator.Objects;
using Concentrator.Objects.ConcentratorService;
using Concentrator.Plugins.BAS.Vendors.BAS.WebService;
using Concentrator.Objects.Models.Vendors;

namespace Concentrator.Plugins.BAS
{
  public class PriceImport : ConcentratorPlugin
  {
    public override string Name
    {
      get { return "BAS Price Import Plugin"; }
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

      if (vendor.VendorSettings.Where(c => c.SettingKey == "CostPriceImportID").FirstOrDefault() != null)
      {

        DataSet contentCostPrice = cl.GenerateFullProductListWithNonStock(
          int.Parse(vendor.VendorSettings.Where(c => c.SettingKey == "CostPriceImportID").FirstOrDefault().Value),
          int.Parse(vendor.VendorSettings.Where(c => c.SettingKey == "InternetAssortment").FirstOrDefault().Value),
          int.Parse(vendor.VendorSettings.Where(c => c.SettingKey == "AllowDC10OItems").FirstOrDefault().Value),
          int.Parse(vendor.VendorSettings.Where(c => c.SettingKey == "AllowDC10ObyStatus").FirstOrDefault().Value),
          false,
          false
          );
        content.Tables[0].Columns.Add("CostPrice", typeof(decimal));
        content.Tables[0].Columns["CostPrice"].AllowDBNull = true;

        foreach (DataRow row in content.Tables[0].AsEnumerable())
        {
          row["CostPrice"] = (from ccp in contentCostPrice.Tables[0].AsEnumerable()
                              where
                                ccp.Field<double>("ShortItemNumber") == row.Field<double>("ShortItemNumber")
                                && ccp.Field<int>("MinimumQuantity") == row.Field<int>("MinimumQuantity")
                              select ccp.Field<decimal>("UnitPrice")).FirstOrDefault();
        }
      }

      return content;
    }


    protected override void Process()
    {
      foreach (Vendor vendor in Vendors.Where(x => ((VendorType)x.VendorType).Has(VendorType.Assortment) && x.IsActive))
      {

        if (vendor.VendorSettings.GetValueByKey<int>("AssortmentImportID", 0) < 1)
          continue;

        log.DebugFormat("Start Assortment Import for Vendor '{0} ({1})'", vendor.Name, vendor.VendorID);

        _retailStock = false; //vendor.VendorSettings.GetValueByKey<bool>("RetailStock", false);
        _auctionStock = vendor.VendorSettings.GetValueByKey<bool>("AuctionStock", false);
        _shopAssortment = vendor.VendorSettings.GetValueByKey<bool>("ShopAssortment", false);


        using (var content = GetContent(vendor))
        {
          var ass = new ProcessPrice();
          ass.Process(content, vendor, log);
        }

        //if (vendor.RetailStock)
        //{
        //  var rs = new ProcessRetailStock();
        //  rs.Process(Content, VendorID, log);
        //}

        log.DebugFormat("Finished Assortment Import for vendor '{0} ({1})'", vendor.Name, vendor.VendorID);

      }
    }
  }
}
