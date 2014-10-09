using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Objects.EDI.Enumerations
{
  public enum EdiOrderStatus
  {
    Received = 0,
    Validate = 100,
    ProcessOrder = 200,
    WaitForOrderResponse = 300,
    OrderComplete = 500
  }
}
