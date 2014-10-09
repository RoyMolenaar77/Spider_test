using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using Concentrator.Objects.Base;
using System.Data.Linq.Mapping;
using Concentrator.Objects.Product;
using System.Data.Linq;
using Concentrator.Objects.EDI.Order;

namespace Concentrator.Objects.EDI.Response
{
  [Table(Name = "dbo.EdiOrderResponse")]
  public class EDIOrderResponse : ObjectBase<EDIOrderResponse>
  {

    private int _ediOrderResponseID;
    private int? _ediOrderID;
    private OrderResponseType _responseType;
    private string _vendorDocument;
    private string _vendorDocumentReference;
    private int _vendorID;
    private decimal? _administrationCost;
    private decimal? _dropShipmentCost;
    private decimal? _shipmentCost;
    private DateTime? _orderDate;
    private bool? _partialDelivery;
    private string _vendorDocumentNumber;
    private DateTime? _vendorDocumentDate;
    private decimal? _vatPercentage;
    private decimal? _vatAmount;
    private decimal? _totalGoods;
    private decimal? _totalExVat;
    private decimal? _totalAmount;
    private int? _paymentConditionDays;
    private string _paymentConditionCode;
    private string _paymentConditionDiscount;
    private string _paymentConditionDiscountDescription;
    private string _trackAndTrace;
    private string _trackAndTraceLink;
    private string _invoiceDocumentNumber;
    private string _shippingNumber;
    private DateTime? _reqDeliveryDate;
    private DateTime? _invoiceDate;
    private string _currency;
    private string _despAdvice;
    private int? _shipToCustomerID;
    private int? _soldToCustomerID;
    private DateTime _receiveDate;
    public List<ReturnError> ResponseErrors { get; set; }
    private EntityRef<EDIOrder> _ediOrder;
    private DateTime _receiveDate;
    public List<ReturnError> ResponseErrors { get; set; }

    EntitySet<EDIOrderResponseLine> _ediOrderResponseLines;

    public EDIOrderResponse()
    {
      _ediOrderResponseLines = new EntitySet<EDIOrderResponseLine>(x => x.EdiOrderResponse = this, x => x.EdiOrderResponse = null);
    }

    [Column(Storage = "_ediOrderResponseID", DbType = "Int NOT NULL", IsDbGenerated = true, IsPrimaryKey = true)]
    public int EdiOrderResponseID
    {
      get { return _ediOrderResponseID; }
      set
      {
        if (_ediOrderResponseID != value)
        {
          PropertyIsChanging("EdiOrderResponseID");
          _ediOrderResponseID = value;
          PropertyHasChanged("EdiOrderResponseID");
        }
      }
    }

    [Column(Storage = "_responseType", DbType = "NVarChar(50) NOT NULL", CanBeNull = false)]
    public string ResponseType
    {
      get { return _responseType; }
      set
      {
        if (_responseType != value)
        {
          PropertyIsChanging("ResponseType");
          _responseType = value;
          PropertyHasChanged("ResponseType");
        }
      }
    }

    [Column(Storage = "_vendorDocument", DbType = "NVarchar(Max) NOT NULL", CanBeNull = false)]
    public string VendorDocument
    {
      get { return _vendorDocument; }
      set
      {
        if (_vendorDocument != value)
        {
          PropertyIsChanging("VendorDocument");
          _vendorDocument = value;
          PropertyHasChanged("VendorDocument");
        }
      }
    }

    [Column(Storage = "_vendorID", DbType = "INT NOT NULL", CanBeNull = false)]
    public int VendorID
    {
      get { return _vendorID; }
      set
      {
        if (_vendorID != value)
        {
          PropertyIsChanging("VendorID");
          _vendorID = value;
          PropertyHasChanged("VendorID");
        }
      }
    }

