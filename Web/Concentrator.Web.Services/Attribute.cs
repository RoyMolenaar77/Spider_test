using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Spider.Web.Service
{
  public class Attributes
  {
    public int ProductID { get; set; }
    public string ManufactureID { get; set; }
    public int BrandID { get; set; }
    public int ProductGroupID { get; set; }
    public int ParentProductGroupID { get; set; }
    public List<Attribute> AttributeList { get; set; }
  }

  public class Attribute
  {
    public int AttributeID { get; set; }
    public string AttributeName { get; set; }
    public string AttributeValue { get; set; }
    public string Sign { get; set; }
    public string OrderIndex { get; set; }
    public int? KeyFeature { get; set; }
    public int? IsSearcheble { get; set; }
    public int GroupID { get; set; }
    public string GroupName { get; set; }
    public int GroupIndex { get; set; }
    public bool NeedsUpdate { get; set; }
    public string AttributeCode { get; set; }
  }

  public class AttributeGroup
  {
    public int AttributeGroupID { get; set; }
    public int AttributeGroupIndex { get; set; }
    public string AttributeGroupName { get; set; }
  }
}
