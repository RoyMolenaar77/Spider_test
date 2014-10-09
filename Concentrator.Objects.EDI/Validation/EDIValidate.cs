using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.EDI.Vendor;
using Concentrator.Objects.EDI.EDI.Communication;

namespace Concentrator.Objects.EDI.Validation
{
  public class EdiValidate
    private string _connection;
    private EDIConnectionType _ediConnectionType;
  {
    public Int32 EdiValidateID { get; set; }

    public string TableName { get; set; }

    public string FieldName { get; set; }

    public Int32 EdiVendorID { get; set; }

    public Int32 MaxLength { get; set; }

    public string Type { get; set; }

    public string Value { get; set; }

    public bool IsActive { get; set; }

    public Int32 EdiType { get; set; }

    public Int32 EDIValidationType { get; set; }

    public virtual EdiVendor EdiVendors { get; set; }
        }
      }
    }

    [Column(Storage = "_connection", DbType = "NVarChar(255) NOT NULL")]
    public string Connection
    {
      get { return _connection; }
      set
      {
        if (_connection != value)
        {
          PropertyIsChanging("Connection");
          _connection = value;
          PropertyHasChanged("Connection");
        }
      }
    }

    [Column(Storage = "_ediConnectionType", DbType = "INT NOT NULL")]
    public EDIConnectionType EdiConnectionType
    {
      get { return _ediConnectionType; }
      set
      {
        if (_ediConnectionType != value)
        {
          PropertyIsChanging("EdiConnectionType");
          _ediConnectionType = value;
          PropertyHasChanged("EdiConnectionType");
  }
}
