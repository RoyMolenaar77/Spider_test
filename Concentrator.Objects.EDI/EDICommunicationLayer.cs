using System.Data;
using Concentrator.Objects.Models.EDI.Enumerations;
using System;
using Concentrator.Objects.EDI.EDI.Communication;

namespace Concentrator.Objects.EDI
{
  public class EDICommunicationLayer
  {
    private EdiConnectionType _ediConnectionType;

    private string _connectionString;

    public EDICommunicationLayer(EdiConnectionType ediConnectionType, string connectionString)
    {
      _ediConnectionType = ediConnectionType;
      _connectionString = connectionString;
    }

    public bool IsValid(string query)
    {
      var result = GetVendorData(query);

      if (result.Tables.Count > 0)
      {
        if (result.Tables[0].Rows.Count > 0)
          return true;
      }
      return false;
    }

    public DataSet GetVendorData(string query)
    {
      using (EDI.Communication.Service1Client client = new EDI.Communication.Service1Client())
      {
        EDI.Communication.CommunicationType communication = new EDI.Communication.CommunicationType();
        communication.Query = query;
        communication.EDIConnectionType = (EDIConnectionType)Enum.Parse(typeof(EDIConnectionType), _ediConnectionType.ToString());
        communication.ConnectionString = _connectionString;


        return client.GetData(communication);
      }
    }
  }
}
