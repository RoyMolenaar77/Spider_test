using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Concentrator.Objects;
using Concentrator.Objects.ConcentratorService;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects.Models.Contents;
using Concentrator.Objects.Models.Orders;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.Ordering.Purchase;
using Concentrator.Payment.Providers;
using Concentrator.Web.Objects.EDI;
using Concentrator.Web.Objects.EDI.ChangeOrder;
using Concentrator.Web.Objects.EDI.Purchase;

namespace Concentrator.Order.OrderDispatch
{
  public class ResponseProcessor : ConcentratorPlugin
  {
    private List<int> array = new List<int>();

    public override string Name
    {
      get { return "Response Processor"; }
    }

    protected override void Process()
    {
      using (var unit = GetUnitOfWork())
      {
        bool keepRunning = true;

        int batchSize = 50;
        while (keepRunning)
        {

          var responseLines = (from o in unit.Scope.Repository<OrderResponseLine>()
                                 .Include(x => x.OrderResponse)
                                 .GetAll(x => x.Processed == false)
                               where o.OrderLine.Order.OrderType == (int)OrderTypes.SalesOrder
                               orderby o.OrderResponse.ResponseType
                               group o by o.OrderResponse into od
                               select new
                               {
                                 Response = od.Key,
                                 ResponseLines = od
                               }).Take(batchSize).ToList();


          if (responseLines.Count < batchSize)
            keepRunning = false;


          log.DebugFormat("Done {0}", batchSize);
          foreach (var response in responseLines)
          {
            try
            {
              OrderResponseTypes c = OrderResponseTypes.Unknown;

              if (Enum.IsDefined(typeof(OrderResponseTypes), response.Response.ResponseType))
                c = (OrderResponseTypes)Enum.Parse(typeof(OrderResponseTypes), response.Response.ResponseType);

              XmlDocument doc = null;

              bool thirdPartyVendor = false;//response.ResponseLines.Where(x => x.OrderLineID.HasValue).Any(l => l.OrderLine.Order.Connector.AdministrativeVendorID.HasValue && l.OrderLine.Order.Connector.AdministrativeVendorID != response.Response.VendorID);

              if (c == OrderResponseTypes.InvoiceNotification)
              {
                if (thirdPartyVendor
                    && !response.ResponseLines.Any(x => x.OrderLineID.HasValue && x.OrderLine.OrderLedgers.Any(Y => Y.Status == (int)OrderLineStatus.ProcessedAdministrativeVendorInvoiceNotification))
                    && response.Response.Orders.Count > 0)
                {

                  ProcessInvoiceAdministrativeVendor(response.ResponseLines.ToList(), response.Response.Orders.FirstOrDefault().Connector.Vendor, unit, response.Response.Orders.FirstOrDefault());
                  response.ResponseLines.Where(x => x.OrderLineID.HasValue).ForEach((x, idx) =>
                  {
                    x.OrderLine.SetStatus(OrderLineStatus.ProcessedAdministrativeVendorInvoiceNotification, unit.Scope.Repository<OrderLedger>());
                  });
                }
              }

              foreach (var order in response.Response.Orders)
              {
                var orderResponseLines = response.Response.OrderResponseLines.Where(x => x.OrderLineID.HasValue && x.OrderLine.OrderID == order.OrderID).ToList();

                // bool thirdPartyVendor = (order.Connector.AdministrativeVendor != null && order.Connector.AdministrativeVendor != response.Response.Vendor);
                bool administrativeVendor = (order.Connector.Vendor != null && order.Connector.Vendor == response.Response.Vendor);

                switch (c)
                {
                  case OrderResponseTypes.CancelNotification:
                    if (thirdPartyVendor)
                    {
                      List<OrderResponseLine> confirmationLines = new List<OrderResponseLine>();

                      foreach (var line in orderResponseLines)
                      {
                        line.setChangeType(ChangeType.Delete);
                        confirmationLines.Add(line);
                      }

                      if (confirmationLines.Count > 0)
                      {
                        ProcessOrderChange(confirmationLines, order.Connector.Vendor, unit, order);
                        confirmationLines.Where(x => x.OrderLineID.HasValue).ForEach((x, idx) =>
                        {
                          x.OrderLine.SetStatus(OrderLineStatus.ProcessedAdministrativeVendorChangeNotification, unit.Scope.Repository<OrderLedger>());
                        });
                      }
                    }

                    doc = GenerateAcknowledgement(response.Response, orderResponseLines, order);
                    ParseDocument(orderResponseLines, unit, OrderLineStatus.Cancelled, true, doc, c, ledgerQuantity: true);
                    break;
                  case OrderResponseTypes.Acknowledgement:
                    if (orderResponseLines.Any(x => x.OrderLineID.HasValue && x.OrderLine.CurrentState() == (int)OrderLineStatus.WaitingForPurchaseConfirmation)
                      && orderResponseLines.Any(x => x.OrderLineID.HasValue && x.OrderLine.isDispatched && x.OrderLine.DispatchedToVendorID == response.Response.VendorID))
                      continue;

                    if (thirdPartyVendor)
                    {
                      List<OrderResponseLine> confirmationLines = new List<OrderResponseLine>();

                      foreach (var line in orderResponseLines)
                      {
                        line.setChangeType(ChangeType.Change);

                        if (line.Cancelled > 0 && line.Ordered != line.Cancelled)
                          confirmationLines.Add(line);
                      }

                      if (confirmationLines.Count > 0)
                      {
                        ProcessOrderChange(confirmationLines, order.Connector.Vendor, unit, order);
                        confirmationLines.Where(x => x.OrderLineID.HasValue).ForEach((x, idx) =>
                        {
                          x.OrderLine.SetStatus(OrderLineStatus.ProcessedAdministrativeVendorChangeNotification, unit.Scope.Repository<OrderLedger>());
                        });
                      }
                    }

                    doc = GenerateAcknowledgement(response.Response, orderResponseLines, order);
                    ParseDocument(orderResponseLines, unit, OrderLineStatus.WaitingForShipmentNotification, true, doc, c);
                    break;
                  case OrderResponseTypes.ShipmentNotification:

                    if (thirdPartyVendor)
                    {
                      List<OrderResponseLine> confirmationLines = new List<OrderResponseLine>();
                      List<OrderResponseLine> changeLines = new List<OrderResponseLine>();

                      foreach (var line in orderResponseLines)
                      {
                        //if (line.OrderLine.CurrentState() == (int)OrderLineStatus.WaitingForShipmentNotification || line.OrderLine.CurrentState() == (int)OrderLineStatus.WaitingForAcknowledgement)
                        //{
                        confirmationLines.Add(line);
                        //}
                        line.setChangeType(ChangeType.Change);

                        if (line.Cancelled > 0 && line.Ordered != line.Cancelled)
                          changeLines.Add(line);
                      }

                      if (confirmationLines.Count > 0)
                      {
                        if (ProcessPurchaseConfirmation(confirmationLines, order.Connector.Vendor, unit, order))
                        {

                          confirmationLines.Where(x => x.OrderLineID.HasValue).ForEach((x, idx) =>
                          {
                            x.OrderLine.SetStatus(OrderLineStatus.ProcessedPurchaseConfirmation, unit.Scope.Repository<OrderLedger>());
                          });
                        }
                        else
                          continue;
                      }

                      if (changeLines.Count > 0)
                      {
                        ProcessOrderChange(changeLines, order.Connector.Vendor, unit, order);
                        changeLines.Where(x => x.OrderLineID.HasValue).ForEach((x, idx) =>
                        {
                          x.OrderLine.SetStatus(OrderLineStatus.ProcessedAdministrativeVendorChangeNotification, unit.Scope.Repository<OrderLedger>());
                        });
                      }
                    }

                    doc = GenerateShipmentNotification(response.Response, orderResponseLines, order);
                    ParseDocument(orderResponseLines, unit, OrderLineStatus.WaitingForInvoiceNotification, true, doc, c);

                    var triggerProduct = order.Connector.ConnectorSettings.GetValueByKey("HarmonyTriggerProduct", string.Empty);
                    var shipmentNotificationUrl = order.Connector.ConnectorSettings.GetValueByKey("ShipmentNotificationUrl", string.Empty);

                    if (!string.IsNullOrEmpty(triggerProduct) && !string.IsNullOrEmpty(shipmentNotificationUrl))
                    {
                      if (order.OrderLines.Any(x => x.CustomerItemNumber == triggerProduct))
                      {
                        ParseDocument(orderResponseLines, unit, OrderLineStatus.WaitingForInvoiceNotification, true, doc, c, shipmentNotificationUrl);
                      }
                    }

                    break;
                  case OrderResponseTypes.InvoiceNotification:

                    var paymentProvider = order.Connector.ConnectorPaymentProviders.FirstOrDefault(x => x.PaymentTermsCode == order.PaymentTermsCode);

                    if (administrativeVendor
                        && paymentProvider != null
                        && paymentProvider.PaymentProvider.IsActive
                        && !orderResponseLines.Any(x => x.OrderLineID.HasValue && x.OrderLine.OrderLedgers.Any(Y => Y.Status == (int)OrderLineStatus.ProcessedInvoicePaymentProvider)))
                    {
                      //var paymentProviderType = context.PaymentProviders.Where(x => x.PaymentProviderID == paymentProvider.PaymentProviderID).FirstOrDefault();
                      string providerType = paymentProvider.PaymentProvider.ProviderType;

                      var provider = (IPayment)Activator.CreateInstance(Assembly.GetAssembly(typeof(IPayment)).GetType(providerType));
                      if (provider.InvoiceOrder(response.Response, paymentProvider, log))
                      {
                        orderResponseLines.Where(x => x.OrderLineID.HasValue).ForEach((x, idx) =>
                        {
                          x.OrderLine.SetStatus(OrderLineStatus.ProcessedInvoicePaymentProvider, unit.Scope.Repository<OrderLedger>());
                        });
                      }
                    }

                    doc = GenerateInvoiceNotification(response.Response, orderResponseLines, order);
                    ParseDocument(orderResponseLines, unit, OrderLineStatus.Processed, true, doc, c);
                    break;
                  case OrderResponseTypes.PurchaseAcknowledgement:
                    var processLines = orderResponseLines;

                    foreach (var line in processLines)
                    {
                      if (line.OrderLineID.HasValue && line.OrderLine.Product != null)
                      {
                        var product = unit.Scope.Repository<Content>().GetSingle(x => x.ProductID == line.OrderLine.ProductID && x.ConnectorID == line.OrderLine.Order.ConnectorID);

                        if (product == null)
                        {
                          ParseDocument(new List<OrderResponseLine>() { line }, unit, OrderLineStatus.PushProduct, false, null, c);
                          continue;
                        }
                      }


                      if (unit.Scope.Repository<OrderResponseLine>().GetAllAsQueryable().Any(x => x.OrderLineID == line.OrderLineID && x.OrderResponse.ResponseType == "Acknowledgement"))
                      {
                        List<OrderResponseLine> orl = new List<OrderResponseLine>();
                        orl.Add(line);
                        ParseDocument(orl, unit, OrderLineStatus.WaitingForAcknowledgement, false, null, c);
                      }
                      else if (line.OrderLineID.HasValue && line.OrderLine.CurrentState() == (int)OrderLineStatus.WaitingForPurchaseConfirmation)
                      {
                        List<OrderResponseLine> orl = new List<OrderResponseLine>();
                        orl.Add(line);
                        ParseDocument(orl, unit, OrderLineStatus.ReadyToOrder, false, null, c);
                      }
                      else
                      {
                        line.Processed = true;
                      }
                    }
                    break;
                  case OrderResponseTypes.Return:
                    XElement element = new XElement("Statuses",
                                           new XAttribute("WebsiteOrderNumber", order.WebSiteOrderNumber),
                                           new XAttribute("OrderID", order.OrderID),
                                           new XAttribute("ConnectorID", order.ConnectorID),
                                           from s in orderResponseLines
                                           select new XElement("Status",
                                             new XElement("LineID", s.OrderLineID.HasValue ? s.OrderLineID.Value : 0),
                                             new XElement("ProductID", s.ProductID.HasValue ? s.ProductID.Value : s.OrderLineID.HasValue ? s.OrderLine.ProductID.Value : 0),
                                             new XElement("VendorItemNumber", s.ProductID.HasValue ? s.Product.VendorItemNumber : s.OrderLineID.HasValue ? s.OrderLine.Product.VendorItemNumber : string.Empty),
                                             new XElement("Quantity", s.Delivered),
                                             new XElement("StatusDescription", s.Remark)));

                    var xmldoc = new XmlDocument();
                    xmldoc.Load(element.CreateReader());

                    ParseDocument(orderResponseLines, unit, OrderLineStatus.ProcessedReturnNotification, true, xmldoc, c, ledgerQuantity: true);
                    break;
                  default:
                    XElement defaultElement = new XElement("Statuses",
                                           new XAttribute("WebsiteOrderNumber", order.WebSiteOrderNumber),
                                           new XAttribute("OrderID", order.OrderID),
                                           new XAttribute("ConnectorID", order.ConnectorID),
                                           from s in orderResponseLines
                                           select new XElement("Status",
                                             new XElement("LineID", s.OrderLineID.HasValue ? s.OrderLineID.Value : 0),
                                             new XElement("ProductID", s.ProductID.HasValue ? s.ProductID.Value : s.OrderLineID.HasValue ? s.OrderLine.ProductID.Value : 0),
                                             new XElement("VendorItemNumber", s.ProductID.HasValue ? s.Product.VendorItemNumber : s.OrderLineID.HasValue ? s.OrderLine.Product.VendorItemNumber : string.Empty),
                                             new XElement("Quantity", s.Delivered),
                                             new XElement("StatusDescription", s.Remark)));

                    var defaultXmldox = new XmlDocument();
                    defaultXmldox.Load(defaultElement.CreateReader());

                    ParseDocument(orderResponseLines, unit, OrderLineStatus.ProcessedUnassignedNotification, true, defaultXmldox, c);
                    break;
                }
              }

              unit.Save();
            }
            catch (Exception ex)
            {
              log.Error("Error processing responses", ex);
              keepRunning = false;
            }
          }
        }
      }

    }

