using System;

namespace Concentrator.Plugins.PFA.Transfer.Model.Mutation
{
  public class MutTotal : MutBase
  {
    public MutTotal(string line)
      : base(line)
    {
      int parsedNumberOfSKUs;

      if (!int.TryParse(line.SubstringNullOrTrim(30, 5), out parsedNumberOfSKUs))
        throw new Exception("Unable trying to parse number of skus");

      NumberOfSKUs = parsedNumberOfSKUs;
    }

    public Int32 NumberOfSKUs;
  }
}
