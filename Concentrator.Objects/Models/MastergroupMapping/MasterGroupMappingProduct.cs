using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.Models.Products;
using System;

namespace Concentrator.Objects.Models.MastergroupMapping
{
  public class MasterGroupMappingProduct : BaseModel<MasterGroupMappingProduct>
  {

    public Int32 MasterGroupMappingID { get; set; }

    public Int32 ProductID { get; set; }

    public Boolean IsApproved { get; set; }

    public Boolean IsCustom { get; set; }

    public Boolean IsProductMapped { get; set; }

    public Int32? ConnectorPublicationRuleID { get; set; }

    public virtual MasterGroupMapping MasterGroupMapping { get; set; }

    public virtual Product Product { get; set; }

    public virtual ConnectorPublicationRule ConnectorPublicationRule { get; set; }

    public override System.Linq.Expressions.Expression<Func<MasterGroupMappingProduct, bool>> GetFilter()
    {
      return null;
    }
  }
}