    private void ProcessInvoiceAdministrativeVendor(List<OrderResponseLine> responseLines, Vendor administrativeVendor, IUnitOfWork unit, Concentrator.Objects.Models.Orders.Order order)
    {
      var response = responseLines.FirstOrDefault().OrderResponse;

      var supplier = response.Vendor.PreferredConnectorVendors.Where(x => x.ConnectorID == order.ConnectorID).FirstOrDefault();

      InvoiceMessage invoice = new InvoiceMessage()
      {
        InvoiceDate = response.InvoiceDate.Value,
        InvoiceNumber = response.InvoiceDocumentNumber,
        SupplierNumber = (supplier != null && !string.IsNullOrEmpty(supplier.VendorIdentifier)) ? supplier.VendorIdentifier : string.Empty,
        Version = "1.0",
        Currency = !string.IsNullOrEmpty(response.Currency) ? response.Currency.ToUpper() : "EUR",
        PaymentType = response.Vendor.VendorSettings.GetValueByKey("PaymentTerm", string.Empty),
        PaymentInstrument = response.Vendor.VendorSettings.GetValueByKey("PaymentInstrument", string.Empty)
      };

      List<InvoiceLine> invoiceLines = new List<InvoiceLine>();
      foreach (var line in responseLines)
      {
        if (line.OrderLineID.HasValue)
        {
          var purchaseAcknowledgement = line.OrderLine.Order.GetOrderResponses(OrderResponseTypes.PurchaseAcknowledgement, unit.Scope.Repository<Concentrator.Objects.Models.Orders.OrderResponse>()).FirstOrDefault();

          if (purchaseAcknowledgement == null)
          {
            purchaseAcknowledgement = (from o in unit.Scope.Repository<Concentrator.Objects.Models.Orders.OrderResponse>().GetAllAsQueryable()
                                       where o.ResponseType == OrderResponseTypes.PurchaseAcknowledgement.ToString()
                                       && (o.OrderID.HasValue && o.OrderID.Value == line.OrderLine.OrderID)
                                       select o).FirstOrDefault();
          }

          if (purchaseAcknowledgement != null)
          {
            InvoiceLine iLine = new InvoiceLine()
            {
              AdditionalLine = false,
              bskIdentifier = line.OrderLine.Order.BSKIdentifier.ToString(),
              PurchaseOrderNumber = purchaseAcknowledgement.VendorDocumentNumber,
              AmountOpen = line.Price,
              ExtendedPrice = line.Price,
              VendorItemNumber = line.OrderLine.Product.VendorItemNumber,
              QuantityOpen = line.Invoiced,
              UnitPrice = line.Price,
              ItemNumber = unit.Scope.Repository<VendorAssortment>().GetSingle(x => x.VendorID == administrativeVendor.VendorID && x.ProductID == line.OrderLine.ProductID).Try(x => x.CustomItemNumber, string.Empty)
            };
            invoiceLines.Add(iLine);
          }
        }
      }

      #region additionalcost
      if (invoiceLines.Count > 0)
      {
        if (response.AdministrationCost.HasValue && response.AdministrationCost.Value > 0)
        {
          InvoiceLine iLine = new InvoiceLine()
          {
            AmountOpen = response.AdministrationCost.Value,
            ExtendedPrice = response.AdministrationCost.Value,
            VendorItemNumber = "AC",
            QuantityOpen = 0,
            UnitPrice = response.AdministrationCost.Value,
            ItemNumber = "AC",
            AdditionalLine = true,
            bskIdentifier = invoiceLines.Last().bskIdentifier,
            PurchaseOrderNumber = invoiceLines.Last().PurchaseOrderNumber
          };
          invoiceLines.Add(iLine);
        }

        if (response.DropShipmentCost.HasValue && response.DropShipmentCost.Value > 0)
        {
          InvoiceLine iLine = new InvoiceLine()
          {
            AmountOpen = response.DropShipmentCost.Value,
            ExtendedPrice = response.DropShipmentCost.Value,
            VendorItemNumber = "DC",
            QuantityOpen = 0,
            UnitPrice = response.DropShipmentCost.Value,
            ItemNumber = "DC",
            AdditionalLine = true,
            bskIdentifier = invoiceLines.Last().bskIdentifier,
            PurchaseOrderNumber = invoiceLines.Last().PurchaseOrderNumber
          };
          invoiceLines.Add(iLine);
        }

        if (response.ShipmentCost.HasValue && response.ShipmentCost.Value > 0)
        {
          InvoiceLine iLine = new InvoiceLine()
          {
            AmountOpen = response.ShipmentCost.Value,
            ExtendedPrice = response.ShipmentCost.Value,
            VendorItemNumber = "SC",
            QuantityOpen = 0,
            UnitPrice = response.ShipmentCost.Value,
            ItemNumber = "SC",
            AdditionalLine = true,
            bskIdentifier = invoiceLines.Last().bskIdentifier,
            PurchaseOrderNumber = invoiceLines.Last().PurchaseOrderNumber
          };
          invoiceLines.Add(iLine);
        }
      #endregion
        invoice.InvoiceLines = invoiceLines.ToArray();

        var invoiceVendor = (IPurchase)
                 Activator.CreateInstance(Assembly.GetAssembly(typeof(IPurchase)).GetType(administrativeVendor.PurchaseOrderType));

        invoiceVendor.InvoiceMessage(invoice, administrativeVendor, responseLines.Where(x => x.OrderLineID.HasValue).Select(x => x.OrderLine).ToList());
      }
    }

