using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Models.Brands;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.Web;

namespace Concentrator.Objects.Models.Contents
{
  public class ContentProduct : AuditObjectBase<ContentProduct>
  {
    public Int32 ProductContentID { get; set; }

    public Int32 ConnectorID { get; set; }

    public Int32 VendorID { get; set; }

    public Int32? ProductGroupID { get; set; }

    public Int32? BrandID { get; set; }

    public Int32? ProductID { get; set; }

    public Int32 ProductContentIndex { get; set; }

    public Boolean IsAssortment { get; set; }

    public virtual Brand Brand { get; set; }

    public virtual Connector Connector { get; set; }

    public virtual ICollection<Content> Contents { get; set; }

    public virtual Products.Product Product { get; set; }

    public virtual ProductGroup ProductGroup { get; set; }

    public virtual Vendor Vendor { get; set; }

    public virtual ICollection<ContentPublicationRule> ContentPublicationRules { get; set; }

    public override System.Linq.Expressions.Expression<Func<ContentProduct, bool>> GetFilter()
    {
      return(c=> Client.User.VendorIDs.Contains(c.VendorID));
    }


  }
}