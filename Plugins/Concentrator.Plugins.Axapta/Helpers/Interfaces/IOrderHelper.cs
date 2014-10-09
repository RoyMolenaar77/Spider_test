using System.Collections.Generic;
using System.IO;
using Concentrator.Plugins.Axapta.Models;

namespace Concentrator.Plugins.Axapta.Helpers
{
  public interface IOrderHelper
  {
    bool IsValidTransferOrder(IEnumerable<DatColPickTicket> listOfTransferOrderLines, int connectorID, IEnumerable<int> sapphVendorIDs, out List<DatColErrorMessage> listOfErrors, bool returnErrorLog = false);
    bool IsValidSalesOrder(IEnumerable<DatColPickTicket> listOfSalesOrderLines, int connectorID, IEnumerable<int> sapphVendorIDs, out List<DatColErrorMessage> listOfErrors, bool returnErrorLog = false);
    bool IsSkuExist(IEnumerable<string> listOfSkus, IEnumerable<int> vendorIDs, out List<string> listOfUnknownSkus, bool returnUnknownList = false);
    bool ConvertOriginalOrderNumberToConcentratorOrderNumber(string originalOrderNumber, ICollection<string> listOfWeborderNumbers, int maxIncrementNumber,
                                                             out string webOrderNumber);

    bool IsValidPurchaseOrder(DatColPurchaseOrder[] listOfTransferOrderLines, int sapphConnectorID, IEnumerable<int> sapphVendorIDs, out List<DatColErrorMessage> listOfErrors, bool returnErrorLog = false);
  }
}
