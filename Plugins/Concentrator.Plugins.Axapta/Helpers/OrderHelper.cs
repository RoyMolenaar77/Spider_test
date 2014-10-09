using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Concentrator.Plugins.Axapta.Enum;
using Concentrator.Plugins.Axapta.Models;
using Concentrator.Plugins.Axapta.Repositories;
using FileHelpers;

namespace Concentrator.Plugins.Axapta.Helpers
{
  public class OrderHelper : IOrderHelper
  {
    private readonly IOrderRepository _orderRepo;
    private readonly IProductRepository _productRepo;

    public OrderHelper(IOrderRepository orderRepo, IProductRepository productRepo)
    {
      _orderRepo = orderRepo;
      _productRepo = productRepo;
    }

    public bool IsValidTransferOrder(IEnumerable<DatColPickTicket> listOfTransferOrderLines, int connectorID, IEnumerable<int> sapphVendorIDs, out List<DatColErrorMessage> listOfErrors, bool returnErrorLog = false)
    {
      var isTransferOrderValid = true;

      var pickTicketLinse = listOfTransferOrderLines as IList<DatColPickTicket> ?? listOfTransferOrderLines.ToList();
      listOfErrors = new List<DatColErrorMessage>();

      var firstOrder = pickTicketLinse.First();
      var transferOrders = pickTicketLinse.GroupBy(x => x.CustomerNumber).ToList();

      if (!IsTransferOrderCountOutsideBoundary(transferOrders))
      {
        if (!returnErrorLog) return false;
        var errorMessage = new DatColErrorMessage()
        {
          Message = ErrorMessageToString(ErrorMessage.OrderCountOutsideBoundary)
        };
        listOfErrors.Add(errorMessage);
        isTransferOrderValid = false;
      }

      string orderNumberPart;
      if (!IsValidTransferOrderNumber(firstOrder.OrderNumber, out orderNumberPart))
      {
        if (!returnErrorLog) return false;
        var errorMessage = new DatColErrorMessage()
        {
          Message = string.Format("{0} - {1}", ErrorMessageToString(ErrorMessage.OrderNumber), firstOrder.OrderNumber)
        };
        listOfErrors.Add(errorMessage);
        isTransferOrderValid = false;
      }

      int orderID;
      if (_orderRepo.IsPartialWebOrderNumberExist(orderNumberPart, connectorID, out orderID))
      {
        if (!returnErrorLog) return false;
        var errorMessage = new DatColErrorMessage()
        {
          Message = string.Format("{0} - {1}", ErrorMessageToString(ErrorMessage.ExistOrderNumber), firstOrder.OrderNumber)
        };
        listOfErrors.Add(errorMessage);
        isTransferOrderValid = false;
      }

      List<string> listOfUnknownSkus;
      if (!IsSkuExist(pickTicketLinse.Select(x => x.CustomItemNumber), sapphVendorIDs, out listOfUnknownSkus, returnErrorLog))
      {
        if (!returnErrorLog) return false;
        listOfErrors.AddRange(listOfUnknownSkus.Select(unknownSku => new DatColErrorMessage
        {
          Message = string.Format("{0} '{1}'", ErrorMessageToString(ErrorMessage.UnknownSku), unknownSku)
        }));
        isTransferOrderValid = false;
      }

      foreach (var pickTicket in pickTicketLinse)
      {
        int quantity;
        if (!int.TryParse(pickTicket.QuantityOnPickTicket, NumberStyles.Number, new CultureInfo("nl-NL"), out quantity) || quantity <= 0)
        {
          if (!returnErrorLog) return false;
          var errorMessage = new DatColErrorMessage
          {
            Message = string.Format(" Line '{0}': Quantity '{1}' field for sku '{2}' is invalid",
            pickTicket.RowNumber,
            pickTicket.QuantityOnPickTicket,
            pickTicket.CustomItemNumber)
          };
          listOfErrors.Add(errorMessage);
          isTransferOrderValid = false;
        }
      }
      return isTransferOrderValid;
    }

