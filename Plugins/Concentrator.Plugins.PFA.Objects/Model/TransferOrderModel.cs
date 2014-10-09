using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Plugins.PFA.Objects.Model
{
  public class TransferOrderModel
  {
    public string ArtikelNumber { get; set; }

    public string ColorCode { get; set; }

    public string SizeCode { get; set; }

    public int Shipped { get; set; }
  }
}
