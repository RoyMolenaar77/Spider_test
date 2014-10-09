using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models;
using Concentrator.Objects.EDI.Order;

namespace Concentrator.Objects.EDI.Base
{
  public class Customer : Concentrator.Objects.Models.Orders.Customer
  {
    public virtual ICollection<EdiOrder> ShippedEdiOrder { get; set; }

    public virtual ICollection<EdiOrder> SoldEdiOrder { get; set; }
  }
}
