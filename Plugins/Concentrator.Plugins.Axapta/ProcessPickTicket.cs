using AuditLog4Net.Adapter;
using AuditLog4Net.AuditLog;
using Concentrator.Objects.ConcentratorService;
using Concentrator.Objects.Models.Orders;
using Concentrator.Plugins.Axapta.Binding;
using Concentrator.Plugins.Axapta.Services;
using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Plugins.Axapta
{
  public class ProcessPickTicket : ConcentratorPlugin
  {
    public override string Name
    {
      get { return "Process Sales and Transfer Orders from Axapta!"; }
    }

    protected override void Process()
    {
      using (IKernel kernel = new StandardKernel())
      {
        var bindings = new Bindings();

        kernel.Load(bindings);
        kernel.Bind<IAuditLogAdapter>().ToConstant(log);
        var webPickTicketService = kernel.Get<IPickTicketService>();

        webPickTicketService.Process();
        bindings.Dispose();
      }
    }
  }

  public class ProcessPickTicketShipmentConfirmation : ConcentratorPlugin
  {
    public override string Name
    {
      get { return "Process Shipment Confirmation To Axapta"; }
    }

    protected override void Process()
    {
      using (IKernel kernel = new StandardKernel())
      {
        var bindings = new Bindings();

        kernel.Load(bindings);
        kernel.Bind<IAuditLogAdapter>().ToConstant(log);
        var pickTicketService = kernel.Get<IExportPickTicketShipmentConfirmation>();

        pickTicketService.Process();
        bindings.Dispose();
      }
    }
  }
}
