using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Linq;
using Concentrator.Objects.Logic;
using Concentrator.Objects.Selector;

namespace Concentrator.Objects.Models.Complex
{
  public class Attributes
  {
    public int ProductID { get; set; }
    public string ManufacturerID { get; set; }
    public int BrandID { get; set; }
    public string ShortDescription { get; set; }
    public string LongDescription { get; set; }
    public string ContentDescription { get; set; }
    public string ModelName { get; set; }
    public string IntraStatCode { get; set; }
    public List<AttributeResult> AttributeList { get; set; }
  }

  public class SelectorAttributes
  {
    public int ProductID { get; set; }
    public List<SelectorProductAttributeView> AttributeList { get; set; }
  }

  public class AttributeGroup
  {
    public int AttributeGroupID { get; set; }
    public int AttributeGroupIndex { get; set; }
    public string AttributeGroupName { get; set; }

    public override bool Equals(object obj)
    {
      if (obj is AttributeGroup)
        return ((AttributeGroup)obj).AttributeGroupID.Equals(AttributeGroupID);

      return false;
    }

    public override int GetHashCode()
    {
      return AttributeGroupID.GetHashCode();
    }
  }
}
