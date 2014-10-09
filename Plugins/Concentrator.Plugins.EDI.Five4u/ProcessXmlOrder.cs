using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects.EDI;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.Models.EDI.Enumerations;
using Concentrator.Objects.Models.EDI.Order;
using Concentrator.Objects.Models.EDI.Post;
using Concentrator.Objects.Models.EDI.Response;
using Concentrator.Objects.Models.EDI.Vendor;
using Concentrator.Objects.Models.Orders;
using Microsoft.Practices.ServiceLocation;
using Concentrator.Web.Objects.EDI;
using System.Data;
using Concentrator.Objects.Enumerations;

namespace Concentrator.Plugins.EDI.Five4u
{
  public class ProcessXmlOrder : IEdiProcessor
  {
    #region IEdiOrder Members

    public Header Header
    {
      get
      {
        return new Header
        {
          WebID = new WebID
          {
            UUID = "?"
          }
        };
      }
    }

    public string DocumentType(string requestDocument)
    {
      XDocument xdoc = XDocument.Parse(requestDocument);

      if (xdoc.Root.Name.LocalName == "Orders")
      {
        return "XML";
      }

      return "UNKNOWN Type";
    }

    /*
      <?xml version="1.0" encoding="utf-8" ?> 
- <Orders>
- <Header HeaderTextPackingSlip="">
  <AccountNumberCustomer>000309</AccountNumberCustomer> 
  <MwEndCustomerPO>51-066923</MwEndCustomerPO> 
  <OrderNumberCustomer>049732040002</OrderNumberCustomer> 
  <OrderDate>20110315</OrderDate> 
  <CompanyName>SAPPI MAASTRICHT</CompanyName> 
  <CompanyClientName>BROEKMANS</CompanyClientName> 
  <Reference1 /> 
  <Reference2 /> 
  <Street>BIESENWEG 16</Street> 
  <Zip>6211 AA</Zip> 
  <City>MAASTRICHT</City> 
  <Country>NL</Country> 
  <DeliveryType>99</DeliveryType> 
- <OrderOptions>
  <DeliveryFull>N</DeliveryFull> 
  <FulfillOptions>K</FulfillOptions> 
  </OrderOptions>
  <OrderInformation>notsupplied</OrderInformation> 
  </Header>
- <Line>
  <LineNumber>000009</LineNumber> 
  <ProductNumberMW>E317560</ProductNumberMW> 
  <ProductNumberPartner>Q1397A</ProductNumberPartner> 
  <ProductDescription>NoDescriptionsupplied</ProductDescription> 
  <Quantity>000000002</Quantity> 
  <Cost>000000000010.00</Cost> 
  <Currency>EUR</Currency> 
  <UnitOfMeasure>EACH</UnitOfMeasure> 
  </Line>
  </Orders>
     */

    public List<EdiOrder> ProcessOrder(string type, string document, int? connectorID, int ediRequestID, IUnitOfWork unit)
    {
      XDocument xdoc = XDocument.Parse(document);
      List<EdiOrder> ediOrderList = new List<EdiOrder>();

      var order = xdoc.Root;
      //foreach (var order in xdoc.Root.Elements("Orders"))
      //{
      int orderID = -1;
      EdiOrder ediOrder = null;
      var xmlHeader = order.Element("Header");

      AddressNL address = null;

      if (xmlHeader.Element("Street") != null)
      {
        address = EDIUtility.FormatAddress(xmlHeader.Element("Street").Value);
      }

      var account = new Concentrator.Objects.Models.Orders.Customer()
      {
        EANIdentifier = xmlHeader.Element("AccountNumberCustomer").Value, //<AccountNumberCustomer>000309</AccountNumberCustomer> 
        CompanyName = xmlHeader.Element("CompanyName") != null ? xmlHeader.Element("CompanyName").Value : string.Empty, // <CompanyName>SAPPI MAASTRICHT</CompanyName>
        CustomerName = xmlHeader.Element("CompanyClientName") != null ? xmlHeader.Element("CompanyClientName").Value : string.Empty, //<CompanyClientName>BROEKMANS</CompanyClientName>
        CustomerAddressLine1 = address.Street, //<Street>BIESENWEG 16</Street> 
        HouseNumber = address.HouseNumber + address.HouseNumberExtension,
        PostCode = xmlHeader.Element("Zip") != null ? xmlHeader.Element("Zip").Value : string.Empty, //<Zip>6211 AA</Zip>
        City = xmlHeader.Element("City") != null ? xmlHeader.Element("City").Value : string.Empty, //<City>MAASTRICHT</City>
        Country = xmlHeader.Element("Country") != null ? xmlHeader.Element("Country").Value : string.Empty //<Country>NL</Country>
      };

      var xmlCustomerID = xmlHeader.Element("AccountNumberCustomer").Value;
      ConnectorRelation rel = unit.Scope.Repository<ConnectorRelation>().GetSingle(x => x.CustomerID == xmlCustomerID);

      if (rel == null || !rel.ConnectorID.HasValue)
        throw new Exception("No Connector Relation found for numer" + xmlHeader.Element("AccountNumberCustomer").Value);

      ediOrder = new EdiOrder()
      {
        ConnectorID = rel.ConnectorID.Value,
        WebSiteOrderNumber = xmlHeader.Element("MwEndCustomerPO") != null ? xmlHeader.Element("MwEndCustomerPO").Value : null, //<MwEndCustomerPO>51-066923</MwEndCustomerPO> 
        CustomerOrderReference = xmlHeader.Element("OrderNumberCustomer").Value, //<OrderNumberCustomer>049732040002</OrderNumberCustomer> 
        //OrderDate = DateTime.ParseExact(xmlHeader.Element("OrderDate").Value, "yyyyMMdd", null), //<OrderDate>20110315</OrderDate> 
        BackOrdersAllowed = xmlHeader.Element("OrderOptions").Element("FulfillOptions").Value == "K" ? false : true,
        ShippedToCustomer = account,
        SoldToCustomer = account,
        ReceivedDate = DateTime.Now,
        PartialDelivery = xmlHeader.Element("OrderOptions").Element("DeliveryFull").Value == "N" ? true : false,
        EdiOrderTypeID = 1,
        EdiRequestID = ediRequestID
        //<Reference1 /> 
        //<Reference2 /> 
        //<DeliveryType>99</DeliveryType> 
        // <OrderInformation>notsupplied</OrderInformation>
        //- <OrderOptions>
        //<DeliveryFull>N</DeliveryFull> 
        //<FulfillOptions>K</FulfillOptions> 
        //</OrderOptions>
      };


      unit.Scope.Repository<Concentrator.Objects.Models.Orders.Customer>().Add(account);


      unit.Scope.Repository<EdiOrder>().Add(ediOrder);

      foreach (var line in order.Elements("Line"))
      {
        var ediLine = new EdiOrderLine()
        {
          CustomerEdiOrderLineNr = line.Element("LineNumber").Value, //<LineNumber>000009</LineNumber> 
          CustomerItemNumber = line.Element("ProductNumberPartner").Value, // <ProductNumberMW>E317560</ProductNumberMW> 
          EndCustomerOrderNr = line.Element("ProductNumberMW") != null ? line.Element("ProductNumberMW").Value : null, //<ProductNumberPartner>Q1397A</ProductNumberPartner> 
          ProductDescription = line.Element("ProductDescription").Value, //<ProductDescription>NoDescriptionsupplied</ProductDescription> 
          Quantity = int.Parse(line.Element("Quantity").Value), //<Quantity>000000002</Quantity> 
          Price = double.Parse(line.Element("Cost").Value), //<Cost>000000000010.00</Cost> 
          Currency = line.Element("Currency") != null ? line.Element("Currency").Value : null, // <Currency>EUR</Currency> 
          UnitOfMeasure = line.Element("UnitOfMeasure") != null ? line.Element("UnitOfMeasure").Value : null, // <UnitOfMeasure>EACH</UnitOfMeasure>
          EdiOrder = ediOrder,
        };

        unit.Scope.Repository<EdiOrderLine>().Add(ediLine);
        ediLine.SetStatus(EdiOrderStatus.Received, unit);
      }
      ediOrderList.Add(ediOrder);
      //}
      unit.Save();
      return ediOrderList;

    }

    private string GetAdressCodes(EdiOrder order, Configuration config, ConnectorRelation ediRelation, out string organistationCode)
    {
      OrganisationAddress oa;
      string addressCode;

      if (!string.IsNullOrEmpty(order.ShippedToCustomer.CustomerAddressLine1) &&
        !string.IsNullOrEmpty(order.ShippedToCustomer.PostCode) &&
        !string.IsNullOrEmpty(order.ShippedToCustomer.City) &&
        !string.IsNullOrEmpty(order.ShippedToCustomer.Country))
      {
        oa = CreateOrganisationAddress(order.ShippedToCustomer, string.Empty, ediRelation, config);
        addressCode = oa.AddressCode;
        organistationCode = oa.NewOrganisationCode;
      }
      else
      {
        var organisationList = GetOrganisations(order.ShippedToCustomer.EANIdentifier, ediRelation, config);
        var addressList = GetCustomerAddresses(order.ShippedToCustomer.EANIdentifier, organisationList.FirstOrDefault().OrganisatieCode, ediRelation, config);
        addressCode = addressList.FirstOrDefault().AddressCode;
        organistationCode = organisationList.FirstOrDefault().OrganisatieCode;
      }

      //AddressResponse address = null;
      //string addressCode = string.Empty;
      //organistationCode = organisationList.FirstOrDefault().OrganisatieCode;

      //foreach (var ogc in organisationList)
      //{
      //  organistationCode = ogc.OrganisatieCode;
      //  var addressList = GetCustomerAddresses(order.ShipToCustomer.EANIdentifier, ogc.OrganisatieCode, config);

      //  address = addressList.FirstOrDefault(x => x.Zipcode == order.ShipToCustomer.PostCode && x.Street == order.ShipToCustomer.CustomerAddressLine1);

      //  if (address != null)
      //    break;
      //}

      //if (address == null)
      //{
      //oa = CreateOrganisationAddress(order.ShipToCustomer, string.Empty, config);
      //addressCode = oa.AddressCode;
      //organistationCode = oa.NewOrganisationCode;
      //}
      //else
      //  addressCode = address.AddressCode;

      return addressCode;
    }

