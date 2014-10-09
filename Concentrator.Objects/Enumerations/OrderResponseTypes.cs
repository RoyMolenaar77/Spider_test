using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator
{
  public enum OrderResponseTypes
  {
    ReceivedNotification = 90,
    Acknowledgement = 100,
    CancelNotification = 110,
    ShipmentNotification = 200,
    InvoiceNotification = 300,
    PurchaseAcknowledgement = 400,
    Return = 500,
    Unknown = 999
  }
}
