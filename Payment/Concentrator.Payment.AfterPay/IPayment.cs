using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Orders;
using Concentrator.Objects.Models.Connectors;

namespace Concentrator.Payment.Providers
{
  public interface IPayment
  {
    /// <summary>
    /// Process invoice for Payment Provicer
    /// </summary>
    /// <param name="orderLines">invoicenumber,unitprice Ex,linenumber website(transactiondetailID),websiteOrderNumber,aantal</param>
    /// <returns></returns>
    bool InvoiceOrder(OrderResponse invoiceResponse, ConnectorPaymentProvider connectorPaymentProvider, AuditLog4Net.Adapter.IAuditLogAdapter log);

    /// <summary>
    /// Process cancelled line for Payment Provicer
    /// </summary>
    /// <param name="orderLines">UnitPrice,DetailReference,OrderReference,Q</param>
    /// <returns></returns>
    bool CancelOrder(OrderResponse invoiceResponse, ConnectorPaymentProvider connectorPaymentProvider, AuditLog4Net.Adapter.IAuditLogAdapter log);
  }
}
