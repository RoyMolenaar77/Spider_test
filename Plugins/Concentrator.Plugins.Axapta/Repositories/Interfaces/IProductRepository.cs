using Concentrator.Objects.Models.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Plugins.Axapta.Models;

namespace Concentrator.Plugins.Axapta.Repositories
{
  public interface IProductRepository
  {
    Product GetProductByID(int productID);
    Product GetProductByVendorItemNumber(string vendorItemNumber);
    Int32 GetProductIDByVendorItemNumber(string vendorItemNumber);

    Product GetProductByVendorItemNumberAndBarcode(string vendorItemNumber, string barcode);
    Int32 GetProductIDByVendorItemNumberAndBarcode(string vendorItemNumber, string barcode);

    IEnumerable<SkuModel> GetListOfSkusByVendorID(int vendorID);
    IEnumerable<SkuModel> GetListOfSkusByVendorIDs(IEnumerable<int> vendorIDs);
  }
}
