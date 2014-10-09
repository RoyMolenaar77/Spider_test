using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Models.Users;

namespace Concentrator.Objects.Models.Connectors
{
    public class CrossLedgerclass : AuditObjectBase<CrossLedgerclass>
    {
        public Int32 ConnectorID { get; set; }

        public String LedgerclassCode { get; set; }

        public String CrossLedgerclassCode { get; set; }

        public String Description { get; set; }

        public virtual Connector Connector { get; set; }

        public virtual User User { get; set; }
        public virtual User User1 { get; set; }

        public override System.Linq.Expressions.Expression<Func<CrossLedgerclass, bool>> GetFilter()
        {
          return null;
        }
    }
}