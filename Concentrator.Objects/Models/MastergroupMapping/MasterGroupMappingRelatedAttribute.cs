using Concentrator.Objects.Models.Attributes;
using Concentrator.Objects.Models.Base;
using System;
using System.Collections.Generic;

namespace Concentrator.Objects.Models.MastergroupMapping
{
  public class MasterGroupMappingRelatedAttribute: BaseModel<MasterGroupMappingRelatedAttribute>
  {
    public Int32 RelatedAttributeID { get; set; }
    public Int32 MasterGroupMappingID { get; set; }
    public Int32? ParentID { get; set; }
    public Int32 AttributeID { get; set; }
    public String AttributeValue { get; set; }

    public virtual MasterGroupMapping MasterGroupMapping { get; set; }
    public virtual ICollection<MasterGroupMappingRelatedAttribute> CrossReferenceRelatedAttribute { get; set; }
    public virtual MasterGroupMappingRelatedAttribute ParentRelatedAttribute { get; set; }
    public virtual ProductAttributeMetaData ProductAttributeMetaData { get; set; }

    public override System.Linq.Expressions.Expression<Func<MasterGroupMappingRelatedAttribute, bool>> GetFilter()
    {
      return null;
    }
  }
}
