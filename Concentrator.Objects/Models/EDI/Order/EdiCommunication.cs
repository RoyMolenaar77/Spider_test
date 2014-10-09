using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Models.EDI.Mapping;

namespace Concentrator.Objects.Models.EDI.Order
{
  public class EdiCommunication : AuditObjectBase<EdiCommunication>
  {
    public Int32 EdiCommunicationID { get; set; }

    public string Name { get; set; }

    public string Schedule { get; set; }

    public DateTime? LastRun { get; set; }

    public DateTime? NextRun { get; set; }

    public string Query { get; set; }

    public Int32 EdiConnectionType { get; set; }

    public string Connection { get; set; }

    public string Remark { get; set; }

    public string ResponseType { get; set; }

    public virtual ICollection<EdiFieldMapping> EdiFieldMappings { get; set; }

    public override System.Linq.Expressions.Expression<Func<EdiCommunication, bool>> GetFilter()
    {
      return null;
    }
  }
}
