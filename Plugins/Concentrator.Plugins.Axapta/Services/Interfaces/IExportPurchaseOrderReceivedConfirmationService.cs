using Concentrator.Objects.Models.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Plugins.Axapta.Services
{
  public interface IExportPurchaseOrderReceivedConfirmationService
  {
    void ExportReceivedConfirmation(IEnumerable<OrderResponseLine> listOfOrderResponseLines);
  }
}
