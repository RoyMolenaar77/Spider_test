using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Products;

namespace Concentrator.Objects.Import
{
  public class VendorImportData
  {
    public int VendorID { get; private set; }

    public Product Product { get; private set; }

    public VendorImportData(int vendorID, Product product)
    {
      VendorID = vendorID;
      Product = product;
    }
  }
}
