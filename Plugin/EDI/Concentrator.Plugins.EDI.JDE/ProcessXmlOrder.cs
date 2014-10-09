using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Microsoft.Practices.ServiceLocation;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects.Models.Orders;
using Concentrator.Objects.Models.EDI.Order;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.Models.EDI.Response;
using Concentrator.Objects.Models.EDI.Enumerations;
using System.Configuration;
using Concentrator.Plugins.EDI.JDE.DataAcces;
using Concentrator.Plugins.EDI.JDE.Utility;
using Concentrator.Plugins.EDI.JDE.Version2;
using Concentrator.Objects.EDI;

namespace Concentrator.Plugins.EDI.JDE
{

  public class ProcessXmlOrder : IEdiProcessor
  {

    #region IEdiProcessor Members

    public string DocumentType(string requestDocument)
    {
      #region Types
      try
      {
        XDocument xdoc = XDocument.Parse(requestDocument);

        if (xdoc.Root.Name.LocalName == "ProductRequest")
        {
          return JDEDocumentType.ProductXML.ToString();
        }

        if (xdoc.Root.Name.LocalName == "PurchaseRequest")
        {
          return JDEDocumentType.PurchaseXML.ToString();
        }

        if (xdoc.Root.Name.LocalName == "DirectShipmentRequest")
        {
          return JDEDocumentType.DirectShipmentXML.ToString();
        }

        if (xdoc.Root.Name.LocalName.Contains("TradeplaceMessage"))
        {
          return JDEDocumentType.XML.ToString();
        }

        if (xdoc.Root.Name.LocalName == "PurchaseConfirmation")
        {
          return JDEDocumentType.PurchaseConfirmationXML.ToString();
        }

        if (xdoc.Root.Name.LocalName == "InvoiceMessage")
        {
          return JDEDocumentType.InvoiceXML.ToString();
        }

        if (xdoc.Root.Name.LocalName == "ChangeOrderRequest")
        {
          return JDEDocumentType.OrderChangeXML.ToString();
        }

        if (xdoc.Root.Name.LocalName == "ChangePurchangeOrderRequest")
        {
          return JDEDocumentType.PurchaseOrderChangeXML.ToString();
        }

        if (xdoc.Root.Name.LocalName.Contains("OrderRequest"))
        {
          return JDEDocumentType.XML.ToString();
        }

        return "UNKNOWN Type";
      }
      catch (Exception ex)
      {
        return "UNKNOWN Type";
      }
      #endregion
    }

    public EdiOrderResponse ValidateOrder(Objects.Models.EDI.Order.EdiOrder order, Objects.Models.EDI.Vendor.EdiVendor ediVendor, Objects.Models.Connectors.ConnectorRelation ediRelation, System.Configuration.Configuration config)
    {
      string connectionString = config.AppSettings.Settings["JDEconnectionString"].Value;
      string environment = config.AppSettings.Settings["Environment"].Value;

      EdiOrderResponse reponse = new EdiOrderResponse();

      if (CheckCustomerReference(order.CustomerOrderReference, order.SoldToCustomer.EANIdentifier, connectionString, environment))
        reponse.ResponseErrors.Add(new ReturnError() { ErrorMessage = "Customer Orderreference already Exists for SoldToCustomer" });

      return reponse;
    }

    public bool CheckCustomerReference(string customerOrderReference, string soldToCustomer, string connection, string environment)
    {
      EDICommunicationLayer communication = new EDICommunicationLayer(EdiConnectionType.SQL, connection);

      var query = string.Format("SELECT Count(*) FROM {0}.F47011 WHERE SYVR01 = {1} and SYAN8 = {2}", environment, customerOrderReference, soldToCustomer);

      return !communication.IsValid(query);
    }


