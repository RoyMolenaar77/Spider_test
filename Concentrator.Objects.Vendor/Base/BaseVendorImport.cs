using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Service;
using Concentrator.Connectors.Vendors.VendorAssortments;
using System.Data;
using Concentrator.Objects;
using Concentrator.Objects.Vendors;

namespace Concentrator.Plugins.Base
{
  public abstract class BaseVendorImport : ConcentratorPlugin
  {
   
    public abstract override string Name
    {
      get;
    }

    protected abstract IProcessVendorContent ProcessVendorContent
    {
      get;
    }

    protected abstract void PopulateContent(Vendor vendor);
    
  }
}
