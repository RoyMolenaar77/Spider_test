using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Objects.Models.EDI.Enumerations
{
  public enum EdiOrderStatus
  {
    Received = 0,
    Validate = 100,
    ProcessOrder = 200,
    WaitForOrderResponse = 300,
    WaitForAcknowledgement = 310,
    ReceiveAcknowledgement = 315,
    WaitForShipmentNotification = 320,
    ReceiveShipmentNotificaiton = 325,
    WaitForInvoiceNotification = 330,
    ReceivedInvoiceNotification = 335,
    OrderComplete = 500,
    Error = 999
  }
}
