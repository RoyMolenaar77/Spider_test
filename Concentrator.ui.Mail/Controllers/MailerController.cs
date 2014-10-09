using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Mvc.Mailer;
using System.Net.Mail;
using Concentrator.ui.Mailer.Mailers;
using System.Xml;
using System.Xml.Serialization;
using Concentrator.ui.Mailer.Models;
using System.IO;

namespace Concentrator.ui.Portal.Controllers
{
  public class MailerController : Controller
  {
    private IUserMailer _userMailer = new UserMailer();
    public IUserMailer UserMailer
    {
      get { return _userMailer; }
      set { _userMailer = value; }
    }

    public ActionResult SendIngramOrder()
    {
      using (StreamReader sreader = new StreamReader(this.HttpContext.Request.InputStream))
      {
        XmlSerializer serializer = new XmlSerializer(typeof(IngramMailData));
        XmlSerializerNamespaces nm = new XmlSerializerNamespaces();
        nm.Add("xsi", "http://www.w3.org/2001/XMLSchema-instance");
        IngramMailData data = (IngramMailData)serializer.Deserialize(sreader);

        UserMailer.SendIngramOrder(data.CustomerName, data.Address, data.Email, data.PhoneNumber, data.ProductList).Send();

        View("IngramOrderSent");

        return Json(new { Success = true });
      }

    }

  }
}
