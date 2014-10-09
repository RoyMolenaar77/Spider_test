using Concentrator.Objects.Models.Users;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Text;

namespace Concentrator.Tasks.Core.Monitoring
{
  [Task("Concentrator Event Notifier")]
  public class Notifier : TaskBase
  {
    protected override void ExecuteTask()
    {
      var users = Database.Query<User>(@"select * from [user] where isnull(email, '') != '' ");

      foreach (var user in users)
      {
        StringBuilder builder = new StringBuilder();

        var eventList = (from @event in Database.Query<dynamic>(@"select e.*, p.PluginName
                          from userplugin up
                          inner join plugin p on p.pluginid = up.pluginid
                          inner join event e on up.pluginid = e.pluginid and up.typeid = e.typeid
                          where userid = @0 and e.notified = 0 and e.creationtime >= up.subscriptiontime", user.UserID)

                         select new
                         {
                           EventID = (int)@event.EventID,
                           PluginID = (int)@event.PluginID,
                           TypeID = (int)@event.TypeID,
                           Message = (string)@event.Message,
                           ProcessName = (string)@event.ProcessName,
                           PluginName = (string)@event.PluginName,
                           ExceptionMessage = (string)@event.ExceptionMessage,
                           StackTrace = (string)@event.StackTrace,
                           ExceptionLocation = (string)@event.ExceptionLocation
                         }).ToList();

        foreach (var @event in eventList)
        {
          builder.AppendFormat(
             "=================================================================================================" +
                 "<br />" + "Plugin Info" + "<br />" + "<br />" +
                 "Event ID : {0}" + "<br />" +
                 "Plugin ID : {1}" + "<br />" +
                 "Type ID : {2}" + "<br />" +
                 "Message : {3}" + "<br />" +
                 "Process Name : {4}" + "<br />" +
                 "Plugin name : {8}" + "<br />" +
                 "Exception Message: {5}" + "<br /> <br />" +
                 "Stack Trace : {6}" + "<br /> <br /> " +
                 "Exception Location : {7}" + "<br /> <br /> ",
                 @event.EventID, @event.PluginID, @event.TypeID, @event.Message, @event.ProcessName,
                 @event.ExceptionMessage, @event.StackTrace, @event.ExceptionLocation, @event.PluginName);
        }
        try
        {
          using (MailMessage message = new MailMessage())
          {
            using (SmtpClient client = new SmtpClient())
            {
              message.BodyEncoding = Encoding.UTF8;
              message.SubjectEncoding = Encoding.UTF8;
              message.From = new MailAddress("support@diract-it.nl");

              MailAddress adress = new MailAddress(user.Email, string.Empty);

              if (string.IsNullOrEmpty(adress.Address))
              {
                return;
              }

              message.To.Add(adress);
              message.IsBodyHtml = true;
              message.Subject = "Error notification from Concentrator";
              message.Body = builder.ToString();

              if (!string.IsNullOrEmpty(message.Body))
                client.Send(message);


              Database.Execute("update [Event] set Notified = 1 where EventID in", @eventList.Select(c => c.EventID).ToList());             
            }
          }
        }
        catch
        {
          TraceWarning("Could not process event notification for user {0}", user.UserID);
        }

      }
    }
  }
}
