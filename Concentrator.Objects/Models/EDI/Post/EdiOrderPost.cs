using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.EDI.Order;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.Models.Orders;
using Concentrator.Objects.Models.Base;

namespace Concentrator.Objects.Models.EDI.Post
{
  public class EdiOrderPost : BaseModel<EdiOrderPost>
  {
    public Int32 EdiOrderPostID { get; set; }

    public Int32? EdiOrderID { get; set; }

    public Int32 ConnectorRelationID { get; set; }

    public string EdiBackendOrderID { get; set; }

    public string CustomerOrderID { get; set; }

    public bool Processed { get; set; }

    public string Type { get; set; }

    public string PostDocument { get; set; }

    public string PostDocumentUrl { get; set; }

    public string PostUrl { get; set; }

    public DateTime Timestamp { get; set; }

    public string ResponseRemark { get; set; }

    public Int32 ResponseTime { get; set; }

    public Int32? ProcessedCount { get; set; }

    public Int32? EdiRequestID { get; set; }

    public string ErrorMessage { get; set; }

    public Int32 BSKIdentifier { get; set; }

    public Int32 DocumentCounter { get; set; }

    public Int32? ConnectorID { get; set; }

    public virtual EdiOrder EdiOrder { get; set; }

    public virtual Connector Connector { get; set; }

    public virtual ConnectorRelation ConnectorRelation { get; set; }

    public virtual EdiOrderListener EdiOrderListener { get; set; }

    public override System.Linq.Expressions.Expression<Func<EdiOrderPost, bool>> GetFilter()
    {
      return null;
    }
  }
}
