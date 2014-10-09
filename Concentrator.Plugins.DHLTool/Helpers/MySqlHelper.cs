using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;

namespace Concentrator.Plugins.DhlTool.Helpers
{
  public class MySqlHelper : IDisposable
  {
    protected MySqlConnection Connection = null;

    public MySqlHelper(string connectionString)
    {
      Connection = new MySqlConnection(connectionString);
      Connection.Open();
    }

    public void Dispose()
    {
      if (Connection != null)
        Connection.Close();
    }

    public DataSet GetDhlOrders()
    {
      DataSet result = new DataSet();
      using (var cmd = Connection.CreateCommand())
      {
        cmd.CommandText = QueryHelper.GetDhlOrdersQuery();

        cmd.CommandType = CommandType.Text;


        using (MySqlDataAdapter da = new MySqlDataAdapter(cmd))
        {
          da.Fill(result);
        }
      }

      return result;
    }

    public DataSet UpdateRetrievedDhlOrders(DataSet dhlOrders)
    {
      var dhlOrdersToUpdate = (from order in dhlOrders.Tables[0].AsEnumerable()
                               select new
                               {
                                 shipmentId = order.Field<object>("shipment_id")
                               }).Distinct().ToList();

      DataSet result = new DataSet();
      using (var cmd = Connection.CreateCommand())
      {
        cmd.CommandText = string.Format(QueryHelper.UpdateRetrievedDhlOrders(), string.Join(",", dhlOrdersToUpdate.Select(x => x.shipmentId).ToArray()));

        cmd.CommandType = CommandType.Text;

        using (MySqlDataAdapter da = new MySqlDataAdapter(cmd))
        {
          da.Fill(result);
        }
      }

      return result;
    }

    public DataSet UpdateStatusSuccessInMagento(DhlStatus dhlStatus)
    {
      DataSet result = new DataSet();
      using (var cmd = Connection.CreateCommand())
      {
        cmd.CommandText = string.Format(QueryHelper.UpdateStatusSuccessInMagentoQuery(), dhlStatus.shipmentNumber);

        cmd.CommandType = CommandType.Text;

        using (MySqlDataAdapter da = new MySqlDataAdapter(cmd))
        {
          da.Fill(result);
        }
      }

      return result;
    }

    public DataSet UpdateStatusFailureInMagento(DhlStatus dhlStatus)
    {
      DataSet result = new DataSet();
      using (var cmd = Connection.CreateCommand())
      {
        cmd.CommandText = string.Format(QueryHelper.UpdateStatusFailureInMagentoQuery(), dhlStatus.shipmentNumber);

        cmd.CommandType = CommandType.Text;

        using (MySqlDataAdapter da = new MySqlDataAdapter(cmd))
        {
          da.Fill(result);
        }
      }

      return result;
    }
  }
}
