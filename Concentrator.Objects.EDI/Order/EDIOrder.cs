using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using Concentrator.Objects.Base;
using System.Data.Linq.Mapping;
using Concentrator.Objects.Product;
using System.Data.Linq;
using Concentrator.Objects.EDI.Base;
using Concentrator.Objects.Orders;
using Concentrator.Objects.EDI.Response;

namespace Concentrator.Objects.EDI.Order
{
  [Table(Name = "dbo.EdiOrder")]
  public class EDIOrder : ObjectBase<EDIOrder>
  {
    private int _ediOrderID;
    private int _ediRequestID;
    private string _document;
    private int _connectorID;
    private bool _isDispatched;
    private DateTime? _dispatchToVendorDate;
    private DateTime _receivedDate;
    private bool _isDropShipment;
    private string _remarks;
    private int? _shipToCustomerID;
    private int? _soldToCustomerID;
    private string _customerOrderReference;
    private string _ediVersion;
    private int _bSKIdentifier;
    private string _webSiteOrderNumber;
    private string _paymentTermsCode;
    private string _paymentInstrument;
    private bool? _backOrdersAllowed;
    private string _routeCode;
    private string _holdCode;
    private bool _holdOrder;
    private int _status;
    private int _ediOrderTypeID;
    private bool? _partialDelivery;
    private int? _connectorRelationID;

    //private EntityRef<Connector> _connector;
    private EntitySet<EDIOrderLine> _ediOrderLines;
    private EntityRef<EDIOrderListener> _ediOrderListener;
    private EntityRef<EDIOrderType> _ediOrderType;
    private EntityRef<EDICustomer> _shipToCustomer;
    private EntityRef<EDICustomer> _soldToCustomer;
    private EntitySet<EDIOrderResponse> _ediOrderResponses;
    private EntityRef<ConnectorRelation> _connectorRelation;


    public EDIOrder()
    {
      //_connector = default(EntityRef<Connector>);
      _ediOrderLines = new EntitySet<EDIOrderLine>(x => x.EdiOrder = this, x => x.EdiOrder = null);
      _ediOrderResponses = new EntitySet<EDIOrderResponse>(x => x.EDIOrder = this, x => x.EDIOrder = null);
      _ediOrderListener = default(EntityRef<EDIOrderListener>);
      _ediOrderType = default(EntityRef<EDIOrderType>);
      _connectorRelation = default(EntityRef<ConnectorRelation>);

      // TEST
      _shipToCustomer = default(EntityRef<EDICustomer>);
      _soldToCustomer = default(EntityRef<EDICustomer>);
    }

    [Column(Storage = "_ediOrderID", DbType = "Int NOT NULL", IsDbGenerated = true, IsPrimaryKey = true)]
    public int EdiOrderID
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

    [Column(Storage = "_ediRequestID", DbType = "INT not null", CanBeNull = false)]
    public int EdiRequestID
    {
      get { return _ediRequestID; }
      set
      {
        if (_ediRequestID != value)
        {
          PropertyIsChanging("EdiRequestID");
          _ediRequestID = value;
          PropertyHasChanged("EdiRequestID");
        }
      }
    }

    [Column(Storage = "_document", DbType = "NVarChar(MAX) null", CanBeNull = true)]
    public string Document
    {
      get { return _document; }
      set
      {
        if (_document != value)
        {
          PropertyIsChanging("Document");
          _document = value;
          PropertyHasChanged("Document");
        }
      }
    }

    [Column(Storage = "_status", DbType = "int Null")]
    public int Status
    {
      get { return _status; }
      set
      {
        if (_status != value)
        {
          PropertyIsChanging("Status");
          _status = value;
          PropertyHasChanged("Status");
        }
      }
    }

    [Column(Storage = "_connectorID", DbType = "INT NOT NULL")]
    public int ConnectorID
    {
      get { return _connectorID; }
      set
      {
        if (_connectorID != value)
        {
          PropertyIsChanging("ConnectorID");
          _connectorID = value;
          PropertyHasChanged("ConnectorID");
        }
      }
    }

    [Column(Storage = "_isDispatched", DbType = "BIT NOT NULL")]
    public bool IsDispatched
    {
      get { return _isDispatched; }
      set
      {
        if (_isDispatched != value)
        {
          PropertyIsChanging("IsDispatched");
          _isDispatched = value;
          PropertyHasChanged("IsDispatched");
        }
      }
    }

    [Column(Storage = "_dispatchToVendorDate", DbType = "DateTime Null")]
    public DateTime? DispatchToVendorDate
    {
      get { return _dispatchToVendorDate; }
      set
      {
        if (_dispatchToVendorDate != value)
        {
          PropertyIsChanging("DispatchToVendorDate");
          _dispatchToVendorDate = value;
          PropertyHasChanged("DispatchToVendorDate");
        }
      }
    }

