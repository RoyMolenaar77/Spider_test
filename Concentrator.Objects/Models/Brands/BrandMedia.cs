using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.Models.Contents;
using Concentrator.Objects.Models.Media;
using Concentrator.Objects.Web;
using Concentrator.Objects.Models.Vendors;

namespace Concentrator.Objects.Models.Brands
{
  public class BrandMedia : BaseModel<BrandMedia>
  {
    public Int32 BrandID { get; set; }

    public Int32 Sequence { get; set; }

    public Int32 TypeID { get; set; }

    public Int32? VendorID { get; set; }

    public String MediaUrl { get; set; }

    public String MediaPath { get; set; }

    public Int32 MediaID { get; set; }

    public String Resolution { get; set; }

    public Int32? Size { get; set; }    

    public String Description { get; set; }

    public virtual Brand Brand { get; set; }

    public virtual MediaType MediaType { get; set; }

        public virtual Vendor Vendor { get; set; }

    
    public override System.Linq.Expressions.Expression<Func<BrandMedia, bool>> GetFilter()
    {
      return null; 
    }
  }
}