    [Column(Storage = "_administrationCost", DbType = "Decimal(18,4) NULL", CanBeNull = true)]
    public decimal? AdministrationCost
    {
      get { return _administrationCost; }
      set
      {
        if (_administrationCost != value)
        {
          PropertyIsChanging("AdministrationCost");
          _administrationCost = value;
          PropertyHasChanged("AdministrationCost");
        }
      }
    }

    [Column(Storage = "_dropShipmentCost", DbType = "Decimal(18,4) NULL", CanBeNull = true)]
    public decimal? DropShipmentCost
    {
      get { return _dropShipmentCost; }
      set
      {
        if (_dropShipmentCost != value)
        {
          PropertyIsChanging("DropShipmentCost");
          _dropShipmentCost = value;
          PropertyHasChanged("DropShipmentCost");
        }
      }
    }

    [Column(Storage = "_shipmentCost", DbType = "Decimal(18,4) NULL", CanBeNull = true)]
    public decimal? ShipmentCost
    {
      get { return _shipmentCost; }
      set
      {
        if (_shipmentCost != value)
        {
          PropertyIsChanging("ShipmentCost");
          _shipmentCost = value;
          PropertyHasChanged("ShipmentCost");
        }
      }
    }

    [Column(Storage = "_orderDate", DbType = "DateTime NULL", CanBeNull = true)]
    public DateTime? OrderDate
    {
      get { return _orderDate; }
      set
      {
        if (_orderDate != value)
        {
          PropertyIsChanging("OrderDate");
          _orderDate = value;
          PropertyHasChanged("OrderDate");
        }
      }
    }

    [Column(Storage = "_partialDelivery", DbType = "Bit NULL", CanBeNull = true)]
    public bool? PartialDelivery
    {
      get { return _partialDelivery; }
      set
      {
        if (_partialDelivery != value)
        {
          PropertyIsChanging("PartialDelivery");
          _partialDelivery = value;
          PropertyHasChanged("PartialDelivery");
        }
      }
    }

    [Column(Storage = "_vendorDocumentNumber", DbType = "NVarChar(50) NOT NULL", CanBeNull = false)]
    public string VendorDocumentNumber
    {
      get { return _vendorDocumentNumber; }
      set
      {
        if (_vendorDocumentNumber != value)
        {
          PropertyIsChanging("VendorDocumentNumber");
          _vendorDocumentNumber = value;
          PropertyHasChanged("VendorDocumentNumber");
        }
      }
    }

    [Column(Storage = "_vendorDocumentDate", DbType = "DateTime NULL", CanBeNull = true)]
    public DateTime? VendorDocumentDate
    {
      get { return _vendorDocumentDate; }
      set
      {
        if (_vendorDocumentDate != value)
        {
          PropertyIsChanging("VendorDocumentDate");
          _vendorDocumentDate = value;
          PropertyHasChanged("VendorDocumentDate");
        }
      }
    }

    [Column(Storage = "_vatPercentage", DbType = "Decimal(18,4) NULL", CanBeNull = true)]
    public decimal? VatPercentage
    {
      get { return _vatPercentage; }
      set
      {
        if (_vatPercentage != value)
        {
          PropertyIsChanging("VatPercentage");
          _vatPercentage = value;
          PropertyHasChanged("VatPercentage");
        }
      }
    }

    [Column(Storage = "_vatAmount", DbType = "Decimal(18,4) NULL", CanBeNull = true)]
    public decimal? VatAmount
    {
      get { return _vatAmount; }
      set
      {
        if (_vatAmount != value)
        {
          PropertyIsChanging("VatAmount");
          _vatAmount = value;
          PropertyHasChanged("VatAmount");
        }
      }
    }

    [Column(Storage = "_totalGoods", DbType = "Decimal(18,4) NULL", CanBeNull = true)]
    public decimal? TotalGoods
    {
      get { return _totalGoods; }
      set
      {
        if (_totalGoods != value)
        {
          PropertyIsChanging("TotalGoods");
          _totalGoods = value;
          PropertyHasChanged("TotalGoods");
        }
      }
    }

