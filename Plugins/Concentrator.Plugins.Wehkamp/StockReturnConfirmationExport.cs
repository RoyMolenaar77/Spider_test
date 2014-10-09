using Concentrator.Objects.ConcentratorService;

namespace Concentrator.Plugins.Wehkamp
{
  public class StockReturnConfirmationExport : ConcentratorPlugin
  {
    private readonly Monitoring.Monitoring _monitoring = new Monitoring.Monitoring();

    public override string Name
    {
      get { return "Wehkamp Return Confirmation Export"; }
    }

    protected override void Process()
    {
      //throw new NotImplementedException();
    }
  }
}
