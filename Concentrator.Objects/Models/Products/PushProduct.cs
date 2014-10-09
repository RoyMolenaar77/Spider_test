using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;

namespace Concentrator.Objects.Models.Products
{
  public class PushProduct : AuditObjectBase<PushProduct>
  {
    public Int32 PushProductID { get; set; }

    public Int32? ProductID { get; set; }

    public String CustomItemNumber { get; set; }

    public Int32 VendorID { get; set; }

    public Int32 ConnectorID { get; set; }

    public Boolean Processed { get; set; }

    public DateTime? LastPushDate { get; set; }


    public override System.Linq.Expressions.Expression<Func<PushProduct, bool>> GetFilter()
    {
      return null;
    }
  }
}