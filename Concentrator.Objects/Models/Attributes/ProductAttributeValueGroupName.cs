using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Localization;
using Concentrator.Objects.Models.Base;

namespace Concentrator.Objects.Models.Attributes
{
  public class ProductAttributeValueGroupName : BaseModel<ProductAttributeValueGroupName>
  {
    public int AttributeValueGroupID { get; set; }

    public int LanguageID { get; set; }

    public string Name { get; set; }

    public virtual Language Language { get; set; }

    public virtual ProductAttributeValueGroup ProductAttributeValueGroup { get; set; }

    public override System.Linq.Expressions.Expression<Func<ProductAttributeValueGroupName, bool>> GetFilter()
    {
      return null;
    }
  }
}