    public bool IsValidSalesOrder(IEnumerable<DatColPickTicket> listOfSalesOrderLines, int connectorID, IEnumerable<int> sapphVendorIDs, out List<DatColErrorMessage> listOfErrors, bool returnErrorLog = false)
    {
      var isSalesOrderValid = true;

      var pickTicketLines = listOfSalesOrderLines as IList<DatColPickTicket> ?? listOfSalesOrderLines.ToList();
      listOfErrors = new List<DatColErrorMessage>();

      var firstOrder = pickTicketLines.First();

      var listOfCurrentWebOrderNumbers = _orderRepo
        .GetOrderByContainsWebOrderNumber(firstOrder.OrderNumber)
        .Select(x => x.WebSiteOrderNumber)
        .ToArray();

      if (firstOrder.OrderNumber.Length > 10)
      {
        if (!returnErrorLog) return false;
        var errorMessage = new DatColErrorMessage
          {
            Message = string.Format("OrderNumber {0} is more than 10 characters",
                                    firstOrder.OrderNumber)
          };
        listOfErrors.Add(errorMessage);
        isSalesOrderValid = false;
      }

      string tempWebOrderNumber;
      if (!ConvertOriginalOrderNumberToConcentratorOrderNumber(firstOrder.OrderNumber, listOfCurrentWebOrderNumbers, 9, out tempWebOrderNumber))
      {
        if (!returnErrorLog) return false;
        var errorMessage = new DatColErrorMessage
          {
            Message = string.Format("OrderNumber {0} cannot be converted. This ordernumbers are already exist: {1}",
            firstOrder.OrderNumber,
            string.Join(", ", listOfCurrentWebOrderNumbers))
          };
        listOfErrors.Add(errorMessage);
        isSalesOrderValid = false;
      }

      List<string> listOfUnknownSkus;
      if (!IsSkuExist(pickTicketLines.Select(x => x.CustomItemNumber), sapphVendorIDs, out listOfUnknownSkus, returnErrorLog))
      {
        if (!returnErrorLog) return false;
        listOfErrors.AddRange(listOfUnknownSkus.Select(unknownSku => new DatColErrorMessage
          {
            Message = string.Format("{0} '{1}'", ErrorMessageToString(ErrorMessage.UnknownSku), unknownSku)
          }));
        isSalesOrderValid = false;
      }

      foreach (var pickTicket in pickTicketLines)
      {
        int quantity;
        if (!int.TryParse(pickTicket.QuantityOnPickTicket, NumberStyles.Number, new CultureInfo("nl-NL"), out quantity) || quantity <= 0)
        {
          if (!returnErrorLog) return false;
          var errorMessage = new DatColErrorMessage
          {
            Message = string.Format(" Line '{0}': Quantity '{1}' field for sku '{2}' is invalid",
            pickTicket.RowNumber,
            pickTicket.QuantityOnPickTicket,
            pickTicket.CustomItemNumber)
          };
          listOfErrors.Add(errorMessage);
          isSalesOrderValid = false;
        }
      }

      return isSalesOrderValid;
    }

    public bool IsValidPurchaseOrder(DatColPurchaseOrder[] listOfPurchaseOrderLines, int connectorID, IEnumerable<int> vendorIDs, out List<DatColErrorMessage> listOfErrors, bool returnErrorLog = false)
    {
      var isPurchaseOrderValid = true;

      var purchaseLines = listOfPurchaseOrderLines as IList<DatColPurchaseOrder> ?? listOfPurchaseOrderLines.ToList();
      listOfErrors = new List<DatColErrorMessage>();

      var firstOrder = purchaseLines.First();

      var listOfCurrentWebOrderNumbers = _orderRepo
        .GetOrderByContainsWebOrderNumber(firstOrder.OrderNumber)
        .Select(x => x.WebSiteOrderNumber)
        .ToArray();

      if (firstOrder.OrderNumber.Length > 10)
      {
        if (!returnErrorLog) return false;
        var errorMessage = new DatColErrorMessage
        {
          Message = string.Format("OrderNumber {0} is more than 10 characters",
                                  firstOrder.OrderNumber)
        };
        listOfErrors.Add(errorMessage);
        isPurchaseOrderValid = false;
      }

      string tempWebOrderNumber;
      if (!ConvertOriginalOrderNumberToConcentratorOrderNumber(firstOrder.OrderNumber, listOfCurrentWebOrderNumbers, 9, out tempWebOrderNumber))
      {
        if (!returnErrorLog) return false;
        var errorMessage = new DatColErrorMessage
        {
          Message = string.Format("OrderNumber {0} cannot be converted. This ordernumbers are already exist: {1}",
          firstOrder.OrderNumber,
          string.Join(", ", listOfCurrentWebOrderNumbers))
        };
        listOfErrors.Add(errorMessage);
        isPurchaseOrderValid = false;
      }

      List<string> listOfUnknownSkus;
      if (!IsSkuExist(purchaseLines.Select(x => x.CustomItemNumber), vendorIDs, out listOfUnknownSkus, returnErrorLog))
      {
        if (!returnErrorLog) return false;
        listOfErrors.AddRange(listOfUnknownSkus.Select(unknownSku => new DatColErrorMessage
        {
          Message = string.Format("{0} '{1}'", ErrorMessageToString(ErrorMessage.UnknownSku), unknownSku)
        }));
        isPurchaseOrderValid = false;
      }

      foreach (var purchaseLine in purchaseLines)
      {
        int quantity;
        if (!int.TryParse(purchaseLine.Quantity, NumberStyles.Number, new CultureInfo("nl-NL"), out quantity) || quantity <= 0)
        {
          if (!returnErrorLog) return false;
          var errorMessage = new DatColErrorMessage
          {
            Message = string.Format(" Quantity '{0}' field for sku '{1}' is invalid",
            purchaseLine.Quantity,
            purchaseLine.CustomItemNumber)
          };
          listOfErrors.Add(errorMessage);
          isPurchaseOrderValid = false;
        }
      }

      return isPurchaseOrderValid;
    }

