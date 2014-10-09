using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;

namespace Concentrator.Objects.EDI.Order
{
  public class EdiCommunication : AuditObjectBase
  {
    public Int32 EdiCommunicationID { get; set; }

    public string Name { get; set; }

    public string Schedule { get; set; }

    public DateTime? LastRun { get; set; }

    public DateTime? NextRun { get; set; }

    public string Query { get; set; }

    public Int32 ConnectionType { get; set; }

    public string Connection { get; set; }

    public string Remark { get; set; }
  }
}