    public List<Objects.Models.EDI.Order.EdiOrder> ProcessOrder(string type, string document, int? connectorID)
    {
        XDocument xdocument = XDocument.Parse(document);

        using (var unit = ServiceLocator.Current.GetInstance<IUnitOfWork>())
        {
          #region ParseXML
          var header = xdocument.Element("WebOrderRequest").Element("WebOrderHeader");
          var details = xdocument.Element("WebOrderRequest").Element("WebOrderDetails").Elements("WebOrderDetail");
          var customer = xdocument.Element("WebOrderRequest").Element("WebCustomer");
          var ord = new
          {
            OrderHeader = new
            {
              isDropShipment =
                header.Element("CustomerOverride").Element("Dropshipment").Try(
                  c => bool.Parse(c.Value), false),
              CustomerOrderReference = header.Element("CustomerOrderReference").Value,
              EdiVersion = header.Element("EdiVersion").Value,
              BSKIdentifier = header.Element("BSKIdentifier").Value,
              WebSiteOrderNumber = header.Element("WebSiteOrderNumber").Value,
              PaymentTermsCode = header.Element("PaymentTermsCode").Value,
              PaymentInstrument = header.Element("PaymentInstrument").Value,
              BackOrdersAllowed = header.Try(c => bool.Parse(c.Element("BackOrdersAllowed").Value), false),
              RouteCode = header.Try(c => c.Element("RouteCode").Value, string.Empty),
              EANIdentifier =
                header.Element("ShipToCustomer").Try(
                  c => c.Element("EanIdentifier").Value, string.Empty).Trim(),
              HoldCode = header.Try(c => c.Element("HoldCode").Value, string.Empty),
              NonDropDetails = new
              {
                ShipToShopID =
                  header.Try(c => c.Element("ShipToShopID").Value,
                             string.Empty)
              }
            },
            Customer = new
            {
              Name = header.Try(dc => dc.Element("CustomerOverride").Element("OrderAddress").Element("Name").Value, string.Empty),
              AddressLine1 = header.Try(dc => dc.Element("CustomerOverride").Element("OrderAddress").Element("AddressLine1").Value, string.Empty),
              AddressLine2 = header.Try(dc => dc.Element("CustomerOverride").Element("OrderAddress").Element("AddressLine2").Value, string.Empty),
              AddressLine3 = header.Try(dc => dc.Element("CustomerOverride").Element("OrderAddress").Element("AddressLine3").Value, string.Empty),
              HouseNumber = header.Try(dc => dc.Element("CustomerOverride").Element("OrderAddress").Element("HouseNumber").Value, string.Empty),
              PostCode = header.Try(dc => dc.Element("CustomerOverride").Element("OrderAddress").Element("ZipCode").Value, string.Empty),
              City = header.Try(dc => dc.Element("CustomerOverride").Element("OrderAddress").Element("City").Value, string.Empty),
              Country = header.Try(dc => dc.Element("CustomerOverride").Element("OrderAddress").Element("Country").Value, string.Empty),
              Email = header.Try(dc => dc.Element("CustomerOverride").Element("CustomerContact").Element("Email").Value, string.Empty)
            },
            OrderLines = (from o in details
                          let custReference = o.Element("CustomerReference")
                          let product = o.Element("ProductIdentifier")
                          select new
                          {
                            CustomerReference = custReference == null ? null : new
                            {
                              CustomerOrder = custReference.Try(c => c.Element("CustomerOrder").Value, string.Empty),
                              CustomerOrderLine = custReference.Try(c => c.Element("CustomerOrderLine").Value, string.Empty),
                              CustomerItemNumber = custReference.Try(c => c.Element("CustomerItemNumber").Value, string.Empty)
                            },
                            Product = product == null ? null : new
                            {
                              ID = product.Element("ProductNumber").Value,
                              EAN = product.Try(c => c.Element("EANIdentifier").Value, string.Empty),
                              ManufacturerItemID = product.Try(c => c.Element("ManufacturerItemID").Value, string.Empty),
                              Qty = o.Try(c => int.Parse(c.Element("Quantity").Value), 1),
                              VendorItemNumber = o.Try(c => c.Element("VendorItemNumber").Value, string.Empty),
                              WarehouseCode = o.Try(c => c.Element("WareHouseCode").Value, string.Empty),
                              UnitPrice = o.Try(c => double.Parse(c.Element("UnitPrice").Value) / 10000, 0),
                              PriceOverride = o.Try(c => bool.Parse(c.Element("PriceOverride").Value), false)
                            }



                          }).ToList(),
            CustomerContactInfo = customer == null ? null : new
            {
              Name = customer.Try(c => c.Element("CustomerAddress").Element("Name").Value, string.Empty),
              AddressLine1 = customer.Try(c => c.Element("CustomerAddress").Element("AddressLine1").Value, string.Empty),
              AddressLine2 = customer.Try(c => c.Element("CustomerAddress").Element("AddressLine2").Value, string.Empty),
              AddressLine3 = customer.Try(c => c.Element("CustomerAddress").Element("AddressLine3").Value, string.Empty),
              Number = customer.Try(c => c.Element("CustomerAddress").Element("Number").Value, string.Empty),
              ZipCode = customer.Try(c => c.Element("CustomerAddress").Element("ZipCode").Value, string.Empty),
              City = customer.Try(c => c.Element("CustomerAddress").Element("City").Value, string.Empty),
              Country = customer.Try(c => c.Element("CustomerAddress").Element("Country").Value, string.Empty),
              Email = customer.Try(c => c.Element("CustomerContact").Element("Email").Value, string.Empty),
              CompanyName = customer.Try(c => c.Element("CustomerContact").Element("Name").Value, string.Empty),
              CoCNumber = customer.Try(c => c.Element("CustomerContact").Element("CoCNumber").Value, string.Empty)
            }
          };
          #endregion

          #region Dropshipment/Non-Drop

          //Customer customerE = (from c in context.Customers
          //                      where c.EANIdentifier == ord.OrderHeader.EANIdentifier
          //                      select c).FirstOrDefault();

          //if (customerE == null ||  ord.OrderHeader.EANIdentifier == "0")
          //{
          Concentrator.Objects.Models.Orders.Customer customerE = new Concentrator.Objects.Models.Orders.Customer
          {
            EANIdentifier = ord.OrderHeader.EANIdentifier
          };
          unit.Scope.Repository<Concentrator.Objects.Models.Orders.Customer>().Add(customerE);
          //}
          if (ord.Customer != null)
          {
            customerE.CustomerAddressLine1 = ord.Customer.AddressLine1;
            customerE.CustomerAddressLine2 = ord.Customer.AddressLine2;
            customerE.CustomerAddressLine3 = ord.Customer.AddressLine3;
            customerE.HouseNumber = ord.Customer.HouseNumber;
            customerE.City = ord.Customer.City;
            customerE.Country = ord.Customer.Country;
            customerE.PostCode = ord.Customer.PostCode;
            customerE.CustomerName = ord.Customer.Name;
            customerE.CustomerEmail = ord.Customer.Email;//ord.Try(c => c.CustomerContactInfo.Email, string.Empty);
          }

          Concentrator.Objects.Models.Orders.Customer customerS = null;
          if (ord.CustomerContactInfo != null)
          {
            customerS = new Concentrator.Objects.Models.Orders.Customer
            {
              EANIdentifier = null
            };
            customerS.CustomerAddressLine1 = ord.CustomerContactInfo.AddressLine1;
            customerS.CustomerAddressLine2 = ord.CustomerContactInfo.AddressLine2;
            customerS.CustomerAddressLine3 = ord.CustomerContactInfo.AddressLine3;
            customerS.HouseNumber = ord.CustomerContactInfo.Number;
            customerS.City = ord.CustomerContactInfo.City;
            customerS.Country = ord.CustomerContactInfo.Country;
            customerS.PostCode = ord.CustomerContactInfo.ZipCode;
            customerS.CustomerName = ord.CustomerContactInfo.Name;
            customerS.CustomerEmail = ord.CustomerContactInfo.Email;
            customerS.CoCNumber = ord.CustomerContactInfo.CoCNumber;

            unit.Scope.Repository<Concentrator.Objects.Models.Orders.Customer>().Add(customerS);
          }

          if (!connectorID.HasValue)
          {
            ConnectorRelation rel = unit.Scope.Repository<ConnectorRelation>().GetSingle(x => x.CustomerID == customerE.EANIdentifier);

            if (rel == null || !rel.ConnectorID.HasValue)
              throw new Exception("No Connector Relation found for numer" + customerE.EANIdentifier);
            else
              connectorID = rel.ConnectorID.Value;
          }

          EdiOrder newOrd = new EdiOrder()
          {
            ShippedToCustomer = customerE,
            SoldToCustomer = customerS,
            Document = document,
            ConnectorID = connectorID.Value,
            isDropShipment = ord.OrderHeader.isDropShipment,
            ReceivedDate = DateTime.Now,
            BackOrdersAllowed = ord.OrderHeader.BackOrdersAllowed,
            CustomerOrderReference = ord.OrderHeader.CustomerOrderReference,
            EdiVersion = ord.OrderHeader.EdiVersion,
            BSKIdentifier = int.Parse(ord.OrderHeader.BSKIdentifier),
            WebSiteOrderNumber = ord.OrderHeader.WebSiteOrderNumber,
            PaymentInstrument = ord.OrderHeader.PaymentInstrument,
            PaymentTermsCode = ord.OrderHeader.PaymentTermsCode,
            RouteCode = ord.OrderHeader.RouteCode,
            HoldCode = ord.OrderHeader.HoldCode
          };
          #endregion
          var orderRepo = unit.Scope.Repository<EdiOrder>();
          var duplicateOrder = orderRepo.GetSingle(d => d.ConnectorID == newOrd.ConnectorID
                                && d.WebSiteOrderNumber == newOrd.WebSiteOrderNumber);


          if (duplicateOrder != null)
            newOrd.HoldOrder = true;
          else
            newOrd.HoldOrder = false;

          orderRepo.Add(newOrd);

          #region Order Lines
          foreach (var line in ord.OrderLines)
          {
            int? productid = null;
            var product = unit.Scope.Repository<Concentrator.Objects.Models.Products.Product>().GetSingle(p => p.ProductID == int.Parse(line.Product.ID));

            if (product != null)
              productid = product.ProductID;

            EdiOrderLine ol = new EdiOrderLine
            {
              CustomerEdiOrderLineNr = line.CustomerReference.CustomerOrderLine,
              CustomerOrderNr = line.CustomerReference.CustomerOrder,
              CustomerItemNumber = line.CustomerReference.CustomerItemNumber,
              EdiOrder = newOrd,
              Quantity = line.Product.Qty,
              ProductID = productid,
              Price = line.Product.UnitPrice,
              WareHouseCode = line.Product.WarehouseCode,
              PriceOverride = line.Product.PriceOverride,
              EdiProductID = line.Product.ID
            };

            unit.Scope.Repository<EdiOrderLine>().Add(ol);
          }
          #endregion

          unit.Save();

          return new List<EdiOrder>(){newOrd};
        }
    }

