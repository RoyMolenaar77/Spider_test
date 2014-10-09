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
  [Table(Name = "dbo.EDIOrderResponseLine")]
  public class EDIOrderResponseLine : ObjectBase<EDIOrderResponseLine>
  {
    private int _ediOrderResponseLineID;
    private int _ediOrderResponseID;
    private int _ordered;
    private int _backordered;
    private int _cancelled;
    private int _shipped;
    private int _delivered;
    private int _invoiced;
    private string _unit;
    private decimal _price;
    private DateTime? _deliveryDate;
    private string _vendorLineNumber;
    private string _vendorItemNumber;
    private string _OEMNumber;
    private string _html;
    private string _productName;
    private string _barcode;
    private string _remark;
    private int? _ediOrderLineID;
    private string _description;
    private bool _processed;
    private DateTime? _requestDate;
    private decimal? _vatAmount;
    private decimal? _vatPercentage;
    private string _carrierCode;
    private int? _numberOfPallets;
    private int? _numberOfUnits;
    private string _trackAndTrace;
    private string _trackAndTraceLink;
    private string _serialNumbers;
    private EntityRef<EDIOrderLine> _ediOrderLine;

    private EntityRef<EDIOrderResponse> _ediOrderResponse;

    public EDIOrderResponseLine()
    {
      _ediOrderResponse = default(EntityRef<EDIOrderResponse>);
      _ediOrderLine = default(EntityRef<EDIOrderLine>);
    }

    [Column(Storage = "_ediOrderResponseLineID", DbType = "Int NOT NULL", IsPrimaryKey = true, IsDbGenerated = true)]
    public int EdiOrderResponseLineID
    {
      get { return _ediOrderResponseLineID; }
      set
      {
        if (_ediOrderResponseLineID != value)
        {
          PropertyIsChanging("EdiOrderResponseLineID");
          _ediOrderResponseLineID = value;
          PropertyHasChanged("EdiOrderResponseLineID");
        }
      }
    }

    [Column(Storage = "_ediOrderResponseID", DbType = "INT NOT NULL", CanBeNull = false)]
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

    [Column(Storage = "_ediOrderLineID", DbType = "INT NULL", CanBeNull = true)]
    public int? EdiOrderLineID
    {
      get { return _ediOrderLineID; }
      set
      {
        if (_ediOrderLineID != value)
        {
          PropertyIsChanging("EdiOrderLineID");
          _ediOrderLineID = value;
          PropertyHasChanged("EdiOrderLineID");
        }
      }
    }

    [Column(Storage = "_ordered", DbType = "INT NOT NULL", CanBeNull = false)]
    public int Ordered
    {
      get { return _ordered; }
      set
      {
        if (_ordered != value)
        {
          PropertyIsChanging("Ordered");
          _ordered = value;
          PropertyHasChanged("Ordered");
        }
      }
    }

    [Column(Storage = "_backordered", DbType = "INT NOT NULL", CanBeNull = false)]
    public int Backordered
    {
      get { return _backordered; }
      set
      {
        if (_backordered != value)
        {
          PropertyIsChanging("Backordered");
          _backordered = value;
          PropertyHasChanged("Backordered");
        }
      }
    }

    [Column(Storage = "_cancelled", DbType = "INT NOT NULL", CanBeNull = false)]
    public int Cancelled
    {
      get { return _cancelled; }
      set
      {
        if (_cancelled != value)
        {
          PropertyIsChanging("Cancelled");
          _cancelled = value;
          PropertyHasChanged("Cancelled");
        }
      }
    }

    [Column(Storage = "_shipped", DbType = "INT NOT NULL", CanBeNull = false)]
    public int Shipped
    {
      get { return _shipped; }
      set
      {
        if (_shipped != value)
        {
          PropertyIsChanging("Shipped");
          _shipped = value;
          PropertyHasChanged("Shipped");
        }
      }
    }

    [Column(Storage = "_invoiced", DbType = "INT NOT NULL", CanBeNull = false)]
    public int Invoiced
    {
      get { return _invoiced; }
      set
      {
        if (_invoiced != value)
        {
          PropertyIsChanging("Invoiced");
          _invoiced = value;
          PropertyHasChanged("Invoiced");
        }
      }
    }

    [Column(Storage = "_unit", DbType = "NVarChar(50) NULL", CanBeNull = true)]
    public string Unit
    {
      get { return _unit; }
      set
      {
        if (_unit != value)
        {
          PropertyIsChanging("Unit");
          _unit = value;
          PropertyHasChanged("Unit");
        }
      }
    }

    [Column(Storage = "_price", DbType = "Decimal(18,4) NOT NULL", CanBeNull = false)]
    public decimal Price
    {
      get { return _price; }
      set
      {
        if (_price != value)
        {
          PropertyIsChanging("Price");
          _price = value;
          PropertyHasChanged("Price");
        }
      }
    }

    [Column(Storage = "_deliveryDate", DbType = "DateTime NULL", CanBeNull = true)]
    public DateTime? DeliveryDate
    {
      get { return _deliveryDate; }
      set
      {
        if (_deliveryDate != value)
        {
          PropertyIsChanging("DeliveryDate");
          _deliveryDate = value;
          PropertyHasChanged("DeliveryDate");
        }
      }
    }

    [Column(Storage = "_vendorLineNumber", DbType = "NVarChar(50) NULL", CanBeNull = true)]
    public string VendorLineNumber
    {
      get { return _vendorLineNumber; }
      set
      {
        if (_vendorLineNumber != value)
        {
          PropertyIsChanging("VendorLineNumber");
          _vendorLineNumber = value;
          PropertyHasChanged("VendorLineNumber");
        }
      }
    }

    [Column(Storage = "_vendorItemNumber", DbType = "NVarChar(50) NULL", CanBeNull = true)]
    public string VendorItemNumber
    {
      get { return _vendorItemNumber; }
      set
      {
        if (_vendorItemNumber != value)
        {
          PropertyIsChanging("VendorItemNumber");
          _vendorItemNumber = value;
          PropertyHasChanged("VendorItemNumber");
        }
      }
    }

    [Column(Storage = "_OEMNumber", DbType = "NVarChar(50) NULL", CanBeNull = true)]
    public string OEMNumber
    {
      get { return _OEMNumber; }
      set
      {
        if (_OEMNumber != value)
        {
          PropertyIsChanging("OEMNumber");
          _OEMNumber = value;
          PropertyHasChanged("OEMNumber");
        }
      }
    }

    [Column(Storage = "_barcode", DbType = "NVarChar(50) NULL", CanBeNull = true)]
    public string Barcode
    {
      get { return _barcode; }
      set
      {
        if (_barcode != value)
        {
          PropertyIsChanging("Barcode");
          _barcode = value;
          PropertyHasChanged("Barcode");
        }
      }
    }

    [Column(Storage = "_remark", DbType = "NVarChar(150) NULL", CanBeNull = true)]
    public string Remark
    {
      get { return _remark; }
      set
      {
        if (_remark != value)
        {
          PropertyIsChanging("Remark");
          _remark = value;
          PropertyHasChanged("Remark");
        }
      }
    }

    [Column(Storage = "_description", DbType = "NVarChar(255) NULL", CanBeNull = true)]
    public string Description
    {
      get { return _description; }
      set
      {
        if (_description != value)
        {
          PropertyIsChanging("Description");
          _description = value;
          PropertyHasChanged("Description");
        }
      }
    }

    [Column(Storage = "_processed", DbType = "BIT NOT NULL", CanBeNull = false)]
    public bool Processed
    {
      get { return _processed; }
      set
      {
        if (_processed != value)
        {
          PropertyIsChanging("Processed");
          _processed = value;
          PropertyHasChanged("Processed");
        }
      }
    }

    [Column(Storage = "_requestDate", DbType = "DateTime NULL", CanBeNull = true)]
    public DateTime? RequestDate
    {
      get { return _requestDate; }
      set
      {
        if (_requestDate != value)
        {
          PropertyIsChanging("RequestDate");
          _requestDate = value;
          PropertyHasChanged("RequestDate");
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

    [Column(Storage = "_carrierCode", DbType = "NVarChar(50) NULL", CanBeNull = true)]
    public string CarrierCode
    {
      get { return _carrierCode; }
      set
      {
        if (_carrierCode != value)
        {
          PropertyIsChanging("CarrierCode");
          _carrierCode = value;
          PropertyHasChanged("CarrierCode");
        }
      }
    }

    [Column(Storage = "_numberOfPallets", DbType = "INT NULL", CanBeNull = true)]
    public int? NumberOfPallets
    {
      get { return _numberOfPallets; }
      set
      {
        if (_numberOfPallets != value)
        {
          PropertyIsChanging("NumberOfPallets");
          _numberOfPallets = value;
          PropertyHasChanged("NumberOfPallets");
        }
      }
    }

    [Column(Storage = "_numberOfUnits", DbType = "INT NULL", CanBeNull = true)]
    public int? NumberOfUnits
    {
      get { return _numberOfUnits; }
      set
      {
        if (_numberOfUnits != value)
        {
          PropertyIsChanging("NumberOfUnits");
          _numberOfUnits = value;
          PropertyHasChanged("NumberOfUnits");
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

    [Column(Storage = "_serialNumbers", DbType = "NVarChar(255) NULL", CanBeNull = true)]
    public string SerialNumbers
    {
      get { return _serialNumbers; }
      set
      {
        if (_serialNumbers != value)
        {
          PropertyIsChanging("SerialNumbers");
          _serialNumbers = value;
          PropertyHasChanged("SerialNumbers");
        }
      }
    }

    [Column(Storage = "_delivered", DbType = "INT NOT NULL", CanBeNull = false)]
    public int Delivered
    {
      get { return _delivered; }
      set
      {
        if (_delivered != value)
        {
          PropertyIsChanging("Delivered");
          _delivered = value;
          PropertyHasChanged("Delivered");
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

    [Column(Storage = "_productName", DbType = "NVarChar(255) NULL", CanBeNull = true)]
    public string ProductName
    {
      get { return _productName; }
      set
      {
        if (_productName != value)
        {
          PropertyIsChanging("ProductName");
          _productName = value;
          PropertyHasChanged("ProductName");
        }
      }
    }

    [Column(Storage = "_html", DbType = "text", CanBeNull = true)]
    public string Html
    {
      get { return _html; }
      set
      {
        if (_html != value)
        {
          PropertyIsChanging("Html");
          _html = value;
          PropertyHasChanged("Html");
        }
      }
    }

    [Association(Name = "FK_EdiOrderResponseLine_EdiOrderResponse", Storage = "_ediOrderResponse", OtherKey = "EdiOrderResponseID", ThisKey = "EdiOrderResponseID", IsForeignKey = true)]
    public EDIOrderResponse EdiOrderResponse
    {
      get { return _ediOrderResponse.Entity; }
      set
      {
        EDIOrderResponse previousValue = _ediOrderResponse.Entity;
        if (((previousValue != value)
             || (_ediOrderResponse.HasLoadedOrAssignedValue == false)))
        {
          PropertyIsChanging("EdiOrderResponse");
          if ((previousValue != null))
          {
            _ediOrderResponse.Entity = null;
            previousValue.EdiOrderResponseLines.Remove(this);
          }
          _ediOrderResponse.Entity = value;
          if ((value != null))
          {
            value.EdiOrderResponseLines.Add(this);
            _ediOrderResponseID = value.EdiOrderResponseID;
          }
          else
          {
            _ediOrderResponseID = default(int);
          }
          PropertyHasChanged("EdiOrderResponse");
        }
      }
    }

     [Association(Name = "FK_EdiOrderResponseLine_EdiOrderLine", Storage = "_ediOrderLine", OtherKey = "EdiOrderLineID", ThisKey = "EdiOrderLineID", IsForeignKey = true)]
    public EDIOrderLine EdiOrderLine
    {
      get { return _ediOrderLine.Entity; }
      set
      {
        EDIOrderLine previousValue = _ediOrderLine.Entity;
        if (((previousValue != value)
             || (_ediOrderLine.HasLoadedOrAssignedValue == false)))
        {
          PropertyIsChanging("EDIOrderLine");
          if ((previousValue != null))
          {
            _ediOrderLine.Entity = null;
            previousValue.EdiOrderResponseLines.Remove(this);
          }
          _ediOrderLine.Entity = value;
          if ((value != null))
          {
            value.EdiOrderResponseLines.Add(this);
            _ediOrderLineID = value.EdiOrderLineID;
          }
          else
          {
            _ediOrderLineID = default(int);
          }
          PropertyHasChanged("EDIOrderLine");
        }
      }
    }

    public List<ReturnError> ResponseErrors { get; set; }
  }

  public class ReturnError
  {
    public string ErrorCode { get; set; }
    public bool SkipOrder { get; set; }
    public string ErrorMessage { get; set; }
  }
}
