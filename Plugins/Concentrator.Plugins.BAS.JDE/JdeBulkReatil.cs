using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Vendors.Base;
using Concentrator.Objects.DataAccess.EntityFramework;
using System.Data.SqlClient;

namespace Concentrator.Plugins.BAS.JDE
{
  public class JdeBulkRetail : VendorImportLoader<ConcentratorDataContext>
  {
    const string _retailStockTable = "[proddta].[FB41021S]";
    const string _retailStockTempTable = "[proddta].[FB41021S_temp]";

    private string _mergeTabelQuery = @"CREATE TABLE {0}(
	[SS55RID] [char](40) NOT NULL,
	[SS55SLID] [char](40) NOT NULL,
	[SSITM] [char](25) NOT NULL,
	[SSLITM] [char](25) NULL,
	[SSSTKT] [char](1) NULL,
	[SSPQOH] [float] NULL,
	[SSUPMT] [float] NULL)";

    private string _jdeConnectionString;
    private IEnumerable<FB41021S> _retailStockList;

    public JdeBulkRetail(IEnumerable<FB41021S> retailStockList, string jdeConnectionString)
    {
      _mergeTabelQuery = string.Format(_mergeTabelQuery, _retailStockTempTable);
      _retailStockList = retailStockList;
      _jdeConnectionString = jdeConnectionString;
    }

    public override void Init(ConcentratorDataContext context)
    {
      base.Init(context);

      try
      {
        using (SqlConnection connection = new SqlConnection(_jdeConnectionString))
        {
          connection.Open();

          using (SqlCommand command = new SqlCommand(_mergeTabelQuery, connection))
          {
            int result = command.ExecuteNonQuery();

            using (GenericCollectionReader<FB41021S> reader = new GenericCollectionReader<FB41021S>(_retailStockList))
            {
              // TODO: Check if this is correct
              //BulkLoad(_retailStockTempTable, 10000, reader, _jdeConnectionString);
              BulkLoad(_retailStockTempTable, 1000, reader, _jdeConnectionString);
            }
          }
        }

      }
      catch (Exception ex)
      {
        _log.Error("Error execture bulk copy");
      }
    }

    public override void Sync(ConcentratorDataContext context)
    {
      Log.DebugFormat("Start merge JDE retail stock");

      string retailMergeQuery = @"merge [proddta].[FB41021S] RS
	using (
		select distinct [SS55RID]
      ,[SS55SLID]
      ,[SSITM]
      ,[SSLITM]
      ,[SSSTKT]
      ,[SSPQOH]
      ,[SSUPMT] from [proddta].[FB41021S_temp]
      inner join [proddta].[F4101] on ssitm = imitm
	) RST on RS.[SS55RID] = RST.[SS55RID] and RS.[SS55SLID] = RST.[SS55SLID] and RS.[SSITM] = RST.[SSITM]
	when matched then update set RS.[SSPQOH] = RST.[SSPQOH], RS.[SSUPMJ] = dbo.JDEVandaag(), RS.[SSUPMT] = RST.[SSUPMT] 
	WHEN NOT Matched by target
THEN
INSERT ([SS55RID]
      ,[SS55SLID]
      ,[SSITM]
      ,[SSLITM]
      ,[SSSTKT]
      ,[SSPQOH]
      ,[SSPPDJ]
      ,[SSUPMJ]
      ,[SSUPMT])
values (RST.[SS55RID]
      ,RST.[SS55SLID]
      ,RST.[SSITM]
      ,RST.[SSLITM]
      ,RST.[SSSTKT]
      ,RST.[SSPQOH]
      ,null
      ,dbo.JDEVandaag()
      ,RST.[SSUPMT])
      WHEN NOT Matched by source and RS.SS55rid in (select distinct ss55rid from [proddta].[FB41021S_temp])
THEN delete;";

      using (SqlConnection connection = new SqlConnection(_jdeConnectionString))
      {
        connection.Open();

        using (SqlCommand command = new SqlCommand(retailMergeQuery, connection))
        {
          command.CommandTimeout = 3600;
          int result = command.ExecuteNonQuery();

          Log.DebugFormat("Finish merge JDE retail stock result: {0} rows", result);
        }
      }
    }

    protected override void TearDown(ConcentratorDataContext context)
    {
      using (SqlConnection connection = new SqlConnection(_jdeConnectionString))
      {
        connection.Open();

        using (SqlCommand command = new SqlCommand(String.Format("DROP TABLE {0}", _retailStockTempTable), connection))
        {
          int result = command.ExecuteNonQuery();

        }
      }

      //context.ExecuteStoreCommand(String.Format("DROP TABLE {0}", _retailStockTempTable));
    }
  }
  public class FB41021S
  {
    public string SS55RID { get; set; }
    public string SS55SLID { get; set; }
    public string SSITM { get; set; }
    public string SSLITM { get; set; }
    public string SSSTKT { get; set; }
    public int SSPQOH { get; set; }
    public string SSUPMT { get; set; }
  }
}
