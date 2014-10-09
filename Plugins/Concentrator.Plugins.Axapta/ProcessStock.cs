using Concentrator.Objects.ConcentratorService;
using Concentrator.Plugins.Axapta.Binding;
using Concentrator.Plugins.Axapta.Models;
using Concentrator.Plugins.Axapta.Services;
using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Plugins.Axapta
{
  public class ProcessStock : ConcentratorPlugin
  {
    public override string Name
    {
      get { return "Process Stock Adjustment from Axapta."; }
    }

    protected override void Process()
    {
      using (IKernel kernel = new StandardKernel())
      {
        Bindings bindings = new Bindings();

        kernel.Load(bindings);
        IAdjustStockService stockService = kernel.Get<IAdjustStockService>();

        stockService.ProcessAdjustmentStock();
        bindings.Dispose();
      }
    }
  }

  public class ProcessExportCorrectionStock : IDisposable
  {
    private Bindings bindings;
    private IExportStockService stockService;

    public ProcessExportCorrectionStock()
    {
      Bootstrapper();
    }

    private void Bootstrapper()
    {
      using (IKernel kernel = new StandardKernel())
      {
        bindings = new Bindings();

        kernel.Load(bindings);
        stockService = kernel.Get<IExportStockService>();
      }
    }

    public void Process(List<DatColStock> listOfStocks)
    {
      stockService.ProcessExportStock(listOfStocks);
    }

    public void Dispose()
    {
      bindings.Dispose();
    }
  }
}
