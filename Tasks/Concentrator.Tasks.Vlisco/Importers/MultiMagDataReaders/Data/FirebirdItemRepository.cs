using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;

namespace Concentrator.Tasks.Vlisco.Importers.MultiMagDataReaders.Data
{
  using Models;

  public class FirebirdItemRepository : FirebirdRepository<Item>
	{
		[Resource]
		private readonly String Query = null;

    public FirebirdItemRepository(String connectionString, DateTime lastExecutionTime)
      : base(connectionString, lastExecutionTime)
		{
		}

    protected override Item GetModel(FirebirdSql.Data.FirebirdClient.FbDataReader reader)
    {
      return new Item
      {
        DateTime          = reader.GetDateTime(0),
        ArticleCode       = reader.GetString(1),
        ColorCode         = reader.GetString(2),
        ColorName         = reader.GetString(3),
        SizeCode          = reader.GetString(4),
        Barcode           = reader.GetString(5),
        DescriptionLong   = reader.GetString(6),
        DescriptionShort  = reader.GetString(7),
        Price             = reader.GetDecimal(8),
        OriginCode        = reader.GetString(9),
        OriginName        = reader.GetString(10),
        LabelCode         = reader.GetString(11),
        LabelName         = reader.GetString(12),
        SegmentCode       = reader.GetString(13),
        SegmentName       = reader.GetString(14),
        GroupCode         = reader.GetString(15),
        GroupName         = reader.GetString(16)
      };
    }

    protected override String GetQuery()
    {
      return String.Format(Query, LastExecutionTime.GetValueOrDefault(DateTime.Now).ToString("yyyy-MM-dd HH:mm:ss"));
    }
	}
}
