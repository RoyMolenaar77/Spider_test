using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.Models.Base;

namespace Concentrator.Objects.Models.Attributes
{
  public class ProductAttributeValueConnectorValueGroup : BaseModel<ProductAttributeValueConnectorValueGroup>
  {
    public int AttributeID { get; set; }

    public int? ConnectorID { get; set; }

    public int AttributeValueGroupID { get; set; }

    public string Value { get; set; }

    public virtual Connector Connector { get; set; }

    public virtual ProductAttributeMetaData ProductAttributeMetaData { get; set; }

    public virtual ProductAttributeValueGroup ProductAttributeValueGroup { get; set; }

    public override System.Linq.Expressions.Expression<Func<ProductAttributeValueConnectorValueGroup, bool>> GetFilter()
    {
      return null;
    }
  }
}
