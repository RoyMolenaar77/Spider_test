using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.Models.Contents;
using Concentrator.Objects.Models.Vendors;

namespace Concentrator.Objects.Models.Products
{
  public class ContentProductMatch : BaseModel<ContentProductMatch>
  {
    public Int32 StoreID { get; set; }

    public Int32 ProductID { get; set; }

    public Int32 Index { get; set; }

    public String Description { get; set; }

    public bool IsLeading { get; set; }

    public virtual Product Product { get; set; }

    public override System.Linq.Expressions.Expression<Func<ContentProductMatch, bool>> GetFilter()
    {
      return null;
    }
  }
}