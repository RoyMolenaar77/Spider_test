using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Plugins.Magento.Helpers
{
	public class MySqlIndexerHelper : MagentoMySqlHelper
	{
		public MySqlIndexerHelper(string connectionString)
			: base(connectionString)
		{

		}

		public void ReindexStock()
		{
			using (var cn = Connection.CreateCommand())
			{
				cn.CommandType = System.Data.CommandType.Text;
				cn.CommandText = @"update index_process set status = 'require_reindex' where indexer_code in ('catalog_product_flat', 'cataloginventory_stock', 'catalog_category_product')";
				cn.ExecuteNonQuery();
			}
		}

		public void ReindexAll()
		{
			using (var cn = Connection.CreateCommand())
			{
				cn.CommandType = System.Data.CommandType.Text;
				cn.CommandText = @"update index_process set status = 'require_reindex'";
				cn.ExecuteNonQuery();
			}
		}
	}
}
