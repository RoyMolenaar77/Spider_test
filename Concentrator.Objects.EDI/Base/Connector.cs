using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.EDI.Order;

namespace Concentrator.Objects.EDI.Base
{
  public class Connector : Concentrator.Objects.Models.Connectors.Connector
  {
    public virtual ICollection<EdiOrder> EdiOrders { get; set; }
  }
}