    public bool ConvertOriginalOrderNumberToConcentratorOrderNumber(string originalOrderNumber, ICollection<string> listOfWeborderNumbers, int maxIncrementNumber, out string webOrderNumber)
    {
      var countOrderStartsWithOriginalOrderNumber = listOfWeborderNumbers.Count;

      webOrderNumber = string.Format("{0}-{1}", originalOrderNumber, countOrderStartsWithOriginalOrderNumber);

      if (countOrderStartsWithOriginalOrderNumber > maxIncrementNumber || listOfWeborderNumbers.Contains(webOrderNumber))
      {
        return AdvancedConvertOriginalOrderNumberToConcentratorOrderNumber(originalOrderNumber, listOfWeborderNumbers, maxIncrementNumber, out webOrderNumber);
      }

      return true;
    }

    #region Private functions

    // todo: rename this function
    private bool IsTransferOrderCountOutsideBoundary<T>(IEnumerable<T> listOfOrders)
    {
      if (listOfOrders.Count() > 999)
      {
        return false;
      }
      return true;
    }

    private bool IsValidTransferOrderNumber(string orderNumber, out string partialOrderNumber)
    {
      partialOrderNumber = string.Empty;

      var orderNumberParts = orderNumber.Split('_');

      if (orderNumberParts.Length == 2 && orderNumberParts[0].Length == 8)
      {
        partialOrderNumber = orderNumberParts[0];
        return true;
      }
      return false;
    }

    public bool IsSkuExist(IEnumerable<string> listOfSkus, IEnumerable<int> vendorIDs, out List<string> listOfUnknownSkus, bool returnUnknownList = false)
    {
      listOfUnknownSkus = new List<string>();

      var currentVendorAssortments = _productRepo
        .GetListOfSkusByVendorIDs(vendorIDs)
        .ToLookup(x => Regex.Replace(x.VendorItemNumber, @"\s+", ""))
        .ToDictionary(x => x.Key);

      foreach (var sku in listOfSkus)
      {
        if (currentVendorAssortments.ContainsKey(Regex.Replace(sku, @"\s+", ""))) continue;

        if (returnUnknownList)
        {
          listOfUnknownSkus.Add(sku);
        }
        else
        {
          return false;
        }
      }

      return !listOfUnknownSkus.Any();
    }

    private static string ErrorMessageToString(ErrorMessage errorStatus)
    {
      switch (errorStatus)
      {
        case ErrorMessage.OrderCountOutsideBoundary:
          return "Amount of orders out of range";
        case ErrorMessage.OrderNumber:
          return "Ordernumber invalid";
        case ErrorMessage.ExistOrderNumber:
          return "Ordernumber already exist";
        case ErrorMessage.InvalidDate:
          return "Date invalid";
        case ErrorMessage.UnknownSku:
          return "Sku don't exist";
        default:
          return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
      }
    }

    private bool AdvancedConvertOriginalOrderNumberToConcentratorOrderNumber(string originalOrderNumber, IEnumerable<string> listOfWeborderNumbers, int maxIncrementNumber, out string webOrderNumber)
    {
      webOrderNumber = "";

      var currentExistExtendPartialNumber = listOfWeborderNumbers
        .Select(x => x.Split('-'))
        .Where(x => x.Length == 2)
        .Select(x => x[1])
        .ToList();

      for (int counter = 0; counter < maxIncrementNumber; counter++)
      {
        if (!currentExistExtendPartialNumber.Contains(counter.ToString()))
        {
          webOrderNumber = string.Format("{0}-{1}", originalOrderNumber, counter.ToString());
          return true;
        }
      }

      return false;
    }

    #endregion

  }
}
