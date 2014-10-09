using Concentrator.Plugins.PFA.Helpers;
using Concentrator.Plugins.PFA.Models;
using Concentrator.Plugins.PFA.Repos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Plugins.PFA.Imports.Stock
{
  public class PFAStockImportAT : StockImportBase
  {
    private const string STOCK_ENABLED_STATUS = "ENA";
    private const string STOCK_DISABLED_STATUS = "NoStock";
    private int _wehkampVendorID = 25;
    private List<int> _vendorIDs;

    public override string Name
    {
      get { return "PFA Stock import AT"; }
    }

    protected override List<Models.VendorStockCollectionModel> GetStock()
    {
      List<Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportStock> result = new List<Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportStock>();
      var pluginConfig = GetConfiguration();

      _vendorIDs = GetConfiguration().AppSettings.Settings["ATPFAStockImportVendorIDs"].Value.Split(',').Select(int.Parse).ToList();

      string connectionString = string.Empty;


      var dsnNameSetting = GetConfiguration().AppSettings.Settings["ATPFADSN"];
      dsnNameSetting.ThrowIfNull("AT DSN Must be specified");

      connectionString = string.Format("DSN={0};PWD=progress", dsnNameSetting.Value);

      AtPFARepository repository = new AtPFARepository(connectionString, log);
      var assortmentHelper = new AssortmentHelper();

      var validProducts = repository.GetValidItemColors();

      log.Info("Found products in total " + validProducts.Count);

      int counter = 0;

      using (var unit = GetUnitOfWork())
      {
        foreach (var validItem in validProducts)
        {
          var itemNumber = validItem.Key;
          counter++;

          foreach (var color in validItem.Value)
          {
            var customItemNumber = string.Format("{0} {1}", itemNumber, color).Trim();

            result.Add(GetVendorStock(customItemNumber, 0, CCStockType.Webshop));
            result.Add(GetVendorStock(customItemNumber, 0, CCStockType.Wehkamp));


            bool noSkus = false;
            int skuCount = 0;
            foreach (var sku in repository.GetValidSkus(itemNumber, color))
            {
              int stock = repository.GetSkuStock(itemNumber, sku.SizeCode, color, 801);
              int stockWehkamp = repository.GetSkuStock(itemNumber, sku.SizeCode, color, 803);

              var skuNew = string.Format("{0} {1} {2}", itemNumber, color, sku.SizeCode);

              if (customItemNumber.StartsWith("7")) //food items
                stock = 1;

              result.Add(new Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportStock()
              {
                CustomItemNumber = skuNew,
                QuantityOnHand = stock,
                StockType = "Webshop",
                StockStatus = stock > 0 ? "ENA" : "NoStock"
              });

              result.Add(new Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportStock()
              {

                CustomItemNumber = skuNew,
                QuantityOnHand = stockWehkamp,
                StockType = "Wehkamp",
                StockStatus = stock > 0 ? "ENA" : "NoStock"
              });
            }
          }
        }
      }


      List<VendorStockCollectionModel> groupedResult = new List<VendorStockCollectionModel>();

      //for the regular vendors add all stock locations without Wehkamp
      foreach (var vendor in _vendorIDs)
      {
        groupedResult.Add(new VendorStockCollectionModel()
        {
          VendorID = vendor,
          DefaultVendorID = 2,
          StockCollection = result.Where(c => c.StockType != CCStockType.Wehkamp.ToString()).ToList()
        });
      }

      var wehkampStockCollection = result.Where(c => c.StockType == CCStockType.Wehkamp.ToString()).ToList();

      //For vendor Wehkamp, add the stock the webshop location for exporting


      var wehkampWebshopStockCollection = (from r in wehkampStockCollection
                                           select new Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportStock()
                  {

                    CustomItemNumber = r.CustomItemNumber,
                    QuantityOnHand = r.QuantityOnHand,
                    StockType = CCStockType.Webshop.ToString(),
                    StockStatus = r.QuantityOnHand > 0 ? "ENA" : "NoStock"
                  }).ToList();

      wehkampStockCollection.AddRange(wehkampWebshopStockCollection);

      //wehkamp
      groupedResult.Add(new VendorStockCollectionModel()
      {
        VendorID = _wehkampVendorID,
        DefaultVendorID = 2,
        StockCollection = wehkampStockCollection
      });

      return groupedResult;
    }

    private Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportStock GetVendorStock(string itemNumber, int qty, CCStockType stockType, string stockStatus = "")
    {
      if (itemNumber.StartsWith("7"))
        qty = 1;

      return new Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportStock()
      {
        CustomItemNumber = itemNumber,
        QuantityOnHand = Math.Max(qty, 0), //Handle negative quantities
        StockType = stockType.ToString(),
        StockStatus = string.IsNullOrEmpty(stockStatus) ? (qty == 0 ? STOCK_DISABLED_STATUS : STOCK_ENABLED_STATUS) : stockStatus
      };
    }
  }
}