    public int ProcessOrderToVendor(EdiOrder order, ConnectorRelation ediRelation, Configuration config)
    {
      string company = ediRelation.EdiVendor.CompanyCode;
      string businessUnit = string.Empty;
      if (company.Contains("-"))
      {
        businessUnit = company.Split('-')[1];
        company = company.Split('-')[0];
      }
      string dcto = ediRelation.EdiVendor.DefaultDocumentType;

      #region CreateOrder
      using (JDEDataContext context = new JDEDataContext(ConfigurationManager.ConnectionStrings["JDE"].ConnectionString))
      {
        if (order.isDropShipment.HasValue && order.isDropShipment.Value)
        {
          #region DropShipment
          Addresses address = new Addresses();
          address.AdressType = '2';
          address.MailingName = order.ShippedToCustomer.CustomerName;
          address.AddressLine1 = order.ShippedToCustomer.CustomerAddressLine1;
          address.AddressLine2 = order.ShippedToCustomer.CustomerAddressLine2;
          address.AddressLine3 = order.ShippedToCustomer.CustomerAddressLine3; ;
          if (!string.IsNullOrEmpty(order.ShippedToCustomer.CustomerEmail))
            address.AddressLine4 = order.ShippedToCustomer.CustomerEmail;

          address.ZIPCode = order.ShippedToCustomer.PostCode;
          address.City = order.ShippedToCustomer.City;
          if (!string.IsNullOrEmpty(order.ShippedToCustomer.Country))
          {
            switch (order.ShippedToCustomer.Country)
            {
              case "Nederland":
              case "Nederlands":
                address.Country = "NL";
                break;
              case "United Kingdom":
                address.Country = "UK";
                break;
              default:
                address.Country = order.ShippedToCustomer.Country;
                break;
            }
          }
          //else
          // address.Country = shipToCustomer.Country;

          address.EDICompany = company;
          address.EDIDocumentType = dcto;
          address.EDIDocumentNumber = order.EdiRequestID;
          address.DocumentCompany = company;
          address.DocumentType = dcto;
          address.FileName = "F47011";

          address.TransactionOriginator = ediRelation.EdiVendor.Name;
          address.UserID = ediRelation.EdiVendor.Name;
          address.ProgramID = "EDI";
          address.Date = JulianDate.FromDate(System.DateTime.Now).Value;
          address.Time = double.Parse(System.DateTime.Now.ToString("HHmmss"));

          context.Addresses.InsertOnSubmit(address);
          #endregion
        }

        OrderHeader header = new OrderHeader();

        header.EDICompany = company;
        header.EDIDocumentType = dcto;
        header.EDIDocumentNumber = order.EdiRequestID;

        header.DocumentCompany = company;
        header.DocumentType = dcto;
        header.BusinessUnit = businessUnit.PadLeft(12);
        header.IsProcessed = ' ';

        header.EDIMessageType = "850";

        header.EDIOrderReference = order.EdiRequestID.ToString();

        header.CustomerReference = JdeUtility.FilterValue(order.CustomerOrderReference, 25);
        header.EndCustomerReference = JdeUtility.FilterValue(order.WebSiteOrderNumber, 25);

        //header.SoldToNumber = (double)soldToCustomer.AddressBookNumber.Value;
        header.ShipToNumber = double.Parse(order.ShippedToCustomer.EANIdentifier);
        header.ParentNumber = 0;
        header.IsProcessed = ' ';

        if (order.RequestDate.HasValue)
          header.RequestedDate = JulianDate.FromDate(order.RequestDate.Value).Value;
        else
          header.RequestedDate = JulianDate.FromDate(DateTime.Now.AddDays(1)).Value;

        header.OrderedBy = ediRelation.EdiVendor.Name;
        header.OrderTakenBy = ediRelation.EdiVendor.OrderBy;

        header.TransactionOriginator = ediRelation.EdiVendor.Name;
        header.UserID = ediRelation.EdiVendor.Name;
        header.ProgramID = "EDI";
        header.Date = JulianDate.FromDate(System.DateTime.Now).Value;
        header.Time = double.Parse(System.DateTime.Now.ToString("HHmmss"));

        if (!string.IsNullOrEmpty(order.HoldCode))
          header.EDIHoldCode = order.HoldCode;

        if (order.BackOrdersAllowed.HasValue)
        {
          if (order.BackOrdersAllowed.Value)
            header.BackordersAllowed = 'Y';
        }

        //if (order.OrderExpireDate != null)
        //{
        //  header.EDIExpireDate = JulianDate.FromDate(ediDocument.OrderHeader.OrderExpireDate).Value;
        //}

        #region Details

        double lineNumber = 1000;

        if (!string.IsNullOrEmpty(ediRelation.FreightProduct))
        {
          OrderRequestDetail freightdetail = new OrderRequestDetail();
          freightdetail.ProductIdentifier = new ProductIdentifier();
          freightdetail.ProductIdentifier.ProductNumber = ediRelation.FreightProduct;
          freightdetail.Quantity = 1;
          header.OrderDetails.Add(AddOrderDetail(freightdetail, header, lineNumber, context));
          lineNumber += 1000;
        }

        if (!string.IsNullOrEmpty(ediRelation.FinChargesProduct))
        {
          OrderRequestDetail finChargesdetail = new OrderRequestDetail();
          finChargesdetail.ProductIdentifier = new ProductIdentifier();
          finChargesdetail.ProductIdentifier.ProductNumber = ediRelation.FinChargesProduct;
          finChargesdetail.Quantity = 1;
          header.OrderDetails.Add(AddOrderDetail(finChargesdetail, header, lineNumber, context));
          lineNumber += 1000;
        }

        foreach (EdiOrderLine detail in order.EdiOrderLines)
        {
          OrderDetail orderDetail = AddOrderDetail(null, detail, header, lineNumber, order.PartialDelivery, context);
          
          if (orderDetail != null)
          {
            header.OrderDetails.Add(orderDetail);
            lineNumber += 1000;
          }
        }

        #endregion
        context.OrderHeaders.InsertOnSubmit(header);
        context.SubmitChanges();
        
        return -1;
      }
      #endregion
    }