    [Column(Storage = "_totalExVat", DbType = "Decimal(18,4) NULL", CanBeNull = true)]
    public decimal? TotalExVat
    {
      get { return _totalExVat; }
      set
      {
        if (_totalExVat != value)
        {
          PropertyIsChanging("TotalExVat");
          _totalExVat = value;
          PropertyHasChanged("TotalExVat");
        }
      }
    }

    [Column(Storage = "_totalAmount", DbType = "Decimal(18,4) NULL", CanBeNull = true)]
    public decimal? TotalAmount
    {
      get { return _totalAmount; }
      set
      {
        if (_totalAmount != value)
        {
          PropertyIsChanging("TotalAmount");
          _totalAmount = value;
          PropertyHasChanged("TotalAmount");
        }
      }
    }

    [Column(Storage = "_paymentConditionDays", DbType = "INT NULL", CanBeNull = true)]
    public int? PaymentConditionDays
    {
      get { return _paymentConditionDays; }
      set
      {
        if (_paymentConditionDays != value)
        {
          PropertyIsChanging("PaymentConditionDays");
          _paymentConditionDays = value;
          PropertyHasChanged("PaymentConditionDays");
        }
      }
    }


    [Column(Storage = "_paymentConditionCode", DbType = "NVarChar(50) NULL", CanBeNull = true)]
    public string PaymentConditionCode
    {
      get { return _paymentConditionCode; }
      set
      {
        if (_paymentConditionCode != value)
        {
          PropertyIsChanging("PaymentConditionCode");
          _paymentConditionCode = value;
          PropertyHasChanged("PaymentConditionCode");
        }
      }
    }

    [Column(Storage = "_paymentConditionDiscount", DbType = "NVarChar(50) NULL", CanBeNull = true)]
    public string PaymentConditionDiscount
    {
      get { return _paymentConditionDiscount; }
      set
      {
        if (_paymentConditionDiscount != value)
        {
          PropertyIsChanging("PaymentConditionDiscount");
          _paymentConditionDiscount = value;
          PropertyHasChanged("PaymentConditionDiscount");
        }
      }
    }

    [Column(Storage = "_paymentConditionDiscountDescription", DbType = "NVarChar(100) NULL", CanBeNull = true)]
    public string PaymentConditionDiscountDescription
    {
      get { return _paymentConditionDiscountDescription; }
      set
      {
        if (_paymentConditionDiscountDescription != value)
        {
          PropertyIsChanging("PaymentConditionDiscountDescription");
          _paymentConditionDiscountDescription = value;
          PropertyHasChanged("PaymentConditionDiscountDescription");
        }
      }
    }


    [Column(Storage = "_trackAndTrace", DbType = "NVarChar(150) NULL", CanBeNull = true)]
    public string TrackAndTrace
    {
      get { return _trackAndTrace; }
      set
      {
        if (_trackAndTrace != value)
        {
          PropertyIsChanging("TrackAndTrace");
          _trackAndTrace = value;
          PropertyHasChanged("TrackAndTrace");
        }
      }
    }

    [Column(Storage = "_invoiceDocumentNumber", DbType = "NVarChar(50) NULL", CanBeNull = true)]
    public string InvoiceDocumentNumber
    {
      get { return _invoiceDocumentNumber; }
      set
      {
        if (_invoiceDocumentNumber != value)
        {
          PropertyIsChanging("InvoiceDocumentNumber");
          _invoiceDocumentNumber = value;
          PropertyHasChanged("InvoiceDocumentNumber");
        }
      }
    }

    [Column(Storage = "_shippingNumber", DbType = "NVarChar(50) NULL", CanBeNull = true)]
    public string ShippingNumber
    {
      get { return _shippingNumber; }
      set
      {
        if (_shippingNumber != value)
        {
          PropertyIsChanging("ShippingNumber");
          _shippingNumber = value;
          PropertyHasChanged("ShippingNumber");
        }
      }
    }

