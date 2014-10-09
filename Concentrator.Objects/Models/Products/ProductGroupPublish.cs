using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Models.Connectors;

namespace Concentrator.Objects.Models.Products
{
  public class ProductGroupPublish : AuditObjectBase<ProductGroupPublish>
  {
    public Int32 ProductGroupID { get; set; }
          
    public Boolean Published { get; set; }
          
    public Int32 ConnectorID { get; set; }
          
    public virtual Connector Connector { get;set;}
            
    public virtual ProductGroup ProductGroup { get;set;}


    public override System.Linq.Expressions.Expression<Func<ProductGroupPublish, bool>> GetFilter()
    {
      return null;
    }
  }
}