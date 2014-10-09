using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;

namespace Concentrator.Objects.Models.Products
{
  public class ProductControl : BaseModel<ProductControl>
  {
    public Int32 ProductControlID { get; set; }
    public String ProductControlName { get; set; }
    public Boolean IsActive { get; set; }
    public String ProductControlDescription { get; set; }

    public override System.Linq.Expressions.Expression<Func<ProductControl, bool>> GetFilter()
    {
      return null;
    }
  }
}