    private XmlDocument GenerateInvoiceNotification(Concentrator.Objects.Models.Orders.OrderResponse response, List<OrderResponseLine> responseLines, Concentrator.Objects.Models.Orders.Order order)
    {
      List<InvoiceOrderDetail> replyItems = new List<InvoiceOrderDetail>();
      foreach (var line in responseLines)
      {
        InvoiceOrderDetail lineItem = new InvoiceOrderDetail();
        lineItem.ProductIdentifier = new Concentrator.Web.Objects.EDI.ProductIdentifier();
        lineItem.ProductIdentifier.ProductNumber = line.ProductID.HasValue ? line.ProductID.Value.ToString() : line.OrderLineID.HasValue && line.OrderLine.ProductID.HasValue ? line.OrderLine.ProductID.Value.ToString() : string.Empty;
        lineItem.ProductIdentifier.ManufacturerItemID = line.ProductID.HasValue ? line.Product.VendorItemNumber : line.OrderLineID.HasValue && line.OrderLine.ProductID.HasValue ? line.OrderLine.Product.VendorItemNumber : string.Empty;
        if (line.OrderLineID.HasValue && line.OrderLine.ProductID.HasValue && line.OrderLine.Product.ProductBarcodes.Count > 0)
          lineItem.ProductIdentifier.EANIdentifier = line.OrderLine.Product.ProductBarcodes.FirstOrDefault().Barcode;
        else if (line.ProductID.HasValue && line.Product.ProductBarcodes.Count > 0)
          lineItem.ProductIdentifier.EANIdentifier = line.Product.ProductBarcodes.FirstOrDefault().Barcode;

        lineItem.CustomerReference = new Concentrator.Web.Objects.EDI.CustomerReference();
        lineItem.CustomerReference.CustomerItemNumber = line.OrderLineID.HasValue ? line.OrderLine.CustomerItemNumber : string.Empty;
        lineItem.CustomerReference.CustomerOrder = line.OrderLineID.HasValue ? line.OrderLine.CustomerOrderNr : string.Empty;
        lineItem.CustomerReference.CustomerOrderLine = line.OrderLineID.HasValue ? line.OrderLine.CustomerOrderLineNr : string.Empty;

        lineItem.LineNumber = line.OrderLineID.HasValue ? line.OrderLineID.ToString() : string.Empty;
        if (line.DeliveryDate.HasValue)
          lineItem.PromissedDeliveryDate = line.DeliveryDate.Value;

        if (line.RequestDate.HasValue)
          lineItem.RequestedDate = line.RequestDate.Value;

        lineItem.Quantity = new Quantity();
        lineItem.Quantity.QuantityBackordered = line.Backordered;
        lineItem.Quantity.QuantityBackorderedSpecified = true;
        lineItem.Quantity.QuantityCancelled = line.Cancelled;
        lineItem.Quantity.QuantityCancelledSpecified = true;
        lineItem.Quantity.QuantityOrdered = line.Ordered;
        lineItem.Quantity.QuantityShipped = line.Invoiced;
        lineItem.Quantity.QuantityShippedSpecified = true;

        if (string.IsNullOrEmpty(line.OrderResponse.VendorDocumentNumber))
          lineItem.StatusCode = StatusCode.Reject;
        else if (line.Cancelled == line.Ordered)
          lineItem.StatusCode = StatusCode.Delete;
        else if (line.Ordered != line.Shipped)
          lineItem.StatusCode = StatusCode.Change;
        else
          lineItem.StatusCode = StatusCode.Accept;

        lineItem.TaxAmount = line.VatAmount.HasValue ? line.VatAmount.Value : 0;
        lineItem.UnitOfMeasure = InvoiceOrderDetailUnitOfMeasure.EA;
        lineItem.UnitPrice = line.Price;

        lineItem.ShipmentInformation = new ShipmentInformation();
        lineItem.ShipmentInformation.CarrierCode = line.CarrierCode;
        lineItem.ShipmentInformation.NumberOfColli = line.NumberOfUnits.HasValue ? line.NumberOfUnits.Value.ToString() : "1";
        lineItem.ShipmentInformation.NumberOfPallet = line.NumberOfPallets.HasValue ? line.NumberOfPallets.Value.ToString() : "0";
        lineItem.ShipmentInformation.TrackAndTraceNumber = line.TrackAndTrace;

        lineItem.ExtendedPrice = line.Price;
        lineItem.ExtendedPriceSpecified = false;

        lineItem.InvoiceNumber = response.InvoiceDocumentNumber;
        if (!string.IsNullOrEmpty(line.SerialNumbers))
          lineItem.SerialNumbers = line.SerialNumbers.Split(';');

        replyItems.Add(lineItem);
      }

      InvoiceOrderHeader header = new InvoiceOrderHeader()
      {
        BSKIdentifier = int.Parse(order.BSKIdentifier),
        CustomerOrder = order.CustomerOrderReference,
        OrderNumber = response.VendorDocumentNumber,
        RequestedDate = response.ReqDeliveryDate.HasValue ? response.ReqDeliveryDate.Value : order.ReceivedDate,
        RequestedDateSpecified = response.ReqDeliveryDate.HasValue,
        WebSiteOrderNumber = order.WebSiteOrderNumber,
      };

      header.PackingInformation = new PackingInformation();
      header.PackingInformation.PackingDateTime = response.ReceiveDate;
      header.PackingInformation.PackingNumber = response.ShippingNumber;

      if (response.PartialDelivery.HasValue && response.PartialDelivery.Value)
        header.FullfillmentCode = InvoiceOrderHeaderFullfillmentCode.Partial;
      else
        header.FullfillmentCode = InvoiceOrderHeaderFullfillmentCode.Complete;

      if (response.ShippedToCustomer != null)
      {
        header.ShipToCustomer = FillShipToCustomer(response.ShippedToCustomer);
        header.CustomerOverride = FillCustomerOverride(response.ShippedToCustomer);
      }
      else
      {
        header.ShipToCustomer = FillShipToCustomer(order.ShippedToCustomer);
        header.CustomerOverride = FillCustomerOverride(order.ShippedToCustomer);
      }

      if (response.SoldToCustomer != null)
        header.SoldToCustomer = FillShipToCustomer(response.SoldToCustomer);
      else
        header.SoldToCustomer = FillShipToCustomer(order.ShippedToCustomer);

      if (response.InvoiceDate.HasValue)
        header.InvoiceDate = response.InvoiceDate.Value;

      header.InvoiceNumber = response.InvoiceDocumentNumber;
      if (response.VatPercentage.HasValue)
        header.InvoiceTax = response.VatPercentage.Value.ToString();
      if (response.VatAmount.HasValue)
        header.InvoiceTaxableAmount = response.VatAmount.Value.ToString();
      if (response.TotalAmount.HasValue)
        header.InvoiceTotalInc = response.TotalAmount.Value.ToString();

      InvoiceNotification invoiceNotification = new InvoiceNotification()
      {
        InvoiceOrderDetails = replyItems.ToArray(),
        InvoiceOrderHeader = header,
        Version = "1.0"
      };

      StringBuilder requestString = new StringBuilder();
      XmlWriterSettings settings = new XmlWriterSettings();
      settings.Encoding = Encoding.UTF8;
      XmlDocument document = new XmlDocument();
      using (XmlWriter xw = XmlWriter.Create(requestString, settings))
      {
        xw.WriteProcessingInstruction("xml", "version=\"1.0\" encoding=\"utf-8\"");
        XmlSerializer serializer = new XmlSerializer(typeof(InvoiceNotification));
        XmlSerializerNamespaces nm = new XmlSerializerNamespaces();
        nm.Add("xsi", "http://www.w3.org/2001/XMLSchema-instance");
        serializer.Serialize(xw, invoiceNotification, nm);

        document.LoadXml(requestString.ToString());
      }
      return document;
    }