    public EdiOrderResponse ValidateOrder(EdiOrder order, EdiVendor ediVendor, ConnectorRelation ediRelation, Configuration config, IUnitOfWork unit)
    {
      string organistationCode = string.Empty;
      string addressCode = string.Empty;
      List<ReturnError> errorList = new List<ReturnError>();

      EdiOrderResponse ors = new EdiOrderResponse()
      {
        ResponseType = (int)OrderResponseTypes.Acknowledgement,
        //VendorDocument = result,
        EdiVendorID = ediVendor.EdiVendorID,
        VendorDocumentNumber = "-1",
        ReceiveDate = DateTime.Now,
        //PaymentConditionCode = XmlCommunicator.ExtractData(data, "DeliveryCondition"),
        //PaymentConditionDays = int.Parse(XmlCommunicator.ExtractData(data, "PaymentCondition")),
        //PartialDelivery = bool.Parse(XmlCommunicator.ExtractData(data, "PartialDelivery")),
        //AdministrationCost = decimal.Parse(XmlCommunicator.ExtractData(data, "OrderCostAmount")),
        //DropShipmentCost = decimal.Parse(XmlCommunicator.ExtractData(data, "OrderCostAmount")),
        EdiOrderID = order.EdiOrderID,
        //PaymentConditionDiscount = XmlCommunicator.ExtractData(data, "OrderDistcountManually"),
        //VendorDocumentReference = XmlCommunicator.ExtractData(data, "HeaderWarning")
      };

      if (string.IsNullOrEmpty(ediRelation.AdministrationCode))
        ors.ResponseErrors.Add(ErrorMessage(ErrorCode.Other, order.PartialDelivery.Value, order.BackOrdersAllowed.Value, string.Format("Administration Code for ConnectionRelation {0} not set", ediRelation.CustomerID)));

      if (string.IsNullOrEmpty(ediRelation.OrderType))
        ors.ResponseErrors.Add(ErrorMessage(ErrorCode.Other, order.PartialDelivery.Value, order.BackOrdersAllowed.Value, string.Format("OrderType for ConnectionRelation {0} not set", ediRelation.CustomerID)));

      try
      {
        addressCode = GetAdressCodes(order, config, ediRelation, out organistationCode);

        if (string.IsNullOrEmpty(organistationCode))
          ors.ResponseErrors.Add(ErrorMessage(ErrorCode.Other, order.PartialDelivery.Value, order.BackOrdersAllowed.Value, "Invalid organistationCode"));

        if (string.IsNullOrEmpty(addressCode))
          ors.ResponseErrors.Add(ErrorMessage(ErrorCode.Other, order.PartialDelivery.Value, order.BackOrdersAllowed.Value, "Invalid addressCode"));
      }
      catch (Exception ex)
      {
        ors.ResponseErrors.Add(ErrorMessage(ErrorCode.Other, order.PartialDelivery.Value, order.BackOrdersAllowed.Value, ex.Message));
      }

      return TestOrder(order, addressCode, organistationCode, ediVendor, config, ors, ediRelation, unit);
    }

    public int ProcessOrderToVendor(EdiOrder order, ConnectorRelation ediRelation, Configuration config, IUnitOfWork unit)
    {
      string organistationCode = string.Empty;
      string addressCode = string.Empty;
      List<string> errorList = new List<string>();

      addressCode = GetAdressCodes(order, config, ediRelation, out organistationCode);

      if (string.IsNullOrEmpty(organistationCode))
        throw new Exception("Invalid organistationCode");

      if (string.IsNullOrEmpty(addressCode))
        throw new Exception("Invalid addressCode");

      var status = 1; // 1 is exported
      int orderID = 0;
      GeneralOderInfo generalOrderInfo = GenerateOrder(order, config, addressCode, organistationCode, ediRelation);

      var customerID = order.ShippedToCustomer.EANIdentifier;
      string branchCode = "NLD";
      if (int.Parse(customerID) < 0 || int.Parse(customerID) > 319999)
        branchCode = "BEL";

      var placeOrder = XmlCommunicator.SendRequest(new PlaceOrderEnvelope
      {
        Header = generalOrderInfo.Header,
        Body = new PlaceOrderBody
        {
          placeorder = new Placeorder
          {
            LanguageCode = "N",
            UserCode = "",
            DebtorCode = customerID,
            AdminCode = ediRelation.AdministrationCode,
            CountPurchase = "false",
            AccountManager = "false",
            SplitOrderLines = "true",
            PriceAlwaysPositive = "false",
            InstructionSaveInTextLine = "false",
            BlockedProdGrpCodeList = "",
            BranchCode = branchCode,
            OrderSort = ediRelation.OrderType,
            ProdCatCode = "ARG",
            iDeal = "false",
            OrderType = "1",
            RemainingQtyInBackOrder = "false",
            ContactPersonCode = "",
            ttShoppingcardHeader = generalOrderInfo.Headers,
            ttShoppingcardLine = generalOrderInfo.OrderLines.ToArray(),
          }
        }
      }, config);

      using (var reader = new StreamReader(placeOrder.GetResponseStream()))
      {
        var result = reader.ReadToEnd();
        var orderResult = XmlCommunicator.ExtractData(result, "OrderKey");

        // no order key was supplied, it must've errored
        if (string.IsNullOrEmpty(orderResult))
        {
          orderResult = XmlCommunicator.ExtractData(result, "errorMessage");
          status = -1; // -1 is error
          throw new Exception(orderResult);
        }
        else
        {
          orderID = int.Parse(orderResult);

          var response = unit.Scope.Repository<EdiOrderResponse>().GetSingle(x => x.EdiOrderID == order.EdiOrderID && x.VendorDocumentNumber == "-1" && x.ResponseType == (int)OrderResponseTypes.Acknowledgement);
          if (response != null)
            response.VendorDocumentNumber = orderResult;

          unit.Save();

          //order.BackendOrderID = orderID;
        }
      }

      return orderID;
    }

