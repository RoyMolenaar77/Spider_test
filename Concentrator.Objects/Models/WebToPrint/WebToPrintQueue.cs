using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;

namespace Concentrator.Objects.Models.WebToPrint
{
  public enum WebToPrintQueueStatus
  {
    Queued = 0,
    Processing = 1,
    Done = 2,
    Deleted = 3,
    Error = 4
  }

  public class WebToPrintQueue :BaseModel<WebToPrintQueue>
  {
    public int QueueID { get; set; }
    public int ProjectID { get; set; }
    public int Status { get; set; }
    public string Data { get; set; }
    public string Message { get; set; }

    public virtual WebToPrintProject WebToPrintProject { get; set; }


    public override System.Linq.Expressions.Expression<Func<WebToPrintQueue, bool>> GetFilter()
    {
      return null;
    }
  }
}
  