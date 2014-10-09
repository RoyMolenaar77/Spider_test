using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Web;

namespace Concentrator.Objects.Models.Products
{
  public class RelatedProduct : AuditObjectBase<RelatedProduct>
  {
    public Int32 ProductID { get; set; }

    public Int32 RelatedProductID { get; set; }

    public Boolean? Preferred { get; set; }

    public Boolean? Reversed { get; set; }

    public Int32 VendorID { get; set; }

    public Int32 RelatedProductTypeID { get; set; }

    public bool IsConfigured { get; set; }

    public bool IsActive { get; set; }

    public Int32 Index { get; set; }

    public bool MarkedForDeletion { get; set; }

    public virtual Product SourceProduct { get; set; }

    public virtual Product RProduct { get; set; }

    public virtual RelatedProductType RelatedProductType { get; set; }

    public virtual Vendor Vendor { get; set; }

    public virtual User User { get; set; }

    public virtual User User1 { get; set; }

    public override System.Linq.Expressions.Expression<Func<RelatedProduct, bool>> GetFilter()
    {
      return (p => Client.User.VendorIDs.Contains(p.VendorID));
    }


    public bool PP_IsConfigurable { get; set; }
    public Int32? PP_ParentProductID { get; set; }
  }
}