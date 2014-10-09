using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;

namespace Concentrator.Tasks.Vlisco.Importers.MultiMagDataReaders.Data
{
  using Models;

  public class FirebirdMovementRepository : FirebirdRepository<Movement>
	{
		[Resource]
		private readonly String Query = null;

    public FirebirdMovementRepository(String connectionString, DateTime lastExecutionTime)
      : base(connectionString, lastExecutionTime)
		{
		}

    protected override Movement GetModel(FirebirdSql.Data.FirebirdClient.FbDataReader reader)
    {
      return new Movement
      {
        ShopCode            = reader.GetString(0),
        MovementNumber      = reader.GetInt32(1),
        MovementType        = reader.GetString(2),
        MovementDescription = reader.GetString(3),
        ArticleCode         = reader.GetString(4),
        ColorCode           = reader.GetString(5),
        SizeCode            = reader.GetString(6),
        MovementDirection   = reader.GetString(7),
        MovementTime        = reader.GetDateTime(9),
        MovementDate        = reader.GetDateTime(8),
        SalesPerson         = reader.GetString(10),
        CostPrice           = reader.Get<Decimal?>(11),
        UnitPrice           = reader.Get<Decimal?>(12),
        DocumentLine        = reader.GetString(13),
        DocumentNumber      = reader.GetString(14),
        LocationFrom        = reader.GetString(15),
        LocationTo          = reader.GetString(16),
        Quantity            = reader.GetInt32(17),
        TransactionNumber   = reader.GetString(18),
        LotNumber           = reader.GetString(19)
      };
    }

    protected override String GetQuery()
    {
      return String.Format(Query, LastExecutionTime.GetValueOrDefault(DateTime.Now).ToString("yyyy-MM-dd HH:mm:ss"));
    }
	}
}
