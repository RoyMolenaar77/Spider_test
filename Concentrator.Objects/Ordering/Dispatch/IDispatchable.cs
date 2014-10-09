using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AuditLog4Net.Adapter;
using Concentrator.Objects;
using Concentrator.Objects.Models.Orders;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.DataAccess.UnitOfWork;
using System.Xml;

namespace Concentrator.Objects.Ordering.Dispatch
{
  public interface IDispatchable
  {
    /// <summary>
    /// Dispatches passed in order lines
    /// </summary>
    /// <param name="orderLines">A collection of orderlines</param>
    /// <returns>Vendor Order Number</returns>
    int DispatchOrders(Dictionary<Concentrator.Objects.Models.Orders.Order,List<OrderLine>> orderLines, Vendor vendor, IAuditLogAdapter log, IUnitOfWork uni);

    /// <summary>
    /// Processes an order response
    /// </summary>
    void GetAvailableDispatchAdvices(Vendor vendor, IAuditLogAdapter log, string logPath, IUnitOfWork unit);

    /// <summary>
    /// Cancel a order
    /// </summary>
    void CancelOrder(Vendor vendor, IAuditLogAdapter log, string logPath);

    /// <summary>
    /// Log Order information
    /// </summary>
    /// <param name="xmlOrder"></param>
    void LogOrder(object orderInformation, int vendorID, string fileName, AuditLog4Net.Adapter.IAuditLogAdapter log);
  }
}
