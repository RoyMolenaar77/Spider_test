using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AuditLog4Net.AuditLog.Providers;
using AuditLog4Net.AuditLog;
using Concentrator.Objects.Models.Management;
using Microsoft.Practices.ServiceLocation;
using Concentrator.Objects.DataAccess.UnitOfWork;


namespace Concentrator.Objects.ConcentratorService
{
  public class ConcentratorAuditLogProvider : IAuditProvider
  {
    #region IAuditProvider Members

    public void SaveAuditEvent(AuditLoggingEvent info)
    {
      try
      {
        EventTypes type = (EventTypes)Enum.Parse(typeof(EventTypes), info.Level.ToString());


        using (var unit = ServiceLocator.Current.GetInstance<IUnitOfWork>())
        {
          Event evt = new Event()
                        {
                          TypeID = (int)type,
                          ProcessName = info.FriendlyProcessName,
                          Message = info.Message.ToString(),
                          ExceptionMessage = info.ExceptionObject == null ? string.Empty : info.ExceptionObject.Message,
                          StackTrace = info.ExceptionObject == null ? string.Empty : info.ExceptionObject.StackTrace
                        };

          unit.Scope.Repository<Event>().Add(evt);
          unit.Save();
        }
      }
      catch (Exception)
      {
        Console.WriteLine("Could not log exception to db. Exception was @0", info.Message.ToString());
      }
    }

    #endregion
  }
}
