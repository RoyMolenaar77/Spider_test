using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Data;
using System.Configuration;
using System.Data.OleDb;
using System.Data.SqlClient;

namespace Concentrator.Plugins.EDI.Communication
{
  // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in both code and config file together.
  public class CommunicationService : IService1
  {

    public DataSet GetData(CommunicationType communicationLayer)
    {
      string prefix = string.Empty;
      switch (communicationLayer.EDIConnectionType)
      {
        case EDIConnectionType.AW:
          prefix = "Provider = PostgreSQL OLE DB Provider;";
          break;
        case EDIConnectionType.Oracle:
          prefix = "Provider=OraOLEDB.Oracle;";
          break;
        case EDIConnectionType.Excel:
          prefix = "Provider=Microsoft.Jet.OLEDB.4.0;";
          break;
        case EDIConnectionType.MySql:
          prefix = "Provider=MySQLProv;";
          break;
      }

      DataSet ds = new DataSet();
      try
      {
        if (EDIConnectionType.SQL == communicationLayer.EDIConnectionType)
        {
          using(SqlConnection conn = new SqlConnection(communicationLayer.ConnectionString))
          {
            conn.Open();
            using (SqlCommand cmd = new SqlCommand(communicationLayer.Query, conn))
            {
              SqlDataAdapter adapter = new SqlDataAdapter(cmd);
              adapter.Fill(ds);
            }
          }
        }
        else
        {
          using (OleDbConnection cn = new OleDbConnection(string.Format("{0}{1}", prefix, communicationLayer.ConnectionString)))
          {
            cn.Open();
            using (OleDbCommand cmd = new OleDbCommand(communicationLayer.Query, cn))
            {
              OleDbDataAdapter adapter = new OleDbDataAdapter(cmd);
              adapter.Fill(ds);
            }
          }
        }
      }
      catch (Exception ex)
      {

      }

      return ds;
    }
  }
}
