using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;

namespace Concentrator.Objects.Models.Products
{
  public class ProductGroupSelector : BaseModel<ProductGroupSelector>
  {
    public Int32 SelectorID { get; set; }
          
    public Int32 ProductGroupID { get; set; }
          
    public Boolean? IsSearchable { get; set; }
          
    public Boolean? isAssortment { get; set; }
          
    public virtual ProductGroup ProductGroup { get;set;}
    
    public override System.Linq.Expressions.Expression<Func<ProductGroupSelector, bool>> GetFilter()
    {
      return null;
    }
  }
}