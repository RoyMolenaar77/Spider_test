using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.Models.Products;

namespace Concentrator.Objects.Models.Scan
{
  public class ScanData : BaseModel<ScanData>
  {
    public Int32 ProductGroupMappingID { get; set; }

    public String ScanTime { get; set; }

    public Int32 ConnectorID { get; set; }

    public String ScanDisplay { get; set; }

    public virtual Connector Connector { get; set; }

    public virtual ProductGroupMapping ProductGroupMapping { get; set; }


    public override System.Linq.Expressions.Expression<Func<ScanData, bool>> GetFilter()
    {
      return null;
    }
  }
}