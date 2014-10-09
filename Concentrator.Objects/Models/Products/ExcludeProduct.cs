using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Models.Connectors;

namespace Concentrator.Objects.Models.Products
{
  public class ExcludeProduct : AuditObjectBase<ExcludeProduct>
  {
    public int ExcludeProductID { get; set; }

    public int ConnectorID { get; set; }

    public string Value { get; set; }

    public virtual Connector Connector { get; set; }

    public override System.Linq.Expressions.Expression<Func<ExcludeProduct, bool>> GetFilter()
    {
      return null;
    }
  }
}