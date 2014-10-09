using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Models.Contents;

namespace Concentrator.Objects.Models.Vendors
{
  public class VendorStockType : BaseModel<VendorStockType>
  {
    public Int32 VendorStockTypeID { get; set; }
          
    public String StockType { get; set; }
          
    public virtual ICollection<VendorStock> VendorStocks { get;set;}

    public virtual ICollection<ContentStock> ContentStocks { get; set; }

    public override System.Linq.Expressions.Expression<Func<VendorStockType, bool>> GetFilter()
    {
      return null;
    }
  }
}