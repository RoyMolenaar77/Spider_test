using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Models.Connectors;

namespace Concentrator.Objects.Models.Attributes
{
  public class ProductAttributeValueGroup : BaseModel<ProductAttributeValueGroup>
  {
    public int AttributeValueGroupID { get; set; }

    public int Score { get; set; }

    public string ImagePath { get; set; }

    public int? ConnectorID { get; set; }

    public virtual Connector Connector { get; set; }

    public virtual ICollection<ProductAttributeValueGroupName> ProductAttributeValueGroupNames { get; set; }

    public virtual ICollection<ProductAttributeValueConnectorValueGroup> ProductAttributeValueConnectorValueGroups { get; set; }

    public override System.Linq.Expressions.Expression<Func<ProductAttributeValueGroup, bool>> GetFilter()
    {
      return null;
    }
  }
}
