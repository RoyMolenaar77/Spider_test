using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Concentrator.Web.Services.Models.Attributes
{
  public class ProductAttributeValuesResult
  {
    public ProductAttributeValuesResult()
    {
      AttributeValueGroupIDs = new List<int>();

    }

    public bool IsConfigurable { get; set; }

    public int AttributeID { get; set; }

    public int AttributeValueID { get; set; }

    public string OriginalValue { get; set; }

    public string AttributeCode { get; set; }

    public bool KeyFeature { get; set; }

    public int Index { get; set; }

    public bool IsSearchable { get; set; }

    public int AttributeGroupID { get; set; }

    public string AttributeGroupName { get; set; }

    public int AttributeGroupIndex { get; set; }

    public bool NeedsUpdate { get; set; }

    public string Name { get; set; }

    public string Value { get; set; }

    public string Sign { get; set; }

    public string AttributePath { get; set; }

    public List<int> AttributeValueGroupIDs { get; set; }
  }
}