    public EdiOrderResponse TestOrder(EdiOrder order, string addressCode, string organisationCode, EdiVendor ediVendor, Configuration config, EdiOrderResponse ors, Concentrator.Objects.Models.Connectors.ConnectorRelation ediRelation, IUnitOfWork unit)
    {
      var orderResponse = PriceAndStockCheck(order, ors, ediRelation, config, unit);
      unit.Scope.Repository<EdiOrderResponse>().Add(orderResponse);

      if ((orderResponse.ResponseErrors != null && orderResponse.ResponseErrors.Any(x => x.SkipOrder))
        || (orderResponse.EdiOrderResponseLines.Count > 0 && orderResponse.EdiOrderResponseLines.Any(x => x.ResponseErrors.Any(y => y.SkipOrder))))
      {

        foreach (var orl in orderResponse.EdiOrderResponseLines)
        {
          orl.EdiOrderLine.SetStatus(EdiOrderStatus.ReceiveAcknowledgement, unit);

          unit.Scope.Repository<EdiOrderResponseLine>().Add(orl);
        }
        return ors;

      }

      var errorLines = orderResponse.EdiOrderResponseLines.Where(x => x.ResponseErrors.Count > 0).Select(x => x.EdiOrderLineID).ToList();

      GeneralTempOderInfo generalOrderInfo = GenerateTempOrder(order, config, addressCode, organisationCode, errorLines, ediRelation);

      var customerID = order.ShippedToCustomer.EANIdentifier;

      string branchCode = "NLD";
      if (int.Parse(customerID) < 0 || int.Parse(customerID) > 319999)
        branchCode = "BEL";

      var placeOrder = XmlCommunicator.SendRequest(new CheckTempOrderEnvelope
      {
        Header = generalOrderInfo.Header,
        Body = new CheckTempOrderBody
        {
          checktemporder = new CheckTempOrder()
          {
            LanguageCode = "N",
            UserCode = "",
            OrderType = "1",
            DebtorCode = customerID,
            AdminCode = ediRelation.AdministrationCode,
            CountPurchase = "false",
            AccountManager = "false",
            SplitOrderLines = "true",
            PriceAlwaysPositive = "false",
            InstructionSaveInTextLine = "false",
            CheckShipmentQty = "true",
            CheckPreliminaryReceipts = "true",
            BlockedProdGrpCodeList = "",
            RemainingQtyInBackOrder = "true",
            BranchCode = branchCode,
            OrderSort = ediRelation.OrderType,
            ProdCatCode = "ARG",
            ttShoppingcardHeader = generalOrderInfo.Headers,
            ttShoppingcardLine = generalOrderInfo.OrderLines.ToArray(),
          }
        }
      }, config);

      List<ReturnError> orderResults = new List<ReturnError>();
      using (var reader = new StreamReader(placeOrder.GetResponseStream()))
      {

        var result = reader.ReadToEnd();

        orderResponse.VendorDocument = result;

        try
        {
          var data = XmlCommunicator.ExtractDataList(result, "ttShoppingCardHeader_outRow")[0];
          orderResponse.PaymentConditionCode = XmlCommunicator.ExtractData(data, "DeliveryCondition");
          orderResponse.PaymentConditionDiscountDescription = XmlCommunicator.ExtractData(data, "PaymentCondition");
          orderResponse.PartialDelivery = bool.Parse(XmlCommunicator.ExtractData(data, "PartialDelivery"));
          orderResponse.AdministrationCost = decimal.Parse(XmlCommunicator.ExtractData(data, "OrderCostsAmount"));
          //orderResponse.DropShipmentCost = decimal.Parse(XmlCommunicator.ExtractData(data, "OrderCostAmount"));
          orderResponse.PaymentConditionDiscount = XmlCommunicator.ExtractData(data, "OrderDistcountManually");
          orderResponse.VendorDocumentReference = XmlCommunicator.ExtractData(data, "HeaderWarning");

          //var orderResult = XmlCommunicator.ExtractData(result, "OrderKey");
          foreach (var value in XmlCommunicator.ExtractDataList(result, "ttShoppingCardLine_outRow"))
          {
            string error = XmlCommunicator.ExtractData(value, "MsgString");
            if (!string.IsNullOrEmpty(error))
            {

              orderResults.Add(new ReturnError()
              {
                ErrorMessage = string.Format("{0} for item itemrow {1}", error, XmlCommunicator.ExtractData(value, "ItemCode"))
              });
            }

            var orl = orderResponse.EdiOrderResponseLines.FirstOrDefault(x => x.VendorItemNumber == XmlCommunicator.ExtractData(value, "ItemCode"));

            orl.Cancelled = 0;
            orl.Backordered = int.Parse(XmlCommunicator.ExtractData(value, "BackOrder"));
            orl.Unit = XmlCommunicator.ExtractData(value, "UnitCode");
            orl.Price = decimal.Parse(XmlCommunicator.ExtractData(value, "NettoPrice"));
            if (!string.IsNullOrEmpty(XmlCommunicator.ExtractData(value, "DeliveryDate")))
              orl.DeliveryDate = DateTime.Parse(XmlCommunicator.ExtractData(value, "DeliveryDate"));

            if (orl.Backordered == 2 && !order.BackOrdersAllowed.Value)
              orl.ResponseErrors.Add(ErrorMessage(ErrorCode.Stock, order.PartialDelivery.Value, order.BackOrdersAllowed.Value));

            if (orl.ResponseErrors == null)
              orl.ResponseErrors = new List<ReturnError>();

            if (orderResults.Count() > 0)
              orl.ResponseErrors.AddRange(orderResults);

            orl.EdiOrderLine.SetStatus(EdiOrderStatus.ReceiveAcknowledgement, unit);

            //unit.Scope.Repository<EdiOrderResponseLine>().Add(orl);
          }
        }
        catch (Exception ex)
        {
          if (orderResponse.ResponseErrors == null)
            orderResponse.ResponseErrors = new List<ReturnError>();

          orderResponse.ResponseErrors.Add(new ReturnError()
          {
            ErrorMessage = result
          });
        }
        unit.Save();

      }

      return orderResponse;
    }

    private EdiOrderResponse PriceAndStockCheck(EdiOrder order, EdiOrderResponse orderResponse, Concentrator.Objects.Models.Connectors.ConnectorRelation ediRelation, Configuration config, IUnitOfWork unit)
    {
      List<EdiOrderResponseLine> orsLineList = new List<EdiOrderResponseLine>();
      foreach (var line in order.EdiOrderLines)
      {
        var check = XmlCommunicator.SendRequest(new GetPricesAndStocks
        {
          Header = Header,
          Body = new GetPriceAndStocksBody
          {
            getpricesandstocks = new PriceAndStocks()
            {
              LanguageCode = "N",
              DebtorCode = order.ShippedToCustomer.EANIdentifier,
              AdminCode = ediRelation.AdministrationCode,
              ProdCatCode = "ARG",
              UsePricePer = "false",
              InclPrice = "true",
              InclStock = "true",
              OrderSort = ediRelation.OrderType,
              CountPurchase = "false",
              OrderQty = line.Quantity.ToString(),
              BlockedProdGrpCodeList = "",
              ItemDescrLong = "1",
              SearchDebtorItem = "false",
              ProductMemoType = "",
              OrderType = "1",
              CalculateStockComposedProducts = "true",
              OneUnitCode = "false",
              SkipInvalidItems = "false",
              ttItemRequest = new ItemRequestRow[]
              { new ItemRequestRow{
              
                ItemCode = line.CustomerItemNumber,
                UnitCode = "STUKS"
              }
              }
            }
          }
        }, config);

        using (var reader = new StreamReader(check.GetResponseStream()))
        {
          var result = reader.ReadToEnd();
          orderResponse.VendorDocument = result;

          EdiOrderResponseLine orsl = new EdiOrderResponseLine()
          {
            EdiOrderResponse = orderResponse,
            EdiOrderLineID = line.EdiOrderLineID,
            Ordered = line.Quantity,
            Cancelled = line.Quantity,
            Shipped = 0,
            Invoiced = 0,
            //DeliveryDate = DateTime.Parse(XmlCommunicator.ExtractData(value, "DeliveryDate")),            
            processed = false,
            VendorItemNumber = line.CustomerItemNumber,
            VendorLineNumber = line.CustomerEdiOrderLineNr,
            Price = 0,
            Backordered = 0,
            Description = string.Empty,
            Unit = string.Empty
          };
          orsLineList.Add(orsl);

          var checkResp = XmlCommunicator.ExtractDataList(result, "ttItemResponseRow");
          List<ReturnError> errors = new List<ReturnError>();

          if (checkResp.Count < 1)
          {
            errors.Add(ErrorMessage(ErrorCode.Product, order.PartialDelivery.Value, order.BackOrdersAllowed.Value));
            orsl.ResponseErrors = errors;
            orsl.Remark = string.Join(";", errors.Select(x => x.ErrorCode).ToArray());
          }
          unit.Scope.Repository<EdiOrderResponseLine>().Add(orsl);

          foreach (var value in checkResp)
          {
            int stock = int.Parse(XmlCommunicator.ExtractData(value, "Stock"));
            double price = double.Parse(XmlCommunicator.ExtractData(value, "NettoPrice"));
            string desc = XmlCommunicator.ExtractData(value, "ItemDescr");


            string error = XmlCommunicator.ExtractData(value, "MsgString");

            if (!string.IsNullOrEmpty(error))
            {
              if (error.Contains("Invalid"))
              {
                errors.Add(ErrorMessage(ErrorCode.Obsolete, order.PartialDelivery.Value, order.BackOrdersAllowed.Value));
              }
              else
              {
                errors.Add(new ReturnError
                {
                  ErrorCode = error,
                  SkipOrder = false
                });
              }
            }

            if (stock < line.Quantity)
              errors.Add(ErrorMessage(ErrorCode.Stock, order.PartialDelivery.Value, order.BackOrdersAllowed.Value));

            if (line.Price.HasValue && price != line.Price)
              errors.Add(ErrorMessage(ErrorCode.Price, order.PartialDelivery.Value, order.BackOrdersAllowed.Value));

            if (string.IsNullOrEmpty(desc))
              errors.Add(ErrorMessage(ErrorCode.Product, order.PartialDelivery.Value, order.BackOrdersAllowed.Value));

            orsl.ResponseErrors = errors;
            orsl.Remark = string.Join(";", errors.Select(x => x.ErrorCode).ToArray());
            orsl.Description = XmlCommunicator.ExtractData(value, "MsgString");
            orsl.Unit = XmlCommunicator.ExtractData(value, "UnitCode");
            orsl.Price = decimal.Parse(XmlCommunicator.ExtractData(value, "NettoPrice"));
          }
        }
      }
      orderResponse.EdiOrderResponseLines = orsLineList;

      return orderResponse;
    }

