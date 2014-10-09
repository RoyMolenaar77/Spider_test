using System;
using System.Data.Objects;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Web;

namespace Concentrator.Objects.DataAccess.EntityFramework
{
  public class ConcentratorDataContext : ObjectContext, IDisposable
  {
    public ConcentratorDataContext()
      : base(string.Concat("metadata=res://*/DataAccess.EntityFramework.ConcentratorDataModel.csdl|res://*/DataAccess.EntityFramework.ConcentratorDataModel.ssdl|res://*/DataAccess.EntityFramework.ConcentratorDataModel.msl;provider=System.Data.SqlClient;provider connection string=\"", Environments.Environments.Current.Connection, "\""))
    {
      ContextOptions.LazyLoadingEnabled = true;
      //ContextOptions.ProxyCreationEnabled = false;
      this.CommandTimeout = 120;
    }

    public override int SaveChanges(SaveOptions options)
    {
      this.DetectChanges();
      //get audit inserts

      //#endregion ILedger

      var auditInserts = this.ObjectStateManager.GetObjectStateEntries(System.Data.EntityState.Added);
      var now = DateTime.Now;

      foreach (var b in auditInserts)
      {
        if (b.Entity is IAuditObject)
        {
          ((IAuditObject)b.Entity).CreatedBy = Client.User.UserID;
          ((IAuditObject)b.Entity).CreationTime = now;
        }

      }

      var auditUpdates = this.ObjectStateManager.GetObjectStateEntries(System.Data.EntityState.Modified);
      foreach (var update in auditUpdates)
      {
        if (update is IAuditObject)
        {
          ((IAuditObject)update).LastModificationTime = now;
          ((IAuditObject)update).LastModifiedBy = Client.User.UserID;
        }
      }

      return base.SaveChanges(options);
    }
  }
}
