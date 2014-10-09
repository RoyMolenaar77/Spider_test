using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects;
using System.Data.Linq;
using System.Data;
using System.Data.SqlClient;
using AuditLog4Net.Adapter;
using log4net;
using Concentrator.Objects.ConcentratorService;
using AuditLog4Net.AuditLog;
using System.Data.Objects;

namespace Concentrator.Objects.Vendors.Base
{
  public abstract class BulkLoader<TContext> : IDisposable
    where TContext : ObjectContext, new()
  {
    protected ILog _log;
      

    public BulkLoader()
    {
      _log = new AuditLogAdapter(log4net.LogManager.GetLogger(GetType()), new AuditLog(new ConcentratorAuditLogProvider()));
    }

    public ILog Log
    {
      get { return _log; }
    }

    /// <summary>
    /// If this object is disposed the object will throw exceptions because resources used by this bulk loader will be disposed as well
    /// </summary>
    public bool IsDisposed { get; private set; }

    /// <summary>
    /// Will be called for every run. Use it to create temporary tables and fetch data
    /// Calling base will make sure TearDown is called first to cleanup any previous runs
    /// </summary>
    public virtual void Init(TContext context)
    {
      try
      {
        TearDown(context);
      }
      catch
      { }
    }

    /// <summary>
    /// Wall be called to clean up temporary resources
    /// </summary>
    protected virtual void TearDown(TContext context)
    {
    }

    /// <summary>
    /// Merges content
    /// </summary>
    public virtual void Sync(TContext context)
    {
    }

    /// <summary>
    /// Creates the IDataReader used by the BulkLoad method
    /// </summary>
    /// <returns></returns>
    protected virtual IDataReader CreateReader()
    {
      return null;
    }

    protected void BulkLoad(string destinationTable)
    {
      BulkLoad(destinationTable, 1000);
    }

    protected void BulkLoad(string destinationTable, IDataReader reader)
    {
      BulkLoad(destinationTable, 1000, reader);
    }

    protected void BulkLoad(string destinationTable, int notifyAfter)
    {
      BulkLoad(destinationTable, notifyAfter, CreateReader());
    }

    protected void BulkLoad(string destinationTable, int notifyAfter, IDataReader reader, string connectionString = null)
    {
      SqlBulkCopy copy;
      using (TContext ctx = new TContext())
      {
        if (string.IsNullOrEmpty(connectionString))
          connectionString = ctx.Connection.ConnectionString.Split('\"')[1];

        copy = new SqlBulkCopy(connectionString)
        {
          BatchSize = 10000,
          BulkCopyTimeout = 3600,
          DestinationTableName = destinationTable,
          NotifyAfter = notifyAfter
        };
      }

      if (notifyAfter > 0)
        copy.SqlRowsCopied += (s, e) => Log.DebugFormat("{0} Records inserted from file into {1}", e.RowsCopied, destinationTable);

      using (reader)
      {
        copy.WriteToServer(reader);
      }
    }

    #region IDisposable Members

    public void Dispose()
    {
      if (!IsDisposed)
      {
        using (var ctx = new TContext())
        {
          try
          { // We don't to stop execution of the application
            TearDown(ctx);
          }
          catch { }
        }
      }
      IsDisposed = true;
    }

    #endregion
  }
}