    private XmlDocument GenerateShipmentNotification(Concentrator.Objects.Models.Orders.OrderResponse response, List<OrderResponseLine> responseLines, Concentrator.Objects.Models.Orders.Order order)
    {
      List<ShipmentOrderDetail> replyItems = new List<ShipmentOrderDetail>();
      foreach (var line in responseLines)
      {
        ShipmentOrderDetail lineItem = new ShipmentOrderDetail();
        lineItem.ProductIdentifier = new Concentrator.Web.Objects.EDI.ProductIdentifier();
        lineItem.ProductIdentifier.ProductNumber = line.ProductID.HasValue ? line.ProductID.Value.ToString() : line.OrderLineID.HasValue && line.OrderLine.ProductID.HasValue ? line.OrderLine.ProductID.Value.ToString() : string.Empty;
        lineItem.ProductIdentifier.ManufacturerItemID = line.ProductID.HasValue ? line.Product.VendorItemNumber : line.OrderLineID.HasValue && line.OrderLine.ProductID.HasValue ? line.OrderLine.Product.VendorItemNumber : string.Empty;
        if (line.OrderLineID.HasValue && line.OrderLine.ProductID.HasValue && line.OrderLine.Product.ProductBarcodes.Count > 0)
          lineItem.ProductIdentifier.EANIdentifier = line.OrderLine.Product.ProductBarcodes.FirstOrDefault().Barcode;
        else if (line.ProductID.HasValue && line.Product.ProductBarcodes.Count > 0)
          lineItem.ProductIdentifier.EANIdentifier = line.Product.ProductBarcodes.FirstOrDefault().Barcode;

        lineItem.CustomerReference = new Concentrator.Web.Objects.EDI.CustomerReference();
        lineItem.CustomerReference.CustomerItemNumber = line.OrderLineID.HasValue ? line.OrderLine.CustomerItemNumber : string.Empty;
        lineItem.CustomerReference.CustomerOrder = line.OrderLineID.HasValue ? line.OrderLine.CustomerOrderNr : string.Empty;
        lineItem.CustomerReference.CustomerOrderLine = line.OrderLineID.HasValue ? line.OrderLine.CustomerOrderLineNr : string.Empty;

        lineItem.LineNumber = line.OrderLineID.HasValue ? line.OrderLineID.ToString() : string.Empty;
        if (line.DeliveryDate.HasValue)
          lineItem.PromissedDeliveryDate = line.DeliveryDate.Value;

        if (line.RequestDate.HasValue)
          lineItem.RequestedDate = line.RequestDate.Value;

        lineItem.Quantity = new Quantity();
        lineItem.Quantity.QuantityBackordered = line.Backordered;
        lineItem.Quantity.QuantityBackorderedSpecified = true;
        lineItem.Quantity.QuantityCancelled = line.Cancelled;
        lineItem.Quantity.QuantityCancelledSpecified = true;
        lineItem.Quantity.QuantityOrdered = line.Ordered;
        lineItem.Quantity.QuantityShipped = line.Shipped;
        lineItem.Quantity.QuantityShippedSpecified = true;

        if (string.IsNullOrEmpty(line.OrderResponse.VendorDocumentNumber))
          lineItem.StatusCode = StatusCode.Reject;
        else if (line.Cancelled == line.Ordered)
          lineItem.StatusCode = StatusCode.Delete;
        else if (line.Ordered != line.Shipped)
          lineItem.StatusCode = StatusCode.Change;
        else
          lineItem.StatusCode = StatusCode.Accept;

        lineItem.TaxAmount = line.VatAmount.HasValue ? line.VatAmount.Value : 0;
        lineItem.UnitOfMeasure = ShipmentOrderDetailUnitOfMeasure.EA;
        lineItem.UnitPrice = line.Price;

        lineItem.ShipmentInformation = new ShipmentInformation();
        lineItem.ShipmentInformation.CarrierCode = line.CarrierCode;
        lineItem.ShipmentInformation.NumberOfColli = line.NumberOfUnits.HasValue ? line.NumberOfUnits.Value.ToString() : "1";
        lineItem.ShipmentInformation.NumberOfPallet = line.NumberOfPallets.HasValue ? line.NumberOfPallets.Value.ToString() : "0";
        lineItem.ShipmentInformation.TrackAndTraceNumber = string.IsNullOrEmpty(line.TrackAndTrace) ? response.TrackAndTrace : line.TrackAndTrace;
        lineItem.ShipmentInformation.TrackAndTraceLink = line.TrackAndTrace == null ? response.TrackAndTraceLink : line.TrackAndTraceLink;

        List<DownloadInformation> fullFillmentInformations = new List<DownloadInformation>();

        line.OrderItemFullfilmentInformations.ForEach((x, idx) =>
        {

          var di = new DownloadInformation
          {
            Code = x.Code,
            Label = x.Label,
            Type = x.Type,
            Value = x.Value,
            Html = line.html
          };

          fullFillmentInformations.Add(di);
        });

        lineItem.DownloadInformation = fullFillmentInformations.ToArray();

        replyItems.Add(lineItem);
      }

      ShipmentOrderHeader header = new ShipmentOrderHeader()
      {
        BSKIdentifier = int.Parse(order.BSKIdentifier),
        CustomerOrder = order.CustomerOrderReference,
        OrderNumber = response.VendorDocumentNumber,
        RequestedDate = response.ReqDeliveryDate.HasValue ? response.ReqDeliveryDate.Value : order.ReceivedDate,
        RequestedDateSpecified = response.ReqDeliveryDate.HasValue,
        WebSiteOrderNumber = order.WebSiteOrderNumber,
      };

      header.PackingInformation = new PackingInformation();
      header.PackingInformation.PackingDateTime = response.ReceiveDate;
      header.PackingInformation.PackingNumber = response.ShippingNumber;

      header.ShipmentInformation = new ShipmentInformation();
      header.ShipmentInformation.TrackAndTraceNumber = response.TrackAndTrace;
      header.ShipmentInformation.TrackAndTraceLink = response.TrackAndTraceLink;
      header.ShipmentInformation.CarrierCode = responseLines.FirstOrDefault().CarrierCode;
      header.ShipmentInformation.NumberOfColli = responseLines.Sum(x => x.NumberOfUnits).HasValue ? responseLines.Sum(x => x.NumberOfUnits).Value.ToString() : "0";
      header.ShipmentInformation.NumberOfColli = responseLines.Sum(x => x.NumberOfPallets).HasValue ? responseLines.Sum(x => x.NumberOfPallets).Value.ToString() : "0";

      if (response.PartialDelivery.HasValue && response.PartialDelivery.Value)
        header.FullfillmentCode = ShipmentOrderHeaderFullfillmentCode.Partial;
      else
        header.FullfillmentCode = ShipmentOrderHeaderFullfillmentCode.Complete;

      if (response.ShippedToCustomer != null)
      {
        header.ShipToCustomer = FillShipToCustomer(response.ShippedToCustomer);
        header.CustomerOverride = FillCustomerOverride(response.ShippedToCustomer);
      }
      else
      {
        header.ShipToCustomer = FillShipToCustomer(order.ShippedToCustomer);
        header.CustomerOverride = FillCustomerOverride(order.ShippedToCustomer);
      }

      if (response.SoldToCustomer != null)
        header.SoldToCustomer = FillShipToCustomer(response.SoldToCustomer);
      else
        header.SoldToCustomer = FillShipToCustomer(order.ShippedToCustomer);

      ShippingNotification shippingNotification = new ShippingNotification()
      {
        ShipmentOrderDetails = replyItems.ToArray(),
        ShipmentOrderHeader = header,
        Version = "1.0"
      };

      StringBuilder requestString = new StringBuilder();
      XmlWriterSettings settings = new XmlWriterSettings();
      settings.Encoding = Encoding.UTF8;
      XmlDocument document = new XmlDocument();
      using (XmlWriter xw = XmlWriter.Create(requestString, settings))
      {
        xw.WriteProcessingInstruction("xml", "version=\"1.0\" encoding=\"utf-8\"");
        XmlSerializer serializer = new XmlSerializer(typeof(ShippingNotification));
        XmlSerializerNamespaces nm = new XmlSerializerNamespaces();
        nm.Add("xsi", "http://www.w3.org/2001/XMLSchema-instance");
        serializer.Serialize(xw, shippingNotification, nm);

        document.LoadXml(requestString.ToString());
      }
      return document;
    }

