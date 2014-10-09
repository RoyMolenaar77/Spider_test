using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MagentoImport
{
  public class AttributeResult
  {
    public int? AttributeGroupID { get; set; }
    public string AttributeGroupName { get; set; }
    public int? AttributeGroupIndex { get; set; }
    public int Product_ID { get; set; }
    public double Feature_ID { get; set; }
    public string AttributeName { get; set; }
    public string Value { get; set; }
    public string Sign { get; set; }
    public int? FeatureIndex { get; set; }
    public int? IsSearchable { get; set; }
    public string Presentation_value { get; set; }
  }
}
