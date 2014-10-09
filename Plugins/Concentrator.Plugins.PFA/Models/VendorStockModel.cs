using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Plugins.PFA.Models
{
  public class VendorStockModel
  {
    public string VendorItemNumber { get; set; }

    public int QuantityOnHand { get; set; }

    public int StockLocation { get; set; }

    public int VendorID { get; set; }

    public int DefaultVendorID { get; set; }    

    public string StockStatus { get; set; }
  }
}
