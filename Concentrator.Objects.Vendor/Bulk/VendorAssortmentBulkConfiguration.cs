using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Objects.Vendors.Bulk
{
  public class VendorAssortmentBulkConfiguration
  {
    public bool IsPartialAssortment { set; get; }
    public bool IncludeBrandMapping { set; get; }
    public bool ProcessProductBarcode { set; get; }

    public VendorAssortmentBulkConfiguration()
    {
      //set defaults
      IsPartialAssortment = false;
      IncludeBrandMapping = false;
      ProcessProductBarcode = true;
    }
  }
}