    private GeneralOderInfo GenerateOrder(EdiOrder order, Configuration config, string addressCode, string organisationCode, Concentrator.Objects.Models.Connectors.ConnectorRelation ediRelation)
    {
      var today = DateTime.Today.ToString("yyyy-MM-dd");
      var lines = new List<ShoppingcardLineRow>();


      var headers = new[]{
            new ShoppingcardHeaderRow
            {
              Instruction = "EDI ORDER",
              Reference = order.CustomerOrderReference,
              InvoiceAddressCode = String.Empty, // AW will fill this in
              OrderCostsManually = "false",
              OrderCostsAmount = "0",
              OrderDiscountManually = "false",
              OrderDiscountAmount = "0"
            }
          };

      var getPartialDelivery = XmlCommunicator.SendRequest(new GetPartialDeliveryEnvelope
      {
        Body = new GetPartialDeliveryBody
        {
          getpartialdelivery = new GetPartialDelivery
          {
            AdminCode = ediRelation.AdministrationCode,
            DebtorCode = order.SoldToCustomerID.HasValue ? order.SoldToCustomer.EANIdentifier : order.ShippedToCustomer.EANIdentifier,
            OrderSort = ediRelation.OrderType, //TODO,
            ParamPartialDeliveryChange = ""
          }
        }
      }, config);

      using (var reader = new StreamReader(getPartialDelivery.GetResponseStream()))
      {
        var result = reader.ReadToEnd();
        headers[0].PartialDelivery = XmlCommunicator.ExtractData(result, "PartialDeliveryInitial");
      }

      var loginDebtor = XmlCommunicator.SendRequest(new LoginDebtorEnvelope
      {
        Header = Header,
        Body = new LoginDebtorBody
        {
          logindebtor = new LoginDebtor
          {
            DebtorCode = order.SoldToCustomerID.HasValue ? order.SoldToCustomer.EANIdentifier : order.ShippedToCustomer.EANIdentifier,
            AdminCode = "10"
          }
        }
      }, config);

      using (var reader = new StreamReader(loginDebtor.GetResponseStream()))
      {
        var result = reader.ReadToEnd();
        headers[0].DeliveryCondition = XmlCommunicator.ExtractData(result, "DeliveryCondition");
        headers[0].PaymentCondition = XmlCommunicator.ExtractData(result, "PaymentCondition");
      }

      foreach (var line in order.EdiOrderLines)
      {
        lines.Add(new ShoppingcardLineRow
        {
          ItemCode = line.CustomerItemNumber,
          UnitCode = "STUKS",
          Ordered = line.Quantity.ToString(),
          NettoPrice = "0",
          DiscountPerc = "0",
          DeliveryDate = today,
          DeliveryAddressCode = addressCode,//shipTo[1],
          DeliveryAddressOrg = organisationCode,//shipTo[0],
          Instruction = "EDI",
          BackOrder = "0",
          ReturnOrderReason = "",
          SalesOrder = "",
          InitialDate = today
        });

      }

      return new GeneralOderInfo()
      {
        Header = Header,
        Headers = headers,
        OrderLines = lines
      };
    }

    private GeneralTempOderInfo GenerateTempOrder(EdiOrder order, Configuration config, string addressCode, string organisationCode, List<int?> errorLines, Concentrator.Objects.Models.Connectors.ConnectorRelation ediRelation)
    {
      var today = DateTime.Today.ToString("yyyy-MM-dd");
      var lines = new List<TempShoppingcardLineRow>();


      var headers = new[]{
            new TempShoppingcardHeaderRow
            {
              Instruction = "EDI ORDER",
              Reference = order.CustomerOrderReference,
              InvoiceAddressCode = String.Empty, // AW will fill this in
              OrderCostsManually = "false",
              OrderCostsAmount = "0",
              OrderDiscountManually = "false",
              OrderDiscountAmount = "0",
              HeaderWarning = ""
            }
          };

      var getPartialDelivery = XmlCommunicator.SendRequest(new GetPartialDeliveryEnvelope
      {
        Body = new GetPartialDeliveryBody
        {
          getpartialdelivery = new GetPartialDelivery
          {
            AdminCode = ediRelation.AdministrationCode,
            DebtorCode = order.SoldToCustomerID.HasValue ? order.SoldToCustomer.EANIdentifier : order.ShippedToCustomer.EANIdentifier,
            OrderSort = ediRelation.OrderType, //TODO,
            ParamPartialDeliveryChange = ""
          }
        }
      }, config);

      using (var reader = new StreamReader(getPartialDelivery.GetResponseStream()))
      {
        var result = reader.ReadToEnd();
        headers[0].PartialDelivery = XmlCommunicator.ExtractData(result, "PartialDeliveryInitial");
      }

      var loginDebtor = XmlCommunicator.SendRequest(new LoginDebtorEnvelope
      {
        Header = Header,
        Body = new LoginDebtorBody
        {
          logindebtor = new LoginDebtor
          {
            DebtorCode = order.SoldToCustomerID.HasValue ? order.SoldToCustomer.EANIdentifier : order.ShippedToCustomer.EANIdentifier,
            AdminCode = "10"
          }
        }
      }, config);

      using (var reader = new StreamReader(loginDebtor.GetResponseStream()))
      {
        var result = reader.ReadToEnd();
        headers[0].DeliveryCondition = XmlCommunicator.ExtractData(result, "DeliveryCondition");
        headers[0].PaymentCondition = XmlCommunicator.ExtractData(result, "PaymentCondition");
      }

      foreach (var line in order.EdiOrderLines)
      {
        if (errorLines.Contains(line.EdiOrderLineID))
          continue;

        lines.Add(new TempShoppingcardLineRow
        {
          ItemCode = line.CustomerItemNumber,
          UnitCode = "STUKS",
          Ordered = line.Quantity.ToString(),
          NettoPrice = "0",
          DiscountPerc = "0",
          DeliveryDate = today,
          DeliveryAddressCode = addressCode,//shipTo[1],
          DeliveryAddressOrg = organisationCode,//shipTo[0],
          Instruction = "EDI",
          BackOrder = "0",
          ReturnOrderReason = "",
          SalesOrder = "",
          MsgString = "",
          PreliminaryReceipts = "1",
          InitialDate = today
        });
      }

      return new GeneralTempOderInfo()
      {
        Header = Header,
        Headers = headers,
        OrderLines = lines
      };
    }


    private List<BelongsToOrgsRespones> GetOrganisations(string customerID, Concentrator.Objects.Models.Connectors.ConnectorRelation ediRelation, Configuration config)
    {
      var getBelongsToOrgs = XmlCommunicator.SendRequest(new GetBelongsToOrgsEnvelope
      {
        Header = Header,
        Body = new GetBelongsToOrgsBody()
        {
          getBelongsToOrgs = new GetBelongsToOrgs()
          {
            AdminCode = ediRelation.AdministrationCode,
            DebtorCode = customerID,
            LanguageCode = "N"
          }
        }
      }, config);

      List<BelongsToOrgsRespones> organisationList = new List<BelongsToOrgsRespones>();
      using (var reader = new StreamReader(getBelongsToOrgs.GetResponseStream()))
      {
        var result = reader.ReadToEnd();
        foreach (var value in XmlCommunicator.ExtractDataList(result, "ttOrganisationRow"))
        {
          var organistaion = new BelongsToOrgsRespones()
          {
            OrganisatieCode = XmlCommunicator.ExtractData(value, "OrganisationCode"),
            OrganisationName = XmlCommunicator.ExtractData(value, "OrganisationName")
          };
          organisationList.Add(organistaion);
        }
      }
      return organisationList;
    }

    private List<AddressResponse> GetCustomerAddresses(string customerID, string organisationCode, Concentrator.Objects.Models.Connectors.ConnectorRelation ediRelation, Configuration config)
    {
      var belongsToOrgsAddress = XmlCommunicator.SendRequest(new GetBelongsToOrgAdressEnvelope
      {
        Header = Header,
        Body = new GetBelongsToOrgAdressBody
        {
          getBelongsToOrgsAddress = new GetBelongsToOrgsAddress()
          {
            AdminCode = ediRelation.AdministrationCode,
            DebtorCode = customerID,
            LanguageCode = "N",
            OrganisationCode = organisationCode
          }
        }
      }, config);

      List<AddressResponse> addressList = new List<AddressResponse>();

      using (var reader = new StreamReader(belongsToOrgsAddress.GetResponseStream()))
      {
        var result = reader.ReadToEnd();
        foreach (var value in XmlCommunicator.ExtractDataList(result, "ttAddressRow"))
        {
          var addrresses = new AddressResponse()
          {
            OrganisatieCode = XmlCommunicator.ExtractData(value, "OrganisationCode"),
            AddressCode = XmlCommunicator.ExtractData(value, "AddressCode"),
            AddressType = XmlCommunicator.ExtractData(value, "AddressType"),
            AddressDescr = XmlCommunicator.ExtractData(value, "AddressDescr"),
            Priority = XmlCommunicator.ExtractData(value, "Priority"),
            Street = XmlCommunicator.ExtractData(value, "Street"),
            Zipcode = XmlCommunicator.ExtractData(value, "Zipcode"),
            Place = XmlCommunicator.ExtractData(value, "Place"),
            CountryName = XmlCommunicator.ExtractData(value, "CountryName"),
            SearchKey = XmlCommunicator.ExtractData(value, "SearchKey"),
            EanCode = XmlCommunicator.ExtractData(value, "EanCode")
          };
          addressList.Add(addrresses);
        }
      }
      return addressList;
    }

