using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Service;


namespace Concentrator.Plugins.ContentVendor
{
  public class UpdateIceCatImport : ConcentratorPlugin
  {
    public override string Name
    {
      get { return "IceCat Import Plugin (Update)"; }
    }

    protected override void Process()
    {
      IceCatImport import = new IceCatImport();
      import.ProcessContent(true, log);
    }
  }
}
