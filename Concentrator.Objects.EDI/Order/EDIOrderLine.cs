using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using Concentrator.Objects.Base;
using System.Data.Linq.Mapping;
using System.Data.Linq;
using Concentrator.Objects.EDI.Response;

namespace Concentrator.Objects.EDI.Order
{
  [Table(Name = "dbo.EdiOrderLine")]
  public class EDIOrderLine : ObjectBase<EDIOrderLine>
  {    
    private int _ediOrderLineID;
    private int _quantity;
    private string _customerEdiOrderLineNr;
    private string _customerOrderNr;
    private int _ediOrderID;
    private bool _isDispatched;
    private decimal? _price;
    private string _remarks;
    private int? _vendorOrderNumber;
    private int? _productID;
    private bool? _centralDelivery;
    private string _customerItemNumber;
    private string _wareHouseCode;
    private bool _priceOverride;
    private string _endCustomerOrderNr;
    private string _productDescription;
    private string _currency;
    private string _unitOfMeasure;
    private string _response;

    private EntityRef<EDIOrder> _ediOrder;
    private EntitySet<EDIOrderLedger> _ediOrderLedger;
    private EntitySet<EDIOrderResponseLine> _ediOrderResponseLines;

    public EDIOrderLine()
    {
      _ediOrderLedger = new EntitySet<EDIOrderLedger>(x => x.EdiOrderLine = this, x => x.EdiOrderLine = null);
      _ediOrder = default(EntityRef<EDIOrder>);
      _ediOrderResponseLines = new EntitySet<EDIOrderResponseLine>(x => x.EdiOrderLine = this, x => x.EdiOrderLine = null);
    }

    [Column(Storage = "_ediOrderLineID", IsDbGenerated=true, DbType = "INT NOT NULL", IsPrimaryKey = true)]
    public int EdiOrderLineID
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

    [Column(Storage = "_remarks", DbType = "NVARCHAR(MAX)")]
    public string Remarks
    {
      get { return _remarks; }
      set
      {
        if (_remarks != value)
        {
          PropertyIsChanging("Remarks");
          _remarks = value;
          PropertyHasChanged("Remarks");
        }
      }
    }

    [Column(Storage = "_ediOrderID", DbType = "Int NOT NULL")]
    public int EdiOrderID
    {
      get { return _ediOrderID; }
      set
      {
        if ((_ediOrderID != value))
        {
          PropertyIsChanging("EdiOrderID");
          _ediOrderID = value;
          PropertyHasChanged("EdiOrderID");
        }
      }
    }

    [Column(Storage = "_customerEdiOrderLineNr", DbType = "NVarChar(100)")]
    public string CustomerEdiOrderLineNr
    {
      get { return _customerEdiOrderLineNr; }
      set
      {
        if (_customerEdiOrderLineNr != value)
        {
          PropertyIsChanging("CustomerEdiOrderLineNr");
          _customerEdiOrderLineNr = value;
          PropertyHasChanged("CustomerEdiOrderLineNr");
        }
      }
    }

    [Column(Storage = "_customerOrderNr", DbType = "NVarChar(100)")]
    public string CustomerOrderNr
    {
      get { return _customerOrderNr; }
      set
      {
        if (_customerOrderNr != value)
        {
          PropertyIsChanging("CustomerOrderNr");
          _customerOrderNr = value;
          PropertyHasChanged("CustomerOrderNr");
        }
      }
    }

    [Column(Storage = "_productID", DbType = "Int NOT NULL")]
    public int? ProductID
    {
      get { return _productID; }
      set
      {
        if ((_productID != value))
        {
          PropertyIsChanging("ProductID");
          _productID = value;
          PropertyHasChanged("ProductID");
        }
      }
    }

    [Column(Storage = "_price", DbType = "float null")]
    public decimal? Price
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

    [Column(Storage = "_quantity", DbType = "int")]
    public int Quantity
    {
      get { return _quantity; }
      set
      {
        if (_quantity != value)
        {
          PropertyIsChanging("Quantity");
          _quantity = value;
          PropertyHasChanged("Quantity");
        }
      }
    }


    [Column(Storage = "_isDispatched", DbType = "BIT")]
    public bool isDispatched
    {
      get { return _isDispatched; }
      set
      {
        if (_isDispatched != value)
        {
          PropertyIsChanging("isDispatched");
          _isDispatched = value;
          PropertyHasChanged("isDispatched");
        }
      }
    }

    [Column(Storage = "_vendorOrderNumber", DbType = "Int")]
    public int? VendorOrderNumber
    {
      get { return _vendorOrderNumber; }
      set
      {
        if (_vendorOrderNumber != value)
        {
          PropertyIsChanging("VendorOrderNumber");
          _vendorOrderNumber = value;
          PropertyHasChanged("VendorOrderNumber");
        }
      }
    }

    [Column(Storage = "_centralDelivery", DbType = "bit NULL", CanBeNull = true)]
    public bool? CentralDelivery
    {
      get { return _centralDelivery; }
      set
      {
        if (_centralDelivery != value)
        {
          PropertyIsChanging("CentralDelivery");
          _centralDelivery = value;
          PropertyHasChanged("CentralDelivery");
        }
      }
    }

    [Column(Storage = "_customerItemNumber", DbType = "NVarChar(100)")]
    public string CustomerItemNumber
    {
      get { return _customerItemNumber; }
      set
      {
        if (_customerItemNumber != value)
        {
          PropertyIsChanging("CustomerItemNumber");
          _customerItemNumber = value;
          PropertyHasChanged("CustomerItemNumber");
        }
      }
    }

    [Column(Storage = "_response", DbType = "nvarchar(MAX)")]
    public string Response
    {
      get { return _response; }
      set
      {
        if (_response != value)
        {
          PropertyIsChanging("Response");
          _response = value;
          PropertyHasChanged("Response");
        }
      }
    }

    [Column(Storage = "_wareHouseCode", DbType = "NVarChar(50) NULL", CanBeNull = true)]
    public string WareHouseCode
    {
      get { return _wareHouseCode; }
      set
      {
        if (_wareHouseCode != value)
        {
          PropertyIsChanging("WareHouseCode");
          _wareHouseCode = value;
          PropertyHasChanged("WareHouseCode");
        }
      }
    }

    [Column(Storage = "_priceOverride", DbType = "BIT NOT NULL", CanBeNull = false)]
    public bool PriceOverride
    {
      get { return _priceOverride; }
      set
      {
        if (_priceOverride != value)
        {
          PropertyIsChanging("PriceOverride");
          _priceOverride = value;
          PropertyHasChanged("PriceOverride");
        }
      }
    }

    [Column(Storage = "_endCustomerOrderNr", DbType = "NVarChar(100) NULL", CanBeNull = true)]
    public string EndCustomerOrderNr
    {
      get { return _endCustomerOrderNr; }
      set
      {
        if (_endCustomerOrderNr != value)
        {
          PropertyIsChanging("EndCustomerOrderNr");
          _endCustomerOrderNr = value;
          PropertyHasChanged("EndCustomerOrderNr");
        }
      }
    }

    [Column(Storage = "_currency", DbType = "NVarChar(10) NULL", CanBeNull = true)]
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

    [Column(Storage = "_unitOfMeasure", DbType = "NVarChar(10) NULL", CanBeNull = true)]
    public string UnitOfMeasure
    {
      get { return _unitOfMeasure; }
      set
      {
        if (_unitOfMeasure != value)
        {
          PropertyIsChanging("UnitOfMeasure");
          _unitOfMeasure = value;
          PropertyHasChanged("UnitOfMeasure");
        }
      }
    }

    [Column(Storage = "_productDescription", DbType = "NVarChar(255) NULL", CanBeNull = true)]
    public string ProductDescription
    {
      get { return _productDescription; }
      set
      {
        if (_productDescription != value)
        {
          PropertyIsChanging("ProductDescription");
          _productDescription = value;
          PropertyHasChanged("ProductDescription");
        }
      }
    }

      [Association(Name = "FK_EdiOrderResponseLine_EdiOrderLine", Storage = "_ediOrderResponseLines", ThisKey = "EdiOrderLineID", OtherKey = "EdiOrderLineID", DeleteRule = "NO ACTION")]
    public EntitySet<EDIOrderResponseLine> EdiOrderResponseLines
    {
      get { return _ediOrderResponseLines; }
      set { _ediOrderResponseLines.Assign(value); }
    }

    [Association(Name = "FK_EdiOrderLedger_EdiOrderLine", Storage = "_ediOrderLedger", ThisKey = "EdiOrderLineID", OtherKey = "EdiOrderLineID", DeleteRule = "NO ACTION")]
    public EntitySet<EDIOrderLedger> EdiOrderLedger
    {
      get { return _ediOrderLedger; }
      set { _ediOrderLedger.Assign(value); }
    }

    [Association(Name = "FK_EdiOrderLine_EdiOrder", Storage = "_ediOrder", OtherKey = "EdiOrderID", ThisKey = "EdiOrderID", IsForeignKey = true)]
    public EDIOrder EdiOrder
    {
      get { return _ediOrder.Entity; }
      set
      {
        EDIOrder previousValue = _ediOrder.Entity;
        if (((previousValue != value)
             || (_ediOrder.HasLoadedOrAssignedValue == false)))
        {
          PropertyIsChanging("EdiOrder");
          if ((previousValue != null))
          {
            _ediOrder.Entity = null;
            previousValue.EdiOrderLines.Remove(this);
          }
          _ediOrder.Entity = value;
          if ((value != null))
          {
            value.EdiOrderLines.Add(this);
            _ediOrderID = value.EdiOrderID;
          }
          else
          {
            _ediOrderID = default(int);
          }
          PropertyHasChanged("EdiOrder");
        }
      }
    }
    
  }
}
