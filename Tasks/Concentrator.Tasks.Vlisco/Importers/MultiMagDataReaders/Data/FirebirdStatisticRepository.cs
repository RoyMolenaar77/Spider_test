using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;

namespace Concentrator.Tasks.Vlisco.Importers.MultiMagDataReaders.Data
{
  using Models;

  public class FirebirdStatisticRepository : FirebirdRepository<Statistic>
	{
		[Resource]
		private readonly String Query = null;

		public FirebirdStatisticRepository(String connectionString, DateTime lastExecutionTime)
      : base(connectionString, lastExecutionTime)
		{
		}

    protected override Statistic GetModel(FirebirdSql.Data.FirebirdClient.FbDataReader reader)
    {
      return new Statistic
      {
        Atmosphere          = reader.GetString(0),
        Context             = reader.GetString(1),
        DateTime            = reader.GetDateTime(2),
        ShopCode            = reader.GetString(3),
        TotalSales          = reader.GetDecimal(4),
        ClientSales         = reader.GetDecimal(5),
        GeneralSales        = reader.GetDecimal(6),
        UnitsSold           = reader.GetDecimal(7),
        UnitsPerClient      = reader.GetDecimal(8),
        TotalAmount         = reader.GetDecimal(9),
        SalesPerClient      = reader.GetDecimal(10),
        VisitorCount        = reader.Get<Int32?>(11),
        VisitorsWithNoSales = reader.Get<Int32?>(12)
      };
    }

    protected override String GetQuery()
    {
      return String.Format(Query, LastExecutionTime.GetValueOrDefault(DateTime.Now).ToString("yyyy-MM-dd HH:mm:ss"));
    }
	}
}
