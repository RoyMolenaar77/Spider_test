using System;
using System.Collections.Generic;

namespace Concentrator.Core.Services.Models.Products
{
  [System.Xml.Serialization.XmlInclude(typeof(ConfigurableProduct))]
  [System.Xml.Serialization.XmlInclude(typeof(SimpleProduct))]
  public abstract class ProductBase
  {
    public string VendorItemNumber { get; set; }

    public string CustomItemNumber { get; set; }

    public int ProductID { get; set; }

    public bool IsNonAssortmentItem { get; set; }

    public DateTime LastModificationTime { get; set; }




    public abstract ProductTypes Type { get; }

    public List<Attribute> Attributes { get; set; }

    public List<RelatedProduct> RelatedProduct { get; set; }

    public List<Media.Media> Media { get; set; }

    //image references

    //category references
  }

  public enum ProductTypes
  {
    Simple = 0,
    Configurable = 1
  }
}
