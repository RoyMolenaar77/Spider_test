using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Models.MastergroupMapping;

namespace Concentrator.Objects.Models.Contents
{
  [PetaPoco.TableName("ContentProductGroup")]
  [PetaPoco.PrimaryKey("ContentProductGroupID")]
  public class ContentProductGroup : AuditObjectBase<ContentProductGroup>
  {
    public ContentProductGroup()
    {
      IsExported = true;
    }

    public Int32 ContentProductGroupID { get; set; }

    public Int32 ConnectorID { get; set; }

    public Int32 ProductID { get; set; }

    public Int32 ProductGroupMappingID { get; set; }
    public Int32? MasterGroupMappingID { get; set; }

    public Boolean IsCustom { get; set; }

    public bool IsExported { get; set; }

    [PetaPoco.ResultColumn]
    public virtual Connector Connector { get; set; }
    [PetaPoco.ResultColumn]
    public virtual Products.Product Product { get; set; }
    [PetaPoco.ResultColumn]
    public virtual ProductGroupMapping ProductGroupMapping { get; set; }
    [PetaPoco.ResultColumn]
    public virtual Content Content { get; set; }

    public virtual MasterGroupMapping MasterGroupMapping { get; set; }

    public bool Exists { get; set; }

    public override System.Linq.Expressions.Expression<Func<ContentProductGroup, bool>> GetFilter()
    {
      return null;
    }
  }
}