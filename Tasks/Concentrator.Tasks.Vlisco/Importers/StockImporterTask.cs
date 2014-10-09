using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Tasks.Vlisco.Importers
{
  using Objects.Models.Products;
  using Objects.Models.Vendors;

  [Task(Constants.Vendor.Vlisco + " Stock Importer Task")]
  public class StockImporterTask : MultiMagImporterTask<Models.Stock, Models.StockMapping>
  {
    protected override String ImportFilePrefix
    {
      get
      {
        return Constants.Prefixes.Stock;
      }
    }

    protected override void Import(IDictionary<Models.Stock, String> stockItems)
    {
      var vendorStockTypeRepository = Unit.Scope.Repository<VendorStockType>();
      var vendorStockTypes = vendorStockTypeRepository.GetAll().ToDictionary(x => x.StockType);

      foreach (var shopCode in stockItems.Keys.Select(stock => stock.ShopCode).Distinct())
      {
        if (!vendorStockTypes.ContainsKey(shopCode))
        {
          var vendorStockType = new VendorStockType
          {
            StockType = shopCode
          };

          vendorStockTypes[shopCode] = vendorStockType;
          vendorStockTypeRepository.Add(vendorStockType);
        }
      }

      Unit.Save();

      var vendorID = Unit.Scope
        .Repository<Vendor>()
        .GetSingle(vendor => vendor.Name == Constants.Vendor.Vlisco)
        .VendorID;
      var vendorStockRepository = Unit.Scope.Repository<VendorStock>();

      foreach (var stockItemsByShopCode in stockItems.Keys.GroupBy(stockItem => stockItem.ShopCode))
      {
        var vendorStockLookup = vendorStockRepository
          .Include(vendorStock => vendorStock.Product)
          .Include(vendorStock => vendorStock.VendorStockType)
          .GetAll(vendorStock => vendorStock.VendorStockType.StockType == stockItemsByShopCode.Key && vendorStock.VendorID == vendorID)
          .ToDictionary(vendorStock => vendorStock.Product.VendorItemNumber);

        foreach (var stockItem in stockItemsByShopCode)
        {
          var vendorItemNumber = Constants.GetVendorItemNumber(stockItem.ArticleCode, stockItem.ColorCode, stockItem.SizeCode);
          var vendorStock = default(VendorStock);

          if (!vendorStockLookup.TryGetValue(vendorItemNumber, out vendorStock))
          {
            var product = Unit.Scope.Repository<Product>().GetSingle(p => p.VendorItemNumber == vendorItemNumber);

            vendorStock = new VendorStock
            {
              ConcentratorStatusID = 1,
              Product = product,
              StockStatus = Constants.Status.Default,
              VendorID = vendorID,
              VendorStatus = Constants.Status.Default,
              VendorStockType = vendorStockTypes[stockItemsByShopCode.Key]
            };

            vendorStockRepository.Add(vendorStock);
          }

          vendorStock.QuantityOnHand = stockItem.Available;
        }
      }

      Unit.Save();
    }
  }
}
