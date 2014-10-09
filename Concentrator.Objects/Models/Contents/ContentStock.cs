using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Models.Brands;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.Models.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Objects.Models.Contents
{
  public class ContentStock : AuditObjectBase<ContentStock>
  {
    public int VendorID { get; set; }
    public int ConnectorID { get; set; }
    public int VendorStockTypeID { get; set; }

    public virtual Vendor Vendor { get; set; }
    public virtual Connector Connector { get; set; }
    public virtual VendorStockType VendorStockType { get; set; }

    public virtual User User { get; set; }
    public virtual User User1 { get; set; }

    public override System.Linq.Expressions.Expression<Func<ContentStock, bool>> GetFilter()
    {
      return null;
    }
  }
}