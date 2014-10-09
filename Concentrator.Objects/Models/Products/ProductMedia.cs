using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Models.Media;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.Web;

namespace Concentrator.Objects.Models.Products
{
  public class ProductMedia : BaseModel<ProductMedia>
  {
    public Int32 ProductID { get; set; }

    public Int32 Sequence { get; set; }

    public Int32 VendorID { get; set; }

    public Int32 TypeID { get; set; }

    public String MediaUrl { get; set; }

    public String MediaPath { get; set; }

    public String FileName { get; set; }

    public Int32 MediaID { get; set; }

    public String Description { get; set; }

    public String Resolution { get; set; }

    public Int32? Size { get; set; }

    public bool IsThumbNailImage { get; set; }

    public virtual MediaType MediaType { get; set; }

    public virtual Product Product { get; set; }

    public virtual Vendor Vendor { get; set; }

    public virtual ICollection<ProductMediaTumbnail> ProductMediaTumbnails { get; set; }

    public DateTime LastChanged { get; set; }  
    
    public override System.Linq.Expressions.Expression<Func<ProductMedia, bool>> GetFilter()
    {
      return (p => Client.User.VendorIDs.Contains(p.VendorID));
    }
  }
}