    private Dictionary<decimal, TaxRateCode> _taxrates;

    public Dictionary<decimal, TaxRateCode> TaxRates
    {
      get
      {
        _taxrates = new Dictionary<decimal, TaxRateCode>();
        _taxrates.Add(19, TaxRateCode.NLH);
        _taxrates.Add(6, TaxRateCode.NLL);
        _taxrates.Add(21, TaxRateCode.BEH);
        _taxrates.Add(12, TaxRateCode.BEM);
        _taxrates.Add(0, TaxRateCode.NUL);
        _taxrates.Add(-1, TaxRateCode.VRY);
        _taxrates.Add(99, TaxRateCode.UNKNOWN);
        return _taxrates;
      }
    }

    private XmlDocument GenerateAcknowledgement(Concentrator.Objects.Models.Orders.OrderResponse response, List<OrderResponseLine> responseLines, Concentrator.Objects.Models.Orders.Order order)
    {
      List<OrderResponseDetail> replyItems = new List<OrderResponseDetail>();
      foreach (var line in responseLines)
      {
        OrderResponseDetail lineItem = new OrderResponseDetail();
        lineItem.ProductIdentifier = new Concentrator.Web.Objects.EDI.ProductIdentifier();
        lineItem.ProductIdentifier.ProductNumber = line.ProductID.HasValue ? line.ProductID.Value.ToString() : line.OrderLineID.HasValue && line.OrderLine.ProductID.HasValue ? line.OrderLine.ProductID.Value.ToString() : string.Empty;
        lineItem.ProductIdentifier.ManufacturerItemID = line.ProductID.HasValue ? line.Product.VendorItemNumber : line.OrderLineID.HasValue && line.OrderLine.ProductID.HasValue ? line.OrderLine.Product.VendorItemNumber : string.Empty;
        if (line.OrderLineID.HasValue && line.OrderLine.ProductID.HasValue && line.OrderLine.Product.ProductBarcodes.Count > 0)
          lineItem.ProductIdentifier.EANIdentifier = line.OrderLine.Product.ProductBarcodes.FirstOrDefault().Barcode;
        else if (line.ProductID.HasValue && line.Product.ProductBarcodes.Count > 0)
          lineItem.ProductIdentifier.EANIdentifier = line.Product.ProductBarcodes.FirstOrDefault().Barcode;

        lineItem.CustomerReference = new Concentrator.Web.Objects.EDI.CustomerReference();
        lineItem.CustomerReference.CustomerItemNumber = line.OrderLineID.HasValue ? line.OrderLine.CustomerItemNumber : string.Empty;
        lineItem.CustomerReference.CustomerOrder = line.OrderLineID.HasValue ? line.OrderLine.CustomerOrderNr : string.Empty;
        lineItem.CustomerReference.CustomerOrderLine = line.OrderLineID.HasValue ? line.OrderLine.CustomerOrderLineNr : string.Empty;

        lineItem.LineNumber = line.OrderLineID.HasValue ? line.OrderLineID.Value : 0;
        if (line.DeliveryDate.HasValue)
          lineItem.PromisedDeliveryDate = line.DeliveryDate.Value;

        if (line.RequestDate.HasValue)
          lineItem.RequestedDate = line.RequestDate.Value;

        lineItem.PromisedDeliveryDateSpecified = true;

        lineItem.Quantity = new ResponseQuantity();
        lineItem.Quantity.QuantityBackordered = line.Backordered;
        lineItem.Quantity.QuantityBackorderedSpecified = true;
        lineItem.Quantity.QuantityCancelled = line.Cancelled;
        lineItem.Quantity.QuantityCancelledSpecified = true;
        lineItem.Quantity.QuantityOrdered = line.Ordered;
        lineItem.Quantity.QuantityShipped = line.Shipped;
        lineItem.Quantity.QuantityShippedSpecified = true;

        if (string.IsNullOrEmpty(line.OrderResponse.VendorDocumentNumber))
          lineItem.StatusCode = ResponseStatusCode.Reject;
        else if (line.Cancelled == line.Ordered)
          lineItem.StatusCode = ResponseStatusCode.Delete;
        else if (line.Ordered != line.Shipped)
          lineItem.StatusCode = ResponseStatusCode.Change;
        else
          lineItem.StatusCode = ResponseStatusCode.Accept;

        lineItem.TaxAmount = line.VatAmount.HasValue ? line.VatAmount.Value : 0;
        lineItem.TaxRate = line.vatPercentage.HasValue ? TaxRates[line.vatPercentage.Value] : TaxRates[0];
        lineItem.UnitOfMeasure = OrderResponseDetailUnitOfMeasure.EA;
        lineItem.UnitPrice = line.Price;
        replyItems.Add(lineItem);
      }

      OrderResponseHeader header = new OrderResponseHeader()
      {
        BSKIdentifier = int.Parse(order.BSKIdentifier),
        CustomerOrder = order.CustomerOrderReference,
        OrderNumber = response.VendorDocumentNumber,
        RequestedDate = response.ReqDeliveryDate.HasValue ? response.ReqDeliveryDate.Value : order.ReceivedDate,
        RequestedDateSpecified = response.ReqDeliveryDate.HasValue,
        WebSiteOrderNumber = order.WebSiteOrderNumber
      };

      if (response.PartialDelivery.HasValue && response.PartialDelivery.Value)
        header.FullfillmentCode = OrderResponseHeaderFullfillmentCode.Partial;
      else
        header.FullfillmentCode = OrderResponseHeaderFullfillmentCode.Complete;

      header.Payment = new Web.Objects.EDI.Payment();
      header.Payment.PaymentTerm = response.PaymentConditionDays.HasValue ? response.PaymentConditionDays.Value.ToString() : string.Empty;
      header.Payment.PaymentType = response.PaymentConditionCode;

      if (response.ShippedToCustomer != null)
      {
        header.ShipToCustomer = FillShipToCustomer(response.ShippedToCustomer);
        header.CustomerOverride = FillCustomerOverride(response.ShippedToCustomer);
      }
      else
      {
        header.ShipToCustomer = FillShipToCustomer(order.ShippedToCustomer);
        header.CustomerOverride = FillCustomerOverride(order.ShippedToCustomer);
      }

      if (response.SoldToCustomer != null)
        header.SoldToCustomer = FillShipToCustomer(response.SoldToCustomer);
      else
        header.SoldToCustomer = FillShipToCustomer(order.ShippedToCustomer);


      Concentrator.Web.Objects.EDI.OrderResponse OrderResponse = new Concentrator.Web.Objects.EDI.OrderResponse()
      {
        OrderDetails = replyItems.ToArray(),
        OrderHeader = header,
        Version = "1.0"
      };

      StringBuilder requestString = new StringBuilder();
      XmlWriterSettings settings = new XmlWriterSettings();
      settings.Encoding = Encoding.UTF8;
      XmlDocument document = new XmlDocument();
      using (XmlWriter xw = XmlWriter.Create(requestString, settings))
      {
        xw.WriteProcessingInstruction("xml", "version=\"1.0\" encoding=\"utf-8\"");
        XmlSerializer serializer = new XmlSerializer(typeof(Concentrator.Web.Objects.EDI.OrderResponse));
        XmlSerializerNamespaces nm = new XmlSerializerNamespaces();
        nm.Add("xsi", "http://www.w3.org/2001/XMLSchema-instance");
        serializer.Serialize(xw, OrderResponse, nm);

        document.LoadXml(requestString.ToString());
      }
      return document;
    }

