using Concentrator.Tasks.Euretco.Rso.EDI.Processor;

namespace Concentrator.Tasks.Euretco.Rso.EDI.Factory
{
  public static class PricatLineProcessorFactory
  {
    public static IPricatLineProcessor Create()
    {
      return new PricatLineProcessor();
    }
  }
}