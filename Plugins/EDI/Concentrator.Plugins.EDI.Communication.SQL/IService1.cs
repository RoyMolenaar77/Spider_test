using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Data;

namespace Concentrator.Plugins.EDI.Communication
{
  // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IService1" in both code and config file together.
  [ServiceContract]
  public interface IService1
  {
    [OperationContract]
    DataSet GetData(CommunicationType communicationLayer);

    // TODO: Add your service operations here
  }

  //// Use a data contract as illustrated in the sample below to add composite types to service operations
  [DataContract]
  public class CommunicationType
  {
    private string query;
    private EDIConnectionType ediConnectionType;
    private string connectionString;

    [DataMember]
    public string Query
    {
      get { return query; }
      set { query = value; }
    }

    [DataMember]
    public EDIConnectionType EDIConnectionType
    {
      get { return ediConnectionType; }
      set { ediConnectionType = value; }
    }

    [DataMember]
    public string ConnectionString
    {
      get { return connectionString; }
      set { connectionString = value; }
    }
  }

  public enum EDIConnectionType
  {
    SQL = 0,
    AW = 1,
    Oracle = 2,
    Excel = 3,
    MySql = 4
  }
}
