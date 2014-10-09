using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using Concentrator.Objects.Base;
using System.Data.Linq.Mapping;
using Concentrator.Objects.Product;
using System.Data.Linq;

namespace Concentrator.Objects.EDI.Post
{
  [Table(Name = "dbo.EdiOrderPost")]
  public class EdiOrdePost : ObjectBase<EdiOrdePost>
  {
    private int _ediOrderPostID;
    private int _ediOrderID;
    private int _customerID;
    private int? _ediBackendOrderID;
    private string _customerOrderID;
    private bool _processed;
    private string _type;
    private string _postDocument; // Is XMl
    private string _postDocumentUrl;
    private string _postUrl;
    private DateTime _timeStamp;
    private string _responseRemark;
    private int? _reponseTime; //Change to DateTime?
    private int? _processedCount;
    private int _ediRequestID;
    private string _errorMessage;
    private int _bSKIdentifier;
    private int _documentCounter;
    private int _connectorID;

    public EdiOrdePost()
    {
    }

    [Column(Storage = "_ediOrderPostID", DbType = "Int NOT NULL", IsPrimaryKey = true)]
    public int EdiOrderPostID
    {
      get { return _ediOrderPostID; }
      set
      {
        if (_ediOrderPostID != value)
        {
          PropertyIsChanging("EdiOrderPostID");
          _ediOrderPostID = value;
          PropertyHasChanged("EdiOrderPostID");
        }
      }
    }

    [Column(Storage = "_ediOrderID", DbType = "Int NOT NULL")]
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

    [Column(Storage = "_customerID", DbType = "Int NOT NULL")]
    public int CustomerID
    {
      get { return _customerID; }
      set
      {
        if (_customerID != value)
        {
          PropertyIsChanging("CustomerID");
          _customerID = value;
          PropertyHasChanged("CustomerID");
        }
      }
    }

    [Column(Storage = "_ediBackendOrderID", DbType = "Int NULL")]
    public int? EdiBackendOrderID
    {
      get { return _ediBackendOrderID; }
      set
      {
        if (_ediBackendOrderID != value)
        {
          PropertyIsChanging("EdiBackendOrderID");
          _ediBackendOrderID = value;
          PropertyHasChanged("EdiBackendOrderID");
        }
      }
    }

    [Column(Storage = "_customerOrderID", DbType = "nvarchar(50)", CanBeNull = true)]
    public string CustomerOrderID
    {
      get { return _customerOrderID; }
      set
      {
        if (_customerOrderID != value)
        {
          PropertyIsChanging("CustomerOrderID");
          _customerOrderID = value;
          PropertyHasChanged("CustomerOrderID");
        }
      }
    }

    [Column(Storage = "_processed", DbType = "bit not null")]
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

    [Column(Storage = "_type", DbType = "nvarchar(50) Not NULL")]
    public string Type
    {
      get { return _type; }
      set
      {
        if (_type != value)
        {
          PropertyIsChanging("Type");
          _type = value;
          PropertyHasChanged("Type");
        }
      }
    }
    

    //TODO: CHANGE TO XML
    [Column(Storage = "_postDocument", DbType = "nvarchar(50) Not NULL")]
    public string PostDocument
    {
      get { return _postDocument; }
      set
      {
        if (_postDocument != value)
        {
          PropertyIsChanging("PostDocument");
          _postDocument = value;
          PropertyHasChanged("PostDocument");
        }
      }
    }

    [Column(Storage = "_postDocumentUrl", DbType = "nvarchar(Max) Not NULL")]
    public string PostDocumentUrl
    {
      get { return _postDocumentUrl; }
      set
      {
        if (_postDocumentUrl != value)
        {
          PropertyIsChanging("PostDocumentUrl");
          _postDocumentUrl = value;
          PropertyHasChanged("PostDocumentUrl");
        }
      }
    }

    [Column(Storage = "_postUrl", DbType = "nvarchar(255) Not NULL")]
    public string PostUrl
    {
      get { return _postUrl; }
      set
      {
        if (_postUrl != value)
        {
          PropertyIsChanging("PostUrl");
          _postUrl = value;
          PropertyHasChanged("PostUrl");
        }
      }
    }

    [Column(Storage = "_timeStamp", DbType = "datetime Not NULL")]
    public DateTime TimeStamp
    {
      get { return _timeStamp; }
      set
      {
        if (_timeStamp != value)
        {
          PropertyIsChanging("TimeStamp");
          _timeStamp = value;
          PropertyHasChanged("TimeStamp");
        }
      }
    }

    [Column(Storage = "_responseRemark", DbType = "nvarchar(50) NULL")]
    public string ResponseRemark
    {
      get { return _responseRemark; }
      set
      {
        if (_responseRemark != value)
        {
          PropertyIsChanging("ResponseRemark");
          _responseRemark = value;
          PropertyHasChanged("ResponseRemark");
        }
      }
    }

    [Column(Storage = "_reponseTime", DbType = "int NULL")]
    public int? ResponseTime
    {
      get { return _reponseTime; }
      set
      {
        if (_reponseTime != value)
        {
          PropertyIsChanging("ResponseTime");
          _reponseTime = value;
          PropertyHasChanged("ResponseTime");
        }
      }
    }

    [Column(Storage = "_processedCount", DbType = "int NULL")]
    public int? ProcessedCount
    {
      get { return _processedCount; }
      set
      {
        if (_processedCount != value)
        {
          PropertyIsChanging("ProcessedCount");
          _processedCount = value;
          PropertyHasChanged("ProcessedCount");
        }
      }
    }

    [Column(Storage = "_ediRequestID", DbType = "INT NOT NULL")]
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

    [Column(Storage = "_errorMessage", DbType = "nvarchar(Max) NULL")]
    public string ErrorMessage
    {
      get { return _errorMessage; }
      set
      {
        if (_errorMessage != value)
        {
          PropertyIsChanging("ErrorMessage");
          _errorMessage = value;
          PropertyHasChanged("ErrorMessage");
        }
      }
    }

    [Column(Storage = "_bSKIdentifier", DbType = "INT NOT NULL")]
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

    [Column(Storage = "_documentCounter", DbType = "INT NOT NULL")]
    public int DocumentCounter
    {
      get { return _documentCounter; }
      set
      {
        if (_documentCounter != value)
        {
          PropertyIsChanging("DocumentCounter");
          _documentCounter = value;
          PropertyHasChanged("DocumentCounter");
        }
      }
    }

    [Column(Storage = "_connectorID", DbType = "Int not NULL")]
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

  }
}