    private OrderDetail AddOrderDetail(OrderRequestDetail detail, OrderHeader header, double lineNumber, JDEDataContext context)
    {
      return AddOrderDetail(detail, null, header, lineNumber, null, context);
    }

    private OrderDetail AddOrderDetail(OrderRequestDetail detail, EdiOrderLine ediDetail, OrderHeader header, double lineNumber, bool? partialshipments, JDEDataContext context)
    {
      OrderDetail orderDetail = new OrderDetail();

      #region Default Values
      orderDetail.EDICompany = header.EDICompany;
      orderDetail.EDIDocumentType = header.EDIDocumentType;
      orderDetail.EDIDocumentNumber = header.EDIDocumentNumber;
      orderDetail.EDILineNumber = lineNumber;

      orderDetail.DocumentCompany = header.DocumentCompany;
      orderDetail.DocumentType = header.DocumentType;

      orderDetail.EDIMessageType = header.EDIMessageType;

      orderDetail.PaymentTerms = header.PaymentTerms;
      orderDetail.PaymentInstrument = header.PaymentInstrument;
      orderDetail.SubstitutesAllowed = header.SubstitutesAllowed;

      orderDetail.CarierNumber = header.CarrierNumber;
      orderDetail.RouteCode = header.RouteCode;

      orderDetail.ApplyFreight = header.ApplyFreight;

      orderDetail.ProgramID = header.ProgramID;
      orderDetail.UserID = header.UserID;
      orderDetail.EDIUserReference = header.EDIUserReference;
      orderDetail.CustomerReference = header.CustomerReference;
      #endregion

      if (detail != null)
      {
        orderDetail.QuantityOrdered = detail.Quantity;
        orderDetail.ProductID = int.Parse(detail.ProductIdentifier.ProductNumber);
        orderDetail.CustomerItemNumber = detail.VendorItemNumber;
      }

      if (ediDetail != null)
      {
        orderDetail.QuantityOrdered = ediDetail.Quantity;
        orderDetail.ProductID = double.Parse(ediDetail.EdiProductID);
        orderDetail.CustomerItemNumber = ediDetail.CustomerItemNumber;
        orderDetail.EDIOrderReference = ediDetail.EdiOrderLineID.ToString();
      }

      orderDetail.Date = JulianDate.FromDate(System.DateTime.Now).Value;
      orderDetail.Time = double.Parse(System.DateTime.Now.ToString("HHmmss"));

      if (partialshipments.HasValue && partialshipments.Value)
        orderDetail.PartialLineShipmentsAllowed = 'Y';

      orderDetail.BusinessUnit = "DC10".PadLeft(12);

      if (!string.IsNullOrEmpty(ediDetail.CompanyCode))
      {
        string company = ediDetail.CompanyCode;
        string businessUnit = string.Empty;
        if (company.Contains("-"))
        {
          businessUnit = company.Split('-')[1];
          company = company.Split('-')[0];
        }

        if (!string.IsNullOrEmpty(ediDetail.WareHouseCode))
        {
          orderDetail.Location = ediDetail.WareHouseCode;
          orderDetail.BusinessUnit = businessUnit.PadLeft(12);
          orderDetail.SZEMCU = company.PadLeft(12);
        }
      }

      return orderDetail;
    }

    public int ProcessVendorResponse(Objects.Models.EDI.Order.EdiOrderType type, System.Configuration.Configuration config)
    {
      throw new NotImplementedException();
    }

    public void GenerateOrderResponse(OrderResponseTypes type, Objects.Models.EDI.Response.EdiOrderResponse orderResponse)
    {
      throw new NotImplementedException();
    }

    #endregion
  }

  public enum JDEDocumentType
  {
    UNKNOWN,
    XML,
    Excel,
    ProductXML,
    ProductExcel,
    PublicationExcel,
    DirectShipmentXML,
    DirectShipmentExcel,
    PurchaseXML,
    PurchaseExcel,
    PurchaseConfirmationXML,
    InvoiceXML,
    OrderChangeXML,
    PurchaseOrderChangeXML,
    RMAOrderXML,
    RMAOrderExcel,
    CMAOrderXML,
    CMAOrderExcel
  }
}