    #region CustomerInformation
    private Concentrator.Web.Objects.EDI.Customer FillSoldToCustomer(Concentrator.Objects.Models.Orders.Customer customer)
    {
      Concentrator.Web.Objects.EDI.Customer soldToCustomer = new Concentrator.Web.Objects.EDI.Customer();
      soldToCustomer.EanIdentifier = customer.EANIdentifier;
      soldToCustomer.Contact = new Contact();
      soldToCustomer.Contact.Email = customer.CustomerEmail;
      soldToCustomer.Contact.Name = customer.CustomerName;
      soldToCustomer.Contact.PhoneNumber = customer.CustomerTelephone;
      soldToCustomer.CustomerAddress = new Address();
      soldToCustomer.CustomerAddress.AddressLine1 = customer.CustomerAddressLine1;
      soldToCustomer.CustomerAddress.AddressLine2 = customer.CustomerAddressLine2;
      soldToCustomer.CustomerAddress.AddressLine3 = customer.CustomerAddressLine3;
      soldToCustomer.CustomerAddress.City = customer.City;
      soldToCustomer.CustomerAddress.Country = customer.Country;
      soldToCustomer.CustomerAddress.HouseNumber = customer.HouseNumber;
      soldToCustomer.CustomerAddress.Name = customer.CustomerName;
      soldToCustomer.CustomerAddress.ZipCode = customer.PostCode;
      return soldToCustomer;
    }

