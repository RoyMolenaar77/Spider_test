using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;

namespace Concentrator.Objects.Models.Products
{
  public class UnMappedProductView
  {
    public Int32 ProductID { get; set; }

    public Int32 ConnectorID { get; set; }

    public Int32 LanguageID { get; set; }

    public String CustomItemNumber { get; set; }

    public String ShortDescription { get; set; }

    public Int32 ProductGroupID { get; set; }

    public String Name { get; set; }

    public Int32 VendorID { get; set; }

    public String VendorName { get; set; }

    public String VendorProductGroupCode1 { get; set; }

    public String VendorProductGroupCode2 { get; set; }

    public String VendorProductGroupCode3 { get; set; }

    public String VendorProductGroupCode4 { get; set; }

    public String VendorProductGroupCode5 { get; set; }

    public String BrandCode { get; set; }

  }
}