    private OrganisationAddress CreateOrganisationAddress(Concentrator.Objects.Models.Orders.Customer customer, string organistaionCode, Concentrator.Objects.Models.Connectors.ConnectorRelation ediRelation, Configuration config)
    {
      var zipcode = XmlCommunicator.SplitZipcode(customer.PostCode);

      var createOrganisationAddress = XmlCommunicator.SendRequest(new CreateOrganisationAddressEnvelope
      {
        Header = Header,
        Body = new CreateOrganisationAddressBody
        {

          createOrganisationAddress = new CreateOrganisationAddress()
          {
            LanguageCode = "N",
            DebtorCode = customer.EANIdentifier,
            AdminCode = ediRelation.AdministrationCode,
            OrganisationCode = organistaionCode,
            Organisationname = customer.CompanyName,
            Streetname = customer.CustomerAddressLine1,
            Streetnumber = customer.HouseNumber,
            Postcodenumm = zipcode.Key,
            Postcodealpha = zipcode.Value,
            City = customer.City,
            Extraline = customer.CustomerName,
            Country = customer.Country
          }
        }
      }, config);

      using (var reader = new StreamReader(createOrganisationAddress.GetResponseStream()))
      {
        var result = reader.ReadToEnd();
        var organisationAddress = new OrganisationAddress()
        {
          oDebtorCode = XmlCommunicator.ExtractData(result, "odebtorcode"),
          oAdminCode = XmlCommunicator.ExtractData(result, "oAdminCode"),
          AddressCode = XmlCommunicator.ExtractData(result, "AddressCode"),
          AddressType = XmlCommunicator.ExtractData(result, "AddressType"),
          Priority = XmlCommunicator.ExtractData(result, "Priority"),
          NewOrganisationCode = XmlCommunicator.ExtractData(result, "OrganisationCodeNew")
        };
        return organisationAddress;
      }
    }

    public int ProcessVendorResponse(EdiOrderType type, Configuration config, IUnitOfWork unit)
    {
      if (type.Name == "Acknowledgement")
        return processAwResponse(unit);
      else if (type.Name == "ShipmentNotification")
        return processWMSResponse(unit);

      return -1;
    }

    private int processAwResponse(IUnitOfWork unit)
    {
      //AW query
      var lines = new List<object>();

      EdiOrderResponse resp = new EdiOrderResponse()
      {

      };

      unit.Scope.Repository<EdiOrderResponse>().Add(resp);
      foreach (var l in lines)
      {
        EdiOrderResponseLine line = new EdiOrderResponseLine()
        {
          EdiOrderResponse = resp
        };

        unit.Scope.Repository<EdiOrderResponseLine>().Add(line);
      }
      unit.Save();
      return resp.EdiOrderResponseID;

    }

    private int processWMSResponse(IUnitOfWork unit)
    {
      //AW query
      var lines = new List<object>();

      EdiOrderResponse resp = new EdiOrderResponse()
      {

      };

      unit.Scope.Repository<EdiOrderResponse>().Add(resp);

      foreach (var l in lines)
      {
        EdiOrderResponseLine line = new EdiOrderResponseLine()
        {
          EdiOrderResponse = resp
        };
        unit.Scope.Repository<EdiOrderResponseLine>().Add(line);
      }
      unit.Save();
      return resp.EdiOrderResponseID;

    }

    //  <ShippingNotification>
    //<Line OrderNumberPartner="30080590" DespatchMessageDate="20110428" ProductNumberCustomer="E694664">
    //<ShipDetails>
    //<DeliveryNumber>(J)VGL08519829280400108683</DeliveryNumber> 
    //<DeliveryNumberLineNumber>000001</DeliveryNumberLineNumber> 
    //</ShipDetails>
    //<OrderNumberCustomer>011017280101</OrderNumberCustomer> 
    //<DeliveryDate>20110428</DeliveryDate> 
    //<LineNumberCustomer>000001</LineNumberCustomer> 
    //<ProductNumberPartner>W9F-00014</ProductNumberPartner> 
    //<DeliveredQuantity>000000002</DeliveredQuantity> 
    //</Line>
    //<Line OrderNumberPartner="30080590" DespatchMessageDate="20110428" ProductNumberCustomer="E702173">
    //<ShipDetails>
    //<DeliveryNumber>(J)VGL08519829280400108683</DeliveryNumber> 
    //<DeliveryNumberLineNumber>000002</DeliveryNumberLineNumber> 
    //</ShipDetails>
    // <OrderNumberCustomer>011017280101</OrderNumberCustomer> 
    // <DeliveryDate>20110428</DeliveryDate> 
    // <LineNumberCustomer>000002</LineNumberCustomer> 
    // <ProductNumberPartner>W6F-00034</ProductNumberPartner> 
    // <DeliveredQuantity>000000001</DeliveredQuantity> 
    // </Line>
    // </ShippingNotification>

