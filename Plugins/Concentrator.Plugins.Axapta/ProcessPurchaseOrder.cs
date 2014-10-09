using AuditLog4Net.Adapter;
using AuditLog4Net.AuditLog;
using Concentrator.Objects.ConcentratorService;
using Concentrator.Objects.Models.Orders;
using Concentrator.Plugins.Axapta.Binding;
using Concentrator.Plugins.Axapta.Services;
using Ninject;
using System;
using System.Collections.Generic;

namespace Concentrator.Plugins.Axapta
{
  public class ProcessPurchaseOrder : ConcentratorPlugin
  {
    public override string Name
    {
      get { return "Process Purchase Orders from Axapta"; }
    }

    protected override void Process()
    {
      using (IKernel kernel = new StandardKernel())
      {
        var bindings = new Bindings();

        kernel.Load(bindings);
        kernel.Bind<IAuditLogAdapter>().ToConstant(log);
        var purchaseOrderService = kernel.Get<IPurchaseOrderService>();

        purchaseOrderService.Process();
        bindings.Dispose();
      }
    }
  }

  public class ProcessPurchaseOrderReceivedConfirmation : IDisposable
  {
    Bindings _bindings;
    IAuditLogAdapter _log;
    IExportPurchaseOrderReceivedConfirmationService _orderService;

    public ProcessPurchaseOrderReceivedConfirmation()
    {
      _log = new AuditLogAdapter(log4net.LogManager.GetLogger("Logger"), new AuditLog(new ConcentratorAuditLogProvider()));

      Bootstrapper();
    }

    private void Bootstrapper()
    {
      using (IKernel kernel = new StandardKernel())
      {
        _bindings = new Bindings();

        kernel.Load(_bindings);
        kernel.Bind<IAuditLogAdapter>().ToConstant(_log);
        _orderService = kernel.Get<IExportPurchaseOrderReceivedConfirmationService>();
      }
    }

    public void Process(List<OrderResponseLine> listOfOrderResponseLines)
    {
      _orderService.ExportReceivedConfirmation(listOfOrderResponseLines);
    }

    public void Dispose()
    {
      _bindings.Dispose();
    }
  }
}
