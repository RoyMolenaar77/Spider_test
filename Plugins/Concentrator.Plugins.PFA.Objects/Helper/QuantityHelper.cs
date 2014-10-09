using System;

namespace Concentrator.Plugins.PFA.Objects.Helper
{
  public static class QuantityHelper
  {
    public static String GetNumberOfDifferences(int ReceivedFromWehkamp, int ShippedFromPFA)
    {
      return String.Format("{0}{1}", Math.Abs(ReceivedFromWehkamp - ShippedFromPFA), ReceivedFromWehkamp > ShippedFromPFA ? "+" : "-");
    }
  }
}
