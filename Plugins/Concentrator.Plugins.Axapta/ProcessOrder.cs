using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Concentrator.Plugins.Axapta
{
  using AuditLog4Net.Adapter;
  using Concentrator.Objects.ConcentratorService;
  using Concentrator.Plugins.Axapta.Binding;
  using Concentrator.Plugins.Axapta.Services;
  using Ninject;
  using Ninject.Activation;

  public class ProcessOrder : ConcentratorPlugin
  {
    public override string Name
    {
      get { return "Process Sales Orders to Axapta"; }
    }

    protected override void Process()
    {
      using (IKernel kernel = new StandardKernel())
      {
        Bindings bindings = new Bindings();

        kernel.Load(bindings);
        kernel.Bind<IAuditLogAdapter>().ToConstant(log);
        IOrderService orderService = kernel.Get<IOrderService>();

        orderService.Process();
        bindings.Dispose();
      }
    }
  }
}
