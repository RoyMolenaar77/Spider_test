using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.Web;

namespace Concentrator.Objects.Models.Products
{
  public class ProductImage : BaseModel<ProductImage>
  {
    public Int32 ImageID { get; set; }
          
    public Int32 ProductID { get; set; }
          
    public Int32 Sequence { get; set; }
          
    public Int32 VendorID { get; set; }
          
    public String ImageUrl { get; set; }
          
    public String ImagePath { get; set; }
          
    public virtual Product Product { get;set;}
            
    public virtual Vendor Vendor { get;set;}


    public override System.Linq.Expressions.Expression<Func<ProductImage, bool>> GetFilter()
    {
      return (p => Client.User.VendorIDs.Contains(p.VendorID));

    }
  }
}