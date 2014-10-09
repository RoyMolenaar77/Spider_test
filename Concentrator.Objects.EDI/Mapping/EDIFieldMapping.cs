using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.EDI.Order;

namespace Concentrator.Objects.EDI.FieldMapping
{
  public class EdiFieldMapping
  {
    public Int32 EdiMappingID { get; set; }

    public string TableName { get; set; }

    public string FieldName { get; set; }

    public Int32 EdiVendorID { get; set; }

    public string VendorFieldName { get; set; }

    public string VendorTableName { get; set; }

     [Column(Storage = "_vendorFieldType", DbType = "Int NULL")]
    public TypeCode VendorFieldType
    {
      get { return _vendorFieldType; }
      set
      {
        if (_vendorFieldType != value)
        {
          PropertyIsChanging("VendorFieldType");
          _vendorFieldType = value;
          PropertyHasChanged("VendorFieldType");
        }
      }
    }
    

    [Column(Storage = "_ediCommunicationID", DbType = "INT NOT NULL")]
    public int? EdiCommunicationID
    {
      get { return _ediCommunicationID; }
      set
      {
        if (_ediCommunicationID != value)
        {
          PropertyIsChanging("EdiCommunicationID");
          _ediCommunicationID = value;
          PropertyHasChanged("EdiCommunicationID");
        }
      }
    }

    [Column(Storage = "_matchField", DbType = "BIT NOT NULL")]
    public bool MatchField
    {
      get { return _matchField; }
      set
      {
        if (_matchField != value)
        {
          PropertyIsChanging("MatchField");
          _matchField = value;
          PropertyHasChanged("MatchField");
        }
      }
    }

    [Association(Name = "FK_EdiFieldMapping_EdiCommunication", Storage = "_ediCommunication", OtherKey = "EdiCommunicationID", ThisKey = "EdiCommunicationID", IsForeignKey = true)]
    public EdiCommunication EdiCommunication
    {
      get { return _ediCommunication.Entity; }
      set
      {
        EdiCommunication previousValue = _ediCommunication.Entity;
        if (((previousValue != value)
             || (_ediCommunication.HasLoadedOrAssignedValue == false)))
        {
          PropertyIsChanging("EdiCommunication");
          if ((previousValue != null))
          {
            _ediCommunication.Entity = null;
            previousValue.EdiFieldMappings.Remove(this);
          }
          _ediCommunication.Entity = value;
          if ((value != null))
          {
            value.EdiFieldMappings.Add(this);
            _ediCommunicationID = value.EdiCommunicationID;
          }
          else
          {
            _ediCommunicationID = default(int);
          }
          PropertyHasChanged("EdiCommunication");
        }
      }
    }
    

    public Int32 EdiType { get; set; }
  }
}
