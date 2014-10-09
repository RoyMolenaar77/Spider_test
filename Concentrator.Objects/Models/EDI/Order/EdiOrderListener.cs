using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.EDI.Post;
using Concentrator.Objects.Models.Base;

namespace Concentrator.Objects.Models.EDI.Order
{
  public class EdiOrderListener : BaseModel<EdiOrderListener>
  {
    public Int32 EdiRequestID { get; set; }

    public string CustomerName { get; set; }

    public string CustomerIP { get; set; }

    public string CustomerHostName { get; set; }

    public string RequestDocument { get; set; }

    public DateTime ReceivedDate { get; set; }

    public bool Processed { get; set; }

    public string ResponseRemark { get; set; }

    public Int32 ResponseTime { get; set; }

    public string ErrorMessage { get; set; }

    public Int32 ConnectorID { get; set; }

    public virtual ICollection<EdiOrderPost> EdiOrderPosts { get; set; }

    public virtual Connectors.Connector Connector { get; set; }

    public virtual ICollection<EdiOrder> EdiOrders { get; set; }

    public override System.Linq.Expressions.Expression<Func<EdiOrderListener, bool>> GetFilter()
    {
      return null;
    }
  }
}