    [Column(Storage = "_reqDeliveryDate", DbType = "DateTime NULL", CanBeNull = true)]
    public DateTime? ReqDeliveryDate
    {
      get { return _reqDeliveryDate; }
      set
      {
        if (_reqDeliveryDate != value)
        {
          PropertyIsChanging("ReqDeliveryDate");
          _reqDeliveryDate = value;
          PropertyHasChanged("ReqDeliveryDate");
        }
      }
    }

    [Column(Storage = "_invoiceDate", DbType = "DateTime NULL", CanBeNull = true)]
    public DateTime? InvoiceDate
    {
      get { return _invoiceDate; }
      set
      {
        if (_invoiceDate != value)
        {
          PropertyIsChanging("InvoiceDate");
          _invoiceDate = value;
          PropertyHasChanged("InvoiceDate");
        }
      }
    }

    [Column(Storage = "_currency", DbType = "nvarchar(50) null", CanBeNull = true)]
    public string Currency
    {
      get { return _currency; }
      set
      {
        if (_currency != value)
        {
          PropertyIsChanging("Currency");
          _currency = value;
          PropertyHasChanged("Currency");
        }
      }
    }

    [Column(Storage = "_despAdvice", DbType = "NVarChar(50) NULL", CanBeNull = true)]
    public string DespAdvice
    {
      get { return _despAdvice; }
      set
      {
        if (_despAdvice != value)
        {
          PropertyIsChanging("DespAdvice");
          _despAdvice = value;
          PropertyHasChanged("DespAdvice");
        }
      }
    }

    [Column(Storage = "_shipToCustomerID", DbType = "INT NOT NULL", CanBeNull = true)]
    public int? ShipToCustomerID
    {
      get { return _shipToCustomerID; }
      set
      {
        if (_shipToCustomerID != value)
        {
          PropertyIsChanging("ShipToCustomerID");
          _shipToCustomerID = value;
          PropertyHasChanged("ShipToCustomerID");
        }
      }
    }

    [Column(Storage = "_soldToCustomerID", DbType = "INT NOT NULL", CanBeNull = true)]
    public int? SoldToCustomerID
    {
      get { return _soldToCustomerID; }
      set
      {
        if (_soldToCustomerID != value)
        {
          PropertyIsChanging("SoldToCustomerID");
          _soldToCustomerID = value;
          PropertyHasChanged("SoldToCustomerID");
        }
      }
    }

    [Column(Storage = "_receiveDate", DbType = "DateTime NOT NULL", CanBeNull = false)]
    public DateTime ReceiveDate
    {
      get { return _receiveDate; }
      set
      {
        if (_receiveDate != value)
        {
          PropertyIsChanging("ReceiveDate");
          _receiveDate = value;
          PropertyHasChanged("ReceiveDate");
        }
      }
    }

    [Column(Storage = "_trackAndTraceLink", DbType = "NVarChar(255) NULL", CanBeNull = true)]
    public string TrackAndTraceLink
    {
      get { return _trackAndTraceLink; }
      set
      {
        if (_trackAndTraceLink != value)
        {
          PropertyIsChanging("TrackAndTraceLink");
          _trackAndTraceLink = value;
          PropertyHasChanged("TrackAndTraceLink");
        }
      }
    }

    [Column(Storage = "_vendorDocumentReference", DbType = "NVarChar(255) NULL", CanBeNull = true)]
    public string VendorDocumentReference
    {
      get { return _vendorDocumentReference; }
      set
      {
        if (_vendorDocumentReference != value)
        {
          PropertyIsChanging("VendorDocumentReference");
          _vendorDocumentReference = value;
          PropertyHasChanged("VendorDocumentReference");
        }
      }
    }

    [Column(Storage = "_orderID", DbType = "INT NULL", CanBeNull = true)]
    public int? OrderID
    {
      get { return _orderID; }
      set
      {
        if (_orderID != value)
        {
          PropertyIsChanging("OrderID");
          _orderID = value;
          PropertyHasChanged("OrderID");
        }
      }
    }