    private Concentrator.Web.Objects.EDI.Customer FillShipToCustomer(Concentrator.Objects.Models.Orders.Customer customer)
    {
      Concentrator.Web.Objects.EDI.Customer shipToCustomer = new Concentrator.Web.Objects.EDI.Customer();
      shipToCustomer.EanIdentifier = customer.EANIdentifier;
      shipToCustomer.Contact = new Contact();
      shipToCustomer.Contact.Email = customer.CustomerEmail;
      shipToCustomer.Contact.Name = customer.CustomerName;
      shipToCustomer.Contact.PhoneNumber = customer.CustomerTelephone;
      shipToCustomer.CustomerAddress = new Address();
      shipToCustomer.CustomerAddress.AddressLine1 = customer.CustomerAddressLine1;
      shipToCustomer.CustomerAddress.AddressLine2 = customer.CustomerAddressLine2;
      shipToCustomer.CustomerAddress.AddressLine3 = customer.CustomerAddressLine3;
      shipToCustomer.CustomerAddress.City = customer.City;
      shipToCustomer.CustomerAddress.Country = customer.Country;
      shipToCustomer.CustomerAddress.HouseNumber = customer.HouseNumber;
      shipToCustomer.CustomerAddress.Name = customer.CustomerName;
      shipToCustomer.CustomerAddress.ZipCode = customer.PostCode;
      return shipToCustomer;
    }

    private CustomerOverride FillCustomerOverride(Concentrator.Objects.Models.Orders.Customer customer)
    {
      CustomerOverride customerOverride = new CustomerOverride();
      customerOverride.CustomerContact = new Contact();
      customerOverride.CustomerContact.Email = customer.CustomerEmail;
      customerOverride.CustomerContact.Name = customer.CustomerName;
      customerOverride.CustomerContact.PhoneNumber = customer.CustomerTelephone;
      customerOverride.Dropshipment = true;
      customerOverride.OrderAddress = new Address();
      customerOverride.OrderAddress.AddressLine1 = customer.CustomerAddressLine1;
      customerOverride.OrderAddress.AddressLine2 = customer.CustomerAddressLine2;
      customerOverride.OrderAddress.AddressLine3 = customer.CustomerAddressLine3;
      customerOverride.OrderAddress.City = customer.City;
      customerOverride.OrderAddress.Country = customer.Country;
      customerOverride.OrderAddress.HouseNumber = customer.HouseNumber;
      customerOverride.OrderAddress.Name = customer.CustomerName;
      customerOverride.OrderAddress.ZipCode = customer.PostCode;

      return customerOverride;
    }
    #endregion

