using Concentrator.Tasks.Vlisco.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Concentrator.Tasks.Vlisco.Importers.MultiMagDataReaders.Data
{
  public class FirebirdOrderRepository : FirebirdRepository<Order>
  {
    [Resource]
    private readonly String Query = null;

    public FirebirdOrderRepository(String connectionString, DateTime lastExecutionTime)
      : base(connectionString, lastExecutionTime)
    {
    }

    protected override Order GetModel(FirebirdSql.Data.FirebirdClient.FbDataReader reader)
    {
      return new Order
      {
        OrderDate           = reader.GetDateTime(0), 
        OrderTime           = reader.GetDateTime(1), 
        SaleType            = reader.GetString(2),
        Ticket              = reader.GetString(3),
        Line                = reader.GetInt32(4),
        ArticleCode         = reader.GetString(5),
        ColorCode           = reader.GetString(6),
        SizeCode            = reader.GetString(7),
        Quantity            = reader.GetInt32(8),
        PurchasePrice       = reader.GetDecimal(9),
        SalePrice           = reader.GetDecimal(10),
        BrutoPrice          = reader.GetDecimal(11),
        VAT                 = reader.GetDecimal(12),
        NettoPrice          = reader.GetDecimal(13),
        DiscountPercentage  = reader.GetDecimal(14),
        DiscountValue       = reader.GetDecimal(15),
        ShopCode            = reader.GetString(16),
        SalesPerson         = reader.GetString(17),
        Client              = reader.GetString(18)
      };
    }

    protected override String GetQuery()
    {
      return String.Format(Query, LastExecutionTime.GetValueOrDefault(DateTime.Now).ToString("yyyy-MM-dd hh:mm:ss"));
    }
  }
}
