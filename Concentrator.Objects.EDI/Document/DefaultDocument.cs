using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Concentrator.Objects.Models.EDI.Response;
using Concentrator.Objects.Models.EDI.Order;
using Concentrator.Web.Objects.EDI;
using System.Xml.Serialization;

namespace Concentrator.Objects.EDI
{
  public class DefaultDocument
  {
    public InvoiceNotification GenerateInvoiceNotification(EdiOrderResponse response, IEdiProcessor processor, System.Configuration.Configuration config)
    {
      List<EdiOrderResponseLine> responseLines = response.EdiOrderResponseLines.ToList();
      EdiOrder order = response.EdiOrder;

      if (order == null)
        order = processor.GetOrderInformation(response, config);

      List<InvoiceOrderDetail> replyItems = new List<InvoiceOrderDetail>();
      foreach (var line in responseLines)
      {
        InvoiceOrderDetail lineItem = new InvoiceOrderDetail();
        if (line.EdiOrderLineID.HasValue)
        {
          lineItem.ProductIdentifier = new Concentrator.Web.Objects.EDI.ProductIdentifier();
          lineItem.ProductIdentifier.ProductNumber = line.EdiOrderLine.ProductID.HasValue ? line.EdiOrderLine.ProductID.Value.ToString() : line.EdiOrderLine.CustomerItemNumber;
          //lineItem.ProductIdentifier.ManufacturerItemID = line.EdiOrderLine.ProductID.HasValue ? line.EdiOrderLine.Product.VendorItemNumber : string.Empty;
          //if (line.EdiOrderLine.ProductID.HasValue && line.EdiOrderLine.Product.ProductBarcodes.Count > 0)
          //  lineItem.ProductIdentifier.EANIdentifier = line.EdiOrderLine.Product.ProductBarcodes.FirstOrDefault().Barcode;

          lineItem.CustomerReference = new Concentrator.Web.Objects.EDI.CustomerReference();
          lineItem.CustomerReference.CustomerItemNumber = line.EdiOrderLine.CustomerItemNumber;
          lineItem.CustomerReference.CustomerOrder = line.EdiOrderLine.CustomerOrderNr;
          lineItem.CustomerReference.CustomerOrderLine = line.EdiOrderLine.CustomerEdiOrderLineNr;
          lineItem.LineNumber = line.EdiOrderLineID.ToString();
        }
        else
        {
          lineItem.ProductIdentifier = new Concentrator.Web.Objects.EDI.ProductIdentifier();
          lineItem.ProductIdentifier.ProductNumber = line.VendorItemNumber;
          //lineItem.ProductIdentifier.ManufacturerItemID = line.EdiOrderLine.ProductID.HasValue ? line.EdiOrderLine.Product.VendorItemNumber : string.Empty;
          //if (line.EdiOrderLine.ProductID.HasValue && line.EdiOrderLine.Product.ProductBarcodes.Count > 0)
          //  lineItem.ProductIdentifier.EANIdentifier = line.EdiOrderLine.Product.ProductBarcodes.FirstOrDefault().Barcode;

          lineItem.CustomerReference = new Concentrator.Web.Objects.EDI.CustomerReference();
          lineItem.CustomerReference.CustomerItemNumber = string.Empty;
          lineItem.CustomerReference.CustomerOrder = line.Remark;
          lineItem.CustomerReference.CustomerOrderLine = line.VendorLineNumber;
        }

        if (line.DeliveryDate.HasValue)
          lineItem.PromissedDeliveryDate = line.DeliveryDate.Value;

        if (line.RequestDate.HasValue)
          lineItem.RequestedDate = line.RequestDate.Value;
        else if (order.RequestDate.HasValue)
          lineItem.RequestedDate = order.RequestDate.Value;

        lineItem.Quantity = new Quantity();
        lineItem.Quantity.QuantityBackordered = line.Backordered;
        lineItem.Quantity.QuantityBackorderedSpecified = true;
        lineItem.Quantity.QuantityCancelled = line.Cancelled;
        lineItem.Quantity.QuantityCancelledSpecified = true;
        lineItem.Quantity.QuantityOrdered = line.Ordered;
        lineItem.Quantity.QuantityShipped = line.Invoiced;
        lineItem.Quantity.QuantityShippedSpecified = true;

        if (string.IsNullOrEmpty(line.EdiOrderResponse.VendorDocumentNumber))
          lineItem.StatusCode = StatusCode.Reject;
        else if (line.Cancelled == line.Ordered)
          lineItem.StatusCode = StatusCode.Delete;
        else if (line.Ordered != line.Shipped)
          lineItem.StatusCode = StatusCode.Change;
        else
          lineItem.StatusCode = StatusCode.Accept;

        if (line.VatAmount.HasValue)
          lineItem.TaxAmount = line.VatAmount.Value;

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
        BSKIdentifier = order.ConnectorRelationID.HasValue ? order.ConnectorRelationID.Value : 0,
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
        header.SoldToCustomer = FillShipToCustomer(order.SoldToCustomer);

      if (response.InvoiceDate.HasValue)
        header.InvoiceDate = response.InvoiceDate.Value;

      header.InvoiceNumber = response.InvoiceDocumentNumber;
      if (response.VatAmount.HasValue)
        header.InvoiceTax = response.VatAmount.Value.ToString();
      if (response.TotalExVat.HasValue)
        header.InvoiceTaxableAmount = response.TotalExVat.Value.ToString();
      if (response.TotalAmount.HasValue)
        header.InvoiceTotalInc = response.TotalAmount.Value.ToString();
      if (!string.IsNullOrEmpty(response.PaymentConditionDiscount))
        header.DisountAmount = response.PaymentConditionDiscount;

      InvoiceNotification invoiceNotification = new InvoiceNotification()
      {
        InvoiceOrderDetails = replyItems.ToArray(),
        InvoiceOrderHeader = header,
        Version = "1.0"
      };

      return invoiceNotification;
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

  }
}
