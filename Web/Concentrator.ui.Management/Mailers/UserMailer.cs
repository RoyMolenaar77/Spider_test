using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Mvc.Mailer;
using System.Net.Mail;

namespace Concentrator.ui.Management.Mailers
{ 
    public class UserMailer : MailerBase, IUserMailer     
	{
		public UserMailer():
			base()
		{
			MasterName="_Layout";
		}

		
		public virtual MailMessage Welcome()
		{
			var mailMessage = new MailMessage{Subject = "Welcome"};
			
			//mailMessage.To.Add("some-email@example.com");
			//ViewBag.Data = someObject;
			PopulateBody(mailMessage, viewName: "Welcome");

			return mailMessage;
		}


    public virtual MailMessage PasswordReset(string email, string password, string firstName, string userName)
		{
      var mailMessage = new MailMessage { Subject = "Password has been reset!" };
      mailMessage.To.Add(email);

      ViewBag.FirstName = String.Format("Dear {0},", firstName);
      ViewBag.UserName = String.Format("Your username is: {0}", userName);
      ViewBag.Password = String.Format("Your password is: {0}", password);

      PopulateBody(mailMessage, viewName: "MailPassword");

      return mailMessage;
		}    
		
	}
}