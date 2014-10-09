using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;

namespace Concentrator.Objects.Models.Attributes
{
  public class ProductAttributeMatch : AuditObjectBase<ProductAttributeMatch>
  {
    public Int32 ProductAttributeMatchID { get; set; }
          
    public Int32 ProductAttributeGroupID { get; set; }
          
    public Int32 AttributeID { get; set; }
          
    public Int32 CorrespondingProductAttributeGroupID { get; set; }
          
    public Int32 CorrespondingAttributeID { get; set; }
          
    public Boolean IsMatched { get; set; }
          
    public Int32 ConnectorID { get; set; }


    public override System.Linq.Expressions.Expression<Func<ProductAttributeMatch, bool>> GetFilter()
    {
      return null;
    }
  }
}