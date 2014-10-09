using Concentrator.Objects.ConcentratorService;
using Concentrator.Objects.Vendors.Bulk;
using Concentrator.Plugins.PFA.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Plugins.PFA.Imports.Stock
{
  public abstract class StockImportBase : ConcentratorPlugin
  {
    private readonly Monitoring.Monitoring _monitoring = new Monitoring.Monitoring();

    public abstract override string Name { get; }

    protected abstract List<VendorStockCollectionModel> GetStock();

    protected override void Process()
    {
      _monitoring.Notify(Name, 0);
      var stock = GetStock();

      if (stock == null)
      {
        log.AuditError("Nothing to process. Process exiting");
        return;
      }

      using (var unit = GetUnitOfWork())
      {
        foreach (var vendorStockCollection in stock)
        {
          _monitoring.Notify(Name, vendorStockCollection.VendorID);
#if DEBUG
          if (vendorStockCollection.VendorID != 15) continue;
#endif
          using (var stockBulk = new VendorStockBulk(vendorStockCollection.StockCollection, vendorStockCollection.VendorID, vendorStockCollection.DefaultVendorID))
          {
            stockBulk.Init(unit.Context);
            stockBulk.Sync(unit.Context);
          }
        }
      }
      _monitoring.Notify(Name, 1);
    }
  }
}
