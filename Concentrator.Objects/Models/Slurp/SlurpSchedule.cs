using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Models.Scan;

namespace Concentrator.Objects.Models.Slurp
{
  public class SlurpSchedule : BaseModel<SlurpSchedule>
  {
    public Int32 SlurpScheduleID { get; set; }

    public Int32 ProductCompareSourceID { get; set; }

    public Int32? ProductGroupMappingID { get; set; }

    public Int32? ProductID { get; set; }

    public Int32 Interval { get; set; }

    public Int32 IntervalType { get; set; }

    public virtual Products.Product Product { get; set; }

    public virtual ProductCompareSource ProductCompareSource { get; set; }

    public virtual ProductGroupMapping ProductGroupMapping { get; set; }

    //public virtual SlurpQueue SlurpQueues { get; set; }
    public virtual ICollection<SlurpQueue> SlurpQueues { get; set; }

    public override System.Linq.Expressions.Expression<Func<SlurpSchedule, bool>> GetFilter()
    {
      return null;
    }
  }
}