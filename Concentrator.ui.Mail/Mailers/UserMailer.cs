using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Mvc.Mailer;
using System.Net.Mail;
using Concentrator.ui.Mailer.Models;

namespace Concentrator.ui.Mailer.Mailers
{
  public class UserMailer : MailerBase, IUserMailer
  {
    public UserMailer() :
      base()
    {
      MasterName = "_Layout";
    }


    public virtual MailMessage Welcome()
    {
      var mailMessage = new MailMessage { Subject = "Welcome" };

      //mailMessage.To.Add("some-email@example.com");
      //ViewBag.Data = someObject;
      PopulateBody(mailMessage, viewName: "Welcome");

      return mailMessage;
    }


    public virtual MailMessage PasswordReset()
    {
      var mailMessage = new MailMessage { Subject = "PasswordReset" };

      //mailMessage.To.Add("some-email@example.com");
      //ViewBag.Data = someObject;
      PopulateBody(mailMessage, viewName: "PasswordReset");

      return mailMessage;
    }

    public virtual MailMessage SendIngramOrder(string CustomerName, string Address, string Email, string PhoneNumber,
                                               List<IngramMailProduct> ProductList)
    {
      var mailMessage = new MailMessage { Subject = "Ingram Order" };
      mailMessage.To.Add("d.ariese@diract-it.nl");                  

      ViewBag.CustomerName = String.Format("Customer Name: {0}", CustomerName);
      ViewBag.Address = String.Format("Address: {0}", Address);
      ViewBag.Email = String.Format("Email: {0}", Email);
      ViewBag.PhoneNumber = String.Format("Phone Number: {0}", PhoneNumber);
      ViewBag.ProductList = ProductList;

      PopulateBody(mailMessage, viewName: "MailIngramOrder");

      return mailMessage;
    }


  }
}