using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FileHelpers;

namespace Concentrator.Plugins.Axapta.Models
{
  [DelimitedRecord(" ")]
  public class DatColCorruptPurchaseOrder
  {
    public string OriginalLine;
  }
}
