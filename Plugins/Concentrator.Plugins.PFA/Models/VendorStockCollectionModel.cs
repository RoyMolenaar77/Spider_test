using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Plugins.PFA.Models
{
  public class VendorStockCollectionModel
  {
    public int VendorID { get; set; }

    public int DefaultVendorID { get; set; }

    public List<Concentrator.Objects.Vendors.Bulk.VendorAssortmentBulk.VendorImportStock> StockCollection { get; set; }
  }
}
