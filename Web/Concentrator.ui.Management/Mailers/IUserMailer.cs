using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Mvc.Mailer;
using System.Net.Mail;

namespace Concentrator.ui.Management.Mailers
{ 
    public interface IUserMailer
    {
				
		MailMessage Welcome();


    MailMessage PasswordReset(string email, string password, string firstName, string userName);			
	}
}