using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Objects.EDI.Order
{
  public class EdiOrderLedger
  {
    public Int32 EdiOrderLedgerID { get; set; }

    public Int32 EdiOrderLineID { get; set; }

    public Int32 Status { get; set; }

    public DateTime LedgerDate { get; set; }

    public virtual ICollection<EdiOrderLine> EdiOrderLine { get; set; }
  }
}
