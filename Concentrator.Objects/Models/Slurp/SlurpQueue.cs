using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Models.Products;

namespace Concentrator.Objects.Models.Slurp
{
  public class SlurpQueue : BaseModel<SlurpQueue>
  {
    public Int32 QueueID { get; set; }

    public Int32 ProductID { get; set; }

    public Int32 ProductCompareSourceID { get; set; }

    public Int32? SlurpScheduleID { get; set; }

    public DateTime? CompletionTime { get; set; }

    public Boolean IsCompleted { get; set; }

    public DateTime? StartTime { get; set; }

    public virtual Products.Product Product { get; set; }

    public virtual ProductCompareSource ProductCompareSource { get; set; }

    public DateTime? CreationTime { get; set; }

    public virtual SlurpSchedule SlurpSchedule { get; set; }

    public override System.Linq.Expressions.Expression<Func<SlurpQueue, bool>> GetFilter()
    {
      return null;
    }
  }
}