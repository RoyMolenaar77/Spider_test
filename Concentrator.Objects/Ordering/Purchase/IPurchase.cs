using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Web.Objects.EDI.Purchase;
using Concentrator.Web.Objects.EDI;
using Concentrator.Web.Objects.EDI.ChangeOrder;
using Concentrator.Objects.Models.Orders;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.DataAccess.UnitOfWork;
using log4net.Core;
using log4net;

namespace Concentrator.Objects.Ordering.Purchase
{
  public interface IPurchase
  {
    /// <summary>
    /// Purchase Order passed in order lines
    /// </summary>
    /// <param name="orderLines">A collection of orderlines</param>
    /// <returns>Vendor Order Number</returns>
    bool PurchaseOrders(Concentrator.Objects.Models.Orders.Order order, List<OrderLine> orderLines, Vendor administrativeVendor, Vendor vendor, bool directShipment, IUnitOfWork unit, ILog logger);

    /// <summary>
    /// Purchase Confirmation for Vendor
    /// </summary>
    /// <param name="purchaseConfirmation">A purchase confirmation object</param>
    void PurchaseConfirmation(PurchaseConfirmation purchaseConfirmation, Vendor administrativeVendor, List<OrderLine> orderLines);

    /// <summary>
    /// Inovice for Vendor
    /// </summary>
    /// <param name="purchaseConfirmation">An invoice object</param>
    void InvoiceMessage(InvoiceMessage invoiceMessage, Vendor administrativeVendor, List<OrderLine> orderLines);

    /// <summary>
    /// Inovice for ChangeOrderRequest
    /// </summary>
    /// <param name="purchaseConfirmation">An ChangeOrderRequest object</param>
    void OrderChange(ChangeOrderRequest changeOrderRequest, Vendor administrativeVendor, List<OrderLine> orderLines);

  }
}
