using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Concentrator.Plugins.PfaCommunicator.Objects.Models
{
  public enum MessageTypes
  {
    SalesOrder = 1,
    StockMutation = 2,
    TransferOrder = 3,
    TransferOrderConfirmation = 4,
    StockPhoto = 5,
    WehkampReturn = 6
  }
}