    public void GenerateOrderResponse(EdiOrderResponse orderResponse, IUnitOfWork unit, Configuration config)
    {
      Type t = null;
      object o = null;
      //var ediOrderID = orderResponse.EdiOrderID;

      Concentrator.Objects.Models.Orders.Customer customer = null;
      

      int counter = 1;
      string postUrl = string.Empty;
      ProviderTypeEnum providerType = ProviderTypeEnum.ByXml;

      if (orderResponse.EdiOrderID.HasValue){
        if(!orderResponse.EdiOrder.ConnectorRelation.UseFtp.HasValue || !orderResponse.EdiOrder.ConnectorRelation.UseFtp.Value)
         postUrl = orderResponse.EdiOrder.ConnectorRelation.OutboundTo;
        if (orderResponse.EdiOrder.ConnectorRelation.ProviderType.HasValue && orderResponse.EdiOrder.ConnectorRelation.ProviderType.Value == (int)Objects.Enumerations.ProviderTypeEnum.ByExcel)
          providerType = ProviderTypeEnum.ByExcel;
      }else if(orderResponse.ConnectorRelationID.HasValue && (!orderResponse.ConnectorRelation.UseFtp.HasValue || !orderResponse.ConnectorRelation.UseFtp.Value))
         postUrl = orderResponse.ConnectorRelation.OutboundTo;

      OrderResponseTypes type = (OrderResponseTypes)orderResponse.ResponseType;

      EdiOrderPost post = null;

      if (providerType == Objects.Enumerations.ProviderTypeEnum.ByExcel)
      {
        DataTable table = new DataTable(type.ToString());
        table.Columns.Add("EDIIdentifier");
        table.Columns.Add("CustomerOrderNumber");
        table.Columns.Add("ItemNumber");
        table.Columns.Add("QuantityOrdered");
        table.Columns.Add("QuantityBackordered");
        table.Columns.Add("QuantityCancelled");
        table.Columns.Add("Price");
        table.Columns.Add("Message");

        foreach (var line in orderResponse.EdiOrderResponseLines)
        {
          List<OrderResponseLineDifference> diffList = new List<OrderResponseLineDifference>();

          line.Remark.Split(';').ForEach((code, idx) =>
          {
            if (!string.IsNullOrEmpty(code))
            {
              var error = ErrorMessage(ErrorCode.Other, false, false, code: code);
              diffList.Add(new OrderResponseLineDifference()
              {
                Code = error.ErrorCode,
                Text = error.ErrorMessage
              });
            }
          });

          if (diffList.Count < 1)
          {
            diffList.Add(new OrderResponseLineDifference()
            {
              Code = "200",
              Text = "OK"
            });
          }

          table.Rows.Add(new string[] { 
            orderResponse.EdiOrderID.HasValue ? orderResponse.EdiOrder.ConnectorRelationID.Value.ToString() : orderResponse.ConnectorRelationID.Value.ToString(),
            orderResponse.EdiOrder.CustomerOrderReference,
            line.EdiOrderLine.CustomerItemNumber,
            line.EdiOrderLine.Quantity.ToString(),
            line.Backordered.ToString(),
            line.Cancelled.ToString(),
            line.Price.ToString(),
            string.Join(";",diffList.Select(x => x.Text).ToArray())
          });
        }

        StringBuilder requestString = new StringBuilder();
         using (XmlWriter xw = XmlWriter.Create(requestString))
        {
          table.WriteXml(xw);
        }

        post = new EdiOrderPost()
        {
          ConnectorRelationID = orderResponse.EdiOrderID.HasValue ? orderResponse.EdiOrder.ConnectorRelationID.Value : orderResponse.ConnectorRelationID.Value,
          EdiBackendOrderID = orderResponse.VendorDocumentNumber,
          CustomerOrderID = orderResponse.VendorDocumentReference,
          Processed = false,
          Type = "Excel",
          PostDocument = requestString.ToString(),
          PostDocumentUrl = postUrl,
          Timestamp = DateTime.Now,
          BSKIdentifier = 0,
          DocumentCounter = 0,
          //ConnectorID = orderResponse.EdiOrder.ConnectorID
        };

        if (orderResponse.EdiOrderID.HasValue)
        {
          post.EdiRequestID = orderResponse.EdiOrder.EdiRequestID;
          post.BSKIdentifier = orderResponse.EdiOrder.BSKIdentifier.HasValue ? orderResponse.EdiOrder.BSKIdentifier.Value : 0;
          post.ConnectorID = orderResponse.EdiOrder.ConnectorID;
          post.EdiOrderID = orderResponse.EdiOrderID.Value;
        }
      }
      else
      {
        switch (type)
        {
          case OrderResponseTypes.Acknowledgement:
            #region Acknowledgement
            customer = orderResponse.EdiOrder.ShippedToCustomer;
            t = typeof(OrderResponse);
            OrderResponse resp = new OrderResponse();

            if (!string.IsNullOrEmpty(orderResponse.EdiOrder.ConnectorRelation.OutboundOrderConfirmation))
              postUrl = orderResponse.EdiOrder.ConnectorRelation.OutboundOrderConfirmation;

            ReturnError headerError = null;

            if (headerError == null)
            {
              foreach (var line in orderResponse.EdiOrderResponseLines)
              {
                line.Remark.Split(';').ForEach((code, idx) =>
                {
                  if (!string.IsNullOrEmpty(code))
                  {
                    var error = ErrorMessage(ErrorCode.Other, false, false, code: code);
                    if (error.SkipOrder)
                    {
                      headerError = error;
                    }
                  }
                });
              }
            }

            OrderResponseHeader header = new OrderResponseHeader()
            {
              OrderResponseDate = DateTime.Now.ToString("yyyyMMdd"),
              CompanyName = customer.CompanyName,
              CompanyClientName = customer.CustomerName,
              OrderNumberCustomer = orderResponse.EdiOrder.CustomerOrderReference,
              MWEndCustomerPO = orderResponse.EdiOrder.WebSiteOrderNumber,
              OrderNumberPartner = orderResponse.VendorDocumentNumber,
              Currency = orderResponse.Currency,
              OrderDate = orderResponse.OrderDate.HasValue ? orderResponse.OrderDate.Value.ToString("yyyMMdd") : DateTime.Now.ToString("yyyyMMdd"),
              Reference1 = string.Empty,
              Reference2 = string.Empty,
              Street = customer.CustomerAddressLine1,
              Zip = customer.PostCode,
              City = customer.City,
              Country = customer.Country,
              DeliveryType = "FIVE STAND 04",
              DeliveryFull = orderResponse.PartialDelivery.HasValue && orderResponse.PartialDelivery.Value ? "N" : "Y",
              Code = headerError != null ? headerError.ErrorCode : "200",
              Text = headerError != null ? headerError.ErrorMessage : "OK"
            };
            resp.Items = new object[orderResponse.EdiOrderResponseLines.Count + 1];
            resp.Items[0] = header;

            foreach (var line in orderResponse.EdiOrderResponseLines)
            {
              List<OrderResponseLineDifference> diffList = new List<OrderResponseLineDifference>();

              line.Remark.Split(';').ForEach((code, idx) =>
              {
                if (!string.IsNullOrEmpty(code))
                {
                  var error = ErrorMessage(ErrorCode.Other, false, false, code: code);
                  diffList.Add(new OrderResponseLineDifference()
                  {
                    Code = error.ErrorCode,
                    Text = error.ErrorMessage
                  });
                }
              });

              if (diffList.Count < 1)
              {
                diffList.Add(new OrderResponseLineDifference()
                {
                  Code = "200",
                  Text = "OK"
                });
              }

              OrderResponseLine respLine = new OrderResponseLine()
              {
                OriginalOrderQuantity = line.EdiOrderLine.Quantity.ToString(),
                UOM = line.EdiOrderLine.UnitOfMeasure,
                LineNumber = line.EdiOrderLine.CustomerEdiOrderLineNr,
                ProductNumberMW = line.EdiOrderLine.EndCustomerOrderNr,
                ProductNumberPartner = line.EdiOrderLine.CustomerItemNumber,
                ConfirmedQuantity = (line.Ordered - line.Backordered).ToString(),
                ConfirmedPrice = line.Price.ToString(),
                ConfirmedDeliveryDate = line.DeliveryDate.HasValue ? line.DeliveryDate.Value.ToString("yyyyMMdd") : string.Empty,
                Difference = diffList.ToArray()
              };
              resp.Items[counter] = respLine;
              counter++;
              o = resp;
            }
            #endregion
            break;
          case OrderResponseTypes.ShipmentNotification:
            #region ShipmentNotification
            customer = orderResponse.EdiOrder.ShippedToCustomer;
            t = typeof(ShippingNotification);
            ShippingNotification ship = new ShippingNotification();

            ship.Items = new ShippingNotificationLine[orderResponse.EdiOrderResponseLines.Count];
            counter = 0;

            if (!string.IsNullOrEmpty(orderResponse.EdiOrder.ConnectorRelation.OutboundShipmentConfirmation))
              postUrl = orderResponse.EdiOrder.ConnectorRelation.OutboundShipmentConfirmation;

            foreach (var line in orderResponse.EdiOrderResponseLines)
            {
              ShippingNotificationLine respLine = new ShippingNotificationLine()
              {
                OrderNumberCustomer = orderResponse.EdiOrder.CustomerOrderReference,
                OrderNumberPartner = orderResponse.VendorDocumentNumber,
                DespatchMessageDate = DateTime.Now.ToString("yyyyMMdd"),
                ProductNumberCustomer = line.EdiOrderLine.EndCustomerOrderNr,
                ShipDetails = new ShippingNotificationLineShipDetails[1] { new ShippingNotificationLineShipDetails { DeliveryNumber = line.TrackAndTrace, DeliveryNumberLineNumber = line.VendorLineNumber } },
                DeliveryDate = line.DeliveryDate.HasValue ? line.DeliveryDate.Value.ToString("yyyyMMdd") : string.Empty,
                LineNumberCustomer = line.EdiOrderLine.CustomerEdiOrderLineNr,
                ProductNumberPartner = line.EdiOrderLine.CustomerItemNumber,
                DeliveredQuantity = line.Shipped.ToString()
              };
              ship.Items[counter] = respLine;
              counter++;
              o = ship;
            }
            #endregion
            break;
          case OrderResponseTypes.InvoiceNotification:
            t = typeof(InvoiceNotification);
            DefaultDocument defaultDoc = new DefaultDocument();
            o = defaultDoc.GenerateInvoiceNotification(orderResponse, this, config);
            break;
          default:
            break;
        }


        StringBuilder requestString = new StringBuilder();
        XmlWriterSettings settings = new XmlWriterSettings();
        settings.Encoding = Encoding.UTF8;
        //XmlDocument document = new XmlDocument();
        using (XmlWriter xw = XmlWriter.Create(requestString, settings))
        {
          xw.WriteProcessingInstruction("xml", "version=\"1.0\" encoding=\"utf-8\"");
          XmlSerializer serializer = new XmlSerializer(t);
          XmlSerializerNamespaces nm = new XmlSerializerNamespaces();
          nm.Add("xsi", "http://www.w3.org/2001/XMLSchema-instance");
          serializer.Serialize(xw, o, nm);

          //document.LoadXml(requestString.ToString());
        }

        post = new EdiOrderPost()
        {
          ConnectorRelationID = orderResponse.EdiOrderID.HasValue ? orderResponse.EdiOrder.ConnectorRelationID.Value : orderResponse.ConnectorRelationID.Value,
          EdiBackendOrderID = orderResponse.VendorDocumentNumber,
          CustomerOrderID = orderResponse.VendorDocumentReference,
          Processed = false,
          Type = type.ToString(),
          PostDocument = requestString.ToString(),
          PostDocumentUrl = postUrl,
          Timestamp = DateTime.Now,
          BSKIdentifier = 0,
          DocumentCounter = 0,
          //ConnectorID = orderResponse.EdiOrder.ConnectorID
        };

        if (orderResponse.EdiOrderID.HasValue)
        {
          post.EdiRequestID = orderResponse.EdiOrder.EdiRequestID;
          post.BSKIdentifier = orderResponse.EdiOrder.BSKIdentifier.HasValue ? orderResponse.EdiOrder.BSKIdentifier.Value : 0;
          post.ConnectorID = orderResponse.EdiOrder.ConnectorID;
          post.EdiOrderID = orderResponse.EdiOrderID.Value;
        }
      }

      unit.Scope.Repository<EdiOrderPost>().Add(post);
      unit.Save();

    }

    #endregion

