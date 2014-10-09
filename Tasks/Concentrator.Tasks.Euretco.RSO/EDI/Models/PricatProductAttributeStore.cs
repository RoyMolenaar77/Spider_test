using System;
using System.Diagnostics;

using Concentrator.Objects.Models.Attributes;
using Concentrator.Tasks.Stores;

namespace Concentrator.Tasks.Euretco.Rso.EDI.Models
{
  public class PricatProductAttributeStore : ProductAttributeStore
  {
    [ProductAttribute("Color")]
    public ProductAttributeMetaData ColorAttribute
    {
      get;
      set;
    }

    [ProductAttribute("Color_Code")]
    public ProductAttributeMetaData ColorCodeAttribute
    {
      get;
      set;
    }

    [ProductAttribute("Color_Supplier")]
    public ProductAttributeMetaData ColorSupplierAttribute
    {
      get;
      set;
    }

    [ProductAttribute("Product_Code")]
    public ProductAttributeMetaData ProductCodeAttribute
    {
      get;
      set;
    }

    [ProductAttribute("Size")]
    public ProductAttributeMetaData SizeAttribute
    {
      get;
      set;
    }

    [ProductAttribute("Size_Supplier")]
    public ProductAttributeMetaData SizeSupplierAttribute
    {
      get;
      set;
    }

    [ProductAttribute("Size_Ruler")]
    public ProductAttributeMetaData SizeRulerAttribute
    {
      get;
      set;
    }

    [ProductAttribute("Subsize")]
    public ProductAttributeMetaData SubsizeAttribute
    {
      get;
      set;
    }

    public PricatProductAttributeStore(TraceSource traceSource = null)
      : base(traceSource)
    {
    }
  }
}