    [Column(Storage = "_receivedDate", DbType = "DateTime")]
    public DateTime ReceivedDate
    {
      get { return _receivedDate; }
      set
      {
        if (_receivedDate != value)
        {
          PropertyIsChanging("ReceivedDate");
          _receivedDate = value;
          PropertyHasChanged("ReceivedDate");
        }
      }
    }

    [Column(Storage = "_isDropShipment", DbType = "Bit")]
    public bool IsDropShipment
    {
      get { return _isDropShipment; }
      set
      {
        if (_isDropShipment != value)
        {
          PropertyIsChanging("IsDropShipment");
          _isDropShipment = value;
          PropertyHasChanged("IsDropShipment");
        }
      }
    }

    [Column(Storage = "_remarks", DbType = "NvarChar(MAX)")]
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

    [Column(Storage = "_shipToCustomerID", DbType = "Int NULL")]
    public int? ShipToCustomerID
    {
      get
      {
        return _shipToCustomerID;
      }
      set
      {
        if ((_shipToCustomerID != value))
        {
          PropertyIsChanging("ShipToCustomerID");
          _shipToCustomerID = value;
          PropertyHasChanged("ShipToCustomerID");
        }
      }
    }

    [Column(Storage = "_soldToCustomerID", DbType = "Int NULL")]
    public int? SoldToCustomerID
    {
      get
      {
        return _soldToCustomerID;
      }
      set
      {
        if ((_soldToCustomerID != value))
        {
          PropertyIsChanging("SoldToCustomerID");
          _soldToCustomerID = value;
          PropertyHasChanged("SoldToCustomerID");
        }
      }
    }

    [Column(Storage = "_customerOrderReference", DbType = "NVarChar(MAX)")]
    public string CustomerOrderReference
    {
      get { return _customerOrderReference; }
      set
      {
        if (_customerOrderReference != value)
        {
          PropertyIsChanging("CustomerOrderReference");
          _customerOrderReference = value;
          PropertyHasChanged("CustomerOrderReference");
        }
      }
    }

    [Column(Storage = "_ediVersion", DbType = "NVarChar(50)")]
    public string EdiVersion
    {
      get { return _ediVersion; }
      set
      {
        if (_ediVersion != value)
        {
          PropertyIsChanging("EdiVersion");
          _ediVersion = value;
          PropertyHasChanged("EdiVersion");
        }
      }
    }

    [Column(Storage = "_bSKIdentifier", DbType = "Int NULL")]
    public int BSKIdentifier
    {
      get { return _bSKIdentifier; }
      set
      {
        if (_bSKIdentifier != value)
        {
          PropertyIsChanging("BSKIdentifier");
          _bSKIdentifier = value;
          PropertyHasChanged("BSKIdentifier");
        }
      }
    }

    [Column(Storage = "_webSiteOrderNumber", DbType = "NVarChar(100)")]
    public string WebSiteOrderNumber
    {
      get { return _webSiteOrderNumber; }
      set
      {
        if (_webSiteOrderNumber != value)
        {
          PropertyIsChanging("WebSiteOrderNumber");
          _webSiteOrderNumber = value;
          PropertyHasChanged("WebSiteOrderNumber");
        }
      }
    }

    [Column(Storage = "_paymentTermsCode", DbType = "NVarChar(50)")]
    public string PaymentTermsCode
    {
      get { return _paymentTermsCode; }
      set
      {
        if (_paymentTermsCode != value)
        {
          PropertyIsChanging("PaymentTermsCode");
          _paymentTermsCode = value;
          PropertyHasChanged("PaymentTermsCode");
        }
      }
    }

    [Column(Storage = "_paymentInstrument", DbType = "NVarChar(50)")]
    public string PaymentInstrument
    {
      get { return _paymentInstrument; }
      set
      {
        if (_paymentInstrument != value)
        {
          PropertyIsChanging("PaymentInstrument");
          _paymentInstrument = value;
          PropertyHasChanged("PaymentInstrument");
        }
      }
    }

    [Column(Storage = "_backOrdersAllowed", DbType = "BIT NULl")]
    public bool? BackOrdersAllowed
    {
      get { return _backOrdersAllowed; }
      set
      {
        if (_backOrdersAllowed != value)
        {
          PropertyIsChanging("BackOrdersAllowed");
          _backOrdersAllowed = value;
          PropertyHasChanged("BackOrdersAllowed");
        }
      }
    }

    [Column(Storage = "_routeCode", DbType = "NVarChar(50)")]
    public string RouteCode
    {
      get { return _routeCode; }
      set
      {
        if (_routeCode != value)
        {
          PropertyIsChanging("RouteCode");
          _routeCode = value;
          PropertyHasChanged("RouteCode");
        }
      }
    }

    [Column(Storage = "_holdCode", DbType = "NVarChar(50)")]
    public string HoldCode
    {
      get { return _holdCode; }
      set
      {
        if (_holdCode != value)
        {
          PropertyIsChanging("HoldCode");
          _holdCode = value;
          PropertyHasChanged("HoldCode");
        }
      }
    }

    [Column(Storage = "_holdOrder", DbType = "Bit")]
    public bool HoldOrder
    {
      get { return _holdOrder; }
      set
      {
        if (_holdOrder != value)
        {
          PropertyIsChanging("HoldOrder");
          _holdOrder = value;
          PropertyHasChanged("HoldOrder");
        }
      }
    }

    [Column(Storage = "_ediOrderTypeID", DbType = "Int Not Null", CanBeNull = false)]
    public int EdiOrderTypeID
    {
      get { return _ediOrderTypeID; }
      set
      {
        if (_ediOrderTypeID != value)
        {
          PropertyIsChanging("EdiOrderTypeID");
          _ediOrderTypeID = value;
          PropertyHasChanged("EdiOrderTypeID");
        }
      }
    }

    [Column(Storage = "_partialDelivery", DbType = "Bit Null", CanBeNull = true)]
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

    [Column(Storage = "_connectorRelationID", DbType = "Int NULL")]
    public int? ConnectorRelationID
    {
      get { return _connectorRelationID; }
      set
      {
        if (_connectorRelationID != value)
        {
          PropertyIsChanging("ConnectorRelationID");
          _connectorRelationID = value;
          PropertyHasChanged("ConnectorRelationID");
        }
      }
    }

    //[Association(Name = "FK_EdiOrder_Connector", Storage = "_connector", OtherKey = "ConnectorID", ThisKey = "ConnectorID", IsForeignKey = true)]
    //public Connector Connector
    //{
    //  get { return _connector.Entity; }
    //  set
    //  {
    //    Connector previousValue = _connector.Entity;
    //    if (((previousValue != value)
    //         || (_connector.HasLoadedOrAssignedValue == false)))
    //    {
    //      PropertyIsChanging("Connector");
    //      if ((previousValue != null))
    //      {
    //        _connector.Entity = null;
    //        previousValue.EDIOrders.Remove(this);
    //      }
    //      _connector.Entity = value;
    //      if ((value != null))
    //      {
    //        value.EDIOrders.Add(this);
    //        _connectorID = value.ConnectorID;
    //      }
    //      else
    //      {
    //        _connectorID = default(int);
    //      }
    //      PropertyHasChanged("Connector");
    //    }
    //  }
    //}

    [Association(Name = "FK_EdiOrderResponse_EdiOrder", Storage = "_ediOrderResponses", ThisKey = "EdiOrderID", OtherKey = "EdiOrderID", DeleteRule = "NO ACTION")]
    public EntitySet<EDIOrderResponse> EDIOrderResponses
    {
      get { return _ediOrderResponses; }
      set { _ediOrderResponses.Assign(value); }
    }

    [Association(Name = "FK_EdiOrder_EdiOrderLine", Storage = "_ediOrderLines", ThisKey = "EdiOrderID", OtherKey = "EdiOrderID", DeleteRule = "NO ACTION")]
    public EntitySet<EDIOrderLine> EdiOrderLines
    {
      get { return _ediOrderLines; }
      set { _ediOrderLines.Assign(value); }
    }

    [Association(Name = "FK_EdiOrder_EdiOrderListener", Storage = "_ediOrderListener", OtherKey = "EdiRequestID", ThisKey = "EdiRequestID", IsForeignKey = true)]
    public EDIOrderListener EDIOrderListener
    {
      get { return _ediOrderListener.Entity; }
      set
      {
        this._ediOrderListener.Entity = value;
      }
    }

    [Association(Name = "FK_EdiOrder_EdiOrderType", Storage = "_ediOrderType", OtherKey = "EdiOrderTypeID", ThisKey = "EdiOrderTypeID", IsForeignKey = true)]
    public EDIOrderType EDIOrderType
    {
      get { return _ediOrderType.Entity; }
      set
      {
        EDIOrderType previousValue = _ediOrderType.Entity;
        if (((previousValue != value)
             || (_ediOrderType.HasLoadedOrAssignedValue == false)))
        {
          PropertyIsChanging("EDIOrderType");
          if ((previousValue != null))
          {
            _ediOrderType.Entity = null;
            previousValue.EDIOrders.Remove(this);
          }
          _ediOrderType.Entity = value;
          if ((value != null))
          {
            value.EDIOrders.Add(this);
            _ediOrderTypeID = value.EdiOrderTypeID;
          }
          else
          {
            _ediOrderTypeID = default(int);
          }
          PropertyHasChanged("EDIOrderType");
        }
      }
    }

       [Association(Name = "FK_EdiOrder_ConnectorRelation", Storage = "_connectorRelation", OtherKey = "ConnectorRelationID", ThisKey = "ConnectorRelationID", IsForeignKey = true)]
    public ConnectorRelation ConnectorRelation
    {
      get { return _connectorRelation.Entity; }
      set
      {
        ConnectorRelation previousValue = _connectorRelation.Entity;
        if (((previousValue != value)
             || (_connectorRelation.HasLoadedOrAssignedValue == false)))
        {
          PropertyIsChanging("ConnectorRelation");
          if ((previousValue != null))
          {
            _connectorRelation.Entity = null;
            previousValue.EDIOrders.Remove(this);
          }
          _connectorRelation.Entity = value;
          if ((value != null))
          {
            value.EDIOrders.Add(this);
            _connectorRelationID = value.ConnectorRelationID;
          }
          else
          {
            _connectorRelationID = default(int);
          }
          PropertyHasChanged("ConnectorRelation");
        }
      }
    }
    //[Association(Name = "FK_EdiOrder_Customer", Storage = "_shipToCustomer", OtherKey = "CustomerID", ThisKey = "ShipToCustomerID", IsForeignKey = true)]
    //public Customer ShipToCustomer
    //{
    //  get { return _shipToCustomer.Entity; }
    //  set
    //  {
    //    Customer previousValue = _shipToCustomer.Entity;
    //    if (((previousValue != value)
    //         || (_shipToCustomer.HasLoadedOrAssignedValue == false)))
    //    {
    //      PropertyIsChanging("ShipToCustomer");
    //      if ((previousValue != null))
    //      {
    //        _shipToCustomer.Entity = null;
    //        previousValue.EDIOrdersShippedTo.Remove(this);
    //      }
    //      _shipToCustomer.Entity = value;
    //      if ((value != null))
    //      {
    //        value.EDIOrdersShippedTo.Add(this);
    //        _shipToCustomerID = value.CustomerID;
    //      }
    //      else
    //      {
    //        _shipToCustomerID = default(int);
    //      }
    //      PropertyHasChanged("ShipToCustomer");
    //    }
    //  }
    //}


    [Association(Name = "FK_EdiOrder_Customer", Storage = "_shipToCustomer", OtherKey = "CustomerID", ThisKey = "ShipToCustomerID", IsForeignKey = true)]
    public EDICustomer ShipToCustomer
    {
      get { return _shipToCustomer.Entity; }
      set
      {
        EDICustomer previousValue = _shipToCustomer.Entity;
        if (((previousValue != value)
             || (_shipToCustomer.HasLoadedOrAssignedValue == false)))
        {
          PropertyIsChanging("ShipToCustomer");
          if ((previousValue != null))
          {
            _shipToCustomer.Entity = null;
            previousValue.EDIOrdersShippedTo.Remove(this);
          }
          _shipToCustomer.Entity = value;
          if ((value != null))
          {
            value.EDIOrdersShippedTo.Add(this);
            _shipToCustomerID = value.CustomerID;
          }
          else
          {
            _shipToCustomerID = default(int);
          }
          PropertyHasChanged("ShipToCustomer");
        }
      }
    }

    [Association(Name = "FK_EdiOrder_SoldToCustomer", Storage = "_soldToCustomer", OtherKey = "CustomerID", ThisKey = "SoldToCustomerID", IsForeignKey = true)]
    public EDICustomer SoldToCustomer
    {
      get { return _soldToCustomer.Entity; }
      set
      {
        EDICustomer previousValue = _soldToCustomer.Entity;
        if (((previousValue != value)
             || (_soldToCustomer.HasLoadedOrAssignedValue == false)))
        {
          PropertyIsChanging("SoldToCustomer");
          if ((previousValue != null))
          {
            _soldToCustomer.Entity = null;
            previousValue.EDIOrdersSoldTo.Remove(this);
          }
          _soldToCustomer.Entity = value;
          if ((value != null))
          {
            value.EDIOrdersSoldTo.Add(this);
            _soldToCustomerID = value.CustomerID;
          }
          else
          {
            _soldToCustomerID = default(int);
          }
          PropertyHasChanged("SoldToCustomer");
        }
      }
    }

  }
}
