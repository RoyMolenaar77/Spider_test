using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Models.Connectors;

namespace Concentrator.Objects.Models.Attributes
{
  public class ProductAttributeOption : BaseModel<ProductAttributeOption>
  {
    public int OptionID { get; set; }

    public int AttributeID { get; set; }

    public string AttributeOption { get; set; }
    
    public virtual ProductAttributeMetaData ProductAttributeMetaData { get; set; }

    public override System.Linq.Expressions.Expression<Func<ProductAttributeOption, bool>> GetFilter()
    {
      return null;
    }
  }
}
