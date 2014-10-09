using System;

namespace Concentrator.Core.Services.Models.Helper
{
  internal class ProductData
  {
    public int RowNum { get; set; }
    public string VendorItemNumber { get; set; }
    public string CustomItemNumber { get; set; }
    public int ProductID { get; set; }
    public bool IsConfigurable { get; set; }
    public DateTime LastModificationTime { get; set; }
    public bool IsNonAssortmentItem { get; set; }
  }
}
