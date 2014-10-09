using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Concentrator.Web.Services.Models.Attributes
{
  public class ProductAttributeResult
  {
    public ProductAttributeResult()
    {
      AttributeValueGroups = new List<ProductAttributeGroupsResult>();
      ProductAttributeValues = new SortedList<int, ProductAttributeValuesResult>();
    }

    public string ManufacturerID { get; set; }

    public int ProductID { get; set; }

    public int BrandID { get; set; }

    public List<ProductAttributeGroupsResult> AttributeValueGroups { get; set; }

    public SortedList<int, ProductAttributeValuesResult> ProductAttributeValues { get; set; }
  }
}