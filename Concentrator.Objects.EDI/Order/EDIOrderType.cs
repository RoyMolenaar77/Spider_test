using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Objects.EDI.Order
{
  public class EdiOrderType
  {
    public Int32 EdiOrderTypeID { get; set; }

    public string Name { get; set; }

    public virtual ICollection<EdiOrder> EdiOrder { get; set; }
  }
}
