using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Web;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Threading;
using System.IO;
using System.Xml;
using System.Data;

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
    public void SendOrderDetails(string toAddr, Stream content, string name)
    {
      LoadTemplate("OrderDetail.xml");

      XmlDocument xml = new XmlDocument();
      xml.Load(content);

      SetField("OrderContent", xml.OuterXml);
      SetField("Name", name);

      Send(toAddr);
    }

    public void SendErrorMail(string content)
    {
      LoadTemplate("ErrorMail.xml");

      SetField("OrderContent", content);
      SetField("Name", "Beste ICT beheerder");
      SetFieldInSubject("Subject", "Foutmelding EDI");


      if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["ErrorMailAddress"]))
      {
        string[] errorAdresses = ConfigurationManager.AppSettings["ErrorMailAddress"].Split(new char[] { ',' });
        Send(errorAdresses);
      }
    }

    public void SendErrorMail(List<string> list, string xmldocument, string name, string email)
    {
      LoadTemplate("ErrorMail.xml");

      StringBuilder content = new StringBuilder();
      content.AppendLine("De door u verzonden order(s) is of zijn door ons ontvangen.");
      content.AppendLine("Wij kunnen de order(s) niet verwerken door fouten in het meegezonden Excel bestand. ");
      content.AppendLine("EDI orders kunnen alleen worden verwerkt als deze aan het door ons aangereikte “ordertemplate” voldoen.");
      content.AppendLine("Heeft u geen “ordertemplate”, neem dan contact op met uw account manager.");

      content.Append("Uw order bevat een aantal fouten: <ul>");

      foreach (string c in list)
      {
        content.AppendLine("<li>" + c + "</li>");
      }
      content.AppendLine("</li>");

      SetField("OrderContent", content.ToString());
      SetField("Name", name);
      SetFieldInSubject("Subject", "Foutmelding BAS EDI orderverwerking");


      using (Stream s = new MemoryStream(ASCIIEncoding.Default.GetBytes(xmldocument)))
      {
        Attachment att = new Attachment(s, "Order.xml");
        AddAttachment(att);
      }

      Send(email);
    }

    public void SendErrorMail(string message, string mailaddress, string type)
    {
      LoadTemplate("ErrorMail.xml");

      StringBuilder content = new StringBuilder();

      if (type == "Excel")
      {
        content.AppendLine("De door u verzonden order(s) is of zijn door ons ontvangen.");
        content.AppendLine("Wij kunnen de order(s) niet verwerken door fouten in het meegezonden Excel bestand. ");
        content.AppendLine("EDI orders kunnen alleen worden verwerkt als deze aan het door ons aangereikte “ordertemplate” voldoen.");
        content.AppendLine("Heeft u geen “ordertemplate”, neem dan contact op met uw account manager.");
      }
      else if (type == "ProductExcel")
      {
        content.AppendLine("De door u verzonden producten zijn door ons ontvangen.");
        content.AppendLine("Wij kunnen de producten niet verwerken door fouten in het meegezonden Excel bestand. ");
        content.AppendLine("EDI producten kunnen alleen worden verwerkt als deze aan het door ons aangereikte “productentemplate” voldoen.");
        content.AppendLine("Heeft u geen “productentemplate”, neem dan contact op met ICT Helpdesk.");
      }
      else if (type == "PublicationExcel")
      {
        content.AppendLine("De door u verzonden publicaties zijn door ons ontvangen.");
        content.AppendLine("Wij kunnen de publicaties niet verwerken door fouten in het meegezonden Excel bestand. ");
        content.AppendLine("EDI publicaties kunnen alleen worden verwerkt als deze aan het door ons aangereikte “publicatietemplate” voldoen.");
        content.AppendLine("Heeft u geen “publicatietemplate”, neem dan contact op met ICT Helpdesk.");
      }

      content.Append("Uw EDI mail bevat de volgende fout: " + message);

      SetField("OrderContent", content.ToString());
      SetField("Name", mailaddress);
      SetFieldInSubject("Subject", "Foutmelding BAS EDI verwerking");

      Send(mailaddress);
    }

    public void SendPublicationMail(string message, string mailaddress)
    {
      LoadTemplate("ErrorMail.xml");

      StringBuilder content = new StringBuilder();
      content.AppendLine("De door u verzonden publicatie(s) is of zijn door ons ontvangen.");

      content.Append(message);

      SetField("OrderContent", content.ToString());
      SetField("Name", mailaddress);
      SetFieldInSubject("Subject", "Publicatie verwerking");

      Send(mailaddress);
    }

    internal void SendErrorMail(DataRow[] rows, string mailaddress)
    {
      LoadTemplate("ErrorMail.xml");

      StringBuilder content = new StringBuilder();
      content.Append("De zojuist ingestuurde Excel is niet volledige verwerkt vanwege onderstaande fouten:");

      foreach (System.Data.DataRow row in rows)
      {
        content.AppendLine(row.RowError);
      }
      content.AppendLine("");
      content.AppendLine("U kunt de lijnen herstellen en de excel opnieuw toesturen (LET OP! de lijnen die niet genoemd staan zijn wel verwerkt!)");

      SetField("OrderContent", content.ToString());
      SetField("Name", "Beste klant");
      SetFieldInSubject("Subject", "Foutmelding BAS EDI orderverwerking");

      Send(mailaddress);
    }

    public void OrderReceivedSucceed(string name, string email, bool orderConfirmation)
    {
      LoadTemplate("ProcessSucceed.xml");
      string content;

      if (orderConfirmation)
        content = "<p>U krijgt automatisch een bevestiging van de door u verzonden order(s).</p>";
      else
        content = "<p>U heeft er tevens voor gekozen geen verdere bevestigingen zoals “orderconfirmation”, “shipment notification” en/ of invoice te ontvangen.</p><p>Mocht u toch geïnteresseerd zijn in het ontvangen van deze documenten, neem dan contact op met uw account manager</p>";

      SetField("OrderContent", content);
      SetField("Name", name);

      Send(email);
    }

    public void OrderReceivedError(string errormail)
    {
      LoadTemplate("ProcessError.xml");
      Send(errormail);
    }

    //public void AcknowledgementNotification(bool xml)
    //{
    //  if (xml)
    //  {
    //    LoadTemplate("AcknowledgementNotification.xml");
    //    string content;

    //    if (orderInfo.CustomerInfo.ShipmentNotification)
    //      content = "<p>U ontvangt  een “shipment notification” zodra uw order wordt verzonden.</p>";
    //    else
    //      content = "<p>U heeft er tevens voor gekozen geen verdere bevestigingen zoals “shipment notification” en/ of invoice te ontvangen.</p><p>Mocht u toch geïnteresseerd zijn in het ontvangen van deze documenten, neem dan contact op met uw account manager.</p>";

    //    SetField("OrderContent", content);
    //    SetField("Name", orderInfo.CustomerInfo.Name);
    //    XmlDocument xmldoc = new XmlDocument();
    //    xmldoc.LoadXml(orderInfo.AcknowledgmentDocument);
    //    MemoryStream str = new MemoryStream();
    //    xmldoc.Save(str);

    //    Attachment att = new Attachment(str, "BAS Orderbevestiging");
    //    AddAttachment(att);
    //    Send(orderInfo.CustomerInfo.OrderConfirmationEmail);
    //  }
    //  else
    //  {
    //    AcknowledgementNotification(orderInfo);
    //  }
    //}

        public void AcknowledgementNotification(string name, string email, bool shipment, string path)
    {
      LoadTemplate("AcknowledgementNotification.xml");
      string content;

      if (shipment)
        content = "<p>U ontvangt  een “shipment notification” zodra uw order wordt verzonden.</p>";
      else
        content = "<p>U heeft er tevens voor gekozen geen verdere bevestigingen zoals “shipment notification” en/ of invoice te ontvangen.</p><p>Mocht u toch geïnteresseerd zijn in het ontvangen van deze documenten, neem dan contact op met uw account manager.</p>";

      SetField("OrderContent", content);
      SetField("Name", name);
      Attachment att = new Attachment(path);
      AddAttachment(att);
      Send(email);
    }

    public void ShipmentNotification(string name, string email, string path, bool invoice, List<string> links)
    {
      LoadTemplate("ShipmentNotification.xml");
      string content;

      if (invoice)
        content = "<p>U ontvangt een “invoice” zodra uw order wordt verzonden.</p>";
      else
        content = "<p>U heeft er tevens voor gekozen geen invoice te ontvangen.</p><p>Mocht u toch geïnteresseerd zijn in het ontvangen van dit documenten, neem dan contact op met uw account manager.</p>";

      string tnt = "Track and Trace links:<ul>";
      foreach (string link in links)
      {
        tnt += "<li>" + link + "</li>";
      }
      tnt += "</ul>";

      SetField("TNT", tnt);

      SetField("OrderContent", content);
      SetField("Name", name);
      Attachment att = new Attachment(path);
      AddAttachment(att);

      Send(email);
    }

    public void InvoiceNotification(string name, string email, string path)
    {
      LoadTemplate("InvoiceNotification.xml");

      SetField("Name", name);
      Attachment att = new Attachment(path);
      AddAttachment(att);
      Send(email);
    }



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
        _log.Debug("Mail kan niet worden verstuurd");
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

    //internal static string ApplicationPhysicalRoot
    //{
    //  get
    //  {
    //    if (HttpContext.Current != null && HttpContext.Current.Request != null)
    //      return HttpContext.Current.Request.PhysicalApplicationPath;
    //    else
    //      if (AppDomain.CurrentDomain != null && AppDomain.CurrentDomain.BaseDirectory != null)
    //        return AppDomain.CurrentDomain.BaseDirectory;
    //    return string.Empty;
    //  }
    //}
    #endregion




  }
}
