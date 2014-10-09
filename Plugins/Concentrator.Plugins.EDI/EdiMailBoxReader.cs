using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.ConcentratorService;
using System.Configuration;
using Lesnikowski.Mail;
using Lesnikowski.Client.IMAP;
using System.IO;
using System.Text.RegularExpressions;
using Concentrator.Objects.Utility;
using Lesnikowski.Client;
using System.Net.Security;

namespace Concentrator.Plugins.EDI
{
  public class EdiMailBoxReader : ConcentratorPlugin
  {

    public override string Name
    {
      get { return "EDI mailbox reader"; }
    }

    protected override void Process()
    {
      var config = GetConfiguration();

      ReadMailBox(config);
    }

    private static void Validate(
        object sender,
        ServerCertificateValidateEventArgs e)
    {
      const SslPolicyErrors ignoredErrors =
          SslPolicyErrors.RemoteCertificateChainErrors |
          SslPolicyErrors.RemoteCertificateNameMismatch;

      if ((e.SslPolicyErrors & ~ignoredErrors) == SslPolicyErrors.None)
      {
        e.IsValid = true;
        return;
      }
      e.IsValid = false;
    }

    private void ReadMailBox(Configuration config)
    {
      log.Info("Check for Mail");
      if (!string.IsNullOrEmpty(config.AppSettings.Settings["POPServer"].Value))
      {
        try
        {
          if (config.AppSettings.Settings["MailType"].Value == "POP")
          {
            using (Pop3 pop3 = new Pop3())
            {
              pop3.ServerCertificateValidate +=
                new ServerCertificateValidateEventHandler(Validate);


              pop3.ConnectSSL(config.AppSettings.Settings["POPServer"].Value);
              //pop3.STLS();
              pop3.Login(config.AppSettings.Settings["POPMailUser"].Value, config.AppSettings.Settings["POPMailPassword"].Value);

              foreach (string uid in pop3.GetAll())
              {
                string eml = pop3.GetMessageByUID(uid);
                IMail mail = new MailBuilder()
                  .CreateFromEml(eml);

                try
                {
                  mail.Attachments.ForEach(att =>
                  {
                    processAttachment(att, config, mail.Sender.Address);

                  });
                  pop3.DeleteMessageByUID(uid);
                }
                catch (IOException ex)
                {
                  log.Error(ex.InnerException);
                }
                catch (Exception ex)
                {
                  log.Error(ex);
                }
              }
              pop3.Close();
            }
          }
          else
          {
            #region IMAP
            Imap imap = new Imap();

            //imap.User = ;

            //imap.Password =;
            imap.Connect(config.AppSettings.Settings["POPServer"].Value);
            imap.Login(config.AppSettings.Settings["POPMailUser"].Value, config.AppSettings.Settings["POPMailPassword"].Value);

            imap.SelectInbox();

            List<long> uidList = imap.SearchFlag(Flag.Unseen);
            log.Debug("Go to process: " + uidList.Count + "mails");

            foreach (long uid in uidList)
            {
              ISimpleMailMessage imail = new SimpleMailMessageBuilder()

                  .CreateFromEml(imap.GetMessageByUID(uid));

              log.Info("Email received: " + imail.From.First().Name);

              foreach (var att in imail.Attachments)
              {
                try
                {
                  processAttachment(att, config, imail.From.First().Address);
                }
                catch (IOException ex)
                {
                  log.Error(ex.InnerException);
                  imap.FlagMessageByUID(uid, Flag.Flagged);
                  imap.MarkMessageSeenByUID(uid);
                }
                catch (Exception ex)
                {
                  log.Error(ex);
                  imap.FlagMessageByUID(uid, Flag.Flagged);
                  imap.MarkMessageSeenByUID(uid);
                }
              }

              imap.MarkMessageSeenByUID(uid);
              imap.DeleteMessageByUID(uid);
            }

            imap.Close(true);
            #endregion
          }
          log.Info("Mail check complete");
        }
        catch (Exception ex)
        {
          log.Error(ex.InnerException);
        }
      }
      else
        log.Info("Mail check skipped!");

    }

    public void processAttachment(MimeData att, Configuration config, string mailAddress)
    {
      if (att.FileName.Contains("xls") || att.FileName.Contains("xlsx") || att.FileName.Contains("csv") || att.FileName.Contains("xml"))
      {
        Guid guid = Guid.NewGuid();
        using (var unit = GetUnitOfWork())
        {
          //BAS.EDI.Objects.Mail.MailLog mailLog = new MailLog();
          //mailLog.Mailaddress = imail.From.First().Address;
          //mailLog.Filename = att.FileName;
          //mailLog.ReceiveDate = System.DateTime.Now;
          string ext = ".unknown";
          if (att.FileName.Contains(".xlsx"))
            ext = ".xlsx";
          else if (att.FileName.Contains(".xls"))
            ext = ".xls";
          else if (att.FileName.Contains(".csv"))
            ext = ".csv";

          string fileName = att.FileName;
          if (fileName.Contains(")"))
            fileName = fileName.Replace(")", "-");
          if (fileName.Contains("("))
            fileName = fileName.Replace("(", "-");
          if (fileName.Contains("_"))
            fileName = fileName.Replace("_", "-");
          Regex reg = new Regex("([0-9a-z_-]+[\\.][0-9a-z_-]{1,6})$");
          fileName = reg.Match(fileName).Value;

          if (string.IsNullOrEmpty(fileName))
            fileName = ext;

          //mailLog.UniqueCode = guid.ToString() + "-" + fileName;
          //context.MailLogs.InsertOnSubmit(mailLog);
          //context.SubmitChanges();

          //if (!Directory.Exists(ConfigurationManager.AppSettings["ExcelMailDirectory"]))
          //  Directory.CreateDirectory(ConfigurationManager.AppSettings["ExcelMailDirectory"]);

          string filepath = config.AppSettings.Settings["EdiOrderDir"].Value + guid.ToString() + "-" + fileName;

          using (MemoryStream ms = att.GetMemoryStream())
          {
            FileStream outStream = File.OpenWrite(filepath);
            ms.WriteTo(outStream);
            outStream.Flush();
            outStream.Close();
          }

          ExcelReader excel = new ExcelReader(config.AppSettings.Settings["EdiOrderDir"].Value, log, unit);
          excel.ProcessFile(filepath, mailAddress);
        }
      }
    }
  }
}