    [Association(Name = "FK_EdiOrderResponseLine_EdiOrderResponse", Storage = "_ediOrderResponseLines", ThisKey = "EdiOrderResponseID", OtherKey = "EdiOrderResponseID", DeleteRule = "NO ACTION")]
    public EntitySet<EDIOrderResponseLine> EdiOrderResponseLines
    {
      get { return _ediOrderResponseLines; }
      set { _ediOrderResponseLines.Assign(value); }
    }
>>>>>>> main/edi

    EntitySet<EDIOrderResponseLine> _ediOrderResponseLines;

    public EDIOrderResponse()
    {
      _ediOrderResponseLines = new EntitySet<EDIOrderResponseLine>(x => x.EdiOrderResponse = this, x => x.EdiOrderResponse = null);
      _ediOrder = default(EntityRef<EDIOrder>);
    }

    [Column(Storage = "_ediOrderResponseID", DbType = "Int NOT NULL", IsDbGenerated = true, IsPrimaryKey = true)]
    public int EdiOrderResponseID
    {
      get { return _ediOrderResponseID; }
      set
      {
        if (_ediOrderResponseID != value)
        {
          PropertyIsChanging("EdiOrderResponseID");
          _ediOrderResponseID = value;
          PropertyHasChanged("EdiOrderResponseID");
        }
      }
    }

    [Column(Storage = "_responseType", DbType = "Int NOT NULL", CanBeNull = false)]
    public OrderResponseType ResponseType
    {
      get { return _responseType; }
      set
      {
        if (_responseType != value)
        {
          PropertyIsChanging("ResponseType");
          _responseType = value;
          PropertyHasChanged("ResponseType");
        }
      }
    }

    [Column(Storage = "_vendorDocument", DbType = "NVarchar(Max) NOT NULL", CanBeNull = false)]
    public string VendorDocument
    {
      get { return _vendorDocument; }
      set
      {
        if (_vendorDocument != value)
        {
          PropertyIsChanging("VendorDocument");
          _vendorDocument = value;
          PropertyHasChanged("VendorDocument");
        }
      }
    }

    [Column(Storage = "_vendorID", DbType = "INT NOT NULL", CanBeNull = false)]
    public int VendorID
    {
      get { return _vendorID; }
      set
      {
        if (_vendorID != value)
        {
          PropertyIsChanging("VendorID");
          _vendorID = value;
          PropertyHasChanged("VendorID");
        }
      }
    }

    [Column(Storage = "_administrationCost", DbType = "Decimal(18,4) NULL", CanBeNull = true)]
    public decimal? AdministrationCost
    {
      get { return _administrationCost; }
      set
      {
        if (_administrationCost != value)
        {
          PropertyIsChanging("AdministrationCost");
          _administrationCost = value;
          PropertyHasChanged("AdministrationCost");
        }
      }
    }

    [Column(Storage = "_dropShipmentCost", DbType = "Decimal(18,4) NULL", CanBeNull = true)]
    public decimal? DropShipmentCost
    {
      get { return _dropShipmentCost; }
      set
      {
        if (_dropShipmentCost != value)
        {
          PropertyIsChanging("DropShipmentCost");
          _dropShipmentCost = value;
          PropertyHasChanged("DropShipmentCost");
        }
      }
    }

    [Column(Storage = "_shipmentCost", DbType = "Decimal(18,4) NULL", CanBeNull = true)]
    public decimal? ShipmentCost
    {
      get { return _shipmentCost; }
      set
      {
        if (_shipmentCost != value)
        {
          PropertyIsChanging("ShipmentCost");
          _shipmentCost = value;
          PropertyHasChanged("ShipmentCost");
        }
      }
    }

    [Column(Storage = "_orderDate", DbType = "DateTime NULL", CanBeNull = true)]
    public DateTime? OrderDate
    {
      get { return _orderDate; }
      set
      {
        if (_orderDate != value)
        {
          PropertyIsChanging("OrderDate");
          _orderDate = value;
          PropertyHasChanged("OrderDate");
        }
      }
    }

    [Column(Storage = "_partialDelivery", DbType = "Bit NULL", CanBeNull = true)]
    public bool? PartialDelivery
    {
      get { return _partialDelivery; }
      set
      {
        if (_partialDelivery != value)
        {
          PropertyIsChanging("PartialDelivery");
          _partialDelivery = value;
          PropertyHasChanged("PartialDelivery");
        }
      }
    }

    [Column(Storage = "_vendorDocumentNumber", DbType = "NVarChar(50) NOT NULL", CanBeNull = false)]
    public string VendorDocumentNumber
    {
      get { return _vendorDocumentNumber; }
      set
      {
        if (_vendorDocumentNumber != value)
        {
          PropertyIsChanging("VendorDocumentNumber");
          _vendorDocumentNumber = value;
          PropertyHasChanged("VendorDocumentNumber");
        }
      }
    }

    [Column(Storage = "_vendorDocumentDate", DbType = "DateTime NULL", CanBeNull = true)]
    public DateTime? VendorDocumentDate
    {
      get { return _vendorDocumentDate; }
      set
      {
        if (_vendorDocumentDate != value)
        {
          PropertyIsChanging("VendorDocumentDate");
          _vendorDocumentDate = value;
          PropertyHasChanged("VendorDocumentDate");
        }
      }
    }

    [Column(Storage = "_vatPercentage", DbType = "Decimal(18,4) NULL", CanBeNull = true)]
    public decimal? VatPercentage
    {
      get { return _vatPercentage; }
      set
      {
        if (_vatPercentage != value)
        {
          PropertyIsChanging("VatPercentage");
          _vatPercentage = value;
          PropertyHasChanged("VatPercentage");
        }
      }
    }

    [Column(Storage = "_vatAmount", DbType = "Decimal(18,4) NULL", CanBeNull = true)]
    public decimal? VatAmount
    {
      get { return _vatAmount; }
      set
      {
        if (_vatAmount != value)
        {
          PropertyIsChanging("VatAmount");
          _vatAmount = value;
          PropertyHasChanged("VatAmount");
        }
      }
    }

    [Column(Storage = "_totalGoods", DbType = "Decimal(18,4) NULL", CanBeNull = true)]
    public decimal? TotalGoods
    {
      get { return _totalGoods; }
      set
      {
        if (_totalGoods != value)
        {
          PropertyIsChanging("TotalGoods");
          _totalGoods = value;
          PropertyHasChanged("TotalGoods");
        }
      }
    }

    [Column(Storage = "_totalExVat", DbType = "Decimal(18,4) NULL", CanBeNull = true)]
    public decimal? TotalExVat
    {
      get { return _totalExVat; }
      set
      {
        if (_totalExVat != value)
        {
          PropertyIsChanging("TotalExVat");
          _totalExVat = value;
          PropertyHasChanged("TotalExVat");
        }
      }
    }

    [Column(Storage = "_totalAmount", DbType = "Decimal(18,4) NULL", CanBeNull = true)]
    public decimal? TotalAmount
    {
      get { return _totalAmount; }
      set
      {
        if (_totalAmount != value)
        {
          PropertyIsChanging("TotalAmount");
          _totalAmount = value;
          PropertyHasChanged("TotalAmount");
        }
      }
    }

    [Column(Storage = "_paymentConditionDays", DbType = "INT NULL", CanBeNull = true)]
    public int? PaymentConditionDays
    {
      get { return _paymentConditionDays; }
      set
      {
        if (_paymentConditionDays != value)
        {
          PropertyIsChanging("PaymentConditionDays");
          _paymentConditionDays = value;
          PropertyHasChanged("PaymentConditionDays");
        }
      }
    }


    [Column(Storage = "_paymentConditionCode", DbType = "NVarChar(50) NULL", CanBeNull = true)]
    public string PaymentConditionCode
    {
      get { return _paymentConditionCode; }
      set
      {
        if (_paymentConditionCode != value)
        {
          PropertyIsChanging("PaymentConditionCode");
          _paymentConditionCode = value;
          PropertyHasChanged("PaymentConditionCode");
        }
      }
    }