    private ReturnError ErrorMessage(ErrorCode msg, bool deliveryFull, bool backOrders, string error = "", string code = "")
    {
      //      : Productnumber unknown
      //If an unknown productcode exist in the xml file 
      //If    <DeliveryFull>=Y the whole order will be rejected HTTP error code 508 will be returned
      //Order will not go to ERP
      if (code == "508" || msg == ErrorCode.Product && deliveryFull)
        return new ReturnError
        {
          ErrorCode = "508",
          SkipOrder = true,
          ErrorMessage = "Productnumber unknown"
        };

      //: Productnumber unknown
      //If    <DeliveryFull>N the orderline will be rejected <Code> 40 will be returned
      //We don’t look at  <FulfillOptions> for this error
      //Orderline will not go to ERP
      if (code == "40" || msg == ErrorCode.Product && !deliveryFull)
        return new ReturnError
        {
          ErrorCode = "40",
          SkipOrder = false,
          ErrorMessage = "Productnumber unknown"
        };

      //      : Price not correct Not within the margin
      //If    <DeliveryFull>Y the whole order will be rejected <Code> 516 will be returned
      //Order will go to ERP
      //If    <DeliveryFull>N the orderline will be rejected <Code> 30 will be returned
      //We don’t look at  <FulfillOptions> for this error
      //Order will go to ERP
      if (code == "516" || msg == ErrorCode.Price && deliveryFull)
        return new ReturnError
       {
         ErrorCode = "516",
         SkipOrder = true,
         ErrorMessage = "Price not correct Not within the margin"
       };
      else if (code == "30" || msg == ErrorCode.Price && !deliveryFull)
        return new ReturnError
        {
          ErrorCode = "30",
          SkipOrder = true,
          ErrorMessage = "Price not correct Not within the margin"
        };

      //:  Insufficient Stock If there is not enough stock and <DeliveryFull>Y & <FulfillOptions> K = the whole order will be rejected HTTP error code 515 will be returned
      //Order will not go to ERP
      if (code == "515" || msg == ErrorCode.Stock && deliveryFull && !backOrders)
        return new ReturnError
        {
          ErrorCode = "515",
          SkipOrder = true,
          ErrorMessage = "Insufficient Stock"
        };

      //:  Insufficient Stock If there is not enough stock and <DeliveryFull>N & <FulfillOptions> K = the order line will be rejected <Code> 60 will be returned
      //Orderline will not go to ERP
      if (code == "60" || msg == ErrorCode.Stock && !deliveryFull && !backOrders)
        return new ReturnError
        {
          ErrorCode = "60",
          SkipOrder = false,
          ErrorMessage = "Insufficient Stock"
        };

      //:  Insufficient Stock If there is not enough stock and <DeliveryFull>Y & <FulfillOptions>  = the whole order will be placed in BACKORDER <Code> 60 will be returned
      //Order will go to ERP
      if (code == "60" || msg == ErrorCode.Stock && deliveryFull && backOrders)
        return new ReturnError
        {
          ErrorCode = "60",
          SkipOrder = false,
          ErrorMessage = "Insufficient Stock"
        };

      //:  Insufficient Stock If there is not enough stock and <DeliveryFull>N & <FulfillOptions>  = the order line will be placed in BACKORDER <Code> 60 will be returned
      //Order will go to ERP
      if (code == "60" || msg == ErrorCode.Stock && !deliveryFull && backOrders)
        return new ReturnError
        {
          ErrorCode = "60",
          SkipOrder = false,
          ErrorMessage = "Insufficient Stock"
        };

      // :  Obsolete product & Insufficient Stock   If <DeliveryFull>Y & <FulfillOptions>K  the whole order will be rejected. HTTP error code 515 will be returned.
      //Order will not go to ERP
      if (code == "515" || msg == ErrorCode.Obsolete && deliveryFull && !backOrders)
        return new ReturnError
        {
          ErrorCode = "515",
          SkipOrder = true,
          ErrorMessage = "Obsolete product & Insufficient Stock"
        };

      //:  Obsolete product & Insufficient Stock   If <DeliveryFull>Y & <FulfillOptions>  the whole order will be rejected. HTTP error code 515 will be returned.
      //Order will not go to ERP
      if (code == "515" || msg == ErrorCode.Obsolete && deliveryFull && backOrders)
        return new ReturnError
        {
          ErrorCode = "515",
          SkipOrder = true,
          ErrorMessage = "Obsolete product & Insufficient Stock"
        };

      //:  Obsolete product & Insufficient Stock   If <DeliveryFull>N & <FulfillOptions>K  the order line will be rejected. <Code> 50 will be returned
      //Orderline will not go to ERP
      if (code == "50" || msg == ErrorCode.Obsolete && !deliveryFull && !backOrders)
        return new ReturnError
        {
          ErrorCode = "50",
          SkipOrder = false,
          ErrorMessage = "Obsolete product & Insufficient Stock"
        };

      //:  Obsolete product & Insufficient Stock   If <DeliveryFull>N & <FulfillOptions>  the order line will be rejected. <Code> 50 will be returned
      //Orderline will not go to ERP
      if (code == "50" || msg == ErrorCode.Obsolete && !deliveryFull && backOrders)
        return new ReturnError
        {
          ErrorCode = "50",
          SkipOrder = false,
          ErrorMessage = "Obsolete product & Insufficient Stock"
        };

      return new ReturnError
       {
         ErrorCode = "512",
         SkipOrder = true,
         ErrorMessage = !string.IsNullOrEmpty(error) ? error : "Unexpected error"
       };
    }

    public void GetCustomOrderResponses(IUnitOfWork unit, Configuration config)
    {
      var orderlinesWaitingForInvoice = unit.Scope.Repository<EdiOrderLine>().GetAll(x => x.CurrentState() == (int)EdiOrderStatus.WaitForInvoiceNotification);

      var orders = (from a in unit.Scope.Repository<EdiOrderResponse>().GetAll()
                    where a.EdiOrder.EdiOrderLines.Any(x => x.EdiOrderLedgers.Max(y => y.Status) == (int)EdiOrderStatus.WaitForInvoiceNotification)
                    select a);

      Dictionary<string, List<GetOutstandingInvoiceResponse>> invoiceOverview = new Dictionary<string, List<GetOutstandingInvoiceResponse>>();

      foreach (var order in orders)
      {
        string debtor = order.EdiOrder.SoldToCustomerID.HasValue ? order.EdiOrder.SoldToCustomer.EANIdentifier : order.EdiOrder.ShippedToCustomer.EANIdentifier;

        GetCustomReponses(unit, config, debtor, order.EdiOrder.ConnectorRelation, order);
      }
    }

    public void GetCustomReponses(IUnitOfWork unit, Configuration config, string debtor, ConnectorRelation connectorRelation, EdiOrderResponse ediOrderResponse = null, DateTime? invoiceDate = null)
    {

      var getOutstandingInvoices = XmlCommunicator.SendRequest(new GetOutstandingInvoices
      {
        Header = Header,
        Body = new GetOutStandingInvoiceBody
        {
          getoutstandinginvoice = new GetOutstandingInvoice
          {
            DebtorCode = debtor,
            AdminCode = connectorRelation.AdministrationCode,
          }
        }
      }, config);

      List<GetOutstandingInvoiceResponse> inv = new List<GetOutstandingInvoiceResponse>();
      using (var reader = new StreamReader(getOutstandingInvoices.GetResponseStream()))
      {
        var result = reader.ReadToEnd();

        foreach (var data in XmlCommunicator.ExtractDataList(result, "ttInvoiceHeaderRow"))
        {
          inv.Add(new GetOutstandingInvoiceResponse()
          {
            BalanceAmount = XmlCommunicator.ExtractData(data, "BalanceAmount"),
            CurrencyCode = XmlCommunicator.ExtractData(data, "CurrencyCode"),
            DueDate = XmlCommunicator.ExtractData(data, "DueDate"),
            InvoiceAmount = XmlCommunicator.ExtractData(data, "InvoiceAmount"),
            InvoiceDate = XmlCommunicator.ExtractData(data, "InvoiceDate"),
            InvoiceNr = XmlCommunicator.ExtractData(data, "InvoiceNr"),
            PayedAmount = XmlCommunicator.ExtractData(data, "PayedAmount"),
            Reference = XmlCommunicator.ExtractData(data, "Reference")
          });
        }
      }
      // invoiceOverview.Add(debtor, inv);
      // }

      List<GetInvoiveLineEnvelope> invoices = new List<GetInvoiveLineEnvelope>();

      if (invoiceDate.HasValue)
      {
        inv.Where(x => DateTime.Parse(x.InvoiceDate).CompareTo(invoiceDate.Value) > 0).ForEach((oi, idx) =>
        {
          invoices.Add(new GetInvoiveLineEnvelope
           {
             Header = Header,
             Body = new GetInvoiceLineBody
             {
               getinvoiceline = new GetInvoiceLine()
               {
                 DebtorCode = debtor,
                 AdminCode = connectorRelation.AdministrationCode,
                 InvoiceNr = oi.InvoiceNr,
                 OrderNr = string.Empty
               }
             }
           });
        });
      }
      else if (ediOrderResponse != null)
      {
        invoices.Add(new GetInvoiveLineEnvelope
       {
         Header = Header,
         Body = new GetInvoiceLineBody
         {
           getinvoiceline = new GetInvoiceLine()
           {
             DebtorCode = debtor,
             AdminCode = connectorRelation.AdministrationCode,
             InvoiceNr = string.Empty,
             OrderNr = ediOrderResponse.VendorDocumentNumber
           }
         }
       });
      }

      invoices.ForEach((invoice) =>
      {
        var getInvoiceLines = XmlCommunicator.SendRequest(invoice, config);

        using (var reader = new StreamReader(getInvoiceLines.GetResponseStream()))
        {
          var result = reader.ReadToEnd();
          var data = XmlCommunicator.ExtractDataList(result, "ttInvoiceLineRow");

          if (data.Count > 0)
          {

            var invoicedata = (from i in data
                               select new GetInvoiceLineResponse
                               {
                                 CurrencyCode = XmlCommunicator.ExtractData(i, "CurrencyCode"),
                                 Invoiced = XmlCommunicator.ExtractData(i, "Invoiced"),
                                 InvoiceLineNr = XmlCommunicator.ExtractData(i, "InvoiceLineNr"),
                                 InvoiceNr = XmlCommunicator.ExtractData(i, "InvoiceNr"),
                                 ItemCode = XmlCommunicator.ExtractData(i, "ItemCode"),
                                 ItemDescr = XmlCommunicator.ExtractData(i, "ItemDescr"),
                                 NettoAmount = XmlCommunicator.ExtractData(i, "NettoAmount"),
                                 Ordered = XmlCommunicator.ExtractData(i, "Ordered"),
                                 OrderNr = XmlCommunicator.ExtractData(i, "OrderNr"),
                                 Price = XmlCommunicator.ExtractData(i, "Price"),
                                 Reference = XmlCommunicator.ExtractData(i, "Reference"),
                                 UnitCode = XmlCommunicator.ExtractData(i, "UnitCode")
                               });

            var groupInvoice = new Dictionary<string, List<GetInvoiceLineResponse>>();

            invoicedata.ForEach((x, idx) =>
            {
              if (groupInvoice.ContainsKey(x.InvoiceNr))
              {
                groupInvoice[x.InvoiceNr].Add(x);
              }
              else
              {
                var list = new List<GetInvoiceLineResponse>();
                list.Add(x);
                groupInvoice.Add(x.InvoiceNr, list);
              }
            });

            foreach (var value in groupInvoice.Keys)
            {
              var firstLine = groupInvoice[value].FirstOrDefault();
              var debs = inv.FirstOrDefault(x => x.InvoiceNr == value);

              var priceEx = groupInvoice[value].Sum(x => decimal.Parse(x.Price));
              var discount = groupInvoice[value].Sum(x => decimal.Parse(x.NettoAmount));
              EdiOrderResponse ors = new EdiOrderResponse()
              {
                ResponseType = (int)OrderResponseTypes.InvoiceNotification,
                VendorDocument = string.Join(",", groupInvoice[value]),
                EdiVendorID = 1,
                VendorDocumentNumber = firstLine.OrderNr,
                ReceiveDate = DateTime.Now,
                InvoiceDocumentNumber = value,
                InvoiceDate = DateTime.Parse(debs.InvoiceDate),
                Currency = debs.CurrencyCode,
                TotalAmount = decimal.Parse(debs.InvoiceAmount),
                VatAmount = decimal.Parse(debs.InvoiceAmount) - priceEx,
                TotalExVat = priceEx,
                ConnectorRelationID = connectorRelation.ConnectorRelationID,
                VendorDocumentReference = firstLine.Reference,
                PaymentConditionDiscount = (discount - priceEx).ToString()
                //PaymentConditionCode = XmlCommunicator.ExtractData(data, "DeliveryCondition"),
                //PaymentConditionDays = int.Parse(XmlCommunicator.ExtractData(data, "PaymentCondition")),
                //PartialDelivery = bool.Parse(XmlCommunicator.ExtractData(data, "PartialDelivery")),
                //AdministrationCost = decimal.Parse(XmlCommunicator.ExtractData(data, "OrderCostAmount")),
                //DropShipmentCost = decimal.Parse(XmlCommunicator.ExtractData(data, "OrderCostAmount")),
                //EdiOrderID = order.EdiOrderID,
                //PaymentConditionDiscount = XmlCommunicator.ExtractData(data, "OrderDistcountManually"),
                //VendorDocumentReference = XmlCommunicator.ExtractData(data, "HeaderWarning")
              };

              if (ediOrderResponse != null)
                ors.EdiOrderID = ediOrderResponse.EdiOrderID;

              if (unit.Scope.Repository<EdiOrderResponse>().GetAll().Any(x => x.InvoiceDocumentNumber == value))
                continue;

              unit.Scope.Repository<EdiOrderResponse>().Add(ors);

              foreach (var line in groupInvoice[value])
              {
                EdiOrderResponseLine responseLine = new EdiOrderResponseLine()
                {
                  EdiOrderResponse = ors,
                  Ordered = int.Parse(line.Ordered),
                  Invoiced = int.Parse(line.Invoiced),
                  Backordered = 0,
                  Cancelled = 0,
                  Unit = line.UnitCode,
                  Price = decimal.Parse(line.Price),
                  //VatAmount = decimal.Parse(line.NettoAmount) - decimal.Parse(line.Price),
                  processed = false,
                  Delivered = 0,
                  VendorItemNumber = line.ItemCode,
                  Description = line.ItemDescr,
                  VendorLineNumber = line.InvoiceLineNr,
                  Remark = line.Reference,
                  CarrierCode = string.Empty,
                  TrackAndTrace = string.Empty
                };

                if (ediOrderResponse != null)
                {
                  var orderline = ediOrderResponse.EdiOrder.EdiOrderLines.FirstOrDefault(x => x.CustomerItemNumber == line.ItemCode);
                  responseLine.EdiOrderLine = orderline;
                }

                unit.Scope.Repository<EdiOrderResponseLine>().Add(responseLine);
              }
            }
          }
        }
      });
    }

