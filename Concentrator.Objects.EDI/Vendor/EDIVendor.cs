using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.EDI.Validation;

namespace Concentrator.Objects.EDI.Vendor
{
  public class EdiVendor
  {
    public Int32 EdiVendorID { get; set; }

    public string Name { get; set; }

    public string EdiVendorType { get; set; }

    public virtual ICollection<EdiValidate> EdiValidations { get; set; }
  }
}