    [Column(Storage = "_paymentConditionDiscount", DbType = "NVarChar(50) NULL", CanBeNull = true)]
    public string PaymentConditionDiscount
    {
      get { return _paymentConditionDiscount; }
      set
      {
        if (_paymentConditionDiscount != value)
        {
          PropertyIsChanging("PaymentConditionDiscount");
          _paymentConditionDiscount = value;
          PropertyHasChanged("PaymentConditionDiscount");
        }
      }
    }

    [Column(Storage = "_paymentConditionDiscountDescription", DbType = "NVarChar(100) NULL", CanBeNull = true)]
    public string PaymentConditionDiscountDescription
    {
      get { return _paymentConditionDiscountDescription; }
      set
      {
        if (_paymentConditionDiscountDescription != value)
        {
          PropertyIsChanging("PaymentConditionDiscountDescription");
          _paymentConditionDiscountDescription = value;
          PropertyHasChanged("PaymentConditionDiscountDescription");
        }
      }
    }


    [Column(Storage = "_trackAndTrace", DbType = "NVarChar(150) NULL", CanBeNull = true)]
    public string TrackAndTrace
    {
      get { return _trackAndTrace; }
      set
      {
        if (_trackAndTrace != value)
        {
          PropertyIsChanging("TrackAndTrace");
          _trackAndTrace = value;
          PropertyHasChanged("TrackAndTrace");
        }
      }
    }

    [Column(Storage = "_invoiceDocumentNumber", DbType = "NVarChar(50) NULL", CanBeNull = true)]
    public string InvoiceDocumentNumber
    {
      get { return _invoiceDocumentNumber; }
      set
      {
        if (_invoiceDocumentNumber != value)
        {
          PropertyIsChanging("InvoiceDocumentNumber");
          _invoiceDocumentNumber = value;
          PropertyHasChanged("InvoiceDocumentNumber");
        }
      }
    }

    [Column(Storage = "_shippingNumber", DbType = "NVarChar(50) NULL", CanBeNull = true)]
    public string ShippingNumber
    {
      get { return _shippingNumber; }
      set
      {
        if (_shippingNumber != value)
        {
          PropertyIsChanging("ShippingNumber");
          _shippingNumber = value;
          PropertyHasChanged("ShippingNumber");
        }
      }
    }

    [Column(Storage = "_reqDeliveryDate", DbType = "DateTime NULL", CanBeNull = true)]
    public DateTime? ReqDeliveryDate
    {
      get { return _reqDeliveryDate; }
      set
      {
        if (_reqDeliveryDate != value)
        {
          PropertyIsChanging("ReqDeliveryDate");
          _reqDeliveryDate = value;
          PropertyHasChanged("ReqDeliveryDate");
        }
      }
    }

    [Column(Storage = "_invoiceDate", DbType = "DateTime NULL", CanBeNull = true)]
    public DateTime? InvoiceDate
    {
      get { return _invoiceDate; }
      set
      {
        if (_invoiceDate != value)
        {
          PropertyIsChanging("InvoiceDate");
          _invoiceDate = value;
          PropertyHasChanged("InvoiceDate");
        }
      }
    }

    [Column(Storage = "_currency", DbType = "nvarchar(50) null", CanBeNull = true)]
    public string Currency
    {
      get { return _currency; }
      set
      {
        if (_currency != value)
        {
          PropertyIsChanging("Currency");
          _currency = value;
          PropertyHasChanged("Currency");
        }
      }
    }

    [Column(Storage = "_despAdvice", DbType = "NVarChar(50) NULL", CanBeNull = true)]
    public string DespAdvice
    {
      get { return _despAdvice; }
      set
      {
        if (_despAdvice != value)
        {
          PropertyIsChanging("DespAdvice");
          _despAdvice = value;
          PropertyHasChanged("DespAdvice");
        }
      }
    }

