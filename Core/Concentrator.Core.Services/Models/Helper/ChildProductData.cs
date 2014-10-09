using System;

namespace Concentrator.Core.Services.Models.Helper
{
  internal class ChildProductData
  {
    public string VendorItemNumber { get; set; }
    public string CustomItemNumber { get; set; }
    public int ProductID { get; set; }
    public int ParentProductID { get; set; }
    public bool IsConfigurable { get; set; }
    public DateTime LastModificationTime { get; set; }
    public bool IsNonAssortmentItem { get; set; }
  }
}
