using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Import;

namespace Concentrator.Objects.ConcentratorService
{
  public abstract class ImportPlugin : ConcentratorPlugin
  {
    public abstract override string Name { get; }

    public abstract VendorImportData GetImportData();

    protected override void Process()
    {
      throw new NotImplementedException();
    }
  }
}
