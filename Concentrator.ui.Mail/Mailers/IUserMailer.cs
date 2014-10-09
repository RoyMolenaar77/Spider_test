using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Mvc.Mailer;
using System.Net.Mail;
using Concentrator.ui.Mailer.Models;

namespace Concentrator.ui.Mailer.Mailers
{
  public interface IUserMailer
  {

    MailMessage Welcome();


    MailMessage PasswordReset();


    MailMessage SendIngramOrder(string CustomerName, string Address, string Email, string PhoneNumber, List<IngramMailProduct> ProductList);
  }
}