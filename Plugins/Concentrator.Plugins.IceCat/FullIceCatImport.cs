using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using Concentrator.Objects.Service;

namespace Concentrator.Plugins.ContentVendor
{
  public class FullIceCatImport : ConcentratorPlugin
  {
    public override string Name
    {
      get { return "IceCat Import Plugin (Full)"; }
    }

    protected override void Process()
    {
      IceCatImport import = new IceCatImport();
      import.ProcessContent(false, log);
    }
  }
}
