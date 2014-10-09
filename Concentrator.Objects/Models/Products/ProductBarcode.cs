using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.Web;

namespace Concentrator.Objects.Models.Products
{
  public class ProductBarcode : BaseModel<ProductBarcode>
  {
    public Int32 ProductID { get; set; }
          
    public String Barcode { get; set; }
          
    public Int32? VendorID { get; set; }
          
    public Int32? BarcodeType { get; set; }
          
    public virtual Product Product { get;set;}
            
    public virtual Vendor Vendor { get;set;}


    public override System.Linq.Expressions.Expression<Func<ProductBarcode, bool>> GetFilter()
    {
      if (VendorID.HasValue)
        return (pb => Client.User.VendorIDs.Contains((int)pb.VendorID));
      else
        return null;
    }
  }
}