using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;

namespace Concentrator.Objects.Models.Products
{
  public class ProductGroupVendor_new
  {
    public Int32 ProductGroupVendorID { get; set; }
          
    public Int32 ProductGroupID { get; set; }
          
    public Int32 VendorID { get; set; }
          
    public String VendorName { get; set; }
          
    public String BrandCode { get; set; }
          
    public String VendorProductGroupCode1 { get; set; }
          
    public String VendorProductGroupCode2 { get; set; }
          
    public String VendorProductGroupCode3 { get; set; }
          
  }
}