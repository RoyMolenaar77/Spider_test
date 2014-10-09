using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Plugins.DhlTool
{
  public class DhlStatus
  {
    public string shipmentNumber { get; set; }
    public string ggb { get; set; }
    public bool success { get; set; }
  }
}