    [Column(Storage = "_shipToCustomerID", DbType = "INT NOT NULL", CanBeNull = true)]
    public int? ShipToCustomerID
    {
      get { return _shipToCustomerID; }
      set
      {
        if (_shipToCustomerID != value)
        {
          PropertyIsChanging("ShipToCustomerID");
          _shipToCustomerID = value;
          PropertyHasChanged("ShipToCustomerID");
        }
      }
    }

    [Column(Storage = "_soldToCustomerID", DbType = "INT NOT NULL", CanBeNull = true)]
    public int? SoldToCustomerID
    {
      get { return _soldToCustomerID; }
      set
      {
        if (_soldToCustomerID != value)
        {
          PropertyIsChanging("SoldToCustomerID");
          _soldToCustomerID = value;
          PropertyHasChanged("SoldToCustomerID");
        }
      }
    }

    [Column(Storage = "_receiveDate", DbType = "DateTime NOT NULL", CanBeNull = false)]
    public DateTime ReceiveDate
    {
      get { return _receiveDate; }
      set
      {
        if (_receiveDate != value)
        {
          PropertyIsChanging("ReceiveDate");
          _receiveDate = value;
          PropertyHasChanged("ReceiveDate");
        }
      }
    }

    [Column(Storage = "_trackAndTraceLink", DbType = "NVarChar(255) NULL", CanBeNull = true)]
    public string TrackAndTraceLink
    {
      get { return _trackAndTraceLink; }
      set
      {
        if (_trackAndTraceLink != value)
        {
          PropertyIsChanging("TrackAndTraceLink");
          _trackAndTraceLink = value;
          PropertyHasChanged("TrackAndTraceLink");
        }
      }
    }

    [Column(Storage = "_vendorDocumentReference", DbType = "NVarChar(255) NULL", CanBeNull = true)]
    public string VendorDocumentReference
    {
      get { return _vendorDocumentReference; }
      set
      {
        if (_vendorDocumentReference != value)
        {
          PropertyIsChanging("VendorDocumentReference");
          _vendorDocumentReference = value;
          PropertyHasChanged("VendorDocumentReference");
        }
      }
    }

    [Column(Storage = "_ediOrderID", DbType = "INT NULL", CanBeNull = true)]
    public int? EdiOrderID
    {
      get { return _ediOrderID; }
      set
      {
        if (_ediOrderID != value)
        {
          PropertyIsChanging("EdiOrderID");
          _ediOrderID = value;
          PropertyHasChanged("EdiOrderID");
        }
      }
    }

        [Association(Name = "FK_EdiOrderResponse_EdiOrder", Storage = "_ediOrder", OtherKey = "EdiOrderID", ThisKey = "EdiOrderID", IsForeignKey = true)]
    public EDIOrder EDIOrder
    {
      get { return _ediOrder.Entity; }
      set
      {
        EDIOrder previousValue = _ediOrder.Entity;
        if (((previousValue != value)
             || (_ediOrder.HasLoadedOrAssignedValue == false)))
        {
          PropertyIsChanging("EDIOrder");
          if ((previousValue != null))
          {
            _ediOrder.Entity = null;
            previousValue.EDIOrderResponses.Remove(this);
          }
          _ediOrder.Entity = value;
          if ((value != null))
          {
            value.EDIOrderResponses.Add(this);
            _ediOrderID = value.EdiOrderID;
          }
          else
          {
            _ediOrderID = default(int);
          }
          PropertyHasChanged("EDIOrder");
        }
      }
    }

    [Association(Name = "FK_EdiOrderResponseLine_EdiOrderResponse", Storage = "_ediOrderResponseLines", ThisKey = "EdiOrderResponseID", OtherKey = "EdiOrderResponseID", DeleteRule = "NO ACTION")]
    public EntitySet<EDIOrderResponseLine> EdiOrderResponseLines
    {
      get { return _ediOrderResponseLines; }
      set { _ediOrderResponseLines.Assign(value); }
    }


  }
}
