using Concentrator.Objects.ConcentratorService;
using Concentrator.Plugins.Axapta.Binding;
using Concentrator.Plugins.Axapta.Services;
using Ninject;

namespace Concentrator.Plugins.Axapta
{
  public class ProcessCustomerInformation : ConcentratorPlugin
  {
    public override string Name
    {
      get { return "Process Customer information from Axapta to concentrator to TNT"; }
    }

    protected override void Process()
    {
      using (IKernel kernel = new StandardKernel())
      {
        var bindings = new Bindings();

        kernel.Load(bindings);
        var stockService = kernel.Get<ICustomerInformationService>();

        stockService.Process();
        bindings.Dispose();
      }      
    }
  }
}
