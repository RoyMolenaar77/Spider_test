using Concentrator.Plugins.Axapta.Helpers;
using Concentrator.Plugins.Axapta.Repositories;
using Concentrator.Plugins.Axapta.Services;
using Ninject.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Concentrator.Plugins.Axapta.Binding
{
  public class Bindings : NinjectModule
  {
    public override void Load()
    {
      Bind<ILogger>().To<Logger>();
      Bind<IOrderHelper>().To<OrderHelper>();
      Bind<INotificationHelper>().To<NotificationHelper>();

      Bind<IOrderService>().To<OrderService>();
      Bind<IArchiveService>().To<ArchiveService>();
      Bind<IAdjustStockService>().To<StockAdjustmentService>();
      Bind<IExportStockService>().To<StockAdjustmentService>();
      Bind<IPurchaseOrderService>().To<PurchaseOrderService>();
      Bind<IPickTicketService>().To<PickTicketService>();
      Bind<IExportPickTicketShipmentConfirmation>().To<ConfirmShipmentService>();
      Bind<IExportPurchaseOrderReceivedConfirmationService>().To<PurchaseOrderService>();
      Bind<ICustomerInformationService>().To<CustomerInformationService>();

      Bind<IOrderRepository>().To<OrderRepository>();
      Bind<IProductRepository>().To<ProductRepository>();
      Bind<IConnectorRepository>().To<ConnectorRepository>();
      Bind<IVendorStockRepository>().To<VendorStockRepository>();
      Bind<IVendorRepository>().To<VendorRepository>();
      Bind<IVendorSettingRepository>().To<VendorSettingRepository>();
      Bind<IVendorAssortmentRepository>().To<VendorAssortmentRepository>();
    }
  }
}
