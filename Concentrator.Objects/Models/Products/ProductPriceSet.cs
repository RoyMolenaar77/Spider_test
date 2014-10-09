using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Web;

namespace Concentrator.Objects.Models.Products
{
  public class ProductPriceSet : BaseModel<ProductPriceSet>
  {
    public Int32 PriceSetID { get; set; }

    public Int32 ProductID { get; set; }

    public Int32 Quantity { get; set; }

    public virtual Product Product { get; set; }

    public virtual PriceSet PriceSet { get; set; }

    public override System.Linq.Expressions.Expression<Func<ProductPriceSet, bool>> GetFilter()
    {
      //return (c => Client.User.VendorIDs.Contains(c.Product.SourceVendorID));
      return (p => p.Product.VendorAssortments.Any(c => Client.User.VendorIDs.Contains(c.VendorID)));
    }
  }
}
