using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Objects.ProductAttribute
{
  public class AttributeImportTemplate
  {
    public string VendorItemNumber { get; set; }
    public string ProductName { get; set; }
    public string Attributegroup { get; set; }
    public string Attributename { get; set; }
    public string Value { get; set; }
    public string Isvisible { get; set; }
    public string Ismandatory { get; set; }
    public string DefaultValue { get; set; }
    public string AttributeIndex { get; set; }
  }

  public class DescriptionImportTemplate
  {
    public string VendorItemNumber { get; set; }
    public string ProductName { get; set; }
    public string ShortContentDescription { get; set; }
    public string LongContentDescription { get; set; }
    public string ShortSummaryDescription { get; set; }
    public string LongSummaryDescription { get; set; }
    public string WarrantyInfo { get; set; }
    public string ModelName { get; set; }
    public string Language { get; set; }
  }
}
