using System;
using System.Linq;
using System.Configuration;
using System.Net.Mail;
using System.IO;
using System.Xml;

namespace Concentrator.Objects.Utility
{
  public class EmailDaemon
  {
    protected MailMessage message;
    private log4net.ILog _log;

    public EmailDaemon(log4net.ILog log)
    {
      _log = log;
    }

    public void SendMail(string email, MailMessage m)
    {
      message = m;
      Send(email);
    }
    #region Mailtemplates

    #endregion

    #region Protected fields

    protected void Send(MailAddress mailTo, MailAddress sender)
    {
      try
      {

        message.From = sender;
        message.To.Clear();
        message.To.Add(mailTo);
        SmtpClient mailClient = new SmtpClient();
        var deliveryMethod = String.IsNullOrEmpty(ConfigurationManager.AppSettings["MailDeliveryMethod"]) ? "network" : ConfigurationManager.AppSettings["MailDeliveryMethod"].ToLower();

        switch (deliveryMethod)
        {
          case "specifiedpickupdirectory":
            mailClient.DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory;
            mailClient.PickupDirectoryLocation = ConfigurationManager.AppSettings["MailDeliveryLocation"];
            break;
          case "pickupdirectoryfromiis":
            mailClient.DeliveryMethod = SmtpDeliveryMethod.PickupDirectoryFromIis;
            break;
          case "network":
          default:
            mailClient.DeliveryMethod = SmtpDeliveryMethod.Network;
            if (!String.IsNullOrEmpty(ConfigurationManager.AppSettings["MailServer"]))
            {
              mailClient.Host = ConfigurationManager.AppSettings["MailServer"];
            }
            //mailClient.Host = String.IsNullOrEmpty(ConfigurationManager.AppSettings["MailServer"]) ? String.Empty : ConfigurationManager.AppSettings["MailServer"];
            break;
        }

        //debug code make sure that if debug mail is entered only these adresses will be used.
        //if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["DebugEmail"]))
        //{
        //  string[] debugAdresses = ConfigurationManager.AppSettings["DebugEmail"].Split(new char[] { ',' });

        //  if (debugAdresses.Count() > 0)
        //  {
        //    message.Bcc.Clear();
        //    message.CC.Clear();
        //    foreach (string debugAdress in debugAdresses)
        //    {
        //      message.Bcc.Add(new MailAddress(debugAdress));
        //    }
        //  }


        //}
        //else
        //{
        //  _log.Debug("No DebugEmail in configfile");
        //}
        mailClient.Send(message);
      }
      catch (Exception ex)
      {
        _log.Debug("Mail kan niet worden verstuurd naar: " + mailTo + " Message: " + ex.Message, ex.InnerException);
      }
    }

    protected void Send(string mailTo)
    {
      if (!string.IsNullOrEmpty(mailTo))
      {
        if (mailTo.Contains(','))
          Send(mailTo.Split(new char[] { ',' }));
        else
          Send(new MailAddress(mailTo));
      }
    }

    private void Send(string[] toAddress)
    {
      foreach (string s in toAddress)
      {
        Send(new MailAddress(s.Trim()));
      }
    }

    private void Send(MailAddress mailTo)
    {
      string fromAddress = ConfigurationManager.AppSettings["DefaultFromAddress"];
      string fromName = ConfigurationManager.AppSettings["DefaultFromName"];

      Send(mailTo, new MailAddress(fromAddress, fromName));
    }

    protected void SetField(string fieldName, string value)
    {
      if (message != null)
        message.Body = message.Body.Replace("##" + fieldName + "##", value);
    }

    protected void SetFieldInSubject(string fieldName, string value)
    {
      if (message != null)
        message.Subject = message.Subject.Replace("##" + fieldName + "##", value);
    }

    protected void AddAttachment(Attachment att)
    {
      if (message != null)
        message.Attachments.Add(att);
    }

    /// <summary>
    /// Loads a mailtemplate if invariantculture is false then a templated is loaded from culturedepending folder
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="invariantCulture"></param>
    protected void LoadTemplate(string fileName)
    {
      string templateDir = ConfigurationManager.AppSettings["MailTemplatesDirectory"];

      if (!templateDir.Contains(":"))
      {
        //fileName = ApplicationPhysicalRoot + templateDir + @"\" + fileName;
      }
      else
      {
        fileName = templateDir + @"\" + fileName;
      }

      if (File.Exists(fileName))
      {
        XmlDocument doc = new XmlDocument();
        doc.Load(fileName);

        message = new MailMessage();
        XmlNode subject = doc.SelectSingleNode(@"//subject");
        message.Subject = subject.InnerText;

        XmlNode body = doc.SelectSingleNode(@"//body");
        message.Body = body.InnerText;

        message.IsBodyHtml = body.Attributes["isHtmlBody"] == null ? false : bool.Parse(body.Attributes["isHtmlBody"].Value);
      }
      else
        throw new Exception("Mail template not found @ " + fileName);
    }

    
    #endregion

   

  }
}