    public EdiOrder GetOrderInformation(EdiOrderResponse ediOrderResponse, Configuration config)
    {
      EdiOrder ediOrder = new EdiOrder();
      ediOrder.ConnectorRelationID = ediOrderResponse.ConnectorRelationID;

      var getOrderInformation = XmlCommunicator.SendRequest(new GetOrderLineEnvelope
      {
        Header = Header,
        Body = new GetOrderLineBody
        {
          getorderline = new GetOrderLine
          {
            AdminCode = ediOrderResponse.ConnectorRelation.AdministrationCode,
            DebtorCode = ediOrderResponse.ConnectorRelation.CustomerID,
            LanguageCode = "N",
            OrderNr = ediOrderResponse.VendorDocumentNumber
          }
        }
      }, config);

      using (var reader = new StreamReader(getOrderInformation.GetResponseStream()))
      {
        var result = reader.ReadToEnd();

        foreach (var data in XmlCommunicator.ExtractDataList(result, "ttOrderHeaderRow"))
        {
          ediOrder.CustomerOrderReference = XmlCommunicator.ExtractData(data, "Reference");
          ediOrder.ReceivedDate = DateTime.Parse(XmlCommunicator.ExtractData(data, "OrderDate"));
          ediOrder.WebSiteOrderNumber = string.Empty;
          ediOrder.SoldToCustomer = new Objects.Models.Orders.Customer()
          {
            EANIdentifier = ediOrderResponse.ConnectorRelation.CustomerID,
            CustomerName = XmlCommunicator.ExtractData(data, "InvoiceAddressName"),
            CustomerAddressLine1 = XmlCommunicator.ExtractData(data, "InvoiceAddressStreet"),
            PostCode = XmlCommunicator.ExtractData(data, "InvoiceAddressZipcode"),
            City = XmlCommunicator.ExtractData(data, "InvoiceAddressPlacce"),
            Country = XmlCommunicator.ExtractData(data, "InvoiceAddressCountry"),
            CustomerAddressLine2 = XmlCommunicator.ExtractData(data, "InvoiceAddressExtraLines")
          };
        }

        var shipData = XmlCommunicator.ExtractDataList(result, "ttOrderLineRow").FirstOrDefault();
          ediOrder.ShippedToCustomer = new Objects.Models.Orders.Customer()
          {
            EANIdentifier = ediOrderResponse.ConnectorRelation.CustomerID,
            CustomerName = XmlCommunicator.ExtractData(shipData, "DeliveryAddressOrganisationName"),
            CustomerAddressLine1 = XmlCommunicator.ExtractData(shipData, "DeliveryAddressStreet"),
            PostCode = XmlCommunicator.ExtractData(shipData, "DeliveryAddressZipcode"),
            City = XmlCommunicator.ExtractData(shipData, "DeliveryAddressPlacce"),
            Country = XmlCommunicator.ExtractData(shipData, "DeliveryAddressCountry"),
            CustomerAddressLine2 = XmlCommunicator.ExtractData(shipData, "DeliveryAddressExtraLines")
          };
      }

      return ediOrder;
    }
  }

  public class GeneralOderInfo
  {
    public Header Header { get; set; }
    public ShoppingcardHeaderRow[] Headers { get; set; }
    public List<ShoppingcardLineRow> OrderLines { get; set; }
  }

  public class GeneralTempOderInfo
  {
    public Header Header { get; set; }
    public TempShoppingcardHeaderRow[] Headers { get; set; }
    public List<TempShoppingcardLineRow> OrderLines { get; set; }
  }

  public class AddressResponse
  {
    public string OrganisatieCode { get; set; }
    public string AddressCode { get; set; }
    public string AddressType { get; set; }
    public string AddressDescr { get; set; }
    public string Priority { get; set; }
    public string Street { get; set; }
    public string Zipcode { get; set; }
    public string Place { get; set; }
    public string CountryName { get; set; }
    public string SearchKey { get; set; }
    public string EanCode { get; set; }
  }

  public class OrganisationAddress
  {
    public string oDebtorCode { get; set; }
    public string oAdminCode { get; set; }
    public string AddressCode { get; set; }
    public string AddressType { get; set; }
    public string Priority { get; set; }
    public string NewOrganisationCode { get; set; }
  }

  public class BelongsToOrgsRespones
  {
    public string OrganisatieCode { get; set; }
    public string OrganisationName { get; set; }
  }

  public class GetInvoiceLineResponse
  {
    public string InvoiceNr { get; set; }
    public string InvoiceLineNr { get; set; }
    public string OrderNr { get; set; }
    public string ItemCode { get; set; }
    public string ItemDescr { get; set; }
    public string UnitCode { get; set; }
    public string Ordered { get; set; }
    public string Invoiced { get; set; }
    public string Price { get; set; }
    public string NettoAmount { get; set; }
    public string CurrencyCode { get; set; }
    public string Reference { get; set; }
  }

  public class GetOutstandingInvoiceResponse
  {
    public string InvoiceNr { get; set; }
    public string InvoiceDate { get; set; }
    public string DueDate { get; set; }
    public string CurrencyCode { get; set; }
    public string InvoiceAmount { get; set; }
    public string PayedAmount { get; set; }
    public string BalanceAmount { get; set; }
    public string Reference { get; set; }
  }

  public enum ErrorCode
  {
    Product,
    Stock,
    Price,
    Obsolete,
    Other
  }
}
