using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Objects.Enumerations
{
  public enum BarcodeTypes
  {
    Default = 0,
    EAN = 1,
    UPC = 2,
    SAP = 3,
    /// <summary>
    /// PFA lookup for sizes
    /// </summary>
    PFA = 4
  }
}