    private bool ProcessPurchaseConfirmation(List<OrderResponseLine> responseLines, Vendor administrativeVendor, IUnitOfWork unit, Concentrator.Objects.Models.Orders.Order order)
    {
      List<ConfirmationLine> confirmationLines = new List<ConfirmationLine>();
      string purchaseOrderNumber = string.Empty;

      foreach (var responseLine in responseLines)
      {
        var type = OrderResponseTypes.PurchaseAcknowledgement.ToString();

        var purchaseAcknowledgement = unit.Scope.Repository<OrderResponseLine>().GetSingle(x => x.OrderResponse.ResponseType == type && x.OrderLineID == responseLine.OrderLineID);

        if (purchaseAcknowledgement == null)
        {
          log.DebugFormat("Failed generate ProcessPurchaseConfirmation no purchaseack for orderline {0} order {1}", responseLine.OrderLineID, responseLine.OrderResponse.OrderID);
          if (responseLine.OrderResponse.OrderID.HasValue)
            array.Add(responseLine.OrderResponse.OrderID.Value);

          return false;
        }
        var cLine = new ConfirmationLine()
      {
        ItemNumber = purchaseAcknowledgement.VendorItemNumber,
        LineNumber = purchaseAcknowledgement.VendorLineNumber,
        Quantity = responseLine.Shipped.ToString(),
        BackendOrderVendor = order.OrderID.ToString(),
        LineReference = responseLine.OrderLineID.ToString()
      };

        confirmationLines.Add(cLine);
        if (string.IsNullOrEmpty(purchaseOrderNumber))
          purchaseOrderNumber = purchaseAcknowledgement.OrderResponse.VendorDocumentNumber;
      }

      var administrativeVendorSettings = administrativeVendor.PreferredConnectorVendors.Where(x => x.ConnectorID == order.ConnectorID).FirstOrDefault();

      PurchaseConfirmation conf = new PurchaseConfirmation()
    {
      bskIdentifier = !string.IsNullOrEmpty(administrativeVendorSettings.VendorIdentifier) ? administrativeVendorSettings.VendorIdentifier : responseLines.FirstOrDefault(x => x.OrderLineID.HasValue).OrderLine.Order.BSKIdentifier.ToString(),
      PurchaseOrderNumber = purchaseOrderNumber,
      Version = "1.0",
      PurchaseConfirmationLine = confirmationLines.ToArray()
    };

      var purchaseOrderVendor = (IPurchase)
                 Activator.CreateInstance(Assembly.GetAssembly(typeof(IPurchase)).GetType(administrativeVendor.PurchaseOrderType));

      purchaseOrderVendor.PurchaseConfirmation(conf, administrativeVendor, responseLines.Where(x => x.OrderLineID.HasValue).Select(x => x.OrderLine).ToList());

      return true;
    }

    private void ProcessOrderChange(List<OrderResponseLine> responseLines, Vendor administrativeVendor, IUnitOfWork unit, Concentrator.Objects.Models.Orders.Order order)
    {
      List<OrderChangeDetail> OrderChangeLines = new List<OrderChangeDetail>();
      string orderNumber = string.Empty;

      foreach (var responseLine in responseLines)
      {
        var type = OrderResponseTypes.PurchaseAcknowledgement.ToString();

        var purchaseAcknowledgement = unit.Scope.Repository<OrderResponseLine>().GetSingle(x => x.OrderResponse.ResponseType == type && x.OrderLineID == responseLine.OrderLineID);


        if (purchaseAcknowledgement == null)
          continue;

        var cLine = new OrderChangeDetail()
        {
          ChangeType = responseLine.GetChangeType().HasValue ? responseLine.GetChangeType().Value : ChangeType.Unkown,
          VendorItemNumber = responseLine.OrderLineID.HasValue ? responseLine.OrderLine.Product.VendorItemNumber : responseLine.ProductID.HasValue ? responseLine.Product.VendorItemNumber : string.Empty
        };

        switch (cLine.ChangeType)
        {
          case ChangeType.Change:
            cLine.Quantity = responseLine.Ordered - responseLine.Cancelled;
            break;
          case ChangeType.Delete:
            cLine.Quantity = responseLine.Cancelled;
            break;
          case ChangeType.Add:
            cLine.Quantity = responseLine.Ordered;
            break;
        }

        cLine.ProductIdentifier = new Concentrator.Web.Objects.EDI.ChangeOrder.ProductIdentifier();
        cLine.ProductIdentifier.ProductNumber = purchaseAcknowledgement.VendorItemNumber;

        OrderChangeLines.Add(cLine);

        if (string.IsNullOrEmpty(orderNumber))
          orderNumber = purchaseAcknowledgement.OrderResponse.VendorDocumentNumber;
      }

      if (OrderChangeLines.Count > 0)
      {
        var administrativeVendorSettings = administrativeVendor.PreferredConnectorVendors.Where(x => x.ConnectorID == order.ConnectorID).FirstOrDefault();

        OrderChangeHeader conf = new OrderChangeHeader()
        {
          BSKIdentifier = administrativeVendorSettings.VendorIdentifier != null ? int.Parse(administrativeVendorSettings.VendorIdentifier) : int.Parse(order.BSKIdentifier),
          OrderNumber = int.Parse(orderNumber),
          OrderNumberSpecified = true
        };

        ChangeOrderRequest request = new ChangeOrderRequest();
        request.ChangeType = ChangeType.Change;
        request.OrderChangeDetails = OrderChangeLines.ToArray();
        request.OrderChangeHeader = conf;
        request.Version = "1.0";

        var OrderChangeVendor = (IPurchase)
                   Activator.CreateInstance(Assembly.GetAssembly(typeof(IPurchase)).GetType(administrativeVendor.PurchaseOrderType));

        OrderChangeVendor.OrderChange(request, administrativeVendor, responseLines.Where(x => x.OrderLineID.HasValue).Select(x => x.OrderLine).ToList());
      }
    }

    private void ParseDocument(List<OrderResponseLine> responseLines, IUnitOfWork unit, OrderLineStatus status, bool isOutboundMessage, XmlDocument doc, OrderResponseTypes type, string outboundUrl = null, bool ledgerQuantity = false)
    {
      if (isOutboundMessage)
      {
        Outbound outbound = new Outbound()
        {
          ConnectorID = responseLines.FirstOrDefault(x => x.OrderLineID.HasValue).OrderLine.Order.ConnectorID,
          CreationTime = DateTime.Now.ToUniversalTime(),
          OrderID = responseLines.FirstOrDefault(x => x.OrderLineID.HasValue).OrderLine.OrderID,
          OutboundMessage = doc.OuterXml,
          Processed = false,
          Type = type.ToString(),
          OutboundUrl = !string.IsNullOrEmpty(outboundUrl) ? outboundUrl : responseLines.FirstOrDefault(x => x.OrderLineID.HasValue) != null && !string.IsNullOrEmpty(responseLines.FirstOrDefault(x => x.OrderLineID.HasValue).OrderLine.Order.Connector.OutboundUrl) ? responseLines.FirstOrDefault(x => x.OrderLineID.HasValue).OrderLine.Order.Connector.OutboundUrl : "MISSING"
        };

        unit.Scope.Repository<Outbound>().Add(outbound);
      }

      responseLines.ForEach(c =>
      {
        if (ledgerQuantity)
          c.OrderLine.SetStatus(status, unit.Scope.Repository<OrderLedger>(), c.Delivered, useStatusOnNonAssortmentItems: true);

        else
          c.OrderLine.SetStatus(status, unit.Scope.Repository<OrderLedger>(), useStatusOnNonAssortmentItems: true);

        c.Processed = true;
      });

    }
  }
}
