using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Web;

namespace Concentrator.Objects.Models.Vendors
{
  public class VendorFreeGood : BaseModel<VendorFreeGood>
  {
    public Int32 VendorAssortmentID { get; set; }
          
    public Int32 ProductID { get; set; }
          
    public Int32 MinimumQuantity { get; set; }
          
    public Int32 OverOrderedQuantity { get; set; }
          
    public Int32 FreeGoodQuantity { get; set; }
          
    public Decimal UnitPrice { get; set; }
          
    public String Description { get; set; }
          
    public virtual Products.Product Product { get;set;}
            
    public virtual VendorAssortment VendorAssortment { get;set;}


    public override System.Linq.Expressions.Expression<Func<VendorFreeGood, bool>> GetFilter()
    {
      return (v => Client.User.VendorIDs.Contains(v.VendorAssortment.VendorID));

    }
  }
}