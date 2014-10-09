using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.Web;

namespace Concentrator.Objects.Models.Attributes
{
  public class ProductAttributeGroupMetaData : BaseModel<ProductAttributeGroupMetaData>
  {
    public Int32 ProductAttributeGroupID { get; set; }

    public Int32? ConnectorID { get; set; }

    public Int32 Index { get; set; }

    public String GroupCode { get; set; }

    public Int32 VendorID { get; set; }

    public Int32? SourceGroupID { get; set; }

    public virtual Connector Connector { get; set; }

    public virtual ICollection<ProductAttributeMetaData> ProductAttributeMetaDatas { get; set; }

    public virtual ICollection<ProductAttributeGroupName> ProductAttributeGroupNames { get; set; }

    public virtual Vendor Vendor { get; set; }

    public override System.Linq.Expressions.Expression<Func<ProductAttributeGroupMetaData, bool>> GetFilter()
    {
      return (p => Client.User.VendorIDs.Contains(p.VendorID));
    }
  }
}