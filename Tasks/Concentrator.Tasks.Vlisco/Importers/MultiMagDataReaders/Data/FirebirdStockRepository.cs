using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;

namespace Concentrator.Tasks.Vlisco.Importers.MultiMagDataReaders.Data
{
  using Models;

  public class FirebirdStockRepository : FirebirdRepository<Stock>
	{
		[Resource]
		private readonly String Query = null;

    public FirebirdStockRepository(String connectionString, DateTime lastExecutionTime)
      : base(connectionString, lastExecutionTime)
		{
		}

    protected override Stock GetModel(FirebirdSql.Data.FirebirdClient.FbDataReader reader)
    {
      return new Stock
      {
        DateTime        = reader.GetDateTime(0),
        ShopCode        = reader.GetString(1),
        ArticleCode     = reader.GetString(2),
        ColorCode       = reader.GetString(3),
        SizeCode        = reader.GetString(4),
        InStock         = reader.GetInt32(5),
        Maximum         = reader.GetInt32(6),
        Minimum         = reader.GetInt32(7),
        Reserved        = reader.GetInt32(8),
        Ordered         = reader.GetInt32(9),
        Delivered       = reader.GetInt32(10),
        Available       = reader.GetInt32(11),
        TotalIn         = reader.GetInt32(12),
        TotalOut        = reader.GetInt32(13),
        WAC             = reader.Get<Decimal?>(14)
      };
    }

    protected override String GetQuery()
    {
      return String.Format(Query, LastExecutionTime.GetValueOrDefault(DateTime.Now).ToString("yyyy-MM-dd HH:mm:ss"));
    }
	}
}
