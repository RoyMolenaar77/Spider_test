using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Models.Localization;
using Concentrator.Objects.Models.Users;

namespace Concentrator.Objects.Models.Attributes
{
  public class ProductAttributeValueLabel : BaseModel<ProductAttributeValueLabel>
  {

    public int LanguageID { get; set; }

    public string Value { get; set; }

    public int AttributeID { get; set; }

    public string Label { get; set; }

    public int? ConnectorID { get; set; }

    public int OrganizationID { get; set; }

    public int Id { get; set; }

    public virtual Organization Organization { get; set; }

    public virtual Language Language { get; set; }

    public virtual Connectors.Connector Connector { get; set; }

    public virtual ProductAttributeMetaData ProductAttributeMetaData { get; set; }

    public override System.Linq.Expressions.Expression<Func<ProductAttributeValueLabel, bool>> GetFilter()
    {
      return null;
    }
  }
}
