using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Orders;
using Concentrator.Plugins.Axapta.Models;
using Concentrator.Plugins.Axapta.Repositories;

namespace Concentrator.Plugins.Axapta.Helpers
{
  public class NotificationHelper : INotificationHelper
  {
    public bool IsValidSalesNotification(IEnumerable<OrderNotification> listOfSalesOrderLines, out List<DatColErrorMessage> listOfErrors, bool returnErrorLog = false)
    {
      var isSalesNotificationValid = true;
      listOfErrors = new List<DatColErrorMessage>();
      var salesOrderLines = listOfSalesOrderLines as OrderNotification[] ?? listOfSalesOrderLines.ToArray();

      var orderNumber = salesOrderLines.First().WebSiteOrderNumber;

      foreach (var orderLine in salesOrderLines)
      {
        if (string.IsNullOrEmpty(orderLine.OriginalLine))
        {
          if (!returnErrorLog) return false;
          var errorMessage = new DatColErrorMessage
          {
            Message = string.Format("[OrderNumber: {0}, OrderLine: {1}] does not have OriginalLine",
            orderNumber,
            orderLine.OrderLineID)
          };
          listOfErrors.Add(errorMessage);
          isSalesNotificationValid = false;
        }

        if (!orderLine.OrderResponseLineID.HasValue)
        {
          if (!returnErrorLog) return false;
          var errorMessage = new DatColErrorMessage
          {
            Message = string.Format(" Missing OrderResponse from TNT: [OrderNumber: '{0}', OrderLine: '{1}', AxaptaLine: '{2}']",
            orderNumber,
            orderLine.OrderLineID,
            orderLine.OriginalLine != null ? orderLine.OriginalLine.Replace(Environment.NewLine, string.Empty) : string.Empty)
          };
          listOfErrors.Add(errorMessage);
          isSalesNotificationValid = false;
        }
      }

      return isSalesNotificationValid;
    }

    public bool IsValidTransferNotification(IEnumerable<OrderNotification> listOfSalesOrderLines, out List<DatColErrorMessage> listOfErrors, bool returnErrorLog = false)
    {
      var isTransferNotificationValid = true;
      listOfErrors = new List<DatColErrorMessage>();
      var trasferOrderLines = listOfSalesOrderLines as OrderNotification[] ?? listOfSalesOrderLines.ToArray();

      foreach (var orderLine in trasferOrderLines)
      {
        if (string.IsNullOrEmpty(orderLine.OriginalLine))
        {
          if (!returnErrorLog) return false;
          var errorMessage = new DatColErrorMessage
          {
            Message = string.Format("[OrderNumber: {0}, OrderLine: {1}] does not have OriginalLine",
            orderLine.WebSiteOrderNumber,
            orderLine.OrderLineID)
          };
          listOfErrors.Add(errorMessage);
          isTransferNotificationValid = false;
        }

        if (!orderLine.OrderResponseLineID.HasValue)
        {
          if (!returnErrorLog) return false;
          var errorMessage = new DatColErrorMessage
          {
            Message = string.Format("[OrderNumber: {0}, OrderLine: {1}] does not have ResponseLine from TNT",
            orderLine.WebSiteOrderNumber,
            orderLine.OrderLineID)
          };
          listOfErrors.Add(errorMessage);
          isTransferNotificationValid = false;
        }
      }
      return isTransferNotificationValid;
    }

    public bool IsAllSubTransferOrdersAreShipped(IEnumerable<OrderNotification> listOfSalesOrderLines, IEnumerable<Order> listOfCurrentTransferOrders, out List<DatColErrorMessage> listOfErrors, bool returnErrorLog = false)
    {
      var isTransferNotificationValid = true;
      listOfErrors = new List<DatColErrorMessage>();

      var trasferOrderLines = listOfSalesOrderLines as OrderNotification[] ?? listOfSalesOrderLines.ToArray();
      var currenctTransferOrders = listOfCurrentTransferOrders as Order[] ?? listOfCurrentTransferOrders.ToArray();

      var documentOrderNumber = trasferOrderLines.First().Document;

      var subTransferOrders = trasferOrderLines
        .GroupBy(x => x.WebSiteOrderNumber)
        .Select(x => x.Key)
        .ToArray();

      var countSubTransferOrders = subTransferOrders.Count();

      var countCurrentTransferOrders = currenctTransferOrders.Count();

      if (countCurrentTransferOrders != countSubTransferOrders)
      {
        if (!returnErrorLog) return false;

        listOfErrors.AddRange(from currenctTransferOrder in currenctTransferOrders
                              where !subTransferOrders.Contains(currenctTransferOrder.WebSiteOrderNumber)
                              select new DatColErrorMessage
                                {
                                  Message = string.Format("[TransferOrder '{0}'] waiting for shipmentnotification of the OrderNumber '{1}'", documentOrderNumber, currenctTransferOrder.WebSiteOrderNumber)
                                });

        isTransferNotificationValid = false;
      }

      return isTransferNotificationValid;
    }
  }
}
