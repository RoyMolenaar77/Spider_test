using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AuditLog4Net.AuditLog.Providers;
using AuditLog4Net.AuditLog;
using Concentrator.Objects.Models.Management;


namespace Concentrator.Objects.Service
{
  public class ConcentratorAuditLogProvider : IAuditProvider
  {
    #region IAuditProvider Members

    public void SaveAuditEvent(AuditLoggingEvent info)
    {
      EventTypes type = (EventTypes)Enum.Parse(typeof(EventTypes), info.Level.ToString());


      //using (ConcentratorDataContext context = new ConcentratorDataContext())
      //{
      //  Event evt = new Event()
      //                {
      //                  TypeID = (int)type,
      //                  ProcessName = info.FriendlyProcessName,
      //                  Message = info.Message.ToString(),
      //                  ExceptionMessage = info.ExceptionObject == null ? string.Empty : info.ExceptionObject.Message,
      //                  StackTrace = info.ExceptionObject == null ? string.Empty : info.ExceptionObject.StackTrace
      //                };

      //  context.Events.InsertOnSubmit(evt);
      //  context.SubmitChanges();
      //}
    }

    #endregion
  }
}
