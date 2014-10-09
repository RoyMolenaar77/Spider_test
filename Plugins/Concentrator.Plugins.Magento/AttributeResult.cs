using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Plugins.Magento
{
  public class AttributeResult
  {
    #region Props
    public int? AttributeGroupID { get; set; }
    public string AttributeGroupName { get; set; }
    public int? AttributeGroupIndex { get; set; }
    public int Product_ID { get; set; }
    public double Feature_ID { get; set; }
    public string AttributeName { get; set; }
    public string Value { get; set; }
    public string Sign { get; set; }
    public int? FeatureIndex { get; set; }
    public bool IsSearchable { get; set; }
    public string Presentation_value { get; set; }
    public bool KeyFeature { get; set; }
    public bool NeedsUpdate { get; set; }
    public string AttributeCode { get; set; }
    public string ProductGroupName { get; set; }
    #endregion